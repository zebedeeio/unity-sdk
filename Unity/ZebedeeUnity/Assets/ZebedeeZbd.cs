
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
        //1.New Invoice Preparation
        //1.インボイス オブジェクトに必要項目をセットする
        Charge invoiceReq = new Charge();
        invoiceReq.Description = product.text;
        invoiceReq.AmountInSatoshi = int.Parse(amount.text);

        logger.Debug("CreateInvoice:"+invoiceReq.Description);

        //2.Create Invoice with initial data and get the full invoice
        //2.Zebedee Serverにインボイスデータをサブミットして、インボイスの詳細データを取得する。
        await zbdClient.CreateChargeAsync(invoiceReq, handleInvoice);

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

        //4.Set the QR code iamge to image Gameobject
        //4.取得したBOLTからQRコードを作成し、ウオレットでスキャンするために表示する。
        QRcodeBOLT11.GetComponent<Image>().sprite = Sprite.Create(texs, new Rect(0.0f, 0.0f, texs.width, texs.height), new Vector2(0.5f, 0.5f), 100.0f);

        //5.Subscribe the an callback method with invoice ID to be monitored
        //5.支払がされたら実行されるコールバックを引き渡して、コールーチンで実行する
        //        StartCoroutine(btcPayClient.SubscribeInvoiceCoroutine(invoice.Id, printInvoice));
        //StartCoroutine(btcPayClient.listenInvoice(invoice.Id, printInvoice));
        string status = await zbdClient.SubscribeChargeAsync(chargeId);

        if ("completed".Equals(status))
        {
            //インボイスのステータスがcompleteであれば、全額が支払われた状態なので、支払完了のイメージに変更する
            //Change the image from QR to Paid
            QRcodeBOLT11.GetComponent<Image>().sprite = Resources.Load<Sprite>("image/paid");
            logger.Debug("payment is complete");
        }
        else
        {
            //for example, if the amount paid is not full, do something.the line below just print the status.
            //全額支払いでない場合には、なにか処理をおこなう。以下は、ただ　ステータスを表示して終了。
            logger.Error("payment is not completed:" +status);
        }
    }

    public async void CreateWithdrawal()
    {
        //1.New Invoice Preparation
        //1.インボイス オブジェクトに必要項目をセットする
        Withdraw withdrawReq = new Withdraw();
        withdrawReq.Description = product.text;
        withdrawReq.AmountInSatoshi = int.Parse(amount.text);

        //2.Create Invoice with initial data and get the full invoice
        //2.Zebedee Serverにインボイスデータをサブミットして、インボイスの詳細データを取得する。
        await zbdClient.WithDrawAsync(withdrawReq, handleWithdrawal);

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

        //4.Set the QR code iamge to image Gameobject
        //4.取得したBOLTからQRコードを作成し、ウオレットでスキャンするために表示する。
        QRcodeLnURL.GetComponent<Image>().sprite = Sprite.Create(texs, new Rect(0.0f, 0.0f, texs.width, texs.height), new Vector2(0.5f, 0.5f), 100.0f);

        //5.Subscribe the an callback method with invoice ID to be monitored
        //5.支払がされたら実行されるコールバックを引き渡して、コールーチンで実行する
        //        StartCoroutine(btcPayClient.SubscribeInvoiceCoroutine(invoice.Id, printInvoice));
        //StartCoroutine(btcPayClient.listenInvoice(invoice.Id, printInvoice));
        string status = await zbdClient.SubscribeWithDrawAsync(withdraw.Data.Id);

        if ("completed".Equals(status))
        {
            //インボイスのステータスがcompleteであれば、全額が支払われた状態なので、支払完了のイメージに変更する
            //Change the image from QR to Paid
            QRcodeLnURL.GetComponent<Image>().sprite = Resources.Load<Sprite>("image/withdrawn");
            logger.Debug("withdraw is success");
        }
        else
        {
            //for example, if the amount paid is not full, do something.the line below just print the status.
            //全額支払いでない場合には、なにか処理をおこなう。以下は、ただ　ステータスを表示して終了。
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
