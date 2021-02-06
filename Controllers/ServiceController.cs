using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Force.Crc32;
using McNativePayment.Model;
using McNativePayment.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

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

            OrderService.ORDERS.Enqueue(order);

            return Redirect("https://console.mcnative.org?checkout=complete");
        }

       // [HttpPost("PayPal")]
        public async Task<IActionResult> PayPal()
        {
            return NotFound();
            MemoryStream stream = new MemoryStream(16384);
            Request.Body.CopyToAsync(stream);

            HttpRequest request = HttpContext.Request;

            string transmissionId = request.Headers["PAYPAL-TRANSMISSION-ID"];
            string transmissionTime = request.Headers["PAYPAL-TRANSMISSION-TIME"];
            byte[] data = stream.ToArray();
            string crc = Crc32Algorithm.Compute(data).ToString();
            string webhookId = Environment.GetEnvironmentVariable("PAYPAL_WEB_HOOK_ID");
            String verifyString = transmissionId + "|" + transmissionTime + "|" + webhookId + "|" + crc;

            string signature = request.Headers["PAYPAL-TRANSMISSION-SIG"];
            string certificateUrl = request.Headers["PAYPAL-CERT-URL"];

            bool valid = ValidateSignature(signature, verifyString, certificateUrl);

            if (!valid) return BadRequest("Invalid signature");


            JObject json = JObject.Parse(Encoding.UTF8.GetString(data));

            return Ok();
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