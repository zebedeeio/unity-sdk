﻿namespace ZbdUnitySDK.Services
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
        private HttpClient client = new HttpClient();
        private JsonSerializerSettings jsonSettings = null;
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

        public async Task CreateChargeAsync(ChargeData chargeData, Action<ChargeResponse> chargeAction)
        {
            ChargeResponse deserializedCharge = await CreateChargeAsync(chargeData);
            logger.Debug("CreateChargeAsync[RES]:" + deserializedCharge.Data.Id);
            chargeAction(deserializedCharge);
        }

        public async Task<ChargeResponse> CreateChargeAsync(ChargeData chargeData)
        {
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;

            logger.Debug("CreateChargeAsync [REQ]:" + chargeData.Description);

            try
            {
                string bodyJson = JsonConvert.SerializeObject(chargeData, jsonSettings);

                StringContent httpContent = new StringContent(bodyJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(zebedeeUrl + "charges", httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                //Deserialize
                ChargeResponse deserializedCharge = JsonConvert.DeserializeObject<ChargeResponse>(responseBody, jsonSettings);
                logger.Debug("createInvoiceAsync[RES]:" + deserializedCharge.Data.Id);
                return deserializedCharge;

            }
            catch (Exception e)
            {
                logger.Error(string.Format("Error :{0} ", e.Message));
                throw e;
            }

        }

        public async Task PayInvoiceAsync(PaymentRequest paymentRequest, Action<PaymentResponse> paymentAction)
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

                paymentAction(deserializedPayment);

            }
            catch (Exception e)
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
        public async Task<ChargeResponse> SubscribeInvoiceAsync(string invoiceUUid, int timeoutSec = 60)
        {
            logger.Debug("SubscribeInvoice[REQ]:" + invoiceUUid);
            TimeSpan delay = TimeSpan.FromSeconds(1);
            TimeSpan timeout = TimeSpan.FromSeconds(Math.Min(timeoutSec, 60));//Max 60 sec

            Task timeoutTask = Task.Delay(timeout);
            ChargeResponse chargeDetail = null;
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

        private  async Task<ChargeResponse> getChargeDetailAsync(String chargeUuid)
        {
            if (String.IsNullOrEmpty(chargeUuid))
            {
                throw new ZedebeeException("GET Charge Detail Missing chargeUuid :" + chargeUuid);
            }
            ChargeResponse deserializedCharge = null;
            string responseBody = null;
            string url = this.zebedeeUrl + "charges/" + chargeUuid;
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                //Deserialize
                deserializedCharge = JsonConvert.DeserializeObject<ChargeResponse>(responseBody, jsonSettings);
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Get Charge with Exception :" + e));
                throw new ZedebeeException("Get Charge Detail ends with Exception :" + url + "-" + responseBody, e);
            }
            return deserializedCharge;
        }


        public async Task WithdrawAsync(WithdrawData withdrawData, Action<WithdrawResponse> withdrawAction)
        {
            //Deserialize
            WithdrawResponse deserializedCharge = await WithdrawAsync(withdrawData);

            withdrawAction(deserializedCharge);

        }

        public async Task<WithdrawResponse> WithdrawAsync(WithdrawData withdrawData)
        {
            HttpResponseMessage response = null;
            try
            {
                logger.Debug("WithdrawAsync:" + withdrawData.Description);
                string json = JsonConvert.SerializeObject(withdrawData, jsonSettings);
                StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                response = await client.PostAsync(this.zebedeeUrl + "withdrawal-requests", httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                //Deserialize
                WithdrawResponse deserializedCharge = JsonConvert.DeserializeObject<WithdrawResponse>(responseBody, jsonSettings);
                return deserializedCharge;

            }
            catch (Exception e)
            {
                logger.Error(string.Format("WithdrawAsync with Exception : {0}", e));
                logger.Error(response.ToString());
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
            catch (Exception e)
            {
                logger.Error(string.Format("GET WithDraw with Exception: {0}", e));
                throw e;
            }
            return withdrawDetail;
        }


    }
}
