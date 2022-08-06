using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp;
using RosSharp.RosBridgeClient;
using sensor_msgs = RosSharp.RosBridgeClient.MessageTypes.Sensor;


namespace HumanRobotInterface
{
    public class PointCloud2Subscriber : Subscriber<sensor_msgs.PointCloud2>
    {
        public sensor_msgs.PointCloud2 pointCloud { get; private set; }

        public bool isMessageReceived { get; set; }

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }

        protected override void callback(sensor_msgs.PointCloud2 message)
        {
            pointCloud = message;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            // Do something.
        }
    }
}
