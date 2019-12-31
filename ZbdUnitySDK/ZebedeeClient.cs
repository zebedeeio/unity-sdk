namespace ZbdUnitySDK
{
    using System;
    using System.Collections;
    using System.Threading.Tasks;
    using BTCPayServer.Lightning;
    using UnityEngine;
    using ZbdUnitySDK.models;
    using ZbdUnitySDK.Models.Zebedee;
    using ZbdUnitySDK.Services;

    public class ZebedeeClient
    {
        private ZbdLnService zbdService;

        public ZebedeeClient(string baseUrl,string apikey)
        {
            this.zbdService = new ZbdLnService(baseUrl, apikey);
        }
        public async Task CreateInvoice(InvoiceRequest invoice, Action<LightningInvoice> invoiceAction)
        {
            ChargeData chargeData = new ChargeData();
            chargeData.Amount = invoice.MilliSatoshiAmount;
            chargeData.Description = invoice.Description;
            chargeData.Name = invoice.Description;

            await zbdService.createInvoiceAsync(chargeData, charge => {
                LightningInvoice inv = new LightningInvoice();
                inv.BOLT11 = charge.Data.Invoice.Request;
                invoiceAction(inv);
            } );

        }

        public async Task PayInvoiceAsync(string bolt11)
        {
            PaymentRequest paymentRequest = new PaymentRequest();
            paymentRequest.Invoice = bolt11;

            await zbdService.payInvoiceAsync(paymentRequest, paymentResponse => {
                Debug.Log(paymentResponse.Data.Status);
            });

        }

    }
}
