using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using sensor_msgs = RosSharp.RosBridgeClient.MessageTypes.Sensor;


namespace HumanRobotInterface
{
    public class RobotImageSubscriber : Subscriber<sensor_msgs.CompressedImage>
    {
        public MeshRenderer meshRenderer;

        private Texture2D texture2D;
        private byte[] imageData;
        private bool isMessageReceived;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            texture2D = new Texture2D(1, 1);
            meshRenderer.material = new Material(Shader.Find("Standard"));
        }

        // Update is called once per frame
        void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }

        protected override void callback(sensor_msgs.CompressedImage message)
        {
            imageData = message.data;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            texture2D.LoadImage(imageData);
            texture2D.Apply();
            meshRenderer.material.SetTexture("_MainTex", texture2D);
            isMessageReceived = false;
        }
    }
}