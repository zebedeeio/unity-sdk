namespace ZbdUnitySDK
{
    using System;
    using System.Threading.Tasks;
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
        public async Task CreateChargeAsync(Charge charge, Action<ChargeResponse> chargeAction)
        {

            //Conver from SDK class to API class
            ChargeData chargeData = new ChargeData();
            chargeData.Amount = charge.AmountInSatoshi * 1000;
            chargeData.Description = charge.Description;
            chargeData.Name = charge.Description;

            logger.Debug("CreateInvoice with amount:" + chargeData.Amount);

            await zbdService.CreateChargeAsync(chargeData, chargeResponse => {
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

            ChargeResponse chargeDetail = await zbdService.CreateChargeAsync(chargeData);
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
                logger.Debug(paymentResponse.Data.Status);
            });

        }

        //Withdraw with Callback
        public async Task WithDrawAsync(Withdraw withdraw, Action<WithdrawResponse> action)
        {

            //Satoshi to milli satoshi
            WithdrawData withdrawRequest = new WithdrawData();
            withdrawRequest.Amount = withdraw.AmountInSatoshi * 1000;
            withdrawRequest.Name = withdraw.Name;
            withdrawRequest.Description = withdraw.Description;


            await zbdService.WithdrawAsync(withdrawRequest, paymentResponse => {
                action(paymentResponse);
            });

        }

        //Withdraw without Callback
        public async Task<WithdrawResponse> WithDrawAsync(Withdraw withdraw)
        {

            //Satoshi to milli satoshi
            WithdrawData withdrawRequest = new WithdrawData();
            withdrawRequest.Amount = withdraw.AmountInSatoshi * 1000;
            withdrawRequest.Name = withdraw.Name;
            withdrawRequest.Description = withdraw.Description;

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
