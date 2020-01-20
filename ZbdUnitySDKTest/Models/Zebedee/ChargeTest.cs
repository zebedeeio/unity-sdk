using Newtonsoft.Json;
using System;
using Xunit;
using ZbdUnitySDK.Models.Zebedee;
using ZbdUnitySDK.Utils;

namespace ZbdUnitySDKTest
{
    public class ChargeTest
    {
        [Fact]
        public void ChargeSerialize_Test()
        {
            /*
            {
                "name": "Purchased game item #MASA001",
                "description": "Description of the Charge by Masa",
                "amount": "1000",
                "callbackUrl": "http://localhost/callback",
                "internalId": "MASA001"
            } 
            */

            //test text
            string testMessage = "TEST MESSAGE";
            string testPaymentName = "Purchased game item #MASA001";

            ChargeDetail charge = new ChargeDetail();
            charge.Message = testMessage ;
            ChargeData data = new ChargeData();
            data.Name = testPaymentName;
            charge.Data = data;
            //Serialize
            string output = JsonConvert.SerializeObject(charge);
            Assert.Contains(testMessage, output);

            //Deserialize
            ChargeDetail deserializedCharge = JsonConvert.DeserializeObject<ChargeDetail>(output);
            Assert.NotNull(deserializedCharge.Data);
            Assert.Equal(deserializedCharge.Data.Name,testPaymentName);

        }
        [Fact]
        public void ChargeDeserialize_Test()
        {
            //test text
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


            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;

            //Deserialize
            ChargeDetail deserializedCharge = JsonConvert.DeserializeObject<ChargeDetail>(testJson,jsonSettings);
            //1st level
            Assert.Equal("Successfully retrieved Charge.", deserializedCharge.Message);
            Assert.NotNull(deserializedCharge.Data);
            ChargeData data = deserializedCharge.Data;
            //2nd level
            Assert.Equal( "773cc63c-8e4a-437d-87f1-c89049e4d076", data.Id);
            Assert.Equal("Purchased game item #MASA001", data.Name );
            Assert.Equal("Description of the Charge by Masa", data.Description);
//            Assert.Equal(DateTime.Parse("2019-12-28T20:45:39.575Z"), data.CreatedAt);
            Assert.Equal("http://localhost/callback", data.CallbackUrl);
            Assert.Equal("MASA001", data.InternalId);
            Assert.Equal(1000,data.Amount);
            Assert.Equal("expired", data.Status);

            Assert.NotNull(data.Invoice);
            Invoice invoice = data.Invoice;
            //3rd level
            Assert.Equal( DateTime.Parse("2019-12-28T20:55:39.594Z"), invoice.ExpiresAt,TimeSpan.FromSeconds(1));
            Assert.Equal("lnbc10n1p0q00hnpp5ce8yksx5455eh7rtmmx7f6jm0gs4wy3n3794mywm78sm06kwr7tqdp4g3jhxcmjd9c8g6t0dcsx7e3qw35x2gzrdpshyem9yp38jgzdv9ekzcqzpgxqzjcfppj7j3tfup9ph0g0g6hkf5ykh0scw87ffnz9qy9qsqsp5zqeu32n0khtpejtcep2wkavwqxvnvp09wh8fx5k28cdfs4hv8mqs7x426zqdg0wmhy2ta6hz4kdej7hh9rx2alkn7qsdfn3vgq7g2x4n09amt0d6duxpk9znurlwxrz676zyrceghla7yux0p7vpn6ymekcpn5ypxj", invoice.Request);

        }

    }
}
