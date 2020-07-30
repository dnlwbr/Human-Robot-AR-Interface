using QRTracking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QRCodeTarget : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Content of the QR Code to be aligned with.")]
    private string QRCodeContent = "Uni Tuebingen";

    [SerializeField]
    [Tooltip("Thickness of the board or paper in the mount on which the QR code is printed [in mm].")]
    private float boardThickness = 5;

    private GameObject qrCodeObject = null;

    // Start is called before the first frame update
    void Start()
    {
        boardThickness /= 1000;  // mm -> m (Unity units)
        gameObject.transform.Find("Visuals").transform.localPosition -= new Vector3(0, 0, boardThickness);
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
            if (qrComponent.qrCode.Data == QRCodeContent)
            {
                qrCodeObject = qrComponent.gameObject;
                Debug.Log("QR code object found.");
            }
    }
}
