using RosSharp;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nav_msgs = RosSharp.RosBridgeClient.MessageTypes.Nav;

namespace HumanRobotInterface
{
    public class MarkerController : MonoBehaviour
    {
        private GameObject RosSharp;
        private RosSocket rosSocket;
        private string subscriptionId;
        private string topic = "/odom";
        private Vector3 robotInitialPosition;
        private Quaternion robotInitialRotation;
        private Vector3 robotCurrentPosition;
        private Quaternion robotCurrentRotation;
        private Vector3 markerInitialPosition;
        private Quaternion markerInitialRotation;
        private Vector3 robotOriginPosition;
        private Quaternion robotOriginRotation;
        private Vector3 markerOffset;
        private bool isInitialized;

        // Start is called before the first frame update
        void Start()
        {
            RosSharp = GameObject.Find("RosSharp");
            rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
            subscriptionId = rosSocket.Subscribe<nav_msgs.Odometry>(topic, callback, queue_length: 1);
        }

        void OnEnable()
        {
            isInitialized = false;  // Must be placed here instead of in OnDisable, otherwise callback function will change the value again
            markerInitialPosition = gameObject.transform.position;
            markerInitialRotation = gameObject.transform.rotation;
            // Bug in callback? Access to RobotOrigin.transform (in callback) throws error
            robotOriginPosition = GameObject.Find("RobotOrigin").transform.position;
            robotOriginRotation = GameObject.Find("RobotOrigin").transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            if (isInitialized)
                ProcessMessage();
        }

        private void callback(nav_msgs.Odometry message)
        {
            robotCurrentPosition = Conversions.NavMsgsOdomPositionToVec3(message).Ros2Unity();
            robotCurrentPosition = robotCurrentPosition.Robot2UnityFrame(robotOriginPosition, robotOriginRotation);
            robotCurrentRotation = Conversions.NavMsgsOdomOrientationToQuaternion(message).Ros2Unity();
            robotCurrentRotation = robotCurrentRotation.Robot2UnityFrame(robotOriginRotation);

            if (!isInitialized)
            {
                robotInitialPosition = robotCurrentPosition;
                robotInitialRotation = robotCurrentRotation;
                markerOffset = markerInitialPosition - robotInitialPosition;
                isInitialized = true;
                Debug.Log("Bridge: Initialized.");
            }
        }

        private void ProcessMessage()
        {
            //Rotation
            Quaternion diff = robotCurrentRotation * Quaternion.Inverse(robotInitialRotation);  // diff * q1 = q2  -->  diff = q2 * Inverse(q1)
            gameObject.transform.rotation = diff * markerInitialRotation;

            // Position
            gameObject.transform.position = diff * markerOffset + robotCurrentPosition;

            // Alternativ:
            //gameObject.transform.position = markerInitialPosition + robotCurrentPosition - robotInitialPosition;
            //float theta = Quaternion.Angle(robotInitialRotation, robotCurrentRotation) * Mathf.Deg2Rad;
            //gameObject.transform.RotateAround(robotCurrentPosition, Vector3.up, theta);
        }
    }
}
