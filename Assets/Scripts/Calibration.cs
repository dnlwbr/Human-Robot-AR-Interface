using RosSharp;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nav_msgs = RosSharp.RosBridgeClient.Messages.Navigation;

public class Calibration : MonoBehaviour
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
    private Vector3 markerOffset;
    private bool isInitialized;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Search RS Object");
        RosSharp = GameObject.Find("RosSharp");
        Debug.Log("Found RS Object");
        rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
        Debug.Log("Got Component");
        subscriptionId = rosSocket.Subscribe<nav_msgs.Odometry>(topic, callback, queue_length: 1);
        Debug.Log("Subscribed");
    }

    void OnEnable()
    {
        markerInitialPosition = gameObject.transform.position;
        markerInitialRotation = gameObject.transform.rotation;
    }

    void OnDisable() 
    {
        isInitialized = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isInitialized)
            ProcessMessage();
    }

    private void callback(nav_msgs.Odometry message)
    {
        robotCurrentPosition = GetPosition(message).Ros2Unity();
        robotCurrentRotation = GetRotation(message).Ros2Unity();

        if (!isInitialized)
        {
            robotInitialPosition = robotCurrentPosition;
            robotInitialRotation = robotCurrentRotation;
            markerOffset = markerInitialPosition - robotInitialPosition;
            isInitialized = true;
        }
    }

    private void ProcessMessage()
    {
        if (robotCurrentPosition.z != 0)    //
            GameObject.Find("TestCube").transform.Rotate(0, 3, 0);

        //Rotation
        Quaternion diff = robotCurrentRotation * Quaternion.Inverse(robotInitialRotation);  // diff * q1 = q2  -->  diff = q2 * Inverse(q1)
        gameObject.transform.rotation = diff * markerInitialRotation;

        // Position
        gameObject.transform.position = diff * markerOffset + robotCurrentPosition;
    }

    private Vector3 GetPosition(nav_msgs.Odometry message)
    {
        return new Vector3(
            message.pose.pose.position.x,
            message.pose.pose.position.y,
            message.pose.pose.position.z);
    }

    private Quaternion GetRotation(nav_msgs.Odometry message)
    {
        return new Quaternion(
            message.pose.pose.orientation.x,
            message.pose.pose.orientation.y,
            message.pose.pose.orientation.z,
            message.pose.pose.orientation.w);
    }
}
