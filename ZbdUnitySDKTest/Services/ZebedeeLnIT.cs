using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ZbdUnitySDK.Exception;
using ZbdUnitySDK.Models.Zebedee;
using ZbdUnitySDK.Services;

namespace ZbdUnitySDKTest
{
    /// <summary>
    /// This is Test Class for Integration test with ZEBEDEE API
    /// it uses real developer accounts with small amount
    /// </summary>
    public class ZebedeeLnIT    
    {

        private readonly ITestOutputHelper output;


        /// If any thing is wrong, test URL and Api key from Postman 

        private readonly string zebedeeUrl = "https://beta-api.zebedee.io/v0/";
        //developer masashi+5@zebedee.io
        private readonly string apikey = "8NH0HhdWOcbV4YHRUESFAI3HRRSJEx2N";//DONT STEAL SATOSHIS!!
        //user masashi+6@zebedee.io
        private readonly string apikey2 = "PNcD3FnDLuHkN4XESMOJhAenvcumZ9Qz"; //DONT STEAL SATOSHIS!!

        public ZebedeeLnIT(ITestOutputHelper testOutputHelper)
        {
            this.output = testOutputHelper;
        }

        /// <summary>
        ///  Simplly Invoice generation by Charge API
        /// </summary>
        [Fact]
        public async void InvoiceCreationTest()
        {
            string testDesc = "CSHARP IT TEST for Charge ";

            //service setup
            ZbdLnService zbdLnService = new ZbdLnService(zebedeeUrl, apikey);

            ///////////////////////////// Create Invoice
            ChargeData chargeData = new ChargeData();
            chargeData.Name = testDesc;
            chargeData.Description = testDesc;
            chargeData.Amount = 1000;

            //Countdown Latch
            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            String bolt = "";
            //Call the API and assert within the callback
            Task task = zbdLnService.CreateInvoiceAsync(chargeData, charge =>
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
            if (cde.CurrentCount != 0)
            {
                Assert.True(false, "charge call timeout ");
            }

            output.WriteLine("outside:" + bolt);

            cde.Reset();
        }

        /// <summary>
        /// 1st developer pays to 2nd dev  by payment API
        /// </summary>
        [Fact]
        public async void InvoicePaymentTest()
        {
            string testDesc = "CSHARP IT TEST for Charge ";

            //service setup
            ZbdLnService zbdLnService = new ZbdLnService(zebedeeUrl, apikey ); //Pay to 2nd developer
            ZbdLnService zbdLnService2 = new ZbdLnService(zebedeeUrl, apikey2); //Generate Invoice and get paid

            ///////////////////////////// Create Invoice
            ChargeData chargeData = new ChargeData();
            chargeData.Name = testDesc;
            chargeData.Description = testDesc;
            chargeData.Amount = 1000;

            //Countdown Latch
            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            String bolt = "";
            //Call the API and assert within the callback
            Task task = zbdLnService2.CreateInvoiceAsync(chargeData, charge =>
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


            ///////////////////////////// PAYMENT dev1 to BOLT Invoice dev2

            PaymentRequest paymentRequest = new PaymentRequest();
            paymentRequest.Invoice = bolt;
            paymentRequest.Description = "CSHARP TEST SELF INVOICE PAUYMENT " + DateTime.Now;
            paymentRequest.InternalId = "PAYMENT-" + DateTime.Now.ToShortTimeString();

            task = zbdLnService.PayInvoiceAsync(paymentRequest, paymentResponse =>
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

        /// <summary>
        ///  1. dev 1 generate invoice
        ///  2. dev 1 subscribe to the invoice
        ///  3. dev 2 pay to it
        /// </summary>
        [Fact]
        public async void InvoiceCreationAndSubscribeToSettlement()
        {
            string testDesc = "CSHARP IT TEST DES for Charge/Settlement ";

            //service setup
            ZbdLnService zbdLnService = new ZbdLnService(zebedeeUrl, apikey); // invoice generation
            ZbdLnService zbdLnService2 = new ZbdLnService(zebedeeUrl, apikey2); //pay to invoice
            ///////////////////////////// Create Invoice
            ChargeData chargeData = new ChargeData();
            chargeData.Name = testDesc;
            chargeData.Description = testDesc;
            chargeData.Amount = 1000;

            //Countdown Latch
            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            String bolt = "";
            String chargeId = "";

            //Call the API and assert within the callback
            Task task = zbdLnService.CreateInvoiceAsync(chargeData, charge =>
            {
                try
                {
                    Assert.NotNull(charge.Data);
                    Assert.NotNull(charge.Data.Invoice);
                    Assert.NotNull(charge.Data.Invoice.Request);
                    Assert.Equal(testDesc, charge.Data.Description);
                    Assert.StartsWith("lnbc10n1", charge.Data.Invoice.Request);
                    output.WriteLine("in action bolt:" + charge.Data.Invoice.Request);
                    output.WriteLine("in action charge Status:" + charge.Data.Status);
                    output.WriteLine("in action amount:" + charge.Data.Amount);
                    output.WriteLine("in action change ID:" + charge.Data.Id);
                    //Memo BOLT11 for payment
                    bolt = charge.Data.Invoice.Request;
                    chargeId = charge.Data.Id;
                }
                finally
                {
                    cde.Signal();
                }
            });
            await task;

            //Latch wait
            cde.Wait(5000);
            if (cde.CurrentCount != 0)
            {
                Assert.True(false, "charge call timeout ");
            }

            output.WriteLine("outside:" + bolt);

            cde.Reset();

            ///////////////////////////// SUBSCCRIBE to BOLT Invoice
            Task<ChargeResponse> subscribeChargeTask = zbdLnService.SubscribeInvoiceAsync(chargeId);


            ///////////////////////////// PAYMENT to BOLT Invoice
            PaymentRequest paymentRequest = new PaymentRequest();
            paymentRequest.Invoice = bolt;
            paymentRequest.Description = "CSHARP TEST SELF INVOICE PAUYMENT " + DateTime.Now;
            paymentRequest.InternalId = "PAYMENT-" + DateTime.Now.ToShortTimeString();

            task = zbdLnService2.PayInvoiceAsync(paymentRequest, paymentResponse =>
            {
                try
                {
                    Assert.NotNull(paymentResponse.Message);
                    Assert.NotNull(paymentResponse.Data);
                    Assert.NotNull(paymentResponse.Data.Id);
                    Assert.Equal("success", paymentResponse.Data.Status);
                    Assert.Equal(testDesc, paymentResponse.Data.Description);
                    output.WriteLine("Message " + paymentResponse.Message);
                    output.WriteLine("desc " + paymentResponse.Data.Description);
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

            //SUBSCRIPTION  ASSERT
            ChargeResponse chargeResult = await subscribeChargeTask;

            output.WriteLine("Status:" + chargeResult.Data.Status);
            Assert.Equal("completed",chargeResult.Data.Status);



        }

        /// <summary>
        /// Charge Invoice and subscribe but do not pay.
        /// Expects timeout
        /// </summary>
        [Fact]
        public async void InvoiceCreationAndTimeout()
        {
            string testDesc = "CSHARP IT TEST DES for Charge/Timeout ";

            //service setup
            ZbdLnService zbdLnService = new ZbdLnService(zebedeeUrl, apikey);

            ///////////////////////////// Create Invoice
            ChargeData chargeData = new ChargeData();
            chargeData.Name = testDesc;
            chargeData.Description = testDesc;
            chargeData.Amount = 1000;

            //Countdown Latch
            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            String bolt = "";
            String chargeId = "";

            //Call the API and assert within the callback
            Task task = zbdLnService.CreateInvoiceAsync(chargeData, charge =>
            {
                try
                {
                    Assert.NotNull(charge.Data);
                    Assert.NotNull(charge.Data.Invoice);
                    Assert.NotNull(charge.Data.Invoice.Request);
                    Assert.Equal(testDesc, charge.Data.Description);
                    Assert.StartsWith("lnbc10n1", charge.Data.Invoice.Request);
                    output.WriteLine("in action bolt:" + charge.Data.Invoice.Request);
                    output.WriteLine("in action charge Status:" + charge.Data.Status);
                    output.WriteLine("in action amount:" + charge.Data.Amount);
                    output.WriteLine("in action change ID:" + charge.Data.Id);
                    //Memo BOLT11 for payment
                    bolt = charge.Data.Invoice.Request;
                    chargeId = charge.Data.Id;
                }
                finally
                {
                    cde.Signal();
                }
            });
            await task;

            //Latch wait
            cde.Wait(3000);
            if (cde.CurrentCount != 0)
            {
                Assert.True(false, "charge call timeout ");
            }

            output.WriteLine("outside:" + bolt);

            cde.Reset();

            ///////////////////////////// SUBSCCRIBE to BOLT Invoice
            Task<ChargeResponse> subscribeChargeTask = zbdLnService.SubscribeInvoiceAsync(chargeId);

            //SUBSCRIPTION  ASSERT

            try
            {
                ChargeResponse chargeResult = await subscribeChargeTask;
                Assert.True(false, "Timeout Exception should be thrown ");// should not reach this line

            }
            catch (ZedebeeException e) {

                output.WriteLine("Expected Exception is thrown:" + e.Message);
            }
        }




        /// 
        /// <summary>
        /// Create a simple Withdraw
        /// </summary>
        [Fact]
        public async void WithdrawalTest()
        {
            string testDesc = "CSHARP IT TEST for withdrawal";
            int testAmount = 10000;//Default 10 satoshi

            //service setup
            ZbdLnService zbdLnService = new ZbdLnService(zebedeeUrl, apikey);

            ///////////////////////////// Create Invoice
            WithdrawRequest request = new WithdrawRequest();

            request.Description = testDesc;
            request.Amount = testAmount;
            request.InternalId = "IntegTest-Withdrawal" + DateTime.Now.ToString();

            //Countdown Latch
            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            //Call the API and assert within the callback
            Task task = zbdLnService.WithdrawAsync(request, withdrawResponse =>
            {
                try
                {
                    Assert.NotNull(withdrawResponse.Data.Invoice.Request);
                    Assert.NotNull(withdrawResponse.Data.Id);
                    Assert.Equal(testAmount, withdrawResponse.Data.Amount);
                    Assert.StartsWith("lnurl", withdrawResponse.Data.Invoice.Request);

                    output.WriteLine("in action lnurl:" + withdrawResponse.Data.Invoice.Request);
                    output.WriteLine("in action id:" + withdrawResponse.Data.Id);
                    output.WriteLine("in action amount:" + withdrawResponse.Data.Amount);
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
                Assert.True(false, "withdraw call timeout ");
            }
        }

        [Fact]
        public async void SubscribeToWithdrawalAndCompleteTest()
        {
            string testDesc = "CSHARP IT TEST for subscribe to Withdrawal";
            int testAmount = 10000;//default 10 sats

            //service setup
            ZbdLnService zbdLnService = new ZbdLnService(zebedeeUrl, apikey);

            ///////////////////////////// Create Invoice
            WithdrawRequest request = new WithdrawRequest();

            request.Description = testDesc;
            request.Amount = testAmount;
            request.InternalId = "IntegTest-Withdrawal-Complete " + DateTime.Now.ToString();

            //Countdown Latch
            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            //Call the API and assert within the callback
            string withdrawId = null;
            string lnurl = null;
            Task task = zbdLnService.WithdrawAsync(request, withdrawResponse =>
            {
                try
                {
                    Assert.NotNull(withdrawResponse.Data.Invoice.Request);
                    Assert.NotNull(withdrawResponse.Data.Id);
                    Assert.Equal(testAmount, withdrawResponse.Data.Amount);
                    Assert.StartsWith("lnurl", withdrawResponse.Data.Invoice.Request);

                    output.WriteLine("in action lnurl:" + withdrawResponse.Data.Invoice.Request);
                    output.WriteLine("in action id:" + withdrawResponse.Data.Id);
                    output.WriteLine("in action amount:" + withdrawResponse.Data.Amount);
                    withdrawId = withdrawResponse.Data.Id;
                    lnurl = withdrawResponse.Data.Invoice.Request;
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
                Assert.True(false, "withdraw call timeout ");
            }

            output.WriteLine("outside lnurl:" + lnurl);

            cde.Reset();


            /////////////////////////////// SUBSCCRIBE to Withdrawal
            Task<WithdrawResponse> subscribeWithdrawTask = zbdLnService.SubscribeWithdrawAsync(withdrawId);
            ///////////////////////////////  1. PUT BREAK POINT on ABOVE LINE and find the value of variable lnurl
            //////////////////////////////// 2. Go to the QR COde sie and scan by ZEBEDEE wallet (or any LNURL supporting wallet)
            //////////////////////////////// https://www.the-qrcode-generator.com/

            //SUBSCRIPTION  ASSERT
            WithdrawResponse withdrawResult = await subscribeWithdrawTask;

            output.WriteLine("Status:" + withdrawResult.Data.Status);
            Assert.Equal("completed", withdrawResult.Data.Status);



        }

    }
}
