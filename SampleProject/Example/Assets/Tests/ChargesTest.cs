using System.Collections; 
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using ZbdUnitySDK; 
using ZbdUnitySDK.Models.Zebedee;
using ZbdUnitySDK.Models;
using ZbdUnitySDK.Exception;
using System; 

namespace Tests
{

    public class ChargesTest
    {

        ZebedeeClient zbdClient; 

        private string apiKeyAlice = "PkCMR7CulVkBqDA52MaKOA2cef60iK3v";
        private string apiKeyBob = "xmva6qLAQkC2yZd8neOT1qYZj9O1zLei";

        private string zebedeeBaseUrl = "https://beta-api.zebedee.io/v0/";
        string currentChargeId = "";
        int expiresIn = 120;
        int invoiceAmountMsats = 100000;
        int expiresInThreshold = 5; //we test the expiresAt time by comparing the time now to the expires at time and see if its roughly the expires time we set give or take the threshold value here caused by api response delay
        Action<ChargeResponse> currentCallback;
         

        public async void CreateChargeAsync(Charge charge,Action<ChargeResponse> callback)
        {
              

            currentCallback = callback;

            zbdClient = new ZebedeeClient(zebedeeBaseUrl, apiKeyAlice);
            // Use the Assert class to test conditions
            Debug.Log(zbdClient);
            try
            {
                await zbdClient.CreateChargeAsync(charge, HandleChargeAsync);
            }
            catch(ZedebeeException ze)
            {
                Debug.LogError(ze.GetMessage());
                Debug.LogError(ze.Data.ToString());
            }


        }

        public void HandleChargeAsync(ChargeResponse response)
        {

            currentCallback(response);

        }


        public async void TestPayment(string invoice, Action<string> callback)
        {

             

            ZebedeeClient zbdClient2 = new ZebedeeClient(zebedeeBaseUrl, apiKeyBob);



           await zbdClient2.PayInvoiceAsync(invoice);

            string status = await zbdClient2.SubscribeChargeAsync(currentChargeId);

            
                callback(status);
             

        }

        public void HandlePaymentAsync(PaymentResponse response)
        {

            Debug.Log(response.Data.Status);
           // currentCallback(response);

        }





        public void TestChargeCoroutine(Charge charge, Action<ChargeResponse> callback)
        {

            currentCallback = callback;


            GameObject ga = new GameObject();
            ZebedeeClient zbdClient = ga.AddComponent<ZebedeeClient>();
            zbdClient.SetUp(zebedeeBaseUrl, apiKeyAlice);


            zbdClient.CreateCharge(charge, (chargeResponse) => {

                HandleChargeCoroutine(chargeResponse);

            });


        }

        public void HandleChargeCoroutine(ChargeResponse response)
        {

            currentCallback(response);

        }


        [UnityTest]
        public IEnumerator CreateChargeAsync()
        {

            DateTime creationTime = DateTime.Now.ToUniversalTime();
            Charge charge = new Charge();
            charge.Description = "My Description";
            charge.AmountInSatoshi = invoiceAmountMsats/1000;
            charge.ExpiresInSec = expiresIn;
            charge.InternalId = "My Internal ID";

            ChargeResponse cr = null;

            CreateChargeAsync(charge, (chargeResponse) => {
                Debug.Log(chargeResponse);


                cr = chargeResponse;

            });

            while (cr == null)
            {
                yield return null;
            }
         
             
            Assert.AreEqual(invoiceAmountMsats, cr.Data.Amount);

            Assert.AreEqual("My Description", cr.Data.Description); 


            Assert.AreEqual("My Internal ID", cr.Data.InternalId);
             

            Assert.AreEqual("pending", cr.Data.Status);

            double expireTimeDiff = (cr.Data.ExpiresAt.ToUniversalTime() - creationTime).TotalSeconds;

            double timeDiff = expireTimeDiff - expiresIn;

           
            Assert.Less(timeDiff, expiresInThreshold);

            Assert.Greater(timeDiff, 0);


            currentChargeId = cr.Data.Id;

            string status = cr.Data.Status;

            TestPayment(cr.Data.Invoice.Request, (paymentResponse) => {

                status = paymentResponse;

            });

            while (status != "completed")
            {
                yield return null;
            }


            Assert.AreEqual("completed", status);

            yield return null;
        }

        
        [UnityTest]
        public IEnumerator CreateChargeCoroutine()
        {
            DateTime creationTime = DateTime.Now.ToUniversalTime();

            Charge charge = new Charge();
            charge.Description = "My Description";
            charge.AmountInSatoshi = invoiceAmountMsats/1000;
            charge.ExpiresInSec = expiresIn;
            charge.InternalId = "My Internal ID";

            ChargeResponse cr = null;

            TestChargeCoroutine(charge, (chargeResponse) => {
                Debug.Log(chargeResponse);


                cr = chargeResponse;

            });

            while (cr == null)
            {
                yield return null;
            }



            Assert.AreEqual(invoiceAmountMsats, cr.Data.Amount);

            Assert.AreEqual("My Description", cr.Data.Description);


            Assert.AreEqual("My Internal ID", cr.Data.InternalId);


            Assert.AreEqual("pending", cr.Data.Status);

            Debug.Log(cr.Data.ExpiresAt.ToUniversalTime()+" "+ creationTime);
            double expireTimeDiff = (cr.Data.ExpiresAt.ToUniversalTime() - creationTime).TotalSeconds;

            double timeDiff = expireTimeDiff - expiresIn;


            Assert.Less(timeDiff, expiresInThreshold);

            Assert.Greater(timeDiff, 0);

            currentChargeId = cr.Data.Id;

            string status = cr.Data.Status;

            TestPayment(cr.Data.Invoice.Request, (paymentResponse) => {

                status = paymentResponse;

            });

            while (status != "completed")
            {
                yield return null;
            }


            Assert.AreEqual("completed", status);

            yield return null;

             
        }
        





    }
}
