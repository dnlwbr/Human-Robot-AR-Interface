using RosSharp;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using geometry_msgs = RosSharp.RosBridgeClient.MessageTypes.Geometry;
using tf2_msgs = RosSharp.RosBridgeClient.MessageTypes.Tf2.TFMessage;
using std_msgs = RosSharp.RosBridgeClient.MessageTypes.Std;


namespace HumanRobotInterface
{
    /// <summary>
    /// Publishes the transformation from the origin in Unity's world coordinate system to
    /// the origin in the odometry frame of the robot.
    /// </summary>
    public class UnityOrigin2OdomTFPublisher : Publisher<tf2_msgs>
    {
        [SerializeField]
        private MarkerCalibration calibrationMarker;

        private int rate = 10; // in hz
        private tf2_msgs transformMsg;
        private geometry_msgs.TransformStamped transformStamped;
        private Vector3 translation;
        private Quaternion rotation;

        private std_msgs.Time previousStamp = new std_msgs.Time();


        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            transformMsg = new tf2_msgs();
            transformStamped = new geometry_msgs.TransformStamped();
            transformStamped.header.frame_id = "unity_origin";
            transformStamped.child_frame_id = "odom";
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
            translation = gameObject.transform.position;
            transformStamped.transform.translation = Conversions.Vec3ToGeoMsgsVec3(translation.Unity2Ros());

            rotation = gameObject.transform.rotation;
            transformStamped.transform.rotation = Conversions.QuaternionToGeoMsgsQuaternion(rotation.Unity2Ros());

            transformStamped.header.Update();
            transformMsg.transforms = new geometry_msgs.TransformStamped[] { transformStamped };

            // Publish if stamp has changed
            if ((transformStamped.header.stamp.secs != previousStamp.secs) ||
                (transformStamped.header.stamp.nsecs != previousStamp.nsecs))
            {
                Publish(transformMsg, rate);
                previousStamp.secs = transformStamped.header.stamp.secs;
                previousStamp.nsecs = transformStamped.header.stamp.nsecs;
            }
        }
    }
}