﻿namespace ZbdUnitySDK.Models.BTCPay
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides BitPay token policy information.
    /// </summary>
    public class Policy
    {
        public Policy() { }

        [JsonProperty(PropertyName = "policy")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "params")]
        public List<String> Params { get; set; }
    }
}
