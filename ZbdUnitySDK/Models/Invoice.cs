using System;
using System.Collections.Generic;
using System.Text;

namespace ZbdUnitySDK.models
{

    public class Invoice
    {
        private long milliSatoshiAmount;
        private string description;
    }

    public class InvoiceRequest
    {
        private int milliSatoshiAmount;
        private string description;
        private int expiryMin;

        public int MilliSatoshiAmount { get => this.milliSatoshiAmount; set => milliSatoshiAmount = value; }
        public string Description { get => this.description; set => description = value; }
        public int ExpiryMin { get => this.expiryMin; set => expiryMin = value; }
    }

    public enum InvoiceStatus
    {
        OPEN,SETTLED,CANCELED,ACCEPTED
    }
}
