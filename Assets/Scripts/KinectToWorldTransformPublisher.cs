using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using geometry_msgs = RosSharp.RosBridgeClient.MessageTypes.Geometry;


namespace HumanRobotInterface
{
    /// <summary>
    /// Publishes transformation from the Azure Kinect's depth sensor coordinate system to
    /// Unity's world coordinate system.
    /// </summary>
    public class KinectToWorldTransformPublisher : MonoBehaviour
    {
        private GameObject RosSharp;
        private RosSocket rosSocket;
        private string publicationId;
        private geometry_msgs.Transform transormMsg;
        private Vector3 translation;
        private Quaternion rotation;

        // Start is called before the first frame update
        void Start()
        {
            RosSharp = GameObject.Find("RosSharp");
            rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
            publicationId = rosSocket.Advertise<geometry_msgs.Transform>("/HoloLens2/KinectToWorld");
            transormMsg = new geometry_msgs.Transform();
        }

        // Update is called once per frame
        void Update()
        {
            translation = -gameObject.transform.position;
            transormMsg.translation = Conversions.Vec3ToGeoMsgsVec3(translation.Kinect2Unity());
            rotation = Quaternion.Inverse(gameObject.transform.rotation);
            transormMsg.rotation = Conversions.QuaternionToGeoMsgsQuaternion(rotation.Kinect2Unity());
            rosSocket.Publish(publicationId, transormMsg);
        }
    }
}
