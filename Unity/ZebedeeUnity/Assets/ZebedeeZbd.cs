using BTCPayServer.Lightning;
using UnityEngine;
using UnityEngine.UI;
using ZbdUnitySDK;
using ZbdUnitySDK.models;
using ZXing;
using ZXing.QrCode;

public class ZebedeeZbd : MonoBehaviour
{
    public string apiKey;//set pairing code from inspector
    public string zebedeeBaseUrl;//set host from inspector

    public Text product;
    public Text amount;
    public GameObject QRcode;

    private ZebedeeClient zbdClient = null;

    public void Start()
    {
        zbdClient = new ZebedeeClient(zebedeeBaseUrl, apiKey);
    }

    public async void CreateInvoice()
    {
        //1.New Invoice Preparation
        //1.インボイス オブジェクトに必要項目をセットする
        InvoiceRequest invoiceReq = new InvoiceRequest();
        invoiceReq.Description = product.text;
        invoiceReq.MilliSatoshiAmount = int.Parse(amount.text) * 1000;

        //2.Create Invoice with initial data and get the full invoice
        //2.Zebedee Serverにインボイスデータをサブミットして、インボイスの詳細データを取得する。
        await zbdClient.CreateInvoice(invoiceReq, handleInvoice);

    }

    private void handleInvoice(LightningInvoice invoice)
    {
        //3.Lightning BOLT invoice string
        string boltInvoice = invoice.BOLT11;
        if (string.IsNullOrEmpty(boltInvoice))
        {
            Debug.Log("bolt Invoice is not set in Invoice in reponse.Check the BTCpay server's lightning setup");
            return;
        }

        Texture2D texs = GenerateQR(boltInvoice);//Generate QR code image

        //4.Set the QR code iamge to image Gameobject
        //4.取得したBOLTからQRコードを作成し、ウオレットでスキャンするために表示する。
        QRcode.GetComponent<Image>().sprite = Sprite.Create(texs, new Rect(0.0f, 0.0f, texs.width, texs.height), new Vector2(0.5f, 0.5f), 100.0f);

        //5.Subscribe the an callback method with invoice ID to be monitored
        //5.支払がされたら実行されるコールバックを引き渡して、コールーチンで実行する
        //        StartCoroutine(btcPayClient.SubscribeInvoiceCoroutine(invoice.Id, printInvoice));
        //StartCoroutine(btcPayClient.listenInvoice(invoice.Id, printInvoice));


    }

     
    ////Callback method when payment is executed. 
    ////支払実行時に、呼び出されるコールバック 関数（最新のインボイスオブジェクトが渡される）
    //public void printInvoice(Invoice invoice)
    //{
    //    //Hide QR code image to Paied Image file
    //    //ステータス 一覧はこちら。 https://bitpay.com/docs/invoice-states
    //    if (invoice.Status == "complete")
    //    {
    //        //インボイスのステータスがcompleteであれば、全額が支払われた状態なので、支払完了のイメージに変更する
    //        //Change the image from QR to Paid
    //        QRcode.GetComponent<Image>().sprite = Resources.Load<Sprite>("image/paid");
    //        Debug.Log("payment is complete");
    //    }else
    //    {
    //         //for example, if the amount paid is not full, do something.the line below just print the status.
    //        //全額支払いでない場合には、なにか処理をおこなう。以下は、ただ　ステータスを表示して終了。
    //        Debug.Log("payment is not completed:" + invoice.Status);
    //    }

    //}


    private Texture2D GenerateQR(string text)
    {
        Debug.Log("generateQR():generateing Qr for text: " + text);
          
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
