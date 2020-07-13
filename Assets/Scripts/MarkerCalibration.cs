using Microsoft.MixedReality.Toolkit;
using RosSharp;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nav_msgs = RosSharp.RosBridgeClient.Messages.Navigation;

public class MarkerCalibration : MonoBehaviour
{
    public bool isCalibrated { get; private set; } = false;
    public float markerOffset { get; private set; }  // Distance from rotation centre to depth sensor
    public Vector3 robotOrigin { get; private set; }  // Origin of robot odometry in the unity world frame

    [SerializeField]
    [Tooltip("(Optional) Gameobject that visualizes trajectory.")]
    private GameObject trajectoryVisualization = null;

    private GameObject RosSharp;
    private RosSocket rosSocket;
    private string subscriptionId;
    private string topic = "/odom";
    private Vector3 robotInitialPosition;
    private Quaternion robotInitialRotation;
    private Vector3 robotTargetPosition;
    private Quaternion robotTargetRotation;
    private Vector3 robotCurrentPosition;
    private Quaternion robotCurrentRotation;
    private Vector3 markerInitialPosition;
    private Vector3 markerTargetPosition;
    private bool isInitialized;
    private GameObject visualsInitial;
    private GameObject visualsTarget;

    // Start is called before the first frame update
    void Start()
    {
        RosSharp = GameObject.Find("RosSharp");
        rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
        subscriptionId = rosSocket.Subscribe<nav_msgs.Odometry>(topic, callback, queue_length: 1);
    }

    void OnDisable() 
    {
        isInitialized = false;
        if (visualsInitial)
            Destroy(visualsInitial);
        if (visualsTarget)
            Destroy(visualsTarget);

        if (!isCalibrated)
            gameObject.SetActive(false);

        trajectoryVisualization.SetActive(false);
    }

    private void callback(nav_msgs.Odometry message)
    {
        robotCurrentPosition = Conversions.NavMsgsOdomPositionToVec3(message).Ros2Unity();
        robotCurrentRotation = Conversions.NavMsgsOdomOrientationToQuaternion(message).Ros2Unity();
        isInitialized = true;
    }

    public void VisualsIsSelected(GameObject caller)
    {
        if (!isInitialized)
        {
            Debug.Log("Marker is selected, but the robot position is not yet initialized.");
            return;
        }

        if (caller.name == "Visuals")
        {   
            if (visualsInitial && visualsTarget)
            {
                Destroy(visualsInitial);
                visualsInitial = null;
            }

            if (visualsTarget)
            {
                visualsInitial = visualsTarget;
                markerInitialPosition = visualsInitial.transform.position;
                robotInitialPosition = robotTargetPosition;
                robotInitialRotation = robotTargetRotation;
            }

            visualsTarget = Instantiate(gameObject.transform.Find("Visuals").gameObject, caller.transform.position, caller.transform.rotation);
            robotTargetPosition = robotCurrentPosition;
            robotTargetRotation = robotCurrentRotation;
            markerTargetPosition = visualsTarget.transform.position - (robotTargetPosition - robotInitialPosition);
            Debug.Log("Marker selected.");
        }
        else
        {
            Destroy(caller);
            Debug.Log("Marker destroyed.");
        }
        CalculateOffset();
    }

    private void CalculateOffset()
    {
        if (visualsInitial && visualsTarget)
        {
            //Where:
            // - p0 = Initial marker position
            // - p1 = Target marker position
            // - planeNormal = the relative 'up' vector of the plane

            // Get a "forward vector" for each rotation
            Vector3 forwardInitial = robotInitialRotation * Vector3.forward;
            Vector3 forwardTarget = robotTargetRotation * Vector3.forward;

            // Get a numeric angle for each vector, on the X-Z plane (relative to world forward)
            float angleInitial = Mathf.Atan2(forwardInitial.x, forwardInitial.z) * Mathf.Rad2Deg;
            float angleTarget = Mathf.Atan2(forwardTarget.x, forwardTarget.z) * Mathf.Rad2Deg;

            // Get the total angle of rotation (in radians)
            //float theta = Quaternion.Angle(robotInitialRotation, robotTargetRotation) * Mathf.Deg2Rad; // unsigned
            float theta = Mathf.DeltaAngle(angleInitial, angleTarget) * Mathf.Deg2Rad;  // signed

            //Find the vector between p0 and p1
            Vector3 markerPositionDiff = markerTargetPosition - markerInitialPosition;

            //Find the distance^2 between p0 and p1
            float dist2 = markerPositionDiff.sqrMagnitude;

            //Form a triangle with vertices [p0, p1, centre], where sides [p0-centre] and [p1-centre] are equal in length
            //Use the cosine rule to find the length^2 (A2) of said sides; 
            // cosine rule is better then tan because theta can be bigger than 180 degrees.
            // - c^2 = a^2 + b^2 - 2ab cos (C)
            //    - where c = dist, b = a;
            // - dist^2 = 2a^2 - 2a^2 cos (theta)
            // - dist^2 = 2a^2 (1 - cos (theta))
            // - dist^2 / (1 - cos (theta)) = 2a^2
            // - a^2 = dist^2 / 2(1 - cos (theta))
            float sideA2 = dist2 / (2f * (1f - Mathf.Cos(theta)));

            //Find the height of the triangle using Pythagoras' theorem
            markerOffset = Mathf.Sqrt(sideA2 - 0.25f * dist2);

            //Find the midpoint between p0 and p1
            Vector3 midpoint = (markerInitialPosition + markerTargetPosition) * 0.5f;

            //Find the direction of the centre from the midpoint (use the plane's normal to calculate the vector perpendicular to p01)
            Vector3 dir = Vector3.Cross(Mathf.Sign(theta) * Vector3.up, markerPositionDiff / Mathf.Sqrt(dist2)).normalized;

            //Combine and offset to find the centre
            Vector3 centre = midpoint + dir * markerOffset;

            // Calculate origin of robot odometry in the unity world frame
            robotOrigin = centre - robotInitialPosition;

            if (centre.IsValidVector())
            {
                isCalibrated = true;
                if (trajectoryVisualization && isCalibrated)
                {
                    Debug.Log("Show rotation centre");
                    trajectoryVisualization.SetActive(true);
                    trajectoryVisualization.transform.position = centre + (robotTargetPosition - robotInitialPosition);
                    Vector3 trajectoryScale = new Vector3(2 * markerOffset, trajectoryVisualization.transform.GetChild(0).localScale.y, 2 * markerOffset);
                    trajectoryVisualization.transform.GetChild(0).localScale = trajectoryScale;
                }
            }
            else
            {
                isCalibrated = false;
                trajectoryVisualization.SetActive(false);
            }
        }
        else
        {
            isCalibrated = false;
            trajectoryVisualization.SetActive(false);
        }
    }
}
