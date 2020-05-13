namespace ZbdUnitySDK.Models
{
    public class Withdraw
    {
        public long AmountInSatoshi { get; set; }

        public string InternalId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int ExpiresInSec { get; set; } = 300;

    }
}