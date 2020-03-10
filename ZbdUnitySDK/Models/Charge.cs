namespace ZbdUnitySDK.Models
{
    public class Charge
    {
        //in milli satoshi
        public long AmountInSatoshi { get; set; }

        public string InternalId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ExpiresInSec { get; set; }

    }
}
