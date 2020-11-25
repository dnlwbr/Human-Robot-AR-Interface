using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using geometry_msgs = RosSharp.RosBridgeClient.MessageTypes.Geometry;


namespace HumanRobotInterface
{
    public class MovementControl : Publisher<geometry_msgs.Twist>
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

        private geometry_msgs.Twist twist;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            twist = new geometry_msgs.Twist();

            // Not necessary, since the default constructor initializes with zeros.
            //twist.linear = Conversions.Vec3ToGeoMsgsVec3(Vector3.zero);
            //twist.angular = Conversions.Vec3ToGeoMsgsVec3(Quaternion.identity.eulerAngles);

            twist.linear.x = linear;
            twist.angular.z = angular;
        }

        void FixedUpdate()
        {
            Publish(twist, 30);
        }
    }
}
