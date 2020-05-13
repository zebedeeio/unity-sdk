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

    public class WithdrawTest
    {


        private string apiKey = "Kylz6adljcNPzjsccetr2wN7Wt1aLzeW";
        private string zebedeeBaseUrl = "https://beta-api.zebedee.io/v0/";
        int expiresIn = 120;
        int expiresInThreshold = 5; //we test the expiresAt time by comparing the time now to the expires at time and see if its roughly the expires time we set give or take the threshold value here caused by api response delay

        Action<WithdrawResponse> currentCallback;

        public async void TestWithdrawAsync(Withdraw withdraw, Action<WithdrawResponse> callback)
        {


            currentCallback = callback;

            ZebedeeClient zbdClient = new ZebedeeClient(zebedeeBaseUrl, apiKey);
            // Use the Assert class to test conditions
            Debug.Log(zbdClient);
            try
            {
                await zbdClient.WithdrawAsync(withdraw, HandleWithdrawAsync);
            }
            catch (ZedebeeException ze)
            {
                Debug.LogError(ze.GetMessage());
                Debug.LogError(ze.Data.ToString());
            }


        }

        public void HandleWithdrawAsync(WithdrawResponse response)
        {

            currentCallback(response);

        }



        public void TestWithdrawCoroutine(Withdraw withdraw, Action<WithdrawResponse> callback)
        {

            currentCallback = callback;


            GameObject ga = new GameObject();
            ZebedeeClient zbdClient = ga.AddComponent<ZebedeeClient>();
            zbdClient.SetUp(zebedeeBaseUrl, apiKey);


            zbdClient.Withdraw(withdraw, (withdrawResponse) => {

                HandleWithdrawCoroutine(withdrawResponse);

            });


        }

        public void HandleWithdrawCoroutine(WithdrawResponse response)
        {

            currentCallback(response);

        }


        [UnityTest]
        public IEnumerator CreateChargeAsync()
        {
            DateTime creationTime = DateTime.Now.ToUniversalTime();
            Withdraw withdraw = new Withdraw();
            withdraw.Description = "My Description";
            withdraw.AmountInSatoshi = 100;
            withdraw.ExpiresInSec = 120;
            withdraw.InternalId = "My Internal ID";

            WithdrawResponse wr = null;

            TestWithdrawAsync(withdraw, (withdrawResponse) => {
                Debug.Log(withdrawResponse);


                wr = withdrawResponse;

            });

            while (wr == null)
            {
                yield return null;
            }


            Assert.AreEqual(100000, wr.Data.Amount);
            Assert.AreEqual("My Description", wr.Data.Description);


            Assert.AreEqual("My Internal ID", wr.Data.InternalId);


            Assert.AreEqual("pending", wr.Data.Status);

            double expireTimeDiff = (wr.Data.ExpiresAt.ToUniversalTime() - creationTime).TotalSeconds;

            double timeDiff = expireTimeDiff - expiresIn;

            Assert.Less(timeDiff, expiresInThreshold);

            Assert.Greater(timeDiff, 0);
             

            yield return null;
        }


        [UnityTest]
        public IEnumerator WithdrawCoroutine()
        {

            DateTime creationTime = DateTime.Now.ToUniversalTime();
            Withdraw withdraw = new Withdraw();
            withdraw.Description = "My Description";
            withdraw.AmountInSatoshi = 100;
            withdraw.ExpiresInSec = expiresIn;
            withdraw.InternalId = "My Internal ID";

            WithdrawResponse wr = null;

            TestWithdrawCoroutine(withdraw, (withdrawResponse) => {
                Debug.Log(withdrawResponse);


                wr = withdrawResponse;

            });

            while (wr == null)
            {
                yield return null;
            }


            Assert.AreEqual(100000, wr.Data.Amount);
            Assert.AreEqual("My Description", wr.Data.Description);
             
            Assert.AreEqual("My Internal ID", wr.Data.InternalId);
             
            Assert.AreEqual("pending", wr.Data.Status);


            double expireTimeDiff = (wr.Data.ExpiresAt.ToUniversalTime() - creationTime).TotalSeconds;

            double timeDiff = expireTimeDiff - expiresIn;


            Assert.Less(timeDiff, expiresInThreshold);

            Assert.Greater(timeDiff, 0);


            yield return null;
        }






    }
}
