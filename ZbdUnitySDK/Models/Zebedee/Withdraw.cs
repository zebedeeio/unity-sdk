namespace ZbdUnitySDK.Models.Zebedee
{
    using Newtonsoft.Json;

    public class WithdrawRequest
    {
        [JsonProperty(PropertyName = "amount")]
        public long Amount { get; set; }

        [JsonProperty(PropertyName = "internalId")]
        public string InternalId { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }

    public class WithdrawResponse
    {
        public string Message { get; set; }

        public WithdrawData Data { get; set; }
    }
    public class WithdrawData
    {
        public string Id { get; set; }
        public int Fee { get; set; }

        public string Lnurl { get; set; }

        public long Amount { get; set; }

        public string Status { get; set; }
        public string InternalId { get; set; }
        public string Description { get; set; }
    }

}
