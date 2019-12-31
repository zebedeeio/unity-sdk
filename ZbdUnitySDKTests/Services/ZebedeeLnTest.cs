using System;
using System.Threading;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.Server;
using Xunit;
using Xunit.Abstractions;
using ZbdUnitySDK.Models.Zebedee;
using ZbdUnitySDK.Services;

namespace ZbdUnitySDKTest
{
    public class ZebedeeLnTest
    {

        private readonly ITestOutputHelper output;
        private readonly string zebedeeStubHost = "http://localhost";
        private readonly string apikey = "DUMMY-APIKEY";
        public ZebedeeLnTest(ITestOutputHelper testOutputHelper)
        {
            this.output = testOutputHelper;
        }

        [Fact]
        public    void SimpleInvoiceCreationTest()
        {
            var server = FluentMockServer.Start();
            ///////////////////////////// Create Invoice
            string testJson = @"
        {
            ""message"": ""Successfully retrieved Charge."",
            ""data"": {
                    ""id"": ""773cc63c-8e4a-437d-87f1-c89049e4d076"",
                    ""name"": ""Purchased game item #MASA001"",
                    ""description"": ""Description of the Charge by Masa"",
                    ""createdAt"": ""2019-12-28T20:45:39.575Z"",
                    ""successUrl"": ""http://localhost/success"",
                    ""callbackUrl"": ""http://localhost/callback"",
                    ""internalId"": ""MASA001"",
                    ""amount"": ""1000"",
                    ""status"": ""expired"",
                    ""invoice"": {
                        ""expiresAt"": ""2019-12-28T20:55:39.594Z"",
                        ""request"": ""lnbc10n1p0q00hnpp5ce8yksx5455eh7rtmmx7f6jm0gs4wy3n3794mywm78sm06kwr7tqdp4g3jhxcmjd9c8g6t0dcsx7e3qw35x2gzrdpshyem9yp38jgzdv9ekzcqzpgxqzjcfppj7j3tfup9ph0g0g6hkf5ykh0scw87ffnz9qy9qsqsp5zqeu32n0khtpejtcep2wkavwqxvnvp09wh8fx5k28cdfs4hv8mqs7x426zqdg0wmhy2ta6hz4kdej7hh9rx2alkn7qsdfn3vgq7g2x4n09amt0d6duxpk9znurlwxrz676zyrceghla7yux0p7vpn6ymekcpn5ypxj""
                    }
            }
        }";

            server
              .Given(
                Request.Create().WithPath("/v0/charges").UsingPost()
              )
              .RespondWith(
                WireMock.ResponseBuilders.Response.Create()
                  .WithStatusCode(200)
                  .WithHeader("Content-Type", "application/json")
                  .WithBody(testJson)
                  .WithDelay(TimeSpan.FromSeconds(1))
              ) ; 

            //service setup
            string zebedeeUrl = zebedeeStubHost + ":" + server.Ports[0] + "/v0/";
            ZbdLnService zbdLnService = new ZbdLnService( zebedeeUrl, apikey );

            ///////////////////////////// Create Invoice
            ChargeData chargeData = new ChargeData();

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
                    Assert.Equal("Description of the Charge by Masa", charge.Data.Description);
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

        }
    }
}
