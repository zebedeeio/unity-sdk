using System;
using System.Collections.Generic;
using System.Text;

namespace ZbdUnitySDK.models
{
    public class Invoice
    {
        String currency;
        String milliSatoshiAmount;
        String memo;
        InvoiceStatus status;

    }

    public enum InvoiceStatus
    {
        OPEN,SETTLED,CANCELED,ACCEPTED
    }
}
