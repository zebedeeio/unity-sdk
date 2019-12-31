namespace ZbdUnitySDK
{
    using System;
    using UnityEngine;
    using ZbdUnitySDK.models;

    public enum BackendType
    {
        BTCPAY,LND,ZEDEBEE,
    }

    public interface IZebedeeClient
    {
        //Authenticate by specified backend
        void initalize();

        //TODO make it async
        Invoice createInvoice();

        //TODO make it async
        Invoice SubscribePay();

        //TODO make it async
        Texture2D generateQR(String boltPayReq);

    }
}
