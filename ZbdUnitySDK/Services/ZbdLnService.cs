namespace ZbdUnitySDK.Services
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using ZbdUnitySDK.Exception;
    using ZbdUnitySDK.Models.Zebedee;
    using UnityEngine.Networking;
    using UnityEngine; 
    using System.Runtime.CompilerServices;
    using SimpleJSON;
    using System.Collections;
    using System.Globalization;

    public class UnityWebRequestAwaiter : INotifyCompletion 
    {
        private UnityWebRequestAsyncOperation asyncOp;
        private Action continuation;

        public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
        {
            this.asyncOp = asyncOp;
            asyncOp.completed += OnRequestCompleted;
        }

        public bool IsCompleted { get { return asyncOp.isDone; } }

        public void GetResult() { }
  
        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }

        private void OnRequestCompleted(AsyncOperation obj)
        {
            if (continuation != null)
                continuation();
        }
    }

    public static class ExtensionMethods
    {
        public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            return new UnityWebRequestAwaiter(asyncOp);
        }
    }


    public class ZbdLnService : MonoBehaviour
    {
        private bool canContinue;
        private ChargeResponse currentChargeDetail;
        private string zebedeeUrl = null;
        private HttpClient client = new HttpClient();
        private string _zebedeeAuth = null;
        private JsonSerializerSettings jsonSettings = null;
        public ZbdLnService(string zebedeeUrl, string zebedeeAuth)
        {
            this.zebedeeUrl = zebedeeUrl;

            jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            jsonSettings.NullValueHandling = NullValueHandling.Ignore; 
            _zebedeeAuth = zebedeeAuth;

        }

        public void SetUp(string zebedeeUrl, string zebedeeAuth)
        {
            this.zebedeeUrl = zebedeeUrl;
            _zebedeeAuth = zebedeeAuth;
        }

        public IEnumerator CreateChargeCoroutine(ChargeData chargeData, Action<ChargeResponse> callback)
        {

            string bodyJson = JsonUtility.ToJson(chargeData.toJSONFriendly());
           
            byte[] postData = Encoding.UTF8.GetBytes(bodyJson);

            string url = this.zebedeeUrl + "charges";
            UnityWebRequest request = new UnityWebRequest(url, "POST");

            request.uploadHandler = new UploadHandlerRaw(postData);


            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("apikey", this._zebedeeAuth);


            yield return request.SendWebRequest();


            if (request.isHttpError)
            {
                throw new ZedebeeException("CreateCharge Exception :" + url + "-" + request.error);
                throw new Exception("HTTP ERROR " + request.error);
            }

            else
            {

                string json = request.downloadHandler.text;



                JSONNode data = JSON.Parse(json);
                callback(this.ParseChargeResponse(data)); 
            }
        }

        public async Task CreateChargeAsync(ChargeData chargeData, Action<ChargeResponse> chargeAction)
        {
             
            ChargeResponse deserializedCharge = await CreateChargeAsync(chargeData);
            
            chargeAction(deserializedCharge);
        }



        public async Task<ChargeResponse> CreateChargeAsync(ChargeData chargeData)
        {

            
            string bodyJson = JsonUtility.ToJson(chargeData.toJSONFriendly());
             
            byte[] postData = Encoding.UTF8.GetBytes(bodyJson);

            string url = this.zebedeeUrl + "charges";
            UnityWebRequest request = new UnityWebRequest(url, "POST");

            request.uploadHandler = new UploadHandlerRaw(postData);


            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("apikey", this._zebedeeAuth);


            await request.SendWebRequest();


            if (request.isHttpError)
            {
                throw new ZedebeeException("CreateCharge Exception :" + url + "-" + request.error+" "+ request.downloadHandler.text);
                throw new Exception("HTTP ERROR " + request.error);
            }

            else
            {

                string json = request.downloadHandler.text;
                 

                JSONNode data = JSON.Parse(json);

                return this.ParseChargeResponse(data);
            }



        }


        public IEnumerator PayInvoiceCoroutine(PaymentRequest paymentRequest, Action<PaymentResponse> callback)
        {

            string bodyJson = JsonUtility.ToJson(paymentRequest.toJSONFriendly());


            byte[] postData = Encoding.UTF8.GetBytes(bodyJson);

            string url = this.zebedeeUrl + "payments";

            UnityWebRequest request = new UnityWebRequest(url, "POST");

            request.uploadHandler = new UploadHandlerRaw(postData);


            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("apikey", this._zebedeeAuth);


            yield return request.SendWebRequest();


            if (request.isHttpError)
            {
                throw new ZedebeeException("Create Withdraw Exception :" + url + "-" + request.error + " " + request.downloadHandler.text);
            }

            else
            {

                string json = request.downloadHandler.text;



                JSONNode data = JSON.Parse(json);

                callback(this.ParsePaymentResponse(data));
            }
        }

        public async Task PayInvoiceAsync(PaymentRequest paymentRequest, Action<PaymentResponse> paymentAction)
        {
            try
            {
                string json = JsonConvert.SerializeObject(paymentRequest, jsonSettings);
                Debug.Log("paying invoice" + json);
                StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("apikey", this._zebedeeAuth);
                HttpResponseMessage response = await client.PostAsync(this.zebedeeUrl + "payments", httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Debug.Log("paying invoice res" + responseBody);
                //Deserialize
                PaymentResponse deserializedPayment = JsonConvert.DeserializeObject<PaymentResponse>(responseBody, jsonSettings);

                paymentAction(deserializedPayment);

            }
            catch (Exception e)
            {
                Debug.LogError("err "+e);
                throw e;
            }
        }



        public IEnumerator SubscribeInvoice(string invoiceUUID, Action<string>callback)
        {
            if (String.IsNullOrEmpty(invoiceUUID))
            {
                throw new ZedebeeException("GET Charge Detail Missing chargeUuid :" + invoiceUUID);
            }


            string url = this.zebedeeUrl + "charges/" + invoiceUUID;


            UnityWebRequest request = new UnityWebRequest(url, "GET");


            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("apikey", this._zebedeeAuth);

            yield return request.SendWebRequest();

            if (request.isHttpError)
            {
                throw new ZedebeeException("Get Charge Detail ends with Exception :" + url + "-" + request.error);
            }
            else
            {

                string json = request.downloadHandler.text;

                JSONNode data = JSON.Parse(json);
                string status = this.ParseChargeResponse(data).Data.Status;
                
                callback(status);

                if (!status.Equals("completed"))
                {
                    yield return new WaitForSeconds(1);
                    StartCoroutine(SubscribeInvoice(invoiceUUID, callback));
                }
            }
        }

        /// <summary>
        /// This method does polling to Zebedee GetCharge Details API with a charge ID in a loop
        /// 1. It will time out after a certain delay
        /// 2. It will keep hitting the API until get the Target status in certain frequency (e.g. Settled or Failed)
        /// </summary>
        /// <param name="invoiceUUID"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<ChargeResponse> SubscribeInvoiceAsync(string invoiceUUID, int timeoutSec = 60)
        {
            TimeSpan delay = TimeSpan.FromSeconds(1);
            TimeSpan timeout = TimeSpan.FromSeconds(Math.Min(timeoutSec, 60));//Max 60 sec

            Task timeoutTask = Task.Delay(timeout);
            ChargeResponse chargeDetail = null; 
            while ((chargeDetail == null || chargeDetail.Data.Status == "pending") && !timeoutTask.IsCompleted)
            {
              
                chargeDetail = await getChargeDetailAsync(invoiceUUID);


                await Task.Delay(delay);
              
               
            }

           
            if (timeoutTask.IsCompleted)
            {
                throw new ZedebeeException("Get Charge Detail Timed-out in " + timeout);
            }

            return chargeDetail;
        }
         

        private async Task<ChargeResponse> getChargeDetailAsync(String chargeUuid)
        {
            if (String.IsNullOrEmpty(chargeUuid))
            {
                throw new ZedebeeException("GET Charge Detail Missing chargeUuid :" + chargeUuid);
            }

           
            string url = this.zebedeeUrl + "charges/" + chargeUuid;


            UnityWebRequest request = new UnityWebRequest(url, "GET");
             
             
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("apikey", this._zebedeeAuth); 

            await request.SendWebRequest(); 

            if (request.isHttpError)
            {
                throw new ZedebeeException("Get Charge Detail ends with Exception :" + url + "-" + request.error);
            }
            else
            {

                string json = request.downloadHandler.text;
               
                JSONNode data = JSON.Parse(json);

                return this.ParseChargeResponse(data);
            }

               
        }
         

        public async Task WithdrawAsync(WithdrawData withdrawData, Action<WithdrawResponse> withdrawAction)
        {
            //Deserialize
            WithdrawResponse deserializedCharge = await WithdrawAsync(withdrawData);

            withdrawAction(deserializedCharge);

        }

        public IEnumerator WithdrawCoroutine(WithdrawData withdrawData, Action<WithdrawResponse> callback)
        {

            string bodyJson = JsonUtility.ToJson(withdrawData.toJSONFriendly());


            byte[] postData = Encoding.UTF8.GetBytes(bodyJson);

            string url = this.zebedeeUrl + "withdrawal-requests";

            UnityWebRequest request = new UnityWebRequest(url, "POST");

            request.uploadHandler = new UploadHandlerRaw(postData);


            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("apikey", this._zebedeeAuth);


           yield return request.SendWebRequest();


            if (request.isHttpError)
            {
                throw new ZedebeeException("Create Withdraw Exception :" + url + "-" + request.error + " " + request.downloadHandler.text); 
            }

            else
            {

                string json = request.downloadHandler.text;



                JSONNode data = JSON.Parse(json);

                callback(this.ParseWithdrawResponse(data));
            }
        }

        public async Task<WithdrawResponse> WithdrawAsync(WithdrawData withdrawData)
        {


            string bodyJson = JsonUtility.ToJson(withdrawData.toJSONFriendly());


            byte[] postData = Encoding.UTF8.GetBytes(bodyJson);

            string url = this.zebedeeUrl + "withdrawal-requests";

            UnityWebRequest request = new UnityWebRequest(url, "POST");

            request.uploadHandler = new UploadHandlerRaw(postData);


            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("apikey", this._zebedeeAuth);


            await request.SendWebRequest();


            if (request.isHttpError)
            {
                throw new ZedebeeException("Create Withdraw Exception :" + url + "-" + request.error); 
            }

            else
            {

                string json = request.downloadHandler.text;



                JSONNode data = JSON.Parse(json);

                return this.ParseWithdrawResponse(data);
            }
            

        }


        public IEnumerator SubscribeWithdraw(string withdrawUUID, Action<string> callback)
        {
            if (String.IsNullOrEmpty(withdrawUUID))
            {
                throw new ZedebeeException("GET Withdraw Detail Missing withdrawUUID :" + withdrawUUID);
            }


            string url = this.zebedeeUrl + "withdrawal-requests/" + withdrawUUID;


            UnityWebRequest request = new UnityWebRequest(url, "GET");


            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("apikey", this._zebedeeAuth);

            yield return request.SendWebRequest();

            if (request.isHttpError)
            {
                throw new ZedebeeException("Get Withdraw Detail ends with Exception :" + url + "-" + request.error + " " + request.downloadHandler.text);
            }
            else
            {
                string json = request.downloadHandler.text;

                JSONNode data = JSON.Parse(json); 

                string status = this.ParseWithdrawResponse(data).Data.Status;

                callback(status);

                if (!status.Equals("completed"))
                {
                    yield return new WaitForSeconds(1);
                    StartCoroutine(SubscribeWithdraw(withdrawUUID, callback));
                }
            }
        }


        public async Task<WithdrawResponse> SubscribeWithdrawAsync(string withdrawUUID, int timeoutSec = 60)
        {
            TimeSpan delay = TimeSpan.FromSeconds(1);
            TimeSpan timeout = TimeSpan.FromSeconds(Math.Min(timeoutSec, 60));//Max 60 sec
            Task timeoutTask = Task.Delay(timeout);
            WithdrawResponse withdrawDetail = null;
            // Keep retry  when satus is pending or timeoutTask is not completed
            while ((withdrawDetail == null || withdrawDetail.Data.Status == "pending") && !timeoutTask.IsCompleted)
            {

                withdrawDetail = await getWithdrawDetailAsync(withdrawUUID);

                await Task.Delay(delay);

            }

            if (timeoutTask.IsCompleted)
            {
                throw new ZedebeeException("Get withdraw Detail Timeout");
            }

            return withdrawDetail;
        }

        private async Task<WithdrawResponse> getWithdrawDetailAsync(String withdrawUUID)
        {

            string url = this.zebedeeUrl + "withdrawal-requests/" + withdrawUUID;


            UnityWebRequest request = new UnityWebRequest(url, "GET");


            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("apikey", this._zebedeeAuth);
           
            await request.SendWebRequest(); 

            if (request.isHttpError)
            {
                throw new ZedebeeException("Get Withdraw Detail ends with Exception :" + url + "-" + request.error+" "+request.downloadHandler.text);
            }
            else
            { 
                string json = request.downloadHandler.text;

                JSONNode data = JSON.Parse(json); 
                return this.ParseWithdrawResponse(data);
            }

         
        }




        public async Task GetWalletAsync(Action<WalletResponse> walletAction)
        {

            WalletResponse deserializedCharge = await GetWalletAsync();

            walletAction(deserializedCharge);
        }



        public async Task<WalletResponse> GetWalletAsync()
        {


             
            string url = this.zebedeeUrl + "wallet";

            UnityWebRequest request = new UnityWebRequest(url, "GET");

            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("apikey", this._zebedeeAuth);


            await request.SendWebRequest();


            if (request.isHttpError)
            {
                throw new ZedebeeException("GetWallet Exception :" + url + "-" + request.error);
                throw new Exception("HTTP ERROR " + request.error);
            }

            else
            {

                string json = request.downloadHandler.text;

                JSONNode data = JSON.Parse(json);

                return this.ParseWalletResponse(data);
            }



        }




        public IEnumerator GetWalletCoroutine(Action<WalletResponse> callback)
        {

           
             

            string url = this.zebedeeUrl + "wallet";
            UnityWebRequest request = new UnityWebRequest(url, "GET");
             

            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("apikey", this._zebedeeAuth);


            yield return request.SendWebRequest();


            if (request.isHttpError)
            {
                throw new ZedebeeException("GetWallet Exception :" + url + "-" + request.error);
                throw new Exception("HTTP ERROR " + request.error);
            }

            else
            {

                string json = request.downloadHandler.text;
                 

                JSONNode data = JSON.Parse(json);
                callback(this.ParseWalletResponse(data));
            }
        }







        private ChargeResponse ParseChargeResponse(JSONNode data)
        {
            ChargeResponse chargeRespone = new ChargeResponse();
            chargeRespone.Message = data["message"].Value;

            chargeRespone.Data = new ChargeData();
            chargeRespone.Data.Id = data["data"]["id"].Value; 
            chargeRespone.Data.InternalId = data["data"]["internalId"].Value;
            chargeRespone.Data.Status = data["data"]["status"].Value;
            chargeRespone.Data.Amount = data["data"]["amount"].AsInt;
            chargeRespone.Data.Description = data["data"]["description"].Value; 
            chargeRespone.Data.CreatedAt = DateTime.Parse(data["data"]["createdAt"].Value);
            chargeRespone.Data.CallbackUrl = data["data"]["callbackUrl"].Value;
            chargeRespone.Data.ExpiresAt = DateTime.Parse(data["data"]["expiresAt"].Value,
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.AssumeUniversal |
                                       DateTimeStyles.AdjustToUniversal);
            chargeRespone.Data.Invoice = new Invoice();
            chargeRespone.Data.Invoice.Request = data["data"]["invoice"]["request"].Value; 
            chargeRespone.Data.Invoice.URI = data["data"]["invoice"]["uri"].Value;
            return chargeRespone;
        }

        private WithdrawResponse ParseWithdrawResponse(JSONNode data)
        {
            WithdrawResponse withdrawRespone = new WithdrawResponse();
            withdrawRespone.Message = data["message"].Value;

            withdrawRespone.Data = new WithdrawData();
            withdrawRespone.Data.Id = data["data"]["id"].Value;
            withdrawRespone.Data.InternalId = data["data"]["internalId"].Value;
            withdrawRespone.Data.Status = data["data"]["status"].Value;
            withdrawRespone.Data.Amount = data["data"]["amount"].AsInt;
            withdrawRespone.Data.Description = data["data"]["description"].Value;
            withdrawRespone.Data.CreatedAt = DateTime.Parse(data["data"]["createdAt"].Value);
            withdrawRespone.Data.CallbackUrl = data["data"]["callbackUrl"].Value;

            withdrawRespone.Data.ExpiresAt = DateTime.Parse(data["data"]["expiresAt"].Value);
            withdrawRespone.Data.Invoice = new WithdrawInvoice();
            withdrawRespone.Data.Invoice.Request = data["data"]["invoice"]["request"].Value;
            withdrawRespone.Data.Invoice.URI = data["data"]["invoice"]["uri"].Value;
            withdrawRespone.Data.Invoice.FastRequest = data["data"]["invoice"]["fastRequest"].Value;
            withdrawRespone.Data.Invoice.FastURI = data["data"]["invoice"]["fastUri"].Value;
            return withdrawRespone;
        }

        private WalletResponse ParseWalletResponse(JSONNode data)
        {
            WalletResponse walletRespone = new WalletResponse();
            walletRespone.Message = data["message"].Value;

            walletRespone.Data = new WalletData();
            walletRespone.Data.Unit = data["data"]["unit"].Value;
            walletRespone.Data.Balance = data["data"]["balance"].AsInt;
            
            return walletRespone;
        }

        private PaymentResponse ParsePaymentResponse(JSONNode data)
        {
            PaymentResponse paymentResponse = new PaymentResponse();
            paymentResponse.Message = data["message"].Value;
            paymentResponse.Data = new PaymentData();
            paymentResponse.Data.Id = data["data"]["id"].Value;
            paymentResponse.Data.Description = data["data"]["description"].Value;
            paymentResponse.Data.Status = data["data"]["status"].Value;
            paymentResponse.Data.CreatedAt = DateTime.Parse(data["data"]["createdAt"].Value, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
            return paymentResponse;
        }


    }

    
}
