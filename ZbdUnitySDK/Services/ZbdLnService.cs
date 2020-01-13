namespace ZbdUnitySDK.Services
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using ZbdUnitySDK.Exception;
    using ZbdUnitySDK.Models.Zebedee;

    public class ZbdLnService
    {
        private string zebedeeUrl = null;
        private static readonly HttpClient Client = new HttpClient();
        private static JsonSerializerSettings jsonSettings = null;

        public ZbdLnService(string zebedeeUrl, string zebedeeAuth)
        {
            this.zebedeeUrl = zebedeeUrl;

            jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            Client.DefaultRequestHeaders.Add("apikey", zebedeeAuth);

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
                HttpResponseMessage response = await Client.PostAsync(zebedeeUrl + "charges",httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

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


        /// <summary>
        /// This method does polling to Zebedee GetCharge Details API with a charge ID in a loop
        /// 1. It will time out after a certain delay
        /// 2. It will keep hitting the API until get the Target status in certain frequency (e.g. Settled or Failed)
        /// </summary>
        /// <param name="invoiceUUid"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<Charge> SubscribeInvoice(string invoiceUUid, int timeoutSec = 25)
        {
            TimeSpan delay = TimeSpan.FromSeconds(1);
            TimeSpan timeout = TimeSpan.FromSeconds(Math.Min(timeoutSec, 25));//Max 25 sec
            Task timeoutTask = Task.Delay(timeout);
            Charge chargeDetail = null;
            // Keep retry  when satus is pending or timeoutTask is not completed
            while ( (chargeDetail == null || chargeDetail.Data.Status== "pending") &&  ! timeoutTask.IsCompleted  )
            {

                chargeDetail = await getChargeDetail(invoiceUUid);
                await Task.Delay(delay);

            }

            if (timeoutTask.IsCompleted)
            {
                throw new ZedebeeException("Get Charge Detail Timeout");
            }

            return chargeDetail;
        }

        private  async Task<Charge> getChargeDetail(String chargeUuid)
        {

            Charge deserializedCharge = null;
            try
            {
                HttpResponseMessage response = await Client.GetAsync(this.zebedeeUrl + "charges/" + chargeUuid);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                //Deserialize
                deserializedCharge = JsonConvert.DeserializeObject<Charge>(responseBody, jsonSettings);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("GET Charge with Exception :" + e);
                throw e;
            }
            return deserializedCharge;
        }


        public async Task WithdrawAsync(WithdrawRequest withdrawRequest, Action<WithdrawResponse> withdrawAction)
        {
            try
            {
                string json = JsonConvert.SerializeObject(withdrawRequest, jsonSettings);
                StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await Client.PostAsync(this.zebedeeUrl + "withdrawal-requests-create", httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

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

        public async Task<WithdrawResponse> SubscribeWithdraw(string withdrawUuid, int timeoutSec = 25)
        {
            TimeSpan delay = TimeSpan.FromSeconds(1);
            TimeSpan timeout = TimeSpan.FromSeconds(Math.Min(timeoutSec, 25));//Max 25 sec
            Task timeoutTask = Task.Delay(timeout);
            WithdrawResponse withdrawDetail = null;
            // Keep retry  when satus is pending or timeoutTask is not completed
            while ((withdrawDetail == null || withdrawDetail.Data.Status == "pending") && !timeoutTask.IsCompleted)
            {

                withdrawDetail = await getWithdrawDetail(withdrawUuid);
                await Task.Delay(delay);

            }

            if (timeoutTask.IsCompleted)
            {
                throw new ZedebeeException("Get withdraw Detail Timeout");
            }

            return withdrawDetail;
        }
        private async Task<WithdrawResponse> getWithdrawDetail(String withdrawUuid)
        {

            WithdrawResponse withdrawDetail = null;
            try
            {
                HttpResponseMessage response = await Client.GetAsync(this.zebedeeUrl + "withdrawal-requests/" + withdrawUuid);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                //Deserialize
                withdrawDetail = JsonConvert.DeserializeObject<WithdrawResponse>(responseBody, jsonSettings);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("GET WithDraw with Exception :" + e);
                throw e;
            }
            return withdrawDetail;
        }


    }
}
