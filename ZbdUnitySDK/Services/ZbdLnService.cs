namespace ZbdUnitySDK.Services
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
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
                Console.WriteLine(zebedeeUrl + "charges");
                Console.WriteLine("bodyJson:" + bodyJson);
                HttpResponseMessage response = await Client.PostAsync(zebedeeUrl + "charges",httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);

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

        public models.Invoice subscribePayment(string invoiceUUid)
        {
            throw new NotImplementedException();
        }

        public async Task WithdrawAsync(WithdrawRequest withdrawRequest, Action<WithdrawResponse> withdrawAction)
        {
            try
            {
                string json = JsonConvert.SerializeObject(withdrawRequest, jsonSettings);
                Console.WriteLine("Withdraw request:" + json);
                StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                httpContent.Headers.Add("apikey", this.zebedeeAuth);
                HttpResponseMessage response = await Client.PostAsync(this.zebedeeUrl + "withdrawal-requests-create", httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Withdraw response:" + responseBody);

                //Deserialize
                WithdrawResponse deserializedCharge = JsonConvert.DeserializeObject<WithdrawResponse>(responseBody, jsonSettings);

                withdrawAction(deserializedCharge);

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Withdrawal with Exception :" + e);
                throw e;
            }

        }
    }
}
