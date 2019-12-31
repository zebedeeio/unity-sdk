using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ZbdUnitySDK.Models.Zebedee;
using ZbdUnitySDK.Services;

namespace ZbdUnitySDKTest
{
    public class ZebedeeLnIT
    {

        private readonly ITestOutputHelper output;
        private readonly string zebedeeUrl = "http://kong-stag.zebedee.cloud:8000/v0/";
        private readonly string apikey = "y7hd1AQ9GO6VO0TaLOt3Bza2c0PSm0zk";


        public ZebedeeLnIT(ITestOutputHelper testOutputHelper)
        {
            this.output = testOutputHelper;
        }

        [Fact]
        public async void SimpleInvoiceCreationTest()
        {
            string testDesc = "TEST DESCRIPTION";

            //service setup
            ZbdLnService zbdLnService = new ZbdLnService(zebedeeUrl, apikey );
            ///////////////////////////// Create Invoice
            ChargeData chargeData = new ChargeData();
            chargeData.Name = testDesc;
            chargeData.Description = testDesc;
            chargeData.Amount = 1000;

            //Countdown Latch
            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            String bolt = "";
            //Call the API 
            Task task = zbdLnService.createInvoiceAsync(chargeData, charge =>
            {
                try
                {
                    Assert.NotNull(charge.Data);
                    Assert.NotNull(charge.Data.Invoice);
                    Assert.NotNull(charge.Data.Invoice.Request);
                    Assert.Equal(testDesc, charge.Data.Description);
                    Assert.StartsWith("lnbc10n1", charge.Data.Invoice.Request);
                    output.WriteLine("in action bolt:" + charge.Data.Invoice.Request);
                    output.WriteLine("in action amount:" + charge.Data.Amount);
                    bolt = charge.Data.Invoice.Request;
                }
                finally
                {
                    cde.Signal();
                }
            });

            //Latch wait
            cde.Wait(5000);
            if(cde.CurrentCount != 0)
            {
                Assert.True(false,"charge call timeout ");
            }

            output.WriteLine("outside:" + bolt);

            cde.Reset();


            ///////////////////////////// PAYMENT to BOLT Invoice

            PaymentRequest paymentRequest = new PaymentRequest();
            paymentRequest.Invoice = bolt;
            paymentRequest.Description = "TEST SELF INVOICE PAUYMENT " + DateTime.Now;
            paymentRequest.InternalId = "PAYMENT-" + DateTime.Now.ToShortTimeString();

            task = zbdLnService.payInvoiceAsync(paymentRequest, paymentResponse =>
            {
                try
                {
                    Assert.NotNull(paymentResponse.Message);
                    Assert.NotNull(paymentResponse.Data);
                    Assert.NotNull(paymentResponse.Data.Id);
                    Assert.Equal("success", paymentResponse.Data.Status);
                    Assert.Equal(testDesc, paymentResponse.Data.Description);
                    output.WriteLine("Message "+paymentResponse.Message);
                    output.WriteLine("desc "+paymentResponse.Data.Description);
                }
                finally
                {
                    cde.Signal();
                }
            });

            //Latch wait
            cde.Wait(5000);
            if (cde.CurrentCount != 0)
            {
                Assert.True(false, "payment call timeout ");
            }



        }
    }
}
