namespace ZbdUnitySDK.Models.Zebedee
{
    using Newtonsoft.Json;

    public class WithdrawRequest
    {
        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }

        [JsonProperty(PropertyName = "internalId")]
        public string InternalId { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }

    public class WithdrawResponse
    {
        public string URL { get; set; }

        public string Lnurl { get; set; }

    }

}
