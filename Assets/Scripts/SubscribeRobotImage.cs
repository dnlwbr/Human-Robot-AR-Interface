using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using sensor_msgs = RosSharp.RosBridgeClient.Messages.Sensor;

public class SubscribeRobotImage : MonoBehaviour
{
    public MeshRenderer meshRenderer;

    [SerializeField]
    private string topic = "/rgb/image_raw/compressed";
    [SerializeField, Tooltip("The rate (in ms in between messages) at which to throttle the topics.")]
    private float TimeStep = 0;
    
    private GameObject RosSharp;
    private RosSocket rosSocket;
    private string subscriptionId;
    private Texture2D texture2D;
    private byte[] imageData;
    private bool isInitialized;

    // Start is called before the first frame update
    void Start()
    {
        RosSharp = GameObject.Find("RosSharp");
        rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
        subscriptionId = rosSocket.Subscribe<sensor_msgs.CompressedImage>(topic, callback, throttle_rate: (int)(TimeStep), queue_length: 1);

        texture2D = new Texture2D(1, 1);
        meshRenderer.material = new Material(Shader.Find("Standard"));
    }

    // Update is called once per frame
    void Update()
    {
        if (isInitialized)
            ProcessMessage();
    }

    private void callback(sensor_msgs.CompressedImage message)
    {
        imageData = message.data;
        isInitialized = true;
    }

    private void ProcessMessage()
    {
        texture2D.LoadImage(imageData);
        texture2D.Apply();
        meshRenderer.material.SetTexture("_MainTex", texture2D);
        isInitialized = false;
    }
}
