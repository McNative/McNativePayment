using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McNativePayment.Model;
using Stripe;
using Stripe.Checkout;
using Order = McNativePayment.Model.Order;

namespace McNativePayment.Services
{
    public class StripeService
    {

        private readonly SessionService _sessionService;

        public StripeService(string apiKey)
        {
            StripeConfiguration.ApiKey = apiKey;
            _sessionService = new SessionService();
        }

        public async Task CreateOrder(string email, string method,Order order,IList<ProductEdition> products)
        {
            var items = new List<SessionLineItemOptions>();

            foreach (var product in products)
            {
                items.Add(new SessionLineItemOptions()
                {
                    Amount = (int)(product.Price*100.0),
                    Quantity = 1,
                    Currency = "eur",
                    Name = product.Product.Name
                });
            }

            var options = new SessionCreateOptions
            {
                CustomerEmail = email,
                SuccessUrl = order.RedirectUrl,
                CancelUrl = order.CancelUrl??order.RedirectUrl,
                PaymentMethodTypes = new string[] { method.ToLower() }.ToList(),
                LineItems = items,
                Mode = "payment",
            };
            var result = _sessionService.Create(options);

            order.ReferenceId = result.Id;
            order.CheckoutUrl = result.Url;
        }

    }
}
