namespace ZbdUnitySDK
{
    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using ZbdUnitySDK.Logging;
    using ZbdUnitySDK.models;
    using ZbdUnitySDK.Models.Zebedee;
    using ZbdUnitySDK.Services;
    using IZdbLogger = Logging.IZdbLogger;

    public class ZebedeeClient
    {
        private ZbdLnService zbdService;
        private IZdbLogger logger;

        public ZebedeeClient(string baseUrl,string apikey)
        {
            this.zbdService = new ZbdLnService(baseUrl, apikey);
            this.logger = LoggerFactory.GetLogger();
        }
        public async Task CreateInvoice(InvoiceRequest invoice, Action<ChargeDetail> invoiceAction)
        {

            ChargeData chargeData = new ChargeData();
            chargeData.Amount = invoice.MilliSatoshiAmount;
            chargeData.Description = invoice.Description;
            chargeData.Name = invoice.Description;

            logger.Debug("CreateInvoice with amount:" + chargeData.Amount);

            await zbdService.createInvoiceAsync(chargeData, charge => {
                invoiceAction(charge);
            } );

        }

        public async Task<string> SubscribeChargeAsync(string chargeId)
        {

            ChargeDetail chargeDetail =  await zbdService.SubscribeInvoice(chargeId);
            logger.Debug("SubscribeChargeAsync with amount:" + chargeDetail.Data.Amount);
            return chargeDetail.Data.Status;
        }

        public async Task PayInvoiceAsync(string bolt11)
        {
            PaymentRequest paymentRequest = new PaymentRequest();

            paymentRequest.Invoice = bolt11;

            await zbdService.payInvoiceAsync(paymentRequest, paymentResponse => {
                Debug.Log(paymentResponse.Data.Status);
            });

        }

        public async Task WithDrawAsync(WithdrawRequest withdrawRequest, Action<WithdrawResponse> action)
        {

            //Satoshi to milli satoshi
            withdrawRequest.Amount = withdrawRequest.Amount * 1000;

            await zbdService.WithdrawAsync(withdrawRequest, paymentResponse => {
                action(paymentResponse);
            });

        }

        public async Task<string> SubscribeWithDrawAsync(string withdrawId)
        {

            WithdrawResponse withdrawDetail = await zbdService.SubscribeWithdraw(withdrawId);
            return withdrawDetail.Data.Status;
        }


    }
}
