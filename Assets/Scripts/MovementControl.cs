using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using geometry_msgs = RosSharp.RosBridgeClient.Messages.Geometry;

public class MovementControl : MonoBehaviour
{
    [Header("Direction and Speed")]
    [SerializeField]
    [Tooltip("Linear velocity")]
    [Range(-1.0f, 1.0f)]
    private float linear = 0.0f;

    [SerializeField]
    [Tooltip("Angular velocity")]
    [Range(-1.0f, 1.0f)]
    private float angular = 0.0f;

    private GameObject RosSharp;
    private RosSocket rosSocket;
    private string publicationId;
    private geometry_msgs.Twist twist;
    private int frameSkip = 4;

    // Start is called before the first frame update
    void Start()
    {
        RosSharp = GameObject.Find("RosSharp");
        rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
        publicationId = rosSocket.Advertise<geometry_msgs.Twist>("/cmd_vel");
        twist = new geometry_msgs.Twist();

        // Not necessary, since the default constructor initializes with zeros.
        //twist.linear = Conversions.Vec3ToGeoMsgsVec3(Vector3.zero);
        //twist.angular = Conversions.Vec3ToGeoMsgsVec3(Quaternion.identity.eulerAngles);

        twist.linear.x = linear;
        twist.angular.z = angular;
    }

    void FixedUpdate()
    {
        if (Time.frameCount % frameSkip == 0)   // slow down publishing so that the robot can follow
            rosSocket.Publish(publicationId, twist);
    }
}
