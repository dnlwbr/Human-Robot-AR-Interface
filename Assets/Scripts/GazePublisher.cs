using Microsoft.MixedReality.Toolkit;
using RosSharp;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using geometry_msgs = RosSharp.RosBridgeClient.Messages.Geometry;


namespace HumanRobotInterface
{
    /// <summary>
    /// Publishes gaze in the frame of the Azure Kinect's depth sensor.
    /// </summary>
    public class GazePublisher : MonoBehaviour
    {
        private GameObject RosSharp;
        private RosSocket rosSocket;
        private string publicationIdPose;
        private string publicationIdPoint;
        private geometry_msgs.PoseStamped Gaze;
        private geometry_msgs.PointStamped HitPoint;
        private Vector3 GazeOrigin;
        private Vector3 GazeDirection;
        private Vector3 HitPosition;
        //private Vector3 HitNormal;

        // Start is called before the first frame update
        void Start()
        {
            RosSharp = GameObject.Find("RosSharp");
            rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
            publicationIdPose = rosSocket.Advertise<geometry_msgs.PoseStamped>("/HoloLens2/Gaze");
            publicationIdPoint = rosSocket.Advertise<geometry_msgs.PointStamped>("/HoloLens2/Gaze/HitPoint");
            Gaze = new geometry_msgs.PoseStamped();
            HitPoint = new geometry_msgs.PointStamped();
            Gaze.header.frame_id = "depth_camera_link";
            HitPoint.header.frame_id = "depth_camera_link";
        }

        // Update is called once per frame
        void Update()
        {
            if (CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingEnabledAndValid && gameObject.transform.root.GetComponent<MarkerCalibration>().isCalibrated)
            {
                PublishGazeDirectionOrigin();
                PublishGazeHitPoint();
            }
        }

        void PublishGazeDirectionOrigin()
        {
            GazeOrigin = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
            GazeOrigin -= gameObject.transform.position;
            GazeOrigin = Quaternion.Inverse(gameObject.transform.rotation) * GazeOrigin;
            Gaze.pose.position = Conversions.Vec3ToGeoMsgsPoint(GazeOrigin.Unity2Kinect());

            GazeDirection = CoreServices.InputSystem.EyeGazeProvider.GazeDirection;
            GazeDirection = Quaternion.Inverse(gameObject.transform.rotation) * GazeDirection;
            /* Quaternion.identity points towards Kinect's x-axis. Therefore use Unity.x = Vector.right because for orientations
               with quaternions RVIZ uses the x-axis as the forward axis instead of the Kinect's forward axis z:  */
            Gaze.pose.orientation = Conversions.QuaternionToGeoMsgsQuaternion(Quaternion.FromToRotation(Vector3.right, GazeDirection).Unity2Kinect());

            Gaze.header.Update();
            rosSocket.Publish(publicationIdPose, Gaze);

            //Debug.Log("Gaze is looking in direction: " + GazeDirection);
            //Debug.Log("Gaze origin is: " + GazeOrigin);
        }

        void PublishGazeHitPoint()
        {
            HitPosition = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
            HitPosition -= gameObject.transform.position;
            HitPosition = Quaternion.Inverse(gameObject.transform.rotation) * HitPosition;
            HitPoint.point = Conversions.Vec3ToGeoMsgsPoint(HitPosition.Unity2Kinect());

            HitPoint.header.Update();
            rosSocket.Publish(publicationIdPoint, HitPoint);

            //HitNormal = CoreServices.InputSystem.EyeGazeProvider.HitNormal;
            //Debug.Log("HitPosition: " + HitPosition);
            //Debug.Log("HitNormal: " + HitNormal);
            //Debug.Log("HitInfo: " + CoreServices.InputSystem.EyeGazeProvider.HitInfo);
        }
    }
}
