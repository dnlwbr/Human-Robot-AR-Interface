using Microsoft.MixedReality.Toolkit;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using geometry_msgs = RosSharp.RosBridgeClient.Messages.Geometry;

/// <summary>
/// Publishes gaze in the frame of the Azure Kinect's depth sensor.
/// </summary>
public class GazePublisher : MonoBehaviour
{
    private GameObject RosSharp;
    private RosSocket rosSocket;
    private string publicationIdPose;
    private string publicationIdPoint;
    private geometry_msgs.Pose Gaze;
    private geometry_msgs.Point HitPoint;
    private Vector3 GazeOrigin;
    private Vector3 GazeDirection;
    private Vector3 HitPosition;
    //private Vector3 HitNormal;

    // Start is called before the first frame update
    void Start()
    {
        RosSharp = GameObject.Find("RosSharp");
        rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
        publicationIdPose = rosSocket.Advertise<geometry_msgs.Pose>("/HoloLens2/Gaze");
        publicationIdPoint = rosSocket.Advertise<geometry_msgs.Point>("/HoloLens2/Gaze/HitPoint");
        Gaze = new geometry_msgs.Pose();
        HitPoint = new geometry_msgs.Point();
    }

    // Update is called once per frame
    void Update()
    {
        if (CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingEnabledAndValid)
        {
            PublishGazeDirectionOrigin();
            PublishGazeHitPoint();
        }
    }

    void PublishGazeDirectionOrigin()
    {
        GazeOrigin = CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
        GazeOrigin -= gameObject.transform.position;
        Gaze.position = Conversions.Vec3ToGeoMsgsPoint(GazeOrigin.Unity2Kinect());

        GazeDirection = CoreServices.InputSystem.EyeGazeProvider.GazeDirection;
        GazeDirection -= gameObject.transform.rotation.eulerAngles;
        Gaze.orientation = Conversions.Vec3ToGeoMsgsQuaternion(GazeDirection.Unity2Kinect());
        
        rosSocket.Publish(publicationIdPose, Gaze);

        //Debug.Log("Gaze is looking in direction: " + GazeDirection);
        Debug.Log("Gaze origin is: " + GazeOrigin);
    }

    void PublishGazeHitPoint()
    {
        HitPosition = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
        HitPosition -= gameObject.transform.position;
        HitPoint = Conversions.Vec3ToGeoMsgsPoint(HitPosition.Unity2Kinect());
        rosSocket.Publish(publicationIdPoint, HitPoint);

        //HitNormal = CoreServices.InputSystem.EyeGazeProvider.HitNormal;
        //Debug.Log("HitPosition: " + HitPosition);
        //Debug.Log("HitNormal: " + HitNormal);
        //Debug.Log("HitInfo: " + CoreServices.InputSystem.EyeGazeProvider.HitInfo);
    }
}
