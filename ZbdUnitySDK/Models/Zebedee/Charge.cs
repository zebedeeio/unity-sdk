using Newtonsoft.Json;
using System;

namespace ZbdUnitySDK.Models.Zebedee
{


    public class ChargeRequest
    {
        //in milli satoshi
        [JsonProperty(PropertyName = "amount")]
        public long Amount { get; set; }

        [JsonProperty(PropertyName = "internalId")]
        public string InternalId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }

    /// <summary>
    ///  ZEBEDEE Charge API Reponse
    /// </summary>
    public class ChargeResponse
    {

        public string Message { get; set; }

        public ChargeData Data { get; set; }

        public override string ToString()
        {
            return "ChargeDetail: " + Message + " " + Data.ToString();
        }
    }

    /// <summary>
    ///  ZEBEDEE Charge API Request 
    /// </summary>
    public class ChargeData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "unit")]
        public string Unit { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CallbackUrl { get; set; }
        public string InternalId { get; set; }
        [JsonProperty(PropertyName = "amount")]
        public long Amount { get; set; }
        public string Status { get; set; }

        public Invoice Invoice { get; set; }

        public override string ToString()
        {
            return "ChargeData: " + Name + " " + Description + " " + Amount + " "+ Status;
        }

    }

    public class Invoice
    {
        public DateTime ExpiresAt { get; set; }
        public string Request { get; set; }
    }
}
