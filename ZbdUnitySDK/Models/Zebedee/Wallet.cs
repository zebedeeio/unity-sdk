using Newtonsoft.Json;
using System;

namespace ZbdUnitySDK.Models.Zebedee
{

    /// <summary>
    ///  ZEBEDEE WAllet API Reponse
    /// </summary> 

    public class WalletResponse
    {

        public string Message { get; set; }

        public WalletData Data { get; set; }

        public override string ToString()
        {
            return "WalletResponse: " + Message + " " + Data.ToString();
        }
    }


    public class WalletData
    { 
        [JsonProperty(PropertyName = "unit")]
        public string Unit { get; set; }
          
        public long Balance { get; set; }

        public override string ToString()
        {
            return "WalletData: " + Balance + " " + Unit;
        }
    

    }

 
}
 