using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using ZbdUnitySDK;
using ZbdUnitySDK.Logging;
using ZbdUnitySDK.Models;
using ZbdUnitySDK.Models.Zebedee;


using System; 

namespace Tests
{

    public class WalletTest
    {

        int expectedWalletAmount = 10500000;
        private string apiKey = "Kylz6adljcNPzjsccetr2wN7Wt1aLzeW";
        private string zebedeeBaseUrl = "https://beta-api.zebedee.io/v0/";


        Action<WalletResponse> currentCallback;
         
        public async void TestWalletAsync(Action<WalletResponse> callback)
        {

            currentCallback = callback;

            ZebedeeClient zbdClient = new ZebedeeClient(zebedeeBaseUrl, apiKey);
            // Use the Assert class to test conditions
            Debug.Log(zbdClient);

            await zbdClient.GetWalletAsync(HandleWalletAsync);


        }

        public void HandleWalletAsync(WalletResponse response)
        {

            currentCallback(response);


        }



        public void TestWalletCoroutine(Action<WalletResponse> callback)
        {

            currentCallback = callback;


           GameObject ga = new GameObject(); 
            ZebedeeClient zbdClient = ga.AddComponent<ZebedeeClient>();
            zbdClient.SetUp(zebedeeBaseUrl, apiKey);
            // Use the Assert class to test conditions

            Debug.Log(zbdClient);
           

            Debug.Log(zbdClient);
            //Make a call to the wallet details endpoint
            zbdClient.GetWallet((walletResponse) => {
              
                HandleWalletCoroutine(walletResponse);
               
            });


        }

        public void HandleWalletCoroutine(WalletResponse response)
        {

            currentCallback(response);

        }


        [UnityTest]
        public IEnumerator GetWalletAsync()
        {

            WalletResponse wr = null;

            TestWalletAsync((walletResponse) => {
                Debug.Log(walletResponse);

               
                wr = walletResponse;

            });

            while (wr == null)
            {
                yield return null;
            }

            Assert.AreEqual(expectedWalletAmount, wr.Data.Balance);
            Assert.AreEqual("msats", wr.Data.Unit);

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetWalletCoroutine()
        {

            WalletResponse wr = null;

            TestWalletCoroutine((walletResponse) => {
                Debug.Log(walletResponse);


                wr = walletResponse;

            });

            while (wr == null)
            {
                yield return null;
            }

            Assert.AreEqual(expectedWalletAmount, wr.Data.Balance);
            Assert.AreEqual("msats", wr.Data.Unit);

            yield return null;
        }


 



    }
}
