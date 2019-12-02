using System;
using System.Collections.Generic;
using System.Text;
using ZbdUnitySDK.models;

namespace ZbdUnitySDK.Services
{
    public interface ILnService
    {
        Invoice createInvoice();
        Invoice subscribePayment();

    }
}
