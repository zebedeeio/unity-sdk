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

        //Create Invoice asynchronously with Callback 
        public async Task CreateInvoiceAsync(InvoiceRequest invoice, Action<ChargeDetail> invoiceAction)
        {

            ChargeData chargeData = new ChargeData();
            chargeData.Amount = invoice.MilliSatoshiAmount;
            chargeData.Description = invoice.Description;
            chargeData.Name = invoice.Description;

            logger.Debug("CreateInvoice with amount:" + chargeData.Amount);

            await zbdService.CreateInvoiceAsync(chargeData, charge => {
                invoiceAction(charge);
            } );

        }

        //Create Invoice asynchronously without Callback 
        public async Task<ChargeDetail> CreateInvoiceAsync(InvoiceRequest invoice)
        {

            ChargeData chargeData = new ChargeData();
            chargeData.Amount = invoice.MilliSatoshiAmount;
            chargeData.Description = invoice.Description;
            chargeData.Name = invoice.Description;

            logger.Debug("CreateInvoice with amount:" + chargeData.Amount);

            ChargeDetail chargeDetail = await zbdService.CreateInvoiceAsync(chargeData);
            return chargeDetail;

        }

        public async Task<string> SubscribeChargeAsync(string chargeId)
        {

            ChargeDetail chargeDetail =  await zbdService.SubscribeInvoiceAsync(chargeId);
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

        public async Task<string> SubscribeWithDrawAsync(string withdrawId)
        {

            WithdrawResponse withdrawDetail = await zbdService.SubscribeWithdrawAsync(withdrawId);
            return withdrawDetail.Data.Status;
        }


    }
}
