namespace ZbdUnitySDK.Models.BTCPay
{
    using Newtonsoft.Json;

    public class InvoiceCryptoInfo
    {
        [JsonConstructor]
        public InvoiceCryptoInfo() { }
        [JsonProperty(PropertyName = "cryptoCode")]
        public string cryptoCode { get; set; }
        public string paymentType { get; set; }
        public double rate { get; set; }
        public double paid { get; set; }
        public double price { get; set; }
        public double due { get; set; }
        public double totalDue { get; set; }
        public double networkFee { get; set; }
        public double cryptoPaid { get; set; }
        public PaymentUrls paymentUrls { get; set; }
        public string address;
        public string url;
        public int txCount;
    }

    public class PaymentUrls
    {
        public PaymentUrls() { }
        public string BOLT11;
        public string BIP21;
        public string BIP72;
        public string BIP72b;
        public string BIP73;

    }
}
