using RosSharp;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using geometry_msgs = RosSharp.RosBridgeClient.MessageTypes.Geometry;
using tf2_msgs = RosSharp.RosBridgeClient.MessageTypes.Tf2.TFMessage;


namespace HumanRobotInterface
{
    /// <summary>
    /// Publishes transformation from the origin in Unity's world coordinate system to
    /// the Azure Kinect's camera_base frame.
    /// </summary>
    public class UnityOrigin2KinectTFPublisher : Publisher<tf2_msgs>
    {
        private int rate = 10; // in hz
        private tf2_msgs transformMsg;
        private geometry_msgs.TransformStamped transformStamped;
        private Vector3 translation;
        private Quaternion rotation;


        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            transformMsg = new tf2_msgs();
            transformStamped = new geometry_msgs.TransformStamped();
            transformStamped.header.frame_id = "unity_origin";
            transformStamped.child_frame_id = "camera_base";
        }

        void FixedUpdate()
        {
            if (gameObject.transform.root.GetComponent<MarkerCalibration>().isCalibrated)
            {
                PublishTransformation();
            }
        }

        private void PublishTransformation()
        {
            translation = gameObject.transform.position;
            transformStamped.transform.translation = Conversions.Vec3ToGeoMsgsVec3(translation.Unity2Ros());

            rotation = gameObject.transform.rotation;
            transformStamped.transform.rotation = Conversions.QuaternionToGeoMsgsQuaternion(rotation.Unity2Ros());

            transformMsg.transforms = new geometry_msgs.TransformStamped[] { transformStamped };
            transformStamped.header.Update();
            Publish(transformMsg, rate);
        }
    }
}
