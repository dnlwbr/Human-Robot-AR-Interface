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
    /// the Azure Kinect's depth sensor coordinate system.
    /// </summary>
    public class UnityOrigin2KinectTFPublisher : MonoBehaviour
    {
        private GameObject RosSharp;
        private RosSocket rosSocket;
        private string publicationId;
        private float previousRealTime;
        private float rate = 10; // in hz
        private tf2_msgs transformMsg;
        private geometry_msgs.TransformStamped transformStamped;
        private Vector3 translation;
        private Quaternion rotation;


        // Start is called before the first frame update
        void Start()
        {
            RosSharp = GameObject.Find("RosSharp");
            rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
            publicationId = rosSocket.Advertise<tf2_msgs>("/tf");
            transformMsg = new tf2_msgs();
            transformStamped = new geometry_msgs.TransformStamped();
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.realtimeSinceStartup - previousRealTime >= 1 / rate && gameObject.transform.root.GetComponent<MarkerCalibration>().isCalibrated)
            {
                PublishTransformation();
                previousRealTime = Time.realtimeSinceStartup;
            }
        }

        public void PublishTransformation()
        {
            transformStamped.header.Update();
            transformStamped.header.frame_id = "unity_origin";
            transformStamped.child_frame_id = "depth_camera_link";

            translation = gameObject.transform.position;
            transformStamped.transform.translation = Conversions.Vec3ToGeoMsgsVec3(translation.Unity2Ros());

            rotation = gameObject.transform.rotation;
            transformStamped.transform.rotation = Conversions.QuaternionToGeoMsgsQuaternion(rotation.Unity2Ros());

            transformMsg.transforms = new geometry_msgs.TransformStamped[] { transformStamped };
            rosSocket.Publish(publicationId, transformMsg);
        }
    }
}
