using QRTracking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class QRCodeTarget : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Content of the QR Code to be aligned with.")]
    private string QRCodeContent = "Uni Tuebingen"; // mit Umlaut: Tￃﾼbingen = T\uFFC3\uFFBCbingen

    [SerializeField]
    [Tooltip("Thickness of the board or paper in the mount on which the QR code is printed [in mm].")]
    private float boardThickness = 5;

    private GameObject qrCodeObject = null;

    // Start is called before the first frame update
    void Start()
    {
        QRCodeContent = System.Text.RegularExpressions.Regex.Unescape(@QRCodeContent);  // Convert unicodes to the respective characters
        boardThickness /= 1000;  // mm -> m (Unity units)
        if (gameObject.transform.Find("Visuals") != null)
        {
            gameObject.transform.Find("Visuals").transform.localPosition -= new Vector3(0, 0, boardThickness);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check null instead of isInitialized in case object has been destroyed.
        if (qrCodeObject != null && QRCodesManager.Instance.IsTrackerRunning == true)
        {
            gameObject.transform.position = qrCodeObject.transform.position;
            gameObject.transform.rotation = qrCodeObject.transform.rotation;
        } else
        {
            Initialize();
        }
    }

    void Initialize()
    {
        QRCode[] qrComponents = FindObjectsOfType<QRCode>();
        foreach (QRCode qrComponent in qrComponents)
        {
            if (qrComponent.qrCode.Data == QRCodeContent)
            {
                qrCodeObject = qrComponent.gameObject;
                Debug.Log("QR code object found.");
            }
        }
    }
}
