namespace ZbdUnitySDK.Models.Zebedee
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Newtonsoft.Json;

    public class PaymentRequest
    {

        [JsonProperty(PropertyName = "invoice")]
        public string Invoice { get; set; }
        
        [JsonProperty(PropertyName = "internalId")]
        public string InternalId { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }


    }
    public class PaymentResponse
    {
        public string Message { get; set; }


        public PaymentData Data { get; set; }


    }


    public class PaymentData
    {
        public string Id { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; }
    }
}
