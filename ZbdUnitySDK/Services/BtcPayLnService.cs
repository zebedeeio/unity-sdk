// <copyright file="BtcPayLnService.cs" company="Zebedee Inc.">
// Copyright (c) Zebedee Inc.. All rights reserved.
// </copyright>

namespace ZbdUnitySDK.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using NBitcoin;
    using NBitcoin.Crypto;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UnityEngine;
    using UnityEngine.Networking;
    using ZbdUnitySDK.Exception;
    using ZbdUnitySDK.Models.BTCPay;
    using ZbdUnitySDK.Unity;
    using ZbdUnitySDK.UnityUtils;

    internal class BtcPayLnService
    {

        private const string BITPAY_API_VERSION = "2.0.0";
        private const string BITPAY_PLUGIN_INFO = "ZEBEDEE Unity SDK for BTCPay " + BITPAY_API_VERSION;
        private const string FACADE_MERCHANT = "merchant";

        private string baseUrl;
        private Dictionary<string, string> tokenCache; // {facade, token}
        private MonoBehaviour owner = null;

        private Key ecKey = null;
        private string identity = null;

        private String pairingCode;


        public BtcPayLnService(MonoBehaviour owner,string baseUrl, Key ecKey, string identity,string pairingCode)
        {
            this.owner = owner;
            this.baseUrl = "https://" + baseUrl + "/";
            this.identity = identity;
            this.ecKey = ecKey;
            this.pairingCode = pairingCode;
        }

        internal IEnumerator GetAccessTokens()
        {
            this.ClearAccessTokenCache();
            Dictionary<string, string> parameters = this.GetParams();

            CoroutineWithData cd = new CoroutineWithData(owner, get("tokens", parameters));
            yield return cd.Coroutine;
            string tokensStr = (string)cd.Result;
            Debug.Log("getAccessTokens() tokensStr: " + tokensStr);
            this.tokenCache = ResponseToTokenCache(tokensStr);

            yield return this.tokenCache.Count;


            // Is this client already authorized to use the MERCHANT facade?
            if (!ClientIsAuthorized(FACADE_MERCHANT))
            {
                Debug.Log("initialize():client Not authorized yet.Getting authorized.");
                // Get MERCHANT facade authorization.
                yield return AuthorizeClient(pairingCode);
            }
            else
            {
                Debug.Log("initialize():client Already Authorized.");
            }

        }

        /// <summary>
        /// Authorize (pair) this client with the server using the specified pairing code.
        /// </summary>
        /// <param name="pairingCode">A code obtained from the server; typically from bitpay.com/api-tokens.</param>
        public IEnumerator AuthorizeClient(string pairingCode)
        {
            Token token = new Token();
            token.Id = this.identity;
            token.Guid = Guid.NewGuid().ToString();
            token.PairingCode = pairingCode;
            token.Label = BITPAY_PLUGIN_INFO;
            String json = JsonConvert.SerializeObject(token);

            CoroutineWithData cd = new CoroutineWithData(owner, Post("tokens", json));
            yield return cd.Coroutine;

            string tokenStr = (string)cd.Result;

            tokenStr = ResponseToJsonString(tokenStr);
            Debug.Log("authorizeClient(): tokenStr:" + tokenStr);

            this.CacheTokens(tokenStr);
        }

        public IEnumerator CreateInvoice(InvoiceBtcpay invoice, Action<InvoiceBtcpay> invoiceAction)
        {
            invoice.Token = this.GetAccessToken(FACADE_MERCHANT);//get token by facade type
            invoice.Guid = Guid.NewGuid().ToString();
            String json = JsonConvert.SerializeObject(invoice);
            Debug.Log("createInvoice(): About to post an initial Invoice " + json);

            CoroutineWithData cd = new CoroutineWithData(owner, Post("invoices", json, true));
            yield return cd.Coroutine;
            string responseStr = (string)cd.Result;
            Debug.Log("createInvoice():  response:" + responseStr);

            JsonConvert.PopulateObject(this.ResponseToJsonString(responseStr), invoice);
            Debug.Log("createInvoice():  responsejson to Invoice Object done token1:id=" + invoice.Id + " token=" + invoice.Token + " json=" + JsonConvert.SerializeObject(invoice) + " toString:" + invoice.ToString());
            invoice = JsonConvert.DeserializeObject<InvoiceBtcpay>(this.ResponseToJsonString(responseStr));
            Debug.Log("createInvoice():  responsejson to Invoice Object done token2:id=" + invoice.Id + " token=" + invoice.Token + " json=" + JsonConvert.SerializeObject(invoice) + " toString:" + invoice.ToString());

            // Track the token for this invoice
            this.CacheToken(invoice.Id, invoice.Token);
            Debug.Log("createInvoice():  Taking InvoiceAction callback BEFORE");
            invoiceAction(invoice);
            Debug.Log("createInvoice():  Taking InvoiceAction callback AFTER");

        }

        private string GetAccessToken(string key)
        {
            if (!this.tokenCache.ContainsKey(key))
            {
                throw new BitPayException("Error: You do not have access to facade: " + key);
            }
            return this.tokenCache[key];
        }

        private void CacheTokens(string responseStr)
        {
            Debug.Log("cacheTokens(): Got authorized tokens with " + responseStr);

            List<Token> tokens = JsonConvert.DeserializeObject<List<Token>>(responseStr);
            foreach (Token t in tokens)
            {
                Debug.Log("t.Facade, t.Value " + t.Facade + " " + t.Value);
                this.CacheToken(t.Facade, t.Value);
            }
        }
        /// <summary>
        /// Specified whether the client has authorization (a token) for the specified facade.
        /// </summary>
        /// <param name="facade">The facade name for which authorization is tested.</param>
        /// <returns></returns>
        public bool ClientIsAuthorized(String facade)
        {
            return tokenCache.ContainsKey(facade);
        }

        private void ClearAccessTokenCache()
        {
            this.tokenCache = new Dictionary<string, string>();
        }

        private Dictionary<string, string> GetParams()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            return parameters;
        }

        /// <summary>
        /// Put the token from the response to cache
        /// </summary>
        /// <param name="responseStr">response string from /token API.</param>
        /// <returns>Latest Token Cache</returns>
        private Dictionary<string, string> ResponseToTokenCache(string responseStr)
        {
            // The response is expected to be an array of key/value pairs (facade name = token).
            //            dynamic obj = JsonConvert.DeserializeObject(responseToJsonString(response));
            // sample response is, {"data":[{"MERCHANT":"G4AcnWpGtMDw2p1Ci1h1ommxUs4GJdzurbKFdLqCyLEv"}]}
            Debug.Log("responseToTokenCache():responseStr is " + responseStr);

            List<Dictionary<string, string>> obj = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(this.ResponseToJsonString(responseStr));

            try
            {
                // each element of list
                foreach (var kc in obj)
                {
                    if (kc.Count > 1)
                    {
                        throw new BitPayException("Error: Size of Token object is unexpected.  Expected one entry, got " + kc.Count + " entries.");
                    }
                    foreach (var entry in kc)
                    {
                        if (!this.tokenCache.ContainsKey(entry.Key))
                        {
                            this.CacheToken(entry.Key, entry.Value);
                        }
                    }
                }
            }
            catch (BitPayException ex)
            {
                Debug.Log("BitPayException Error: " + ex.ToString());
                throw new BitPayException("Error: " + ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.Log("Error: response to GET /tokens could not be parsed - " + ex.ToString());
                throw new BitPayException("Error: response to GET /tokens could not be parsed - " + ex.ToString());
            }
            return this.tokenCache;
        }

        /// <summary>
        /// Store the retreived token in cache map
        /// </summary>
        /// <param name="key">facade</param>
        /// <param name="token">token itself</param>
        private void CacheToken(string key, string token)
        {
            this.tokenCache.Add(key, token);
        }

        private String ResponseToJsonString(string responseString)
        {
            if (responseString == null)
            {
                throw new BitPayException("Error: HTTP response is null");
            }

            // An error(s) object raises an exception.
            // A data object has its content extracted (throw away the data wrapper object).
            // Check for error response.
            //            if (dynamicObjectHasProperty(obj, "error"))
            if (JsonHasString(responseString, "error"))
            {
                Debug.Log(" responseToJsonString(): error ");
                throw new BitPayException("Error: " + responseString);
            }

            if (JsonHasString(responseString, "errors"))
            {
                Debug.Log(" responseToJsonString(): errors ");
                string message = "Multiple errors:";
                throw new BitPayException(message + responseString);
            }

            Debug.Log(" responseToJsonString(): No Error ");

            // Check for and exclude a "data" object from the response.
            if (JsonHasString(responseString, "data"))
            {
                Debug.Log(" responseToJsonString(): data found in json1 :" + responseString);
                JObject jo = JObject.Parse(responseString);
                responseString = jo["data"].ToString();
                Debug.Log(" responseToJsonString(): data found in json 2:" + responseString);
            }
            return responseString;
        }

        private static bool JsonHasString(string json, string name)
        {
            return json.Contains(name);
        }

        private IEnumerator get(String uri, Dictionary<string, string> parameters = null)
        {
            string fullURL = baseUrl + uri;
            Debug.Log("get() is called with " + fullURL);
            Debug.Log("get() is called parameters:" + parameters.Count);

            if (parameters != null)
            {

                fullURL += "?";
                foreach (KeyValuePair<string, string> entry in parameters)
                {
                    Debug.Log("get():  getting URL parameter :" + entry.Key + "=" + entry.Value);
                    fullURL += entry.Key + "=" + entry.Value + "&";
                }
                fullURL = fullURL.Substring(0, fullURL.Length - 1);
                Debug.Log("get():  fullURL:"+ fullURL);
            }

            using (UnityWebRequest httpClient = UnityWebRequest.Get(fullURL))
            {

                httpClient.SetRequestHeader("x-accept-version", BITPAY_API_VERSION);
                httpClient.SetRequestHeader("x-bitpay-plugin-info", BITPAY_PLUGIN_INFO);
                string text = fullURL;
                byte[] singleHash = null;
                using (var hash256 = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(text);
                    singleHash = hash256.ComputeHash(bytes);
                }
                ECDSASignature eCDSA = this.ecKey.Sign(new uint256(singleHash));
                string newsig = KeyUtils.bytesToHex(eCDSA.ToDER());

                httpClient.SetRequestHeader("x-signature", newsig);
                byte[] pubkBytes = this.ecKey.PubKey.Decompress().ToBytes();
                httpClient.SetRequestHeader("x-identity", KeyUtils.bytesToHex(pubkBytes));
                Debug.Log("GET HEADER:URL+data =" + fullURL);
                Debug.Log("GET HEADER:x-signature:" + newsig);
                Debug.Log("GET HEADER:x-identity(hex pubk):" + KeyUtils.bytesToHex(pubkBytes));

                yield return httpClient.SendWebRequest();// send request
                yield return httpClient.downloadHandler.text; // return donwloadhandler
            }
        }

        private IEnumerator Post(string uri, string json, bool signatureRequired = false)
        {
            Debug.Log(" post() is called URI:" + uri);
            Debug.Log(" post() is called signatureRequired: " + signatureRequired);
            Debug.Log(" post() is called json before: " + json);
            json = UnicodeToAscii(json);
            Debug.Log(" post() is called json after : " + json);
            using (UnityWebRequest httpClient = new UnityWebRequest(baseUrl + uri, UnityWebRequest.kHttpVerbPOST))
            {
                httpClient.SetRequestHeader("x-accept-version", BITPAY_API_VERSION);
                httpClient.SetRequestHeader("x-bitpay-plugin-info", BITPAY_PLUGIN_INFO);
                httpClient.SetRequestHeader("Content-Type", "application/json");
                if (signatureRequired)
                {
                    string text = this.baseUrl + uri + json;
                    byte[] singleHash = null;
                    using (var hash256 = SHA256.Create())
                    {
                        var bytes = Encoding.UTF8.GetBytes(text);
                        singleHash = hash256.ComputeHash(bytes);
                    }
                    ECDSASignature eCDSA = this.ecKey.Sign(new uint256(singleHash));
                    string newsig = KeyUtils.bytesToHex(eCDSA.ToDER());

                    httpClient.SetRequestHeader("x-signature", newsig);
                    byte[] pubkBytes = this.ecKey.PubKey.Decompress().ToBytes();

                    httpClient.SetRequestHeader("x-identity", KeyUtils.bytesToHex(pubkBytes));
                    Debug.Log("POST HEADER:data:" + this.baseUrl + uri + json);
                    Debug.Log("POST HEADER:x-signature:" + newsig);
                    Debug.Log("POST HEADER:x-identity(hex pubk):" + KeyUtils.bytesToHex(pubkBytes));
                }

                UploadHandlerRaw uH = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
                httpClient.uploadHandler = uH;
                httpClient.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                UnityWebRequestAsyncOperation uwr = httpClient.SendWebRequest();// send request

                while (!uwr.isDone)
                {
                    yield return null;
                }

                if (httpClient.isNetworkError || httpClient.isHttpError)
                {
                    Debug.Log("post Error:" + httpClient.error);
                    Debug.Log("post Status:" + httpClient.responseCode);
                    yield return httpClient.downloadHandler.text; // return donwloadhandler.text
                }
                else
                {
                    yield return httpClient.downloadHandler.text; // return donwloadhandler.text
                }
            }
        }

        private string UnicodeToAscii(string json)
        {
            byte[] unicodeBytes = Encoding.Unicode.GetBytes(json);
            byte[] asciiBytes = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, unicodeBytes);
            char[] asciiChars = new char[Encoding.ASCII.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            Encoding.ASCII.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            return new string(asciiChars);
        }
    }
}
