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
    /// Publishes transformation from the base_link frame of the robot to
    /// the Azure Kinect's camera_base frame.
    /// </summary>
    public class BaseLink2KinectTFPublisher : Publisher<tf2_msgs>
    {
        [SerializeField]
        private MarkerCalibration calibrationMarker;

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
            transformStamped.header.frame_id = "base_link";
            transformStamped.child_frame_id = "camera_base";
        }

        void FixedUpdate()
        {
            if (calibrationMarker.isCalibrated)
            {
                PublishTransformation();
            }
        }

        private void PublishTransformation()
        {
            // World position to local position
            translation = gameObject.transform.root.InverseTransformPoint(gameObject.transform.position);
            translation -= new Vector3(0, 0.021f, 0);   // Start at base_footprint -> Start at base_link
            transformStamped.transform.translation = Conversions.Vec3ToGeoMsgsVec3(translation.Unity2Ros());

            // World rotation to local rotation
            rotation = Quaternion.Inverse(gameObject.transform.root.rotation) * gameObject.transform.rotation;
            transformStamped.transform.rotation = Conversions.QuaternionToGeoMsgsQuaternion(rotation.Unity2Ros());

            transformMsg.transforms = new geometry_msgs.TransformStamped[] { transformStamped };
            transformStamped.header.Update();
            Publish(transformMsg, rate);
        }
    }
}
