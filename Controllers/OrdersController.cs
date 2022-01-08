using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McNativePayment.Model;
using McNativePayment.Services;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicenseActive = McNativePayment.Model.LicenseActive;

namespace McNativePayment.Controllers
{

    [ODataRoutePrefix("Orders")]
    public class OrdersController : ODataController
    {

        private readonly PaymentContext _context;
        private readonly PayPalService _payPalService;
        private readonly StripeService _stripeService;

        public OrdersController(PaymentContext context,PayPalService payPalService, StripeService stripeService)
        {
            _context = context;
            _payPalService = payPalService;
            _stripeService = stripeService;
        }

        [HttpPost]
        [ODataRoute("Create")]
        public async Task<ActionResult<LicenseActive>> CreateOrder(ODataActionParameters parameters)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            string method = (string) parameters["PaymentMethod"];
            string organisationId = (string)parameters["OrganisationId"];
            string email = parameters.ContainsKey("Email") ? (string)parameters["Email"] : null;

            IEnumerable<Guid> products0 = (IEnumerable<Guid>)parameters["Products"];

            IList<ProductEdition> products = _context.ProductEditions
                .Where(p => products0.Contains(p.Id))
                .Include(e => e.Product)
                .ToList();
            foreach (Guid product0 in products0)
            {
                ProductEdition product = products.FirstOrDefault(p => p.Id == product0);
                if (product == null || !product.Active) return NotFound(product0 + " not found");
            }

            double price = products.Sum(p => p.Price);
            if (price == 0) return BadRequest();

            bool paypal = method.Equals("PAYPAL");

            Order order = new Order();
            order.OrganisationId = organisationId;
            order.PaymentProvider = paypal ? "PAYPAL" : "STRIPE";
            order.PaymentMethod = method;
            order.Status = "OPEN";
            order.Amount = price;
            order.Created = DateTime.Now;
            order.Expiry = DateTime.Now.AddDays(1);

            if (parameters.ContainsKey("RedirectUrl")) order.RedirectUrl = (string) parameters["RedirectUrl"];
            if (parameters.ContainsKey("CancelUrl")) order.CancelUrl = (string)parameters["CancelUrl"];

            if (parameters.ContainsKey("ReferralCode"))
            {
                string code = (string)parameters["ReferralCode"];
                Referral referral = await _context.Referrals.Where(r => r.IsActive && r.Code == code).FirstOrDefaultAsync();
                order.ReferralId = referral.Id;
            }

            await _context.Orders.AddAsync(order);

            foreach (ProductEdition product in products)
            {
                await _context.OrderProducts.AddAsync(new OrderProduct()
                {
                    OrderId = order.Id,
                    ProductEditionId = product.Id,
                    Amount = product.Price
                });
            }

            if(paypal) await _payPalService.CreateOrder(order, products);
            else await _stripeService.CreateOrder(email,method, order, products);

            await _context.SaveChangesAsync();

            return Ok(order);
        }
    }
}
