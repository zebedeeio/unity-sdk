namespace ZbdUnitySDK
{
    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using ZbdUnitySDK.Logging;
    using ZbdUnitySDK.Models;
    using ZbdUnitySDK.Models.Zebedee;
    using ZbdUnitySDK.Services;
    using IZdbLogger = Logging.IZdbLogger;

    /// <summary>
    /// This is the facade class for ZEBEDEE API
    /// The Class has public methods Unity Csharp Code interacts with ZEBEDEE API server
    /// </summary>
    public class ZebedeeClient
    {
        private ZbdLnService zbdService;
        private IZdbLogger logger;

        /// <summary>
        /// Instantiate the client object with  url with ZEBEDEE API server and API key from Developers Dashboard
        /// </summary>
        /// <param name="apikey">API Key from ZEBEDEE Developers Dashboard for the game</param>
        /// <param name="baseUrl">ZEBEDEE HTTP REST API base Url e.g. https://beta-api.zebedee.io/v0/ (refer API Doc)</param>
        public ZebedeeClient(string baseUrl,string apikey)
        {
            this.zbdService = new ZbdLnService(baseUrl, apikey);
            this.logger = LoggerFactory.GetLogger();
        }

        /// <summary>
        ///Create Invoice asynchronously with Callback function handling ChargeDetail Response
        ///<param name="invoice">invoice reques containing amount in milli satoshi and description</param>
        /// </summary>
        public async Task CreateInvoiceAsync(Charge charge, Action<ChargeResponse> chargeAction)
        {

            //Conver from SDK class to API class
            ChargeData chargeData = new ChargeData();
            chargeData.Amount = charge.AmountInSatoshi * 1000;
            chargeData.Description = charge.Description;
            chargeData.Name = charge.Description;

            logger.Debug("CreateInvoice with amount:" + chargeData.Amount);

            await zbdService.CreateInvoiceAsync(chargeData, chargeResponse => {
                chargeAction(chargeResponse);
            } );

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

            logger.Debug("CreateInvoice with amount:" + chargeData.Amount);

            ChargeResponse chargeDetail = await zbdService.CreateInvoiceAsync(chargeData);
            return chargeDetail;

        }

        public async Task<string> SubscribeChargeAsync(string chargeId, int timeoutSec = 60)
        {
            ChargeResponse chargeDetail =  await zbdService.SubscribeInvoiceAsync(chargeId, timeoutSec);
            logger.Debug("SubscribeChargeAsync with amount:" + chargeDetail.Data.Amount);
            return chargeDetail.Data.Status;
        }

        public async Task PayInvoiceAsync(string bolt11)
        {
            PaymentRequest paymentRequest = new PaymentRequest();

            paymentRequest.Invoice = bolt11;

            await zbdService.PayInvoiceAsync(paymentRequest, paymentResponse => {
                Debug.Log(paymentResponse.Data.Status);
            });

        }

        //Withdraw with Callback
        public async Task WithDrawAsync(WithdrawRequest withdrawRequest, Action<WithdrawResponse> action)
        {

            //Satoshi to milli satoshi
            withdrawRequest.Amount = withdrawRequest.Amount * 1000;

            await zbdService.WithdrawAsync(withdrawRequest, paymentResponse => {
                action(paymentResponse);
            });

        }

        //Withdraw without Callback
        public async Task<WithdrawResponse> WithDrawAsync(WithdrawRequest withdrawRequest)
        {

            //Satoshi to milli satoshi
            withdrawRequest.Amount = withdrawRequest.Amount * 1000;

            WithdrawResponse withdrawResponse = await zbdService.WithdrawAsync(withdrawRequest);
            return withdrawResponse;
        }

        public async Task<string> SubscribeWithDrawAsync(string withdrawId, int timeoutSec = 60)
        {

            WithdrawResponse withdrawDetail = await zbdService.SubscribeWithdrawAsync(withdrawId, timeoutSec);
            return withdrawDetail.Data.Status;
        }


    }
}
