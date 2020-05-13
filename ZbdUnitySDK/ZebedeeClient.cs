namespace ZbdUnitySDK
{
    using System;
    using System.Threading.Tasks; 
    using ZbdUnitySDK.Models;
    using ZbdUnitySDK.Models.Zebedee;
    using ZbdUnitySDK.Services;
    using UnityEngine;
    using ZbdUnitySDK.Exception; 
    /// <summary>
    /// This is the facade class for ZEBEDEE API
    /// The Class has public methods Unity Csharp Code interacts with ZEBEDEE API server
    /// </summary>
    public class ZebedeeClient : MonoBehaviour
    {
        private ZbdLnService zbdService; 

        /// <summary>
        /// Instantiate the client object with  url with ZEBEDEE API server and API key from Developers Dashboard
        /// </summary>
        /// <param name="apikey">API Key from ZEBEDEE Developers Dashboard for the game</param>
        /// <param name="baseUrl">ZEBEDEE HTTP REST API base Url e.g. https://beta-api.zebedee.io/v0/ (refer API Doc)</param>
        public ZebedeeClient(string baseUrl, string apiKey)
        {
            if (baseUrl == null || baseUrl.Equals(string.Empty))
            {
                throw new ZedebeeException("Please set the base url e.g. https://beta-api.zebedee.io/v0/");
            }
            if (apiKey == null || apiKey.Equals(string.Empty))
            {
                throw new ZedebeeException("Please set the your api key which can be found in your dashboard");
            }
            this.zbdService = new ZbdLnService(baseUrl, apiKey);
             
        }

        public void SetUp(string baseUrl, string apiKey)
        {

            if (baseUrl == null || baseUrl.Equals(string.Empty))
            {
                throw new ZedebeeException("Please set the base url e.g. https://beta-api.zebedee.io/v0/");
            }
            if (apiKey == null || apiKey.Equals(string.Empty))
            {
                throw new ZedebeeException("Please set the your api key which can be found in your dashboard");
            }
            this.zbdService = gameObject.AddComponent<ZbdLnService>();
            zbdService.SetUp(baseUrl, apiKey);

        }
        /// <summary>
        ///Create Invoice asynchronously with Callback function handling ChargeDetail Response
        ///<param name="invoice">invoice reques containing amount in milli satoshi and description</param>
        /// </summary>
        public async Task CreateChargeAsync(Charge charge, Action<ChargeResponse> chargeAction)
        {
             
            //Conver from SDK class to API class
            ChargeData chargeData = new ChargeData();
            chargeData.Amount = charge.AmountInSatoshi * 1000;
            chargeData.Description = charge.Description;
            chargeData.Name = charge.Description;
            chargeData.InternalId = charge.InternalId;
           

            if (charge.ExpiresInSec == 0)
            {
                charge.ExpiresInSec = 300;
            }

            chargeData.ExpiresIn = charge.ExpiresInSec;

            await zbdService.CreateChargeAsync(chargeData, chargeResponse => {
                 
                chargeAction(chargeResponse);

            });

        }


        public void CreateCharge(Charge charge, Action<ChargeResponse> callback)
        {
            ChargeData chargeData = new ChargeData();
            chargeData.Amount = charge.AmountInSatoshi * 1000;
            chargeData.Description = charge.Description;
            chargeData.Name = charge.Description;
            chargeData.InternalId = charge.InternalId;
             
            if (charge.ExpiresInSec == 0)
            {
                charge.ExpiresInSec = 300;
            }

            chargeData.ExpiresIn = charge.ExpiresInSec;

            StartCoroutine(zbdService.CreateChargeCoroutine(chargeData,callback));

           
        }

        /// <summary>
        ///Create Invoice asynchronously , returning  ChargeDetail Reponse
        /// </summary>
        public async Task<ChargeResponse> CreateInvoiceAsync(Charge charge)
        {

            ChargeData chargeData = new ChargeData();
            chargeData.Amount = charge.AmountInSatoshi * 1000;
            chargeData.Description = charge.Description;
            chargeData.Name = charge.Description;
            ChargeResponse chargeDetail = await zbdService.CreateChargeAsync(chargeData);
            return chargeDetail;

        }

        public async Task<string> SubscribeChargeAsync(string chargeId, int timeoutSec = 60)
        {
            ChargeResponse chargeDetail =  await zbdService.SubscribeInvoiceAsync(chargeId, timeoutSec); 
            return chargeDetail.Data.Status;
        }

        public void SubscribeCharge(string chargeId, Action<string> callback)
        {

            StartCoroutine(zbdService.SubscribeInvoice(chargeId, callback)); 
            
        }

        public async Task PayInvoiceAsync(string bolt11)
        {
            PaymentRequest paymentRequest = new PaymentRequest();

            paymentRequest.Invoice = bolt11;

            await zbdService.PayInvoiceAsync(paymentRequest, paymentResponse => { 
            });

        }

        public async Task PayInvoice(string bolt11, Action<PaymentResponse> callback)
        {
            PaymentRequest paymentRequest = new PaymentRequest();

            paymentRequest.Invoice = bolt11;

            StartCoroutine(zbdService.PayInvoiceCoroutine(paymentRequest, callback));

        }

        //Withdraw with Callback
        public async Task WithdrawAsync(Withdraw withdraw, Action<WithdrawResponse> action)
        {

            Debug.Log("WARNING, USERS MAY ABUSE WITHDRAW FUNCTIONALITY IN YOUR GAME");
            //Satoshi to milli satoshi
            WithdrawData withdrawRequest = new WithdrawData();
            withdrawRequest.Amount = withdraw.AmountInSatoshi * 1000;
            withdrawRequest.Name = withdraw.Name;
            withdrawRequest.Description = withdraw.Description;
            withdrawRequest.InternalId = withdraw.InternalId;

            if (withdraw.ExpiresInSec == 0)
            {
                withdraw.ExpiresInSec = 300;
            }

            withdrawRequest.ExpiresIn = withdraw.ExpiresInSec;

            await zbdService.WithdrawAsync(withdrawRequest, paymentResponse => {
                action(paymentResponse);
            });

        }

        public void Withdraw(Withdraw withdraw, Action<WithdrawResponse> callback)
        {
            Debug.Log("WARNING, USERS MAY ABUSE WITHDRAW FUNCTIONALITY IN YOUR GAME");
            WithdrawData withdrawRequest = new WithdrawData();
            withdrawRequest.Amount = withdraw.AmountInSatoshi * 1000;
            withdrawRequest.Name = withdraw.Name;
            withdrawRequest.Description = withdraw.Description;
            withdrawRequest.InternalId = withdraw.InternalId;

            if (withdraw.ExpiresInSec == 0)
            {
                withdraw.ExpiresInSec = 300;
            }

            withdrawRequest.ExpiresIn = withdraw.ExpiresInSec;

            StartCoroutine(zbdService.WithdrawCoroutine(withdrawRequest, callback));


        }

        public void SubscribeWithdraw(string withdrawId, Action<string> callback)
        {

            StartCoroutine(zbdService.SubscribeWithdraw(withdrawId, callback));

        }

        //Withdraw without Callback
        public async Task<WithdrawResponse> WithdrawAsync(Withdraw withdraw)
        {

            //Satoshi to milli satoshi
            WithdrawData withdrawRequest = new WithdrawData();
            withdrawRequest.Amount = withdraw.AmountInSatoshi * 1000;
            withdrawRequest.Name = withdraw.Name;
            withdrawRequest.Description = withdraw.Description;

            WithdrawResponse withdrawResponse = await zbdService.WithdrawAsync(withdrawRequest);
            return withdrawResponse;
        }

        public async Task<string> SubscribeWithdrawAsync(string withdrawId, int timeoutSec = 60)
        {

            WithdrawResponse withdrawDetail = await zbdService.SubscribeWithdrawAsync(withdrawId, timeoutSec);
            return withdrawDetail.Data.Status;
        }



        public async Task GetWalletAsync(Action<WalletResponse> walletAction)
        {

            

            await zbdService.GetWalletAsync(walletResponse => {

                walletAction(walletResponse);
            });

        }


        public void GetWallet(Action<WalletResponse> callback)
        {
             

            StartCoroutine(zbdService.GetWalletCoroutine(callback));


        }


    }
}
