# Zebedee Unity SDK

Zebedee Unity SDK allows the game developers to use several Bitcoin Lightning backend services easily and integrate the game with instant micro payment network. Here is the list of the supported backend API services as of writing.

1. Zebedee lightning API
2. BTCPay API
3. Lightning Lab LND

# Zebedee Lightning API integration

The current version supports 3 APIs in Zebedee Lightning API

| Zebedee API     |       Use Case            |
|---------|---------------------------|
|charge   | Game shows QR code embedding BOLT 11 invoivce with which Player can pay satoshis |
|withdraw | Game shows QR code embedding LNURL with which Player can withdraw satoshis from Game |
|payment  | Game scans the BOLT11 QR code from player's wallet and pays satoshis to player|

Game developer is able to start to integrate bitcoin lightning in his/her game easily by using the following SDK constructor/methods
## Zebedee Client Construcor

Zebedee Client is the facade object with which Unity C# code interacts. 
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
You can pass those informatin via public properties via Unity Game object.

![Unity Inspector](README_img/zbdSDK_contructor.png)

## Charge method

Zebedee Charge method is to generate BOLT11 invoice string by passing amount and description.
Developer can generate QR code from BOLT11 string and show for player to pay.
As soon as payment is completed the callback method is executed with BOLT11 string returned.

```
//1.New Invoice Preparation
InvoiceRequest invoiceReq = new InvoiceRequest();
invoiceReq.Description = product.text;
invoiceReq.MilliSatoshiAmount = int.Parse(amount.text) * 1000;

//2.Create Invoice with initial data and get the full invoice
await zbdClient.CreateInvoice(invoiceReq, handleInvoice);
```

## Withdraw method

Zebedee Withdraw method is to generate LNURL string by passing amount and description.
Developer can generate QR code from LNURL string and show for player to withdraw satoshis from game.
As soon as payment is completed the callback method is executed with BOLT11 string returned.

```
//1.New Withdraw requsest Preparation
WithdrawRequest withdrawReq = new WithdrawRequest();
withdrawReq.Description = product.text;
withdrawReq.Amount = int.Parse(amount.text);
withdrawReq.InternalId = product.text;

//2.Create Invoice with initial data and get the full invoice
await zbdClient.WithDrawAsync(withdrawReq, handleWithdrawal);```
```
## Payment method
Game developer implemnt webcam component to scan the Invoice QR code on the player's phone.


# Sample Unity Implementation
This project contains Sample Unity project in /<Project Folder>/Unity/ZebedeeUnity/
1. Open this folder by the latest Unity 
2. Open the zebedee scene
3. Set the Zebedee URL and your apikey  from developer portal
4. Run the game

![Unity Demo project](README_img/zbdSDK_unity.png)
