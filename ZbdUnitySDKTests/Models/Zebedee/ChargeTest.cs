using Newtonsoft.Json;
using System;
using Xunit;
using ZbdUnitySDK.Models.Zebedee;
using ZbdUnitySDK.Utils;
using SimpleJSON;
namespace ZbdUnitySDKTest
{
    public class ChargeTest
    {
        [Fact]
        public void ChargeSerialize_Test()
        {
            
            //test text
            string testMessage = "TEST MESSAGE";
            string testPaymentName = "Purchased game item #MASA001";

            ChargeResponse charge = new ChargeResponse();
            charge.Message = testMessage ;
            ChargeData data = new ChargeData();
            data.Name = testPaymentName;
            charge.Data = data;
            //Serialize
            string output = JsonConvert.SerializeObject(charge);
            Assert.Contains(testMessage, output);

            //Deserialize
            ChargeResponse deserializedCharge = JsonConvert.DeserializeObject<ChargeResponse>(output);
            Assert.NotNull(deserializedCharge.Data);
            Assert.Equal(deserializedCharge.Data.Name,testPaymentName);

        }
        [Fact]
        public void ChargeDeserialize_Test()
        {
            //test text
            string testJson = @"
            {
            ""message"":""Successfully created Charge."",
            ""data"":{
                 ""id"":""3d73bf24-4d60-4be2-92c1-9dce7be3395c"",
                 ""name"":""My Custom Charge Name"",
                 ""unit"":""msats"",
                 ""amount"":""10000"",
                 ""createdAt"":""2020-05-02 00:49:59.306918+00"",
                 ""internalId"":""11af01d092444a317cb33faa6b8304b8"",
                 ""callbackUrl"":""https://your-website.com/callback"",
                 ""description"":""My Custom Charge Description"",
                 ""expiresAt"":""2020-05-07T14:32:53.216Z"",
                 ""confirmedAt"":null,
                 ""status"":""pending"",
                 ""invoice"":{
                        ""request"":""lnbc100n1p0tgxhfpp5q7yjh9ur3ztcavegsdrt2fffzwpcesj57n75cx8jl0fm7qwthnsqdpdf4ujqsm4wd6x7mfqgd5xzun8v5sygetnvdexjur5d9hkucqzpgxqzfvsp5asphsyh3r36nxzztx99ka3um4xefpaewk4pawzeqj2z99vwpsews9qy9qsqn9a7guwe6fwl2dknhjxsr233h9aucag3ulhh8uly7ln64l3vlh6kjsgwgautk55snet69nrzh7dsp4sfhpe7fc5tk73pdjnhfsmyclspta6z0p"",
                        ""uri"":""lightning:lnbc100n1p0tgxhfpp5q7yjh9ur3ztcavegsdrt2fffzwpcesj57n75cx8jl0fm7qwthnsqdpdf4ujqsm4wd6x7mfqgd5xzun8v5sygetnvdexjur5d9hkucqzpgxqzfvsp5asphsyh3r36nxzztx99ka3um4xefpaewk4pawzeqj2z99vwpsews9qy9qsqn9a7guwe6fwl2dknhjxsr233h9aucag3ulhh8uly7ln64l3vlh6kjsgwgautk55snet69nrzh7dsp4sfhpe7fc5tk73pdjnhfsmyclspta6z0p""
                            }
                     }
            }";


            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;

            //Deserialize
            ChargeResponse deserializedCharge = JsonConvert.DeserializeObject<ChargeResponse>(testJson,jsonSettings);

            //1st level
            Assert.Equal("Successfully created Charge.", deserializedCharge.Message);
            Assert.NotNull(deserializedCharge.Data);
            ChargeData data = deserializedCharge.Data;
            //2nd level
            Assert.Equal("3d73bf24-4d60-4be2-92c1-9dce7be3395c", data.Id);
            Assert.Equal("My Custom Charge Name", data.Name );
            Assert.Equal("My Custom Charge Description", data.Description); 
            Assert.Equal("https://your-website.com/callback", data.CallbackUrl);
            Assert.Equal("11af01d092444a317cb33faa6b8304b8", data.InternalId);
            Assert.Equal(10000,data.Amount);
            Assert.Equal("pending", data.Status);

            Assert.NotNull(data.Invoice);
            Invoice invoice = data.Invoice;
            //3rd level
            Assert.Equal( DateTime.Parse("2020-05-07T14:32:53.216Z"), data.ExpiresAt,TimeSpan.FromSeconds(1));
            Assert.Equal("lnbc100n1p0tgxhfpp5q7yjh9ur3ztcavegsdrt2fffzwpcesj57n75cx8jl0fm7qwthnsqdpdf4ujqsm4wd6x7mfqgd5xzun8v5sygetnvdexjur5d9hkucqzpgxqzfvsp5asphsyh3r36nxzztx99ka3um4xefpaewk4pawzeqj2z99vwpsews9qy9qsqn9a7guwe6fwl2dknhjxsr233h9aucag3ulhh8uly7ln64l3vlh6kjsgwgautk55snet69nrzh7dsp4sfhpe7fc5tk73pdjnhfsmyclspta6z0p", invoice.Request);

        }

    }
}
