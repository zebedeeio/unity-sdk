namespace ZbdUnitySDK.Services
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using UnityEngine;
    using ZbdUnitySDK.Models.Zebedee;

    public class ZbdLnService
    {
        private string zebedeeUrl = null;
        private string zebedeeAuth = null;
        private static readonly HttpClient Client = new HttpClient();
        private static JsonSerializerSettings jsonSettings = null;

        public ZbdLnService(string zebedeeUrl, string zebedeeAuth)
        {
            this.zebedeeUrl = zebedeeUrl;
            this.zebedeeAuth = zebedeeAuth;

            jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
        }

        public async Task createInvoiceAsync(ChargeData chargeData, Action<Charge> invoiceAction)
        {
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;


            try
            {
                string bodyJson = JsonConvert.SerializeObject(chargeData,jsonSettings);

                StringContent httpContent = new StringContent(bodyJson, Encoding.UTF8, "application/json");
                httpContent.Headers.Add("apikey", zebedeeAuth);
                Debug.Log(zebedeeUrl + "charges");
                Debug.Log("bodyJson:" + bodyJson);
                HttpResponseMessage response = await Client.PostAsync(zebedeeUrl + "charges",httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.Log(responseBody);

                //Deserialize
                Charge deserializedCharge = JsonConvert.DeserializeObject<Charge>(responseBody, jsonSettings);

                invoiceAction(deserializedCharge);

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                throw e;
            }

        }

        public async Task payInvoiceAsync(PaymentRequest paymentRequest, Action<PaymentResponse> paymentction)
        {
            try
            {
                string json = JsonConvert.SerializeObject(paymentRequest, jsonSettings);

                StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                httpContent.Headers.Add("apikey", this.zebedeeAuth);
                HttpResponseMessage response = await Client.PostAsync(this.zebedeeUrl + "payments", httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                //Deserialize
                PaymentResponse deserializedCharge = JsonConvert.DeserializeObject<PaymentResponse>(responseBody, jsonSettings);


                paymentction(deserializedCharge);

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Message :{0} ", e.Message);
                throw e;
            }

        }

        public models.Invoice subscribePayment()
        {
            throw new NotImplementedException();
        }

        public async Task WithDrawAsync(PaymentRequest paymentRequest, Action<PaymentResponse> paymentction)
        {
            try
            {
                string json = JsonConvert.SerializeObject(paymentRequest, jsonSettings);

                StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                httpContent.Headers.Add("apikey", this.zebedeeAuth);
                HttpResponseMessage response = await Client.PostAsync(this.zebedeeUrl + "payments", httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                //Deserialize
                PaymentResponse deserializedCharge = JsonConvert.DeserializeObject<PaymentResponse>(responseBody, jsonSettings);


                paymentction(deserializedCharge);

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Message :{0} ", e.Message);
                throw e;
            }

        }
    }
}
