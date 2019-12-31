using Newtonsoft.Json;
using System;

namespace ZbdUnitySDK.Models.Zebedee
{
/*
    {
    "message": "Successfully retrieved Charge.",
    "data": {
        "id": "773cc63c-8e4a-437d-87f1-c89049e4d076",
        "name": "Purchased game item #MASA001",
        "description": "Description of the Charge by Masa",
        "createdAt": "2019-12-28T20:45:39.575Z",
        "successUrl": "http://localhost/success",
        "callbackUrl": "http://localhost/callback",
        "internalId": "MASA001",
        "amount": "1000",
        "status": "expired",
        "invoice": {
            "expiresAt": "2019-12-28T20:55:39.594Z",
            "request": "lnbc10n1p0q00hnpp5ce8yksx5455eh7rtmmx7f6jm0gs4wy3n3794mywm78sm06kwr7tqdp4g3jhxcmjd9c8g6t0dcsx7e3qw35x2gzrdpshyem9yp38jgzdv9ekzcqzpgxqzjcfppj7j3tfup9ph0g0g6hkf5ykh0scw87ffnz9qy9qsqsp5zqeu32n0khtpejtcep2wkavwqxvnvp09wh8fx5k28cdfs4hv8mqs7x426zqdg0wmhy2ta6hz4kdej7hh9rx2alkn7qsdfn3vgq7g2x4n09amt0d6duxpk9znurlwxrz676zyrceghla7yux0p7vpn6ymekcpn5ypxj"
        }
    }

*/


    public class Charge
    {

        public string Message { get; set; }

        public ChargeData Data { get; set; }
    }

   public  class ChargeData
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        //public DateTime CreatedAt { get; set; }

        public string CallbackUrl { get; set; }
        public string InternalId { get; set; }
        [JsonProperty(PropertyName = "amount")]
        public long Amount { get; set; }
        public string Status { get; set; }

        public Invoice Invoice { get; set; }
    }

    public class Invoice
    {
        public DateTime ExpiresAt { get; set; }
        public string Request { get; set; }
    }
}
