namespace ZbdUnitySDK.Models.Zebedee
{
    using Newtonsoft.Json;
    using System;

    public class WithdrawRequestJSON
    {
        public string name;
        public string description;
        public long amount;
        public string internalId;
        public long expiresIn;
    }

    public class WithdrawRequest
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

        [JsonProperty(PropertyName = "expiresIn")]
        public int ExpiresIn { get; set; }

    }

    public class WithdrawResponse
    {
        public string Message { get; set; }

        public WithdrawData Data { get; set; }
    }
    public class WithdrawData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "unit")]
        public string Unit { get; set; }

        //in milli satoshi
        [JsonProperty(PropertyName = "amount")]
        public long Amount { get; set; }

        [JsonProperty(PropertyName = "internalId")]
        public string InternalId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
         
        public string CallbackUrl { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "expiresIn")]
        public int ExpiresIn { get; set; }

        public DateTime ExpiresAt { get; set; }

        public string Status { get; set; }

        public WithdrawInvoice Invoice { get; set; }

        public DateTime CreatedAt { get; set; }

        public WithdrawRequestJSON toJSONFriendly()
        {
            WithdrawRequestJSON withdrawRequestJSON = new WithdrawRequestJSON();
            withdrawRequestJSON.name = this.Name;
            withdrawRequestJSON.description = this.Description;
            withdrawRequestJSON.amount = this.Amount;
            withdrawRequestJSON.internalId = this.InternalId;
            withdrawRequestJSON.expiresIn = this.ExpiresIn;
            return withdrawRequestJSON;
        }


    }

    public class WithdrawInvoice
    {
        public string Request { get; set; }

        public string URI { get; set; }

        public string FastRequest { get; set; }

        public string FastURI { get; set; }

    }



}
