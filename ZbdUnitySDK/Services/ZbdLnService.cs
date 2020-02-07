namespace ZbdUnitySDK.Services
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using ZbdUnitySDK.Exception;
    using ZbdUnitySDK.Logging;
    using ZbdUnitySDK.Models.Zebedee;

    public class ZbdLnService
    {
        private string zebedeeUrl = null;
        private static readonly HttpClient client = new HttpClient();
        private static JsonSerializerSettings jsonSettings = null;
        private IZdbLogger logger;
        public ZbdLnService(string zebedeeUrl, string zebedeeAuth)
        {
            this.zebedeeUrl = zebedeeUrl;

            jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            client.DefaultRequestHeaders.Add("apikey", zebedeeAuth);
            this.logger = LoggerFactory.GetLogger();

        }

        public async Task CreateInvoiceAsync(ChargeData chargeData, Action<ChargeDetail> invoiceAction)
        {
            ChargeDetail deserializedCharge = await CreateInvoiceAsync(chargeData);
            logger.Debug("createInvoiceAsync[RES]:" + deserializedCharge.Data.Id);
            invoiceAction(deserializedCharge);
        }

        public async Task<ChargeDetail> CreateInvoiceAsync(ChargeData chargeData)
        {
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;

            logger.Debug("createInvoiceAsync [REQ]:" + chargeData.Description);

            try
            {
                string bodyJson = JsonConvert.SerializeObject(chargeData, jsonSettings);

                StringContent httpContent = new StringContent(bodyJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(zebedeeUrl + "charges", httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                //Deserialize
                ChargeDetail deserializedCharge = JsonConvert.DeserializeObject<ChargeDetail>(responseBody, jsonSettings);
                logger.Debug("createInvoiceAsync[RES]:" + deserializedCharge.Data.Id);
                return deserializedCharge;

            }
            catch (HttpRequestException e)
            {
                logger.Error(string.Format("Error :{0} ", e.Message));
                throw e;
            }

        }

        public async Task PayInvoiceAsync(PaymentRequest paymentRequest, Action<PaymentResponse> paymentction)
        {
            try
            {
                string json = JsonConvert.SerializeObject(paymentRequest, jsonSettings);
                logger.Debug("payInvoiceAsync[REQ]:" + paymentRequest.Description);

                StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(this.zebedeeUrl + "payments", httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                //Deserialize
                PaymentResponse deserializedPayment = JsonConvert.DeserializeObject<PaymentResponse>(responseBody, jsonSettings);
                logger.Debug("payInvoiceAsync[RES]:" + deserializedPayment.Data.Id);

                paymentction(deserializedPayment);

            }
            catch (HttpRequestException e)
            {
                logger.Error(string.Format("Message :{0} ", e.Message));
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
        public async Task<ChargeDetail> SubscribeInvoiceAsync(string invoiceUUid, int timeoutSec = 60)
        {
            logger.Debug("SubscribeInvoice[REQ]:" + invoiceUUid);
            TimeSpan delay = TimeSpan.FromSeconds(1);
            TimeSpan timeout = TimeSpan.FromSeconds(Math.Min(timeoutSec, 60));//Max 60 sec

            Task timeoutTask = Task.Delay(timeout);
            ChargeDetail chargeDetail = null;
            // Keep retry  when satus is pending or timeoutTask is not completed
            while ( (chargeDetail == null || chargeDetail.Data.Status== "pending") &&  ! timeoutTask.IsCompleted  )
            {
                chargeDetail = await getChargeDetailAsync(invoiceUUid);
                await Task.Delay(delay);
            }

            if (timeoutTask.IsCompleted)
            {
                throw new ZedebeeException("Get Charge Detail Timed-out in " + timeout);
            }

            return chargeDetail;
        }

        private  async Task<ChargeDetail> getChargeDetailAsync(String chargeUuid)
        {
            if (String.IsNullOrEmpty(chargeUuid))
            {
                throw new ZedebeeException("GET Charge Detail Missing chargeUuid :" + chargeUuid);
            }
            ChargeDetail deserializedCharge = null;
            string responseBody = null;
            string url = this.zebedeeUrl + "charges/" + chargeUuid;
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                //Deserialize
                deserializedCharge = JsonConvert.DeserializeObject<ChargeDetail>(responseBody, jsonSettings);
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Get Charge with Exception :" + e));
                throw new ZedebeeException("Get Charge Detail ends with Exception :" + url + "-" + responseBody, e);
            }
            return deserializedCharge;
        }


        public async Task WithdrawAsync(WithdrawRequest withdrawRequest, Action<WithdrawResponse> withdrawAction)
        {
            //Deserialize
            WithdrawResponse deserializedCharge = await WithdrawAsync(withdrawRequest);

            withdrawAction(deserializedCharge);

        }

        public async Task<WithdrawResponse> WithdrawAsync(WithdrawRequest withdrawRequest)
        {
            try
            {
                logger.Debug("WithdrawAsync:" + withdrawRequest.Description);
                string json = JsonConvert.SerializeObject(withdrawRequest, jsonSettings);
                StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(this.zebedeeUrl + "withdrawal-requests-create", httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                //Deserialize
                WithdrawResponse deserializedCharge = JsonConvert.DeserializeObject<WithdrawResponse>(responseBody, jsonSettings);
                return deserializedCharge;

            }
            catch (HttpRequestException e)
            {
                logger.Error(string.Format("WithdrawAsync with Exception : {0}", e));
                throw e;
            }

        }

        public async Task<WithdrawResponse> SubscribeWithdrawAsync(string withdrawUuid, int timeoutSec = 60)
        {
            logger.Debug("SubscribeWithdraw:" + withdrawUuid);
            TimeSpan delay = TimeSpan.FromSeconds(1);
            TimeSpan timeout = TimeSpan.FromSeconds(Math.Min(timeoutSec, 60));//Max 60 sec
            Task timeoutTask = Task.Delay(timeout);
            WithdrawResponse withdrawDetail = null;
            // Keep retry  when satus is pending or timeoutTask is not completed
            while ((withdrawDetail == null || withdrawDetail.Data.Status == "pending") && !timeoutTask.IsCompleted)
            {

                withdrawDetail = await getWithdrawDetailAsync(withdrawUuid);
                await Task.Delay(delay);

            }

            if (timeoutTask.IsCompleted)
            {
                throw new ZedebeeException("Get withdraw Detail Timeout");
            }

            return withdrawDetail;
        }
        private async Task<WithdrawResponse> getWithdrawDetailAsync(String withdrawUuid)
        {

            WithdrawResponse withdrawDetail = null;
            try
            {
                HttpResponseMessage response = await client.GetAsync(this.zebedeeUrl + "withdrawal-requests/" + withdrawUuid);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                //Deserialize
                withdrawDetail = JsonConvert.DeserializeObject<WithdrawResponse>(responseBody, jsonSettings);
            }
            catch (HttpRequestException e)
            {
                logger.Error(string.Format("GET WithDraw with Exception: {0}", e));
                throw e;
            }
            return withdrawDetail;
        }


    }
}
