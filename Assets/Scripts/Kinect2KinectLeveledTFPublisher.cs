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
    /// Publishes the transformation from the Azure Kinect's camera_base frame to the
    /// leveled camera_base frame.
    /// </summary>
    public class Kinect2KinectLeveledTFPublisher : Publisher<tf2_msgs>
    {
        private int rate = 10; // in hz
        private tf2_msgs transformMsg;
        private geometry_msgs.TransformStamped transformStamped;
        private Quaternion rotation;


        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            transformMsg = new tf2_msgs();
            transformStamped = new geometry_msgs.TransformStamped();
            transformStamped.header.frame_id = "camera_base";
            transformStamped.child_frame_id = "camera_base_leveled";
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
            transformStamped.transform.translation = Conversions.Vec3ToGeoMsgsVec3(Vector3.zero);

            /*
            Vector3 newForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            // Transfrom from world space to local space
            newForward = Quaternion.Inverse(transform.rotation) * newForward;
            rotation = Quaternion.LookRotation(newForward, Vector3.up);
            */

            rotation = Quaternion.FromToRotation(Vector3.up, Quaternion.Inverse(transform.rotation) * Vector3.up);
            transformStamped.transform.rotation = Conversions.QuaternionToGeoMsgsQuaternion(rotation.Unity2Ros());

            transformMsg.transforms = new geometry_msgs.TransformStamped[] { transformStamped };
            transformStamped.header.Update();
            Publish(transformMsg, rate);
        }
    }
}
