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
        private readonly string zebedeeUrl = "http://kong-qa.zebedee.cloud:8000/v0/";
        private readonly string apikey = "SEJYoxe7oSM6f3UMfhMNRgd5rz2UHEWE";

        public ZebedeeLnIT(ITestOutputHelper testOutputHelper)
        {
            this.output = testOutputHelper;
        }

        [Fact]
        public async void InvoiceCreationAndPaymentTest()
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
            //Call the API and assert within the callback
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
                    //Memo BOLT11 for payment
                    bolt = charge.Data.Invoice.Request;
                }
                finally
                {
                    cde.Signal();
                }
            });
            await task;

            //Latch wait
            cde.Wait(3000);
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

        //WithdrawAsync

        [Fact]
        public async void WithdrawalTest()
        {
            string testDesc = "TEST DESCRIPTION";

            //service setup
            ZbdLnService zbdLnService = new ZbdLnService(zebedeeUrl, apikey);

            ///////////////////////////// Create Invoice
            WithdrawRequest request = new WithdrawRequest();

            request.Description = testDesc;
            request.Amount = 1000;
            request.InternalId = "IntegTest-Withdrawal" + DateTime.Now.ToString();

            //Countdown Latch
            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            //Call the API and assert within the callback
            Task task = zbdLnService.WithdrawAsync(request, withdrawResponse =>
            {
                try
                {
                    Assert.NotNull(withdrawResponse.URL);
                    Assert.NotNull(withdrawResponse.Lnurl);
                    Assert.StartsWith("http", withdrawResponse.URL);
                    Assert.StartsWith("lnurl", withdrawResponse.Lnurl);

                    output.WriteLine("in action url:" + withdrawResponse.URL);
                    output.WriteLine("in action lnurl:" + withdrawResponse.Lnurl);
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
                Assert.True(false, "charge call timeout ");
            }
        }


        }
    }
