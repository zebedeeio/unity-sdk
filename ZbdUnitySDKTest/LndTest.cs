using BTCPayServer.Lightning;
using NBitcoin;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ZbdUnitySDK;
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
        public void LndSImpleInvoiceDirectTest()
        {
            string connectionString = @"type=lnd-rest;server=https://btcpay-test.zebedee.dev/lnd-rest/btc/;macaroon=0201036c6e6402cf01030a10dc3031b65e4a06f269d9cfe30faf27eb1201301a160a0761646472657373120472656164120577726974651a130a04696e666f120472656164120577726974651a170a08696e766f69636573120472656164120577726974651a160a076d657373616765120472656164120577726974651a170a086f6666636861696e120472656164120577726974651a160a076f6e636861696e120472656164120577726974651a140a057065657273120472656164120577726974651a120a067369676e6572120867656e657261746500000620faa1d16fb55485945d894894d966915bb9f87826310c74131064e3f2d1d01650";
            ILightningClientFactory factory = new LightningClientFactory(Network.Main);
            ILightningClient client = factory.Create(connectionString);
            Task<LightningInvoice> task = client.CreateInvoice(10000, "CanCreateInvoice", TimeSpan.FromMinutes(5));
            LightningInvoice invoice = task.Result;
            Assert.NotNull(invoice);
            Assert.NotNull(invoice.BOLT11);
            output.WriteLine("BOLT:"+invoice.BOLT11);
        }

        [Fact]
        public void LndSImpleInvoiceDirectTest2()
        {
            string url = @"https://btcpay-test.zebedee.dev/lnd-rest/btc/";
            string macaroon = "0201036c6e6402cf01030a10dc3031b65e4a06f269d9cfe30faf27eb1201301a160a0761646472657373120472656164120577726974651a130a04696e666f120472656164120577726974651a170a08696e766f69636573120472656164120577726974651a160a076d657373616765120472656164120577726974651a170a086f6666636861696e120472656164120577726974651a160a076f6e636861696e120472656164120577726974651a140a057065657273120472656164120577726974651a120a067369676e6572120867656e657261746500000620faa1d16fb55485945d894894d966915bb9f87826310c74131064e3f2d1d01650";

            string connectionString = "type=lnd-rest;server=" + url + ";macaroon=" + macaroon;
            ILightningClientFactory factory = new LightningClientFactory(Network.Main);
            ILightningClient client = factory.Create(connectionString);
            int amountInSat = 123;
            string description = "test description";
            string bolt11 = null;
            Task<LightningInvoice> invTask = client.CreateInvoice(new LightMoney(amountInSat, LightMoneyUnit.Satoshi), description, TimeSpan.FromMinutes(5));
            invTask.ContinueWith(task =>
            {
               LightningInvoice invoice = task.Result;
                bolt11 = invoice.BOLT11;
               Assert.NotNull(invoice);
               Assert.NotNull(invoice.BOLT11);
               output.WriteLine("bolt inside : "+invoice.BOLT11);

             }
             );
            Thread.Sleep(3000);
            output.WriteLine("bolt11:" + bolt11);

        }



        [Fact]
        public void LndServiceSimpleTest()
        {
            string url = "https://btcpay-test.zebedee.dev/lnd-rest/btc/";
            string macaroon = "0201036c6e6402cf01030a10dc3031b65e4a06f269d9cfe30faf27eb1201301a160a0761646472657373120472656164120577726974651a130a04696e666f120472656164120577726974651a170a08696e766f69636573120472656164120577726974651a160a076d657373616765120472656164120577726974651a170a086f6666636861696e120472656164120577726974651a160a076f6e636861696e120472656164120577726974651a140a057065657273120472656164120577726974651a120a067369676e6572120867656e657261746500000620faa1d16fb55485945d894894d966915bb9f87826310c74131064e3f2d1d01650";
            LndLnService lndClient = new LndLnService(url,macaroon);
            InvoiceRequest invoiceRequest = new InvoiceRequest();
            invoiceRequest.MilliSatoshiAmount = 123;
            invoiceRequest.Description = "Test Invoice";
            invoiceRequest.ExpiryMin = 5;

            CountdownEvent cde = new CountdownEvent(1); // initial count = 1

            Task task = lndClient.CreateLndInvoice(invoiceRequest,
                inv =>
                {
                    try
                    {
                        Assert.NotNull(inv.BOLT11);
                        output.WriteLine("in action bolt:"+inv.BOLT11);
                        output.WriteLine(inv.Amount.ToString());
                    }
                    finally
                    {
                                                          cde.Signal();
                               }
                }
            );

            task.ContinueWith(
                t =>
                {
                    output.WriteLine("in continue:" + t.Status);
                });

            //            Thread.Sleep(3000);
            cde.Wait(TimeSpan.FromSeconds(3));
            output.WriteLine("Main thread completes");

        }

    }
}
