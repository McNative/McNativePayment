using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McNativePayment.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace McNativePayment.Services
{
    public class OrderService : BackgroundService
    {

        private static Random RANDOM = new Random();
        public static ConcurrentQueue<Order> ORDERS = new ConcurrentQueue<Order>();
        private readonly IServiceScopeFactory _scopeFactory;

        public OrderService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@(){};-._&%*+";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[RANDOM.Next(s.Length)]).ToArray());
        }
        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>DoWork(cancellationToken), cancellationToken);
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            PaymentContext payment = scope.ServiceProvider.GetRequiredService<PaymentContext>();
            McNativeContext mcnative = scope.ServiceProvider.GetRequiredService<McNativeContext>();
            while (!cancellationToken.IsCancellationRequested)
            {
                await using var mcnTransaction = await mcnative.Database.BeginTransactionAsync(cancellationToken);
                await using var payTransaction = await payment.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    if (ORDERS.TryDequeue(out var order))
                    {
                        IList<OrderProduct> products = payment.OrderProducts.Where(p => p.OrderId == order.Id)
                            .Include(e => e.ProductEdition)
                            .ThenInclude(e => e.Product)
                            .ThenInclude(e => e.Assignments)
                            .ToList();

                        Transaction transaction = new Transaction
                        {
                            ToOrganisationId = order.OrganisationId,
                            Subject = "Purchase",
                            OrderId = order.Id,
                            AmountIn = order.Amount,
                            AmountOut = order.Amount,
                            Status = "APPROVED",
                            Time = DateTime.Now,
                            IssuerId = "464d60d2-e1fc-4986-b7cd-70ad6288f666"
                        };

                        foreach (OrderProduct orderProduct in products)
                        {
                            ProductEdition edition = orderProduct.ProductEdition;
                            Product product = edition.Product;
                            Transaction internalTransaction = new Transaction
                            {
                                FromOrganisationId = order.OrganisationId,
                                ToOrganisationId = product.OrganisationId,
                                Subject = "Purchase",
                                OrderId = order.Id,
                                ProductId = product.Id,
                                ProductEditionId = product.Id,
                                AmountIn = orderProduct.Amount,
                                AmountOut = orderProduct.Amount*0.8,
                                Status = "APPROVED",
                                Time = DateTime.Now,
                                IssuerId = "464d60d2-e1fc-4986-b7cd-70ad6288f666"
                            };
                            await payment.AddAsync(internalTransaction, cancellationToken);

                            foreach (ProductAssignment assignment in product.Assignments)
                            { 
                                LicenseActive active = await mcnative.ActiveLicenses.Where(a => a.OrganisationId == order.OrganisationId).FirstOrDefaultAsync(cancellationToken);
                                if (active == null)
                                {
                                    active = new LicenseActive
                                    {
                                        LicenseId = assignment.ReferenceId, 
                                        OrganisationId = order.OrganisationId,
                                        Key = RandomString(128),
                                        ActivationDate = DateTime.Now
                                    };
                                    if(edition.Duration != null) active.Expiry = DateTime.Now.AddDays(edition.Duration.Value);
                                    await mcnative.AddAsync(active, cancellationToken);
                                }
                                else
                                {
                                    if(active.Expiry != null) active.Expiry = active.Expiry.Value.AddDays(edition.Duration.Value);
                                    active.LastRenewal = DateTime.Now; 
                                    mcnative.Update(active);
                                }
                            }
                        }

                        order.Status = "COMPLETED";

                        payment.Update(order);
                        await payment.AddAsync(transaction, cancellationToken);

                        await payment.SaveChangesAsync(cancellationToken);
                        await mcnative.SaveChangesAsync(cancellationToken);
                        await payTransaction.CommitAsync(cancellationToken);
                        await mcnTransaction.CommitAsync(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    await payTransaction.RollbackAsync(cancellationToken);
                    await mcnTransaction.RollbackAsync(cancellationToken);
                }
            }
        }
    }
}
