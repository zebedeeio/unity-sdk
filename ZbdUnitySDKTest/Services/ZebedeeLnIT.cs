using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ZbdUnitySDK;
using ZbdUnitySDK.Exception;
using ZbdUnitySDK.Models;
using ZbdUnitySDK.Models.Zebedee;

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
        public async void ChargeCreationTest()
        {
            string testDesc = "CSHARP IT TEST for Charge ";

            //client setup
            ZebedeeClient zebeedeeClient = new ZebedeeClient(zebedeeUrl, apikey);

            ///////////////////////////// Create Invoice
            Charge chargeData = new Charge();
            chargeData.Name = testDesc;
            chargeData.Description = testDesc;
            chargeData.AmountInSatoshi = 1;

            //Countdown Latch
            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            String bolt = "";
            //Call the API and assert within the callback
            Task task = zebeedeeClient.CreateChargeAsync(chargeData, charge =>
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
        public async void ChargeAndPaymentTest()
        {
            string testDesc = "CSHARP IT TEST for Charge ";

            //client setup
            ZebedeeClient zebeedeeClient = new ZebedeeClient(zebedeeUrl, apikey);  //paying 
            ZebedeeClient zebeedeeClient2 = new ZebedeeClient(zebedeeUrl, apikey2); //Generate Invoice and get paid

            ///////////////////////////// Create Invoice
            Charge chargeData = new Charge();
            chargeData.Name = testDesc;
            chargeData.Description = testDesc;
            chargeData.AmountInSatoshi = 1;

            //Countdown Latch
            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            String bolt = "";
            //Call the API and assert within the callback
            Task task = zebeedeeClient2.CreateChargeAsync(chargeData, charge =>
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

            await zebeedeeClient.PayInvoiceAsync(bolt);
            Assert.True(true, "payment call succeeded ");//if exception happens, test fails
        }

        /// <summary>
        ///  1. dev 1 generate invoice
        ///  2. dev 1 subscribe to the invoice
        ///  3. dev 2 pay to it
        /// </summary>
        [Fact]
        public async void ChargeCreationAndSubscribeToSettlement()
        {
            string testDesc = "CSHARP IT TEST DES for Charge/Settlement ";

            //client setup
            ZebedeeClient zebeedeeClient = new ZebedeeClient(zebedeeUrl, apikey);  //paying 
            ZebedeeClient zebeedeeClient2 = new ZebedeeClient(zebedeeUrl, apikey2); //Generate Invoice and get paid
            ///////////////////////////// Create Invoice
            Charge chargeData = new Charge();
            chargeData.Name = testDesc;
            chargeData.Description = testDesc;
            chargeData.AmountInSatoshi = 1;

            //Countdown Latch
            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            String bolt = "";
            String chargeId = "";

            //Call the API and assert within the callback
            Task task = zebeedeeClient.CreateChargeAsync(chargeData, charge =>
            {
                try
                {
                    Assert.NotNull(charge.Data);
                    Assert.NotNull(charge.Data.Invoice);
                    Assert.NotNull(charge.Data.Invoice.Request);
                    Assert.Equal(chargeData.AmountInSatoshi*1000,charge.Data.Amount);
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
            Task<string> subscribeChargeTask = zebeedeeClient.SubscribeChargeAsync(chargeId);


            ///////////////////////////// PAYMENT to BOLT Invoice
            PaymentRequest paymentRequest = new PaymentRequest();
            paymentRequest.Invoice = bolt;
            paymentRequest.Description = "CSHARP TEST SELF INVOICE PAUYMENT " + DateTime.Now;
            paymentRequest.InternalId = "PAYMENT-" + DateTime.Now.ToShortTimeString();

            await zebeedeeClient2.PayInvoiceAsync(bolt);


            //SUBSCRIPTION  ASSERT
            string status = await subscribeChargeTask;

            output.WriteLine("Status:" + status);
            Assert.Equal("completed",status);



        }

        /// <summary>
        /// Charge Invoice and subscribe but do not pay.
        /// Expects timeout
        /// </summary>
        [Fact]
        public async void ChargeCreationAndTimeout()
        {
            string testDesc = "CSHARP IT TEST DES for Charge/Timeout ";

            //client setup
            ZebedeeClient zebeedeeClient = new ZebedeeClient(zebedeeUrl, apikey);  //paying 

            ///////////////////////////// Create Invoice
            Charge chargeData = new Charge();
            chargeData.Name = testDesc;
            chargeData.Description = testDesc;
            chargeData.AmountInSatoshi = 1;

            //Countdown Latch
            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            String bolt = "";
            String chargeId = "";

            //Call the API and assert within the callback
            Task task = zebeedeeClient.CreateChargeAsync(chargeData, charge =>
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

            ///////////////////////////// SUBSCCRIBE to BOLT Invoice , timeout in 10 secs
            Task<string> subscribeChargeTask = zebeedeeClient.SubscribeChargeAsync(chargeId,10);

            //SUBSCRIPTION  ASSERT

            try
            {
                string status = await subscribeChargeTask;
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
            //client setup
            ZebedeeClient zebeedeeClient = new ZebedeeClient(zebedeeUrl, apikey);

            ///////////////////////////// Create Invoice
            Withdraw request = new Withdraw();

            request.Description = "CSHARP IT TEST for withdrawal";
            request.AmountInSatoshi = 10;//Default 10 satoshi
            request.InternalId = "IntegTest-Withdrawal" + DateTime.Now.ToString();

            //Countdown Latch
            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            //Call the API and assert within the callback
            

            Task task = zebeedeeClient.WithDrawAsync(request, withdrawResponse =>
            {
                try
                {
                    Assert.NotNull(withdrawResponse.Data.Invoice.Request);
                    Assert.NotNull(withdrawResponse.Data.Id);
                    Assert.Equal(request.AmountInSatoshi * 1000, withdrawResponse.Data.Amount);
                    Assert.StartsWith("lnurl", withdrawResponse.Data.Invoice.Request);

                    output.WriteLine("in action lnurl:" + withdrawResponse.Data.Invoice.Request);
                    output.WriteLine("in action id:" + withdrawResponse.Data.Id);
                    output.WriteLine("in action amount:" + withdrawResponse.Data.Amount);
                }
                catch(Exception e)
                {
                    output.WriteLine(e.StackTrace);
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

        /// <summary>
        /// Be carefull this tests require a manual intervention by wallet to withdraw
        /// </summary>
        [Fact]
        public async void SubscribeToWithdrawalAndCompleteTest()
        {
            //client setup
            ZebedeeClient zebeedeeClient = new ZebedeeClient(zebedeeUrl, apikey);

            ///////////////////////////// Create Invoice
            Withdraw request = new Withdraw();

            string testDesc = "CSHARP IT TEST for subscribe to Withdrawal";
            request.AmountInSatoshi = 10;//Default 10 satoshi
            request.InternalId = "IntegTest-Withdrawal-Complete " + DateTime.Now.ToString();


            //Countdown Latch
            CountdownEvent cde = new CountdownEvent(1); // initial count = 1
            //Call the API and assert within the callback
            string withdrawId = null;
            string lnurl = null;
            Task task = zebeedeeClient.WithDrawAsync(request, withdrawResponse =>
            {
                try
                {
                    Assert.NotNull(withdrawResponse.Data.Invoice.Request);
                    Assert.NotNull(withdrawResponse.Data.Id);
                    Assert.Equal(request.AmountInSatoshi * 1000, withdrawResponse.Data.Amount);
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
            Task<string> subscribeWithdrawTask = zebeedeeClient.SubscribeWithDrawAsync(withdrawId);
            ///////////////////////////////  1. PUT BREAK POINT on ABOVE LINE and find the string value of variable lnurl
            //////////////////////////////// 2. Go to the QR COde site 
            //////////////////////////////// https://www.the-qrcode-generator.com/
            //////////////////////////////// 3. release the breakpoint to start subscribe withdraw
            //////////////////////////////// 4. Scan QR by ZEBEDEE wallet (or any LNURL supporting wallet) to withdraw

            //SUBSCRIPTION  ASSERT
            string status = await subscribeWithdrawTask;

            output.WriteLine("Status:" + status);
            Assert.Equal("completed", status);



        }

    }
}
