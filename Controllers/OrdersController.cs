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

        public OrdersController(PaymentContext context,PayPalService payPalService)
        {
            _context = context;
            _payPalService = payPalService;
        }

        [HttpPost]
        [ODataRoute("Create")]
        public async Task<ActionResult<LicenseActive>> CreateOrder(ODataActionParameters parameters)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            string method = (string) parameters["PaymentMethod"];
            string organisationId = (string)parameters["OrganisationId"];
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

            Order order = new Order();
            order.OrganisationId = organisationId;
            order.PaymentProvider = "PAYPAL";
            order.PaymentMethod = method;
            order.Status = "OPEN";
            order.Amount = price;
            order.Created = DateTime.Now;
            order.Expiry = DateTime.Now.AddDays(1);

            if (parameters.ContainsKey("RedirectUrl")) order.RedirectUrl = (string) parameters["RedirectUrl"];

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

            await _payPalService.CreateOrder(order, products);

            await _context.SaveChangesAsync();
            return Ok(order);
        }
    }
}
