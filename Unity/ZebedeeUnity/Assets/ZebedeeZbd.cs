
using UnityEngine;
using UnityEngine.UI;
using ZbdUnitySDK;
using ZbdUnitySDK.Logging;
using ZbdUnitySDK.Models;
using ZbdUnitySDK.Models.Zebedee;
using ZXing;
using ZXing.QrCode;

public class ZebedeeZbd : MonoBehaviour
{
    public string apiKey;//set pairing code from inspector
    public string zebedeeBaseUrl;//set host from inspector

    public Text product;
    public Text amount;
    public GameObject QRcodeBOLT11;
    public GameObject QRcodeLnURL;

    private ZebedeeClient zbdClient = null;
    private IZdbLogger logger;

    public void Start()
    {
        zbdClient = new ZebedeeClient(zebedeeBaseUrl, apiKey);
        this.logger = LoggerFactory.GetLogger();
    }

    public async void CreateInvoice()
    {
        //1.New Charge Preparation
        Charge charge = new Charge();
        charge.Description = product.text;
        charge.AmountInSatoshi = int.Parse(amount.text);

        logger.Debug("CreateInvoice:"+charge.Description);

        //2.Create Charge object and get the BOLT11 invoice from ZEBEDEE API
        await zbdClient.CreateChargeAsync(charge, handleInvoice);

    }

    private async void handleInvoice(ChargeResponse invoice)
    {
        //3.Lightning BOLT invoice string
        string boltInvoice = invoice.Data.Invoice.Request;
        string chargeId = invoice.Data.Id;
        if (string.IsNullOrEmpty(boltInvoice))
        {
            Debug.Log("bolt Invoice is not set in Invoice in reponse.Check the BTCpay server's lightning setup");
            return;
        }

        Texture2D texs = GenerateQR(boltInvoice);//Generate QR code image

        //4.Set the QR code Image to image Gameobject
        QRcodeBOLT11.GetComponent<Image>().sprite = Sprite.Create(texs, new Rect(0.0f, 0.0f, texs.width, texs.height), new Vector2(0.5f, 0.5f), 100.0f);

        //5.Subscribe the get notified about payment status
        string status = await zbdClient.SubscribeChargeAsync(chargeId);

        if ("completed".Equals(status))
        {
            //Change the image from QR to Paid
            QRcodeBOLT11.GetComponent<Image>().sprite = Resources.Load<Sprite>("image/paid");
            logger.Debug("payment is complete");
        }
        else
        {
            //for example, if the amount paid is not full, do something.the line below just print the status.
            logger.Error("payment is not completed:" +status);
        }
    }

    public async void CreateWithdrawal()
    {
        //1.New Withdraw Preparation
        Withdraw withdraw = new Withdraw();
        withdraw.Description = product.text;
        withdraw.AmountInSatoshi = int.Parse(amount.text);

        //2.Create withdraw with ZEBEDEE backend and get lnurl
        await zbdClient.WithDrawAsync(withdraw, handleWithdrawal);

    }

    private async void handleWithdrawal(WithdrawResponse withdraw)
    {
        string lnURL = withdraw.Data.Invoice.Request;
        if (string.IsNullOrEmpty(lnURL))
        {
            logger.Debug("lnURL is not set in withdrawal response.");
            logger.Debug(withdraw.Data.Invoice.Request);
            return;
        }

        Texture2D texs = GenerateQR(lnURL);//Generate QR code image

        //4.Set the QR code image to image Gameobject
        QRcodeLnURL.GetComponent<Image>().sprite = Sprite.Create(texs, new Rect(0.0f, 0.0f, texs.width, texs.height), new Vector2(0.5f, 0.5f), 100.0f);

        //5.Subscribe the an callback method with invoice ID to be monitored
        string status = await zbdClient.SubscribeWithDrawAsync(withdraw.Data.Id);

        if ("completed".Equals(status))
        {
            //Change the image from QR to Paid
            QRcodeLnURL.GetComponent<Image>().sprite = Resources.Load<Sprite>("image/withdrawn");
            logger.Debug("withdraw is success");
        }
        else
        {
            //for example, if the amount paid is not full, do something.the line below just print the status.
            logger.Error("withdraw is not success:" + status);
        }

    }


    private Texture2D GenerateQR(string text)
    {
        logger.Debug("generateQR():generateing Qr for text: " + text);
          
        var encoded = new Texture2D(384, 384);
        var color32 = Encode(text, encoded.width, encoded.height);
        encoded.SetPixels32(color32);
        encoded.Apply();
        return encoded;
    }

    private static Color32[] Encode(string textForEncoding,
      int width, int height)
    {

        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        
        return writer.Write(textForEncoding);
    }
}
