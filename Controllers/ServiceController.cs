using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using McNativePayment.Model;
using McNativePayment.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Stripe;
using Stripe.Checkout;
using Order = McNativePayment.Model.Order;

namespace McNativePayment.Controllers
{

    [ApiController]
    [Route("Service")]
    public class ServiceController : Controller
    {

        private readonly PaymentContext _context;
        private readonly PayPalService _payPalService;
        private readonly IMemoryCache _cache;

        public ServiceController(PaymentContext context, PayPalService payPalService, IMemoryCache cache)
        {
            _context = context;
            _payPalService = payPalService;
            _cache = cache;
        }

        [HttpGet("PayPal")]
        public async Task<IActionResult> PayPalComplete(string token)
        {
            Order order = await _context.Orders.FirstOrDefaultAsync(o => o.ReferenceId == token);
            if (order == null) return NotFound();

            bool result = await _payPalService.CaptureOrder(token);
            if (!result) return BadRequest();

            order.Status = "APPROVED";
            await _context.SaveChangesAsync();

            if(order.RedirectUrl == null) return Redirect("https://console.mcnative.org?checkout=complete");
            else return Redirect(order.RedirectUrl);
        }

        [HttpPost("Stripe")]
        public async Task<IActionResult> StripeComplete()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = HttpContext.Request.Headers["Stripe-Signature"];
            var stripeEvent = EventUtility.ConstructEvent(json, signature, Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET"), throwOnApiVersionMismatch: false);

            if(stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                Order order = await _context.Orders.FirstOrDefaultAsync(o => o.ReferenceId == session.Id);
                if (order == null) return NotFound("Order not found");

                order.Status = "APPROVED";
                await _context.SaveChangesAsync();

                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        private bool ValidateSignature(string signature, String verifyString, string certificateUrl)
        {
            if (!certificateUrl.StartsWith("https://api.paypal.com") && !certificateUrl.StartsWith("https://api.sandbox.paypal.com")) return false;
            var serverCert = new X509Certificate2(ReadPemPublicKey(certificateUrl));
            using var publicKey = serverCert.GetRSAPublicKey();
            var dataByteArray = Encoding.UTF8.GetBytes(verifyString);
            var signatureByteArray = Convert.FromBase64String(signature);
            return publicKey.VerifyData(dataByteArray, signatureByteArray, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        private byte[] ReadPemPublicKey(string certificateUrl)
        {
            byte[] bytes;
            if (!_cache.TryGetValue(certificateUrl, out bytes))
            {
                HttpWebRequest request = WebRequest.Create(certificateUrl) as HttpWebRequest;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string encodedPublicKey = reader.ReadToEnd()
                    .Replace("-----BEGIN CERTIFICATE-----\r\n", string.Empty)
                    .Replace("\r\n-----END CERTIFICATE-----", string.Empty)
                    .Trim();
                bytes = Convert.FromBase64String(encodedPublicKey);
                _cache.Set(certificateUrl, bytes);
            }

            return bytes;
        }
    }
}