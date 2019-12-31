using BTCPayServer.Lightning;
using NBitcoin;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ZbdUnitySDK.models;
using ZbdUnitySDK.Services;

namespace ZbdUnitySDKTest
{
    public class LndTest
    {

        private readonly ITestOutputHelper output;

        public LndTest(ITestOutputHelper testOutputHelper)
        {
            this.output = testOutputHelper;
        }

        [Fact]
        public async void LndSimpleInvoiceBTCPAYDirectTest()
        {
            string connectionString = @"type=lnd-rest;server=https://btcpay-test.zebedee.dev/lnd-rest/btc/;macaroon=0201036c6e6402cf01030a10dc3031b65e4a06f269d9cfe30faf27eb1201301a160a0761646472657373120472656164120577726974651a130a04696e666f120472656164120577726974651a170a08696e766f69636573120472656164120577726974651a160a076d657373616765120472656164120577726974651a170a086f6666636861696e120472656164120577726974651a160a076f6e636861696e120472656164120577726974651a140a057065657273120472656164120577726974651a120a067369676e6572120867656e657261746500000620faa1d16fb55485945d894894d966915bb9f87826310c74131064e3f2d1d01650";
            ILightningClientFactory factory = new LightningClientFactory(Network.Main);
            ILightningClient client = factory.Create(connectionString);
            Task<LightningInvoice> task = client.CreateInvoice(1000, "CanCreateInvoice", TimeSpan.FromMinutes(5));

            LightningInvoice invoice = await task;
            Assert.NotNull(invoice);
            Assert.NotNull(invoice.BOLT11);
            output.WriteLine("BOLT:"+invoice.BOLT11);
        }

        string url = "https://btcpay-test.zebedee.dev/lnd-rest/btc/";
        string macaroon = "0201036c6e6402cf01030a10dc3031b65e4a06f269d9cfe30faf27eb1201301a160a0761646472657373120472656164120577726974651a130a04696e666f120472656164120577726974651a170a08696e766f69636573120472656164120577726974651a160a076d657373616765120472656164120577726974651a170a086f6666636861696e120472656164120577726974651a160a076f6e636861696e120472656164120577726974651a140a057065657273120472656164120577726974651a120a067369676e6572120867656e657261746500000620faa1d16fb55485945d894894d966915bb9f87826310c74131064e3f2d1d01650";

        [Fact]
        public void LndServiceSimpleTest()
        {
            LndLnService lndClient = new LndLnService(url,macaroon);
            InvoiceRequest invoiceRequest = new InvoiceRequest();
            invoiceRequest.MilliSatoshiAmount = 1000;
            invoiceRequest.Description = "Test Invoice";
            invoiceRequest.ExpiryMin = 5;

            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            Task task = lndClient.CreateLndInvoice(invoiceRequest,
                inv =>
                {
                    try
                    {
                        Assert.NotNull(inv.BOLT11);
                        Assert.StartsWith("lnbc10n1", inv.BOLT11);
                        output.WriteLine("in action bolt:"+inv.BOLT11);
                        output.WriteLine(inv.Amount.ToString());
                    }
                    finally
                    {
                        cde.Signal();
                    }
                }
            );

            cde.Wait(TimeSpan.FromSeconds(3));
            if (cde.CurrentCount != 0)
            {
                Assert.True(false, "charge call timeout ");
            }
            output.WriteLine("Main thread completes");

        }

        [Fact]
        public  async void LndRestSubscribeSimpleTest()
        {
            //1. setup lnd client
            LndLnService lndClient = new LndLnService(url, macaroon);
            CountdownEvent cde2 = new CountdownEvent(1); // initial count = 1
            string paidAmount = "";
            //2. subscribe for a notification for any invoice change from now on
            Task subsTask = lndClient.SubscribeLndRestInvoice(
                //call back action
                inv =>
                {
                    try
                    {
                        output.WriteLine("in subs invoice status:" + inv.Status);
                        output.WriteLine("in subs invoice Amount:" + inv.Amount.ToString());
                        output.WriteLine("in subs invoice BOLT11:" + inv.BOLT11);
                        //paidAmount = inv.AmountReceived.ToString();
                        //output.WriteLine("in subs action paid:" + inv.AmountReceived.ToString());
                        //output.WriteLine("in subs " +
                        //    "action amount:" + inv.Amount.ToString());
                    }
                    finally
                    {
                        cde2.Signal();
                    }
                }
                );


            await Task.WhenAll(subsTask);


            cde2.Wait(TimeSpan.FromSeconds(30));
            output.WriteLine("Paid Amount:" + paidAmount);

        }
    }
}
