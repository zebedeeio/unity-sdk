namespace ZbdUnitySDK
{
    using System;
    using System.Collections;
    using NBitcoin;
    using UnityEngine;
    using ZbdUnitySDK.Exception;
    using ZbdUnitySDK.Models.BTCPay;
    using ZbdUnitySDK.Services;
    using ZbdUnitySDK.UnityUtils;
    using ZbdUnitySDK.Utils;

    /// <summary>
    /// This is the facade class for BTCPAY API
    /// The Class has public methods Unity Csharp Code interacts with ZEBEDEE API server
    /// </summary>
    public class ZbdBtcpayClient
    {
        private Key ecKey = null;
        private string identity = null;
        private IResourceDataAccess resourceDataAccess;
        private BtcPayLnService btcPayLnService;
        private bool isInitialized = false;

        public ZbdBtcpayClient(MonoBehaviour mono, string pairCode, string btcpayServerHost)
        {
            this.resourceDataAccess = new FileResourceDataAccess();
            this.InitKeys();
            this.DeriveIdentity();
            this.btcPayLnService = new BtcPayLnService(mono, btcpayServerHost, this.ecKey, this.identity, pairCode);
        }
        public IEnumerator Intitalize()
        {
            yield return this.btcPayLnService.GetAccessTokens();
            this.isInitialized = true;
        }


        /// <summary>
        /// Create an invoice using the specified facade.
        /// </summary>
        /// <param name="invoice">An invoice request object.</param>
        /// <returns>A new invoice object returned from the server.</returns>
        public IEnumerator CreateInvoice(InvoiceBtcpay invoice, Action<InvoiceBtcpay> invoiceAction)
        {
            if (!this.isInitialized)
            {
                throw new BitPayException("Client is not initialized");
            }

            if (invoiceAction is null)
            {
                throw new ArgumentNullException(nameof(invoiceAction));
            }

            Debug.Log("createInvoice(): Curr" + invoice.Currency + " Price:" + invoice.Price + " " + invoice.BuyerEmail + " " + invoice.ItemDesc);

            return btcPayLnService.CreateInvoice(invoice, invoiceAction);

        }



        /////////////////private methods///////////////////
        private void InitKeys()
        {
            // load from storage if exists
            if (resourceDataAccess.FileExists())
            {
                this.ecKey = KeyUtils.LoadNBEcKey(this.resourceDataAccess);
            }
            else
            {
                // create a new private key and store

                this.ecKey = KeyUtils.CreateNBEcKey();
                byte[] priv = this.ecKey.ToBytes();
                KeyUtils.SaveEcKey(this.ecKey, this.resourceDataAccess);
            }

        }

        private void DeriveIdentity()
        {
            // Identity in this implementation is defined to be the Bitpay SIN.
            this.identity = KeyUtils.DeriveSIN(this.ecKey);
        }

    }
}
