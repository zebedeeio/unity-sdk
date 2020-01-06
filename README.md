# Zebedee Unity SDK

Zebedee Unity SDK allows Game developers to integrate several Bitcoin Lightning backend services easily. Here is the list of the supported backend services as of writing.

1. Zebedee lightning API
2. BTCPay API
3. Lightning Lab LND API

# Zebedee Lightning API integration

The current version supports 3 APIs in Zebedee Lightning API

| Zebedee API     |       Use Case            |
|---------|---------------------------|
|charge   | Game shows QR code embedding BOLT 11 invoivce with which Player can pay satoshis |
|withdraw | Game shows QR code embedding LNURL with which Player can withdraw satoshis from Game |
|payment  | Game scans the BOLT11 QR code from player's wallet and pays satoshis to player|

#Zebedee Client SDK methods

Game developer is now able to start to integrate bitcoin lightning in his/her game very easily by using the following simple SDK constructor/methods

## Zebedee Client Construcor

Zebedee Client is the facade object with which Unity C# code interacts. Nohing else.
It requires the zebedeeBaseURL and apikey.  
The developer can get an APIKey from Zebedee Developer dashboard -> Games -> Create a New Game.

```
    public string apiKey;//set pairing code from inspector
    public string zebedeeBaseUrl;//set host from inspector
    private ZebedeeClient zbdClient = null;

    public void Start()
    {
        zbdClient = new ZebedeeClient(zebedeeBaseUrl, apiKey);
    }
```
Developer can pass those information via public properties of Unity Game object.

![Unity Inspector](README_img/zbdSDK_contructor.png)

## CreateInvoice method

Zebedee CreateIvoice method is to generate BOLT11 invoice string by passing amount and description.
Developer can generate QR code from BOLT11 string and show it for player to pay to.

As soon as the invoice is generated in backend service,  the callback method is executed with BOLT11 string returned. Normally the callback generates QR code and show 2D Image.

```
//1.New Invoice Preparation
InvoiceRequest invoiceReq = new InvoiceRequest();
invoiceReq.Description = product.text;
invoiceReq.MilliSatoshiAmount = int.Parse(amount.text) * 1000;

//2.Create Invoice with initial data and get the full invoice
await zbdClient.CreateInvoice(invoiceReq, handleInvoice);
```

## Withdraw method

Zebedee WithdrawAsync method is to generate LNURL string by passing amount and description.
Developer can generate QR code from LNURL string and show for player to withdraw satoshis from game.
As soon as the LNURL is generated the callback method is executed with LNURL string returned. Normally the callback generates QR code and show 2D Image.

```
//1.New Withdraw requsest Preparation
WithdrawRequest withdrawReq = new WithdrawRequest();
withdrawReq.Description = product.text;
withdrawReq.Amount = int.Parse(amount.text);
withdrawReq.InternalId = product.text;

//2.Create Invoice with initial data and get the full invoice
await zbdClient.WithDrawAsync(withdrawReq, handleWithdrawal);```
```

## SubscribeToInvoice method
Game developer subscribes to the specfic invoice ID to getnotified and take some action. 
For exmaple, showing **Paid** image.

## SubscribeToWithdraw method
Game developer subscribes to the specfic withdraw ID to getnotified and take some action. 
For exmaple, showing **Withdraw Complete** image.

## Payment method
In case the player doesnot have LNURL supporting wallet, 
Game developer implemnt webcam component to scan the Invoice QR code on the player's phone.

# Sample Unity Implementation
This project contains Sample Unity project in /<Project Folder>/Unity/ZebedeeUnity/
1. Open this folder by the latest Unity 
2. Open the zebedee scene
3. Set the Zebedee URL and your apikey  from developer portal
4. Run the game

![Unity Demo project](README_img/zbdSDK_unity.png)
