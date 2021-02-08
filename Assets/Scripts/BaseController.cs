using RosSharp;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nav_msgs = RosSharp.RosBridgeClient.MessageTypes.Nav;

namespace HumanRobotInterface
{
    public class BaseController : Subscriber<nav_msgs.Odometry>
    {
        [SerializeField]
        private MarkerCalibration calibrationMarker;

        private Vector3 robotCurrentPosition;
        private Quaternion robotCurrentRotation;
        private Vector3 robotOriginPosition;
        private Quaternion robotOriginRotation;
        private bool isInitialized;


        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
        }

        void OnEnable()
        {
            isInitialized = false;  // Must be placed here instead of in OnDisable, otherwise callback function will change the value again

            // Bug in callback? Access to RobotOrigin.transform (in callback) throws error
            robotOriginPosition = GameObject.Find("RobotOrigin").transform.position;
            robotOriginRotation = GameObject.Find("RobotOrigin").transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            if (isInitialized && calibrationMarker.isCalibrated)
                ProcessMessage();
        }

        protected override void callback(nav_msgs.Odometry message)
        {
            robotCurrentPosition = Conversions.NavMsgsOdomPositionToVec3(message).Ros2Unity();
            robotCurrentPosition = robotCurrentPosition.Robot2UnityPosition(robotOriginPosition, robotOriginRotation);
            robotCurrentRotation = Conversions.NavMsgsOdomOrientationToQuaternion(message).Ros2Unity();
            robotCurrentRotation = robotCurrentRotation.Robot2UnityTwist(robotOriginRotation);

            if (!isInitialized)
            {
                isInitialized = true;
                Debug.Log("Bridge: Initialized.");
            }
        }

        private void ProcessMessage()
        {
            gameObject.transform.position = robotCurrentPosition;
            gameObject.transform.rotation = robotCurrentRotation;
        }
    }
}
