using Newtonsoft.Json;
using System;

namespace ZbdUnitySDK.Models.Zebedee
{

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

    public class ChargeDataJSON
    {
        public string name;
        public string description;
        public long amount;
        public string internalId;
        public long expiresIn;
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

        public int ExpiresIn { get; set; }

        public string Status { get; set; }

        public DateTime ExpiresAt { get; set; }

        public Invoice Invoice { get; set; }

        public override string ToString()
        {
            return "ChargeData: " + Name + " " + Description + " " + Amount + " " + Status;
        }



        public ChargeDataJSON toJSONFriendly()
        {
            ChargeDataJSON chargeDataJSON = new ChargeDataJSON();
            chargeDataJSON.name = this.Name;
            chargeDataJSON.description = this.Description;
            chargeDataJSON.amount = this.Amount;
            chargeDataJSON.internalId = this.InternalId;
            chargeDataJSON.expiresIn = this.ExpiresIn;
            return chargeDataJSON;
        }

    }

    public class Invoice
    {
        public string Request { get; set; }
        public string URI { get; set; }
    }

 
}
