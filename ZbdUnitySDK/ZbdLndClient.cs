namespace ZbdUnitySDK
{
    using System;
    using System.Collections;
    using BTCPayServer.Lightning;
    using UnityEngine;
    using ZbdUnitySDK.models;
    using ZbdUnitySDK.Services;

    public class ZbdLndClient
    {
        private LndLnService lndLnService;
        private MonoBehaviour owner = null;
        private string baseUrl;

        public ZbdLndClient(MonoBehaviour owner, string baseUrl,string macaroon)
        {
            this.owner = owner;
            this.baseUrl = baseUrl;
            this.lndLnService = new LndLnService(baseUrl, macaroon);
        }

        public Texture2D GenerateQR(string boltPayReq)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create an invoice using the specified facade.
        /// </summary>
        /// <param name="invoice">An invoice request object.</param>
        /// <returns>A new invoice object returned from the server.</returns>
        public IEnumerator CreateInvoice(InvoiceRequest invoice, Action<LightningInvoice> invoiceAction)
        {
            if (invoiceAction is null)
            {
                throw new ArgumentNullException(nameof(invoiceAction));
            }

            Debug.Log("createInvoice(): Curr milliSatoshi Price:" + invoice.MilliSatoshiAmount );

            lndLnService.CreateLndInvoice(invoice, invoiceAction);
            yield return null;

        }

    }
}
