using Microsoft.MixedReality.Toolkit;
using RosSharp;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using geometry_msgs = RosSharp.RosBridgeClient.MessageTypes.Geometry;


namespace HumanRobotInterface
{
    /// <summary>
    /// Publishes gaze in the frame of the Azure Kinect's RGB camera.
    /// The hit point in the rgb_camera_link frame drifts over time as odom becomes inaccurate, whereas
    /// the hit point in the unity_origin frame is stable with respect to unity_origin frame.
    /// </summary>
    public class GazePublisher : MonoBehaviour
    {
        [SerializeField]
        private MarkerCalibration calibrationMarker;

        private GameObject RosSharp;
        private RosSocket rosSocket;
        private string publicationIdPose;
        private string publicationIdPoseRGB;
        private string publicationIdPoint;
        private string publicationIdPointRGB;
        private geometry_msgs.PoseStamped Gaze;
        private geometry_msgs.PoseStamped GazeRGB;
        private geometry_msgs.PointStamped HitPoint;
        private geometry_msgs.PointStamped HitPointRGB;
        private Vector3 GazeOrigin;
        private Vector3 GazeOriginRGB;
        private Vector3 GazeDirection;
        private Vector3 GazeDirectionRGB;
        private Vector3 HitPosition;
        private Vector3 HitPositionRGB;
        //private Vector3 HitNormal;

        // Start is called before the first frame update
        void Start()
        {
            RosSharp = GameObject.Find("RosSharp");
            rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
            publicationIdPose = rosSocket.Advertise<geometry_msgs.PoseStamped>("/hololens2/gaze");
            publicationIdPoint = rosSocket.Advertise<geometry_msgs.PointStamped>("/hololens2/gaze/hitpoint");
            publicationIdPoseRGB = rosSocket.Advertise<geometry_msgs.PoseStamped>("/hololens2/gaze_to_rgb_camera_link");
            publicationIdPointRGB = rosSocket.Advertise<geometry_msgs.PointStamped>("/hololens2/gaze_to_rgb_camera_link/hitpoint");
            Gaze = new geometry_msgs.PoseStamped();
            GazeRGB = new geometry_msgs.PoseStamped();
            HitPoint = new geometry_msgs.PointStamped();
            HitPointRGB = new geometry_msgs.PointStamped();
            Gaze.header.frame_id = "unity_origin";
            HitPoint.header.frame_id = "unity_origin";
            GazeRGB.header.frame_id = "rgb_camera_link";
            HitPointRGB.header.frame_id = "rgb_camera_link";
        }

        // Update is called once per frame
        void Update()
        {
            if (CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingEnabledAndValid && calibrationMarker.isCalibrated)
            {
                PublishGazeDirectionOrigin();
                PublishGazeDirectionOriginRGB();
                PublishGazeHitPoint();
                PublishGazeHitPointRGB();
            }
        }

        void PublishGazeDirectionOrigin()
        {
            GazeOrigin = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
            Gaze.pose.position = Conversions.Vec3ToGeoMsgsPoint(GazeOrigin.Unity2Ros());

            GazeDirection = CoreServices.InputSystem.EyeGazeProvider.GazeDirection;
            Gaze.pose.orientation = Conversions.QuaternionToGeoMsgsQuaternion(Quaternion.FromToRotation(Vector3.forward, GazeDirection).Unity2Ros());

            Gaze.header.Update();
            rosSocket.Publish(publicationIdPose, Gaze);
        }

        void PublishGazeDirectionOriginRGB()
        {
            GazeOriginRGB = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
            GazeOriginRGB -= gameObject.transform.position;
            GazeOriginRGB = Quaternion.Inverse(gameObject.transform.rotation) * GazeOriginRGB;
            GazeRGB.pose.position = Conversions.Vec3ToGeoMsgsPoint(GazeOriginRGB.Unity2Kinect());

            GazeDirectionRGB = CoreServices.InputSystem.EyeGazeProvider.GazeDirection;
            GazeDirectionRGB = Quaternion.Inverse(gameObject.transform.rotation) * GazeDirectionRGB;
            /* In Ros/RVIZ Quaternion.identity points along the forward axis, which is the Ros' x-Axis. Therefore we
             * have to use Kinect.x = Unity.x = Vector.right here instead of the Kinect's forward axis z. */
            GazeRGB.pose.orientation = Conversions.QuaternionToGeoMsgsQuaternion(Quaternion.FromToRotation(Vector3.right, GazeDirectionRGB).Unity2Kinect());

            GazeRGB.header.Update();
            rosSocket.Publish(publicationIdPoseRGB, GazeRGB);
        }

        void PublishGazeHitPoint()
        {
            HitPosition = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
            HitPoint.point = Conversions.Vec3ToGeoMsgsPoint(HitPosition.Unity2Ros());

            HitPoint.header.Update();
            rosSocket.Publish(publicationIdPoint, HitPoint);

            //HitNormal = CoreServices.InputSystem.EyeGazeProvider.HitNormal;
            //Debug.Log("HitPosition: " + HitPosition);
            //Debug.Log("HitNormal: " + HitNormal);
            //Debug.Log("HitInfo: " + CoreServices.InputSystem.EyeGazeProvider.HitInfo);
        }

        void PublishGazeHitPointRGB()
        {
            HitPositionRGB = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
            HitPositionRGB -= gameObject.transform.position;
            HitPositionRGB = Quaternion.Inverse(gameObject.transform.rotation) * HitPositionRGB;
            HitPointRGB.point = Conversions.Vec3ToGeoMsgsPoint(HitPositionRGB.Unity2Kinect());

            HitPointRGB.header.Update();
            rosSocket.Publish(publicationIdPointRGB, HitPointRGB);
        }
    }
}
