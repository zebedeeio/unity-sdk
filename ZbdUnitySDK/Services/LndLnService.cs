namespace ZbdUnitySDK.Services
{
    using System;
    using System.Threading.Tasks;
    using BTCPayServer.Lightning;
    using ZbdUnitySDK.models;

    public class LndLnService
    {
        private string lndUrl = null;
        private string macaroon = null;
        public LndLnService(string lndUrl,string macaroon)
        {
            this.lndUrl = lndUrl;
            this.macaroon = macaroon;
        }

        public async Task CreateLndInvoice(InvoiceRequest invoiceReq, Action<LightningInvoice> invoiceAction)
        {
            //https://btcpay-test.zebedee.dev/lnd-rest/btc/;macaroon=223423423sdfs
            string connectionString = "type=lnd-rest;server="+lndUrl+";macaroon="+macaroon;
            ILightningClientFactory factory = new LightningClientFactory(NBitcoin.Network.Main);
            ILightningClient client = factory.Create(connectionString);
            LightningInvoice lightningInvoice = await client.CreateInvoice(new LightMoney(invoiceReq.MilliSatoshiAmount, LightMoneyUnit.Satoshi), invoiceReq.Description, TimeSpan.FromMinutes(invoiceReq.ExpiryMin));
            invoiceAction(lightningInvoice);
            return;
        }

    }
}
