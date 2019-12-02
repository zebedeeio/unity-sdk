namespace ZbdUnitySDK
{
    using System;
    using System.Collections;
    using NBitcoin;
    using UnityEngine;
 
    using ZbdUnitySDK.models;
    using ZbdUnitySDK.Models.BTCPay;
    using ZbdUnitySDK.Services;
    using ZbdUnitySDK.UnityUtils;
    using ZbdUnitySDK.Utils;
 
    public class UnityBtcpayClient
    {
        private Key ecKey = null;
        private string identity = null;
        private IResourceDataAccess resourceDataAccess;
        private BtcPayLnService btcPayLnService;


        public Invoice CreateInvoice()
        {
            throw new NotImplementedException();
        }

        public Texture2D GenerateQR(string boltPayReq)
        {
            throw new NotImplementedException();
        }


        public IEnumerator Intitalize(MonoBehaviour mono, string pairCode,string btcpayServerHost)
        {
            this.resourceDataAccess = new FileResourceDataAccess();
            this.InitKeys();
            this.DeriveIdentity();
 
            this.btcPayLnService = new BtcPayLnService(mono,btcpayServerHost,this.ecKey,this.identity,pairCode);
            yield return this.btcPayLnService.GetAccessTokens();

        }


        /// <summary>
        /// Create an invoice using the specified facade.
        /// </summary>
        /// <param name="invoice">An invoice request object.</param>
        /// <returns>A new invoice object returned from the server.</returns>
        public IEnumerator CreateInvoice(InvoiceBtcpay invoice, Action<InvoiceBtcpay> invoiceAction)
        {
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
