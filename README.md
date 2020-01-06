# ZEBEDEE Unity SDK

The ZEBEDEE Unity SDK allows developers to easily integrate Bitcoin Lightning microtransactions into their games and digital experiences. The SDK aims to provide helper methods to improve upon developer experience. The following backend systems are available for the Unity SDK:

1. [ZEBEDEE Lightning API](https://zebedee.io)
2. BTCPay Server API
3. LND API

# ZEBEDEE Lightning API

## General Information

The current ZEBEDEE API exposes 3 major entities and endpoints:

| ZEBEDEE API     |    Use Case   |
|----------|---------------------------|
| Charge   | Shows QR code with Invoice (BOLT 11) that Player can pay for in satoshis. |
| Withdraw | Shows QR code with LNURL Invoice that allows Player to Withdraw satoshis. |
| Payment  | Scans the Invoice (BOLT11) QR code from Player's wallet and pays satoshis. |

The SDK exposes a set of helper methods, including a client constructor that facilitates the interaction with the ZEBEDEE APIs.

## API Client Constructor

ZEBEDEE Client is the only object with which Unity C# code interacts with. The constructor requires a `zebedeeBaseURL` and a `apikey`. You can get those and more documentation at the [ZEBEDEE website](http://zebedee.io). Developers can get an API Key from the Developer Dashboard -> Games -> Create a New Game. API calls without an API Key will fail.

```csharp
    public string zebedeeBaseUrl;   //set host from inspector
    public string apiKey;           //set pairing code from inspector
    
    private ZebedeeClient zbdClient = null;

    public void Start()
    {
        zbdClient = new ZebedeeClient(zebedeeBaseUrl, apiKey);
    }
```
Developers can also pass these settings via the Public Properties of Unity Game object. See below for reference.

![Unity Inspector](README_img/zbdSDK_contructor.png)

## CreateInvoice Method

The `CreateIvoice` method is used to generate BOLT11 Invoices. The method expects an `amount` and a `description`. It is commonplace to showcase the Invoice in a QR code format, for users and players to scan.
As soon as the Invoice is generated in the ZEBEDEE backend services, the callback method is executed with the corresponding BOLT11 Invoice string returned (e.g. `lnbc125bj1...`).

```csharp
// New Invoice Preparation
InvoiceRequest invoiceReq = new InvoiceRequest();

invoiceReq.MilliSatoshiAmount = int.Parse(amount.text) * 1000;
invoiceReq.Description = product.text;

// Create Invoice with data and receive Invoice BOLT11 string
await zbdClient.CreateInvoice(invoiceReq, handleInvoice);
```

## Withdraw Method

ZEBEDEE `WithdrawAsync` method is used to generate LNURL strings for withdrawing funds. The method expects an `amount` and a `description`. Developers are encouraged to show these as QR codes so players and users can scan them.
As soon as the Withdrawal Request LNURL is generated the callback method is executed with the LNURL string returned.

```
// Withdrawal Request Preparation
WithdrawRequest withdrawReq = new WithdrawRequest();

withdrawReq.Amount = int.Parse(amount.text);
withdrawReq.Description = product.text;
withdrawReq.InternalId = product.text;

// Create Invoice with data and receive LNURL string
await zbdClient.WithDrawAsync(withdrawReq, handleWithdrawal);```
```

## SubscribeToInvoice Method
In order to get information about the status of a recently created Invoice, one can use the `SubscribeToInvoice` method. This allows developers to respond to any updates on the status of a payment inside of their applications and games.
For example, showing **Paid Invoice** image.

## SubscribeToWithdraw Method
In order to get information about the status of a recently created Withdrawal Request, one can use the `SubscribeToWithdraw` method. This allows developers to respond to any updates on the status of a withdrawal inside of their applications and games.
For example, showing **Withdrawal Complete** image.

## Payment Method
In case the Player's wallet does not support LNURL QR codes, you may pay an invoice directly. Game developers can possibly implement an input for user to paste an invoice, or even a webcam feature to scan the Invoice QR code on the Player's phone.

# Examples / Demo Implementation
This repository also contains a Sample Unity project under `/<Project Folder>/Unity/ZebedeeUnity/`. To run the project, follow the steps below:

1. Open this folder with the latest Unity software.
2. Open the ZEBEDEE scene.
3. Set the ZEBEDEE URL and your API Key (found in ZEBEDEE's Developer Dashboard).
4. Start the game.

![Unity Demo project](README_img/zbdSDK_unity.png)
