using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using vision_msgs = RosSharp.RosBridgeClient.MessageTypes.Vision;

namespace HumanRobotInterface
{
    public class BoundingBoxSubscriber : Subscriber<vision_msgs.Detection3D>
    {
        [Tooltip("Reference frame where the bounding boxes live.")]
        [SerializeField]
        private Transform referenceFrame;

        private BoundsControl boundsControl;

        private vision_msgs.Detection3D detectionMsg;
        private bool isMessageReceived;


        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            boundsControl = gameObject.GetComponent<BoundsControl>();
        }

        // Update is called once per frame
        void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }

        protected override void callback(vision_msgs.Detection3D message)
        {
            detectionMsg = message;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            // Activate if "Activate Manually" has been selected
            //boundsControl.Active = true;
            
            Vector3 position = new Vector3((float)detectionMsg.bbox.center.position.x,
                                           (float)detectionMsg.bbox.center.position.y,
                                           (float)detectionMsg.bbox.center.position.z).Kinect2Unity();
            Quaternion rotation = new Quaternion((float)detectionMsg.bbox.center.orientation.x,
                                                 (float)detectionMsg.bbox.center.orientation.y,
                                                 (float)detectionMsg.bbox.center.orientation.z,
                                                 (float)detectionMsg.bbox.center.orientation.w).Kinect2Unity();
            Vector3 scale = (new Vector3((float)detectionMsg.bbox.size.x,
                                         (float)detectionMsg.bbox.size.y,
                                         (float)detectionMsg.bbox.size.z));

            if (position.IsValidVector() && rotation.IsValidRotation() && scale.IsValidVector())
            {
                gameObject.transform.position = position.Robot2UnityPosition(referenceFrame);
                gameObject.transform.rotation = rotation.Robot2UnityTwist(referenceFrame);
                gameObject.transform.localScale = scale;
            }
            isMessageReceived = false;
        }
    }
}
