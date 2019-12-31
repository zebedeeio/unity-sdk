namespace ZbdUnitySDK.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BTCPayServer.Lightning;
    using BTCPayServer.Lightning.LND;
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

        //Utilize Generic BTCPAY lightning client
        public async Task CreateLndInvoice(InvoiceRequest invoiceReq, Action<LightningInvoice> invoiceAction)
        {
            string connectionString = "type=lnd-rest;server="+lndUrl+";macaroon="+macaroon;
            ILightningClientFactory factory = new LightningClientFactory(NBitcoin.Network.Main);
            ILightningClient client = factory.Create(connectionString);
            LightningInvoice lightningInvoice = await client.CreateInvoice(new LightMoney(invoiceReq.MilliSatoshiAmount, LightMoneyUnit.MilliSatoshi), invoiceReq.Description, TimeSpan.FromMinutes(invoiceReq.ExpiryMin));
            invoiceAction(lightningInvoice);
            return;
        }

        //Utilize BTCPAY lnd lightning client
        public async Task CreateLndRestInvoice(InvoiceRequest invoiceReq, Action<LightningInvoice> invoiceAction)
        {
            //Set up client
            string connectionString = "type=lnd-rest;server=" + lndUrl + ";macaroon=" + macaroon;
            ILightningClientFactory factory = new LightningClientFactory(NBitcoin.Network.Main);
            LndClient client = (LndClient)factory.Create(connectionString);
            //setup request
            LnrpcInvoice lnrpcInvoice = new LnrpcInvoice();
            lnrpcInvoice.Value = (invoiceReq.MilliSatoshiAmount / 1000).ToString();
            lnrpcInvoice.Expiry = (invoiceReq.ExpiryMin * 60).ToString();
            lnrpcInvoice.Memo = invoiceReq.Description;
            //hit API
            LnrpcAddInvoiceResponse res = await client.SwaggerClient.AddInvoiceAsync(lnrpcInvoice);
            //conver request
            LightningInvoice lightningInvoice = new LightningInvoice();
            lightningInvoice.BOLT11 = res.Payment_request;
            lightningInvoice.Amount = invoiceReq.MilliSatoshiAmount;
            invoiceAction(lightningInvoice);
            return;
        }

        public async Task SubscribeLndRestInvoice(Action<LightningInvoice> invoiceAction)
        {
            string connectionString = "type=lnd-rest;server=" + lndUrl + ";macaroon=" + macaroon;

            ILightningClientFactory factory = new LightningClientFactory(NBitcoin.Network.Main);
            ILightningClient client = factory.Create(connectionString);
            var waitToken = default(CancellationToken);
            ILightningInvoiceListener invoiceListner =  await client.Listen(waitToken);
            LightningInvoice lightningInvoice = await invoiceListner.WaitInvoice(waitToken);
            invoiceAction(lightningInvoice);
            return;
        }

    }
}
