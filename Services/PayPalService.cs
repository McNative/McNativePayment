using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using McNativePayment.Model;
using Newtonsoft.Json.Linq;

namespace McNativePayment.Services
{
    public class PayPalService
    {
        private readonly string _payPalAuthUrl;
        private readonly string _payPalOrderUrl;

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUrl;

        private string _accessToken;
        private DateTime _expiry;

        public PayPalService(string payPalAuthUrl, string payPalOrderUrl, string clientId, string clientSecret, string redirectUrl)
        {
            _payPalAuthUrl = payPalAuthUrl;
            _payPalOrderUrl = payPalOrderUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUrl = redirectUrl;
        }

        public async Task CreateOrder(Order order,IList<ProductEdition> products)
        {
            if (_accessToken == null || _expiry < DateTime.Now) await Authorize();

            object[] requestProducts = new object[products.Count];
            int index = 0;
            foreach (var product in products)
            {
                requestProducts[index] = new
                {
                    unit_amount = new
                    {
                        currency_code = "EUR",
                        value = product.Price
                    },
                    name = product.Product.Name+" ("+product.Name+")",
                    category = "DIGITAL_GOODS",
                    quantity = "1",
                };
                index++;
            }

            object content = new {
                intent = "CAPTURE",
                purchase_units = new []
                {
                    new {
                        amount = new {
                            currency_code = "EUR",
                            value = order.Amount,
                            breakdown = new
                            {
                                item_total = new
                                {
                                    currency_code = "EUR",
                                    value = order.Amount,
                                }
                            }
                        },
                        description =  "McNative Store payment",
                        invoice_id = order.Id.ToString(),
                        items = requestProducts
                    }
                },
                application_context = new {
                    brand_name = "McNative Store",
                    user_action = "PAY_NOW",
                    shipping_preference = "NO_SHIPPING",
                    return_url = _redirectUrl
                }
            };

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_payPalOrderUrl);
            request.Headers.Add("Authorization", "Bearer " + _accessToken);
            request.Headers.Add("Preferstring", "return=representation");
            request.Headers.Add("Accept-Language", "en_US");
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Method = "POST";

            string jsonContent = JsonSerializer.Serialize(content);
            byte[] postBytes = Encoding.ASCII.GetBytes(jsonContent);
            request.ContentLength = postBytes.Length;

            Stream postStream = request.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Flush();
            postStream.Close();

            HttpWebResponse response = null;
            try
            {
                response = await request.GetResponseAsync() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var readerx = new StreamReader(stream))
                {
                    Console.WriteLine(readerx.ReadToEnd());
                }
            }


            StreamReader reader = new StreamReader(response.GetResponseStream());
            string json = reader.ReadToEnd();
            JObject data = JObject.Parse(json);

            string appending = "";
            if (order.PaymentMethod.Equals("card",StringComparison.OrdinalIgnoreCase)) appending = "&fundingSource=card";

            order.ReferenceId = data["id"].ToString();
            order.CheckoutUrl = data["links"][1]["href"].ToString()+ appending;
        }

        public async Task<bool> CaptureOrder(string token)
        {
            if (_accessToken == null || _expiry < DateTime.Now) await Authorize();

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_payPalOrderUrl + "/" + token + "/capture");
                request.Method = "POST";
                request.Headers.Add("Authorization", "Bearer " + _accessToken);
                request.ContentType = "application/json";
                request.Headers.Add("Preferstring", "return=representation");
                HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
                bool result =  response.StatusCode == HttpStatusCode.Created;
                response.Dispose();
                return result;
            }
            catch(Exception){}
            return false;
        }

        private async Task Authorize()
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_payPalAuthUrl);
            request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(_clientId + ":" + _clientSecret));
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en_US");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 10000;

            byte[] postBytes = Encoding.ASCII.GetBytes("grant_type=client_credentials");
            request.ContentLength = postBytes.Length;

            Stream postStream = request.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Flush();
            postStream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string json = reader.ReadToEnd();
            JObject data = JObject.Parse(json);

            this._accessToken = data["access_token"].ToString();
            this._expiry = DateTime.Now.AddHours(1);
        }

    }
}
