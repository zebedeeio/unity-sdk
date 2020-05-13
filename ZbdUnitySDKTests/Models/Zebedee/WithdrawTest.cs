using Newtonsoft.Json;
using System;
using Xunit;
using ZbdUnitySDK.Models.Zebedee;
using ZbdUnitySDK.Utils;
using SimpleJSON;
namespace ZbdUnitySDKTest
{
    public class WithdrawTest
    {
        [Fact]
        public void WithdrawSerialize_Test()
        {
            //test text
            string testMessage = "TEST MESSAGE";
            string testPaymentName = "Purchased game item #MASA001";

            WithdrawResponse withdraw = new WithdrawResponse();
            withdraw.Message = testMessage ;
            WithdrawData data = new WithdrawData();
            data.Name = testPaymentName;
            withdraw.Data = data;
            //Serialize
            string output = JsonConvert.SerializeObject(withdraw);
            Assert.Contains(testMessage, output);

            //Deserialize
            WithdrawResponse deserializedCharge = JsonConvert.DeserializeObject<WithdrawResponse>(output);
            Assert.NotNull(deserializedCharge.Data);
            Assert.Equal(deserializedCharge.Data.Name,testPaymentName);

        }
        [Fact]
        public void WithdrawDeserialize_Test()
        {
           
            string testJson = @"
            {
            ""message"":""Successfully created Withdrawal Request."",
            ""data"":{
                ""id"":""c32102ad-408a-4212-a91a-9811b3ff5a45"",
                ""unit"":""msats"",
                ""amount"":""12000"",
                ""createdAt"":""2020-05-11T15:42:09.207Z"",
                ""expiresAt"":""2020-05-11T15:47:09.207Z"",
                ""internalId"":""11af01d092444a317cb33faa6b8304b8"",
                ""description"":""My Withdrawal Description"",
                ""callbackUrl"":""https://your-website.com/callback"",
                ""status"":""pending"",
                ""invoice"":{
                       ""request"":""lnurl1dp68gurn8ghj7ctsdyh85etzv4jx2efwd9hj7a3s9acxz7tvdaskgtthd96xserjv9mkzmpdwfjhzat9wd6r7um9vdex2apav9nx2venxqenjdesx56xxvnr8yckyc3kxy6kycfhxycnvvfhve3kzc33xy6xxwf4vgcnqwrxvf3rxvfnxvuk2etrvfnrvwrx8q6xxeqvk9nqm"",
                       ""fastRequest"":""lnurl1dp68gurn8ghj7ctsdyh85etzv4jx2efwd9hj7a3s9acxz7tvdaskgtthd96xserjv9mkzmpdwfjhzat9wd6r7arpvu7hw6t5dpj8ycth2fjhzat9wd6zv6e384skvefnxvcrxwfhxq6ngcejvvunzcnzxccn2cnpxucnzd33xanxxctzxycngceex43rzvpcve3xyve3xvenjet9vd3xvd3cvcurgcmyyekkjmjhd96xserjv9mkzcnvv57nzv3sxqczvmtp0ptkjargv3exzampvfkx20f3xgcrqvpxv3jkvct4d36ygetnvdexjur5d9hku02d0ys9w6t5dpj8ycthv9kzq3r9wd3hy6tsw35k7m3xvdskcmrzv93kk0tgw368que69uhkzurf9eax2cn9v3jk2tnfduhhvvp0wpex7cm9wdej6amfw35xgunpwaskcttjv4ch2etnwshn9dv3"",
                       ""uri"":""lightning:lnurl1dp68gurn8ghj7ctsdyh85etzv4jx2efwd9hj7a3s9acxz7tvdaskgtthd96xserjv9mkzmpdwfjhzat9wd6r7um9vdex2apav9nx2venxqenjdesx56xxvnr8yckyc3kxy6kycfhxycnvvfhve3kzc33xy6xxwf4vgcnqwrxvf3rxvfnxvuk2etrvfnrvwrx8q6xxeqvk9nqm"",
                       ""fastUri"":""lightning:lnurl1dp68gurn8ghj7ctsdyh85etzv4jx2efwd9hj7a3s9acxz7tvdaskgtthd96xserjv9mkzmpdwfjhzat9wd6r7arpvu7hw6t5dpj8ycth2fjhzat9wd6zv6e384skvefnxvcrxwfhxq6ngcejvvunzcnzxccn2cnpxucnzd33xanxxctzxycngceex43rzvpcve3xyve3xvenjet9vd3xvd3cvcurgcmyyekkjmjhd96xserjv9mkzcnvv57nzv3sxqczvmtp0ptkjargv3exzampvfkx20f3xgcrqvpxv3jkvct4d36ygetnvdexjur5d9hku02d0ys9w6t5dpj8ycthv9kzq3r9wd3hy6tsw35k7m3xvdskcmrzv93kk0tgw368que69uhkzurf9eax2cn9v3jk2tnfduhhvvp0wpex7cm9wdej6amfw35xgunpwaskcttjv4ch2etnwshn9dv3""
                        }
                    }
            }";


            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;

            //Deserialize
            WithdrawResponse deserializedCharge = JsonConvert.DeserializeObject<WithdrawResponse>(testJson,jsonSettings);

            //1st level
            Assert.Equal("Successfully created Withdrawal Request.", deserializedCharge.Message);
            Assert.NotNull(deserializedCharge.Data);
            WithdrawData data = deserializedCharge.Data;
            //2nd level
            Assert.Equal("c32102ad-408a-4212-a91a-9811b3ff5a45", data.Id); 
            Assert.Equal("My Withdrawal Description", data.Description);
//            Assert.Equal(DateTime.Parse("2019-12-28T20:45:39.575Z"), data.CreatedAt);
            Assert.Equal("https://your-website.com/callback", data.CallbackUrl);
            Assert.Equal("11af01d092444a317cb33faa6b8304b8", data.InternalId);
            Assert.Equal(12000,data.Amount);
            Assert.Equal("pending", data.Status);

            Assert.NotNull(data.Invoice);
            WithdrawInvoice invoice = data.Invoice;
            //3rd level
            Assert.Equal( DateTime.Parse("2020-05-11T15:47:09.207Z"), data.ExpiresAt,TimeSpan.FromSeconds(1));
            Assert.Equal("lnurl1dp68gurn8ghj7ctsdyh85etzv4jx2efwd9hj7a3s9acxz7tvdaskgtthd96xserjv9mkzmpdwfjhzat9wd6r7um9vdex2apav9nx2venxqenjdesx56xxvnr8yckyc3kxy6kycfhxycnvvfhve3kzc33xy6xxwf4vgcnqwrxvf3rxvfnxvuk2etrvfnrvwrx8q6xxeqvk9nqm", invoice.Request);
            Assert.Equal("lnurl1dp68gurn8ghj7ctsdyh85etzv4jx2efwd9hj7a3s9acxz7tvdaskgtthd96xserjv9mkzmpdwfjhzat9wd6r7arpvu7hw6t5dpj8ycth2fjhzat9wd6zv6e384skvefnxvcrxwfhxq6ngcejvvunzcnzxccn2cnpxucnzd33xanxxctzxycngceex43rzvpcve3xyve3xvenjet9vd3xvd3cvcurgcmyyekkjmjhd96xserjv9mkzcnvv57nzv3sxqczvmtp0ptkjargv3exzampvfkx20f3xgcrqvpxv3jkvct4d36ygetnvdexjur5d9hku02d0ys9w6t5dpj8ycthv9kzq3r9wd3hy6tsw35k7m3xvdskcmrzv93kk0tgw368que69uhkzurf9eax2cn9v3jk2tnfduhhvvp0wpex7cm9wdej6amfw35xgunpwaskcttjv4ch2etnwshn9dv3", invoice.FastRequest);
            Assert.Equal("lightning:lnurl1dp68gurn8ghj7ctsdyh85etzv4jx2efwd9hj7a3s9acxz7tvdaskgtthd96xserjv9mkzmpdwfjhzat9wd6r7um9vdex2apav9nx2venxqenjdesx56xxvnr8yckyc3kxy6kycfhxycnvvfhve3kzc33xy6xxwf4vgcnqwrxvf3rxvfnxvuk2etrvfnrvwrx8q6xxeqvk9nqm", invoice.URI);
            Assert.Equal("lightning:lnurl1dp68gurn8ghj7ctsdyh85etzv4jx2efwd9hj7a3s9acxz7tvdaskgtthd96xserjv9mkzmpdwfjhzat9wd6r7arpvu7hw6t5dpj8ycth2fjhzat9wd6zv6e384skvefnxvcrxwfhxq6ngcejvvunzcnzxccn2cnpxucnzd33xanxxctzxycngceex43rzvpcve3xyve3xvenjet9vd3xvd3cvcurgcmyyekkjmjhd96xserjv9mkzcnvv57nzv3sxqczvmtp0ptkjargv3exzampvfkx20f3xgcrqvpxv3jkvct4d36ygetnvdexjur5d9hku02d0ys9w6t5dpj8ycthv9kzq3r9wd3hy6tsw35k7m3xvdskcmrzv93kk0tgw368que69uhkzurf9eax2cn9v3jk2tnfduhhvvp0wpex7cm9wdej6amfw35xgunpwaskcttjv4ch2etnwshn9dv3", invoice.FastURI);

        }

    }
}
