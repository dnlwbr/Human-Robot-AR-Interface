using RosSharp;
using RosSharp.RosBridgeClient;
using std_srvs = RosSharp.RosBridgeClient.MessageTypes.Std;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using std_msgs = RosSharp.RosBridgeClient.MessageTypes.Std;
using vision_msgs = RosSharp.RosBridgeClient.MessageTypes.Vision;
using hri_msgs = RosSharp.RosBridgeClient.MessageTypes.HriRobotArm;


namespace HumanRobotInterface
{
    public class RobotArm : MonoBehaviour
    {
        private GameObject RosSharp;
        private RosSocket rosSocket;

        private hri_msgs.RecordRequest requestMsg;


        // Start is called before the first frame update
        void Start()
        {
            RosSharp = GameObject.Find("RosSharp");
            rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
            requestMsg = new hri_msgs.RecordRequest();
        }

        public void StartRecord()
        {
            FillMsg();
            rosSocket.CallService<hri_msgs.RecordRequest, hri_msgs.RecordResponse>("/hri_robot_arm/Record", ServiceCallHandler, requestMsg);
        }

        private void ServiceCallHandler(hri_msgs.RecordResponse response)
        {
            Debug.Log("Completed: " + response.completed);
        }

        private void FillMsg()
        {
            requestMsg.bbox.center.position = Conversions.Vec3ToGeoMsgsPoint(transform.position.Unity2Ros());
            requestMsg.bbox.center.orientation = Conversions.QuaternionToGeoMsgsQuaternion(transform.rotation.Unity2Ros());

            requestMsg.bbox.size = Conversions.Vec3ToGeoMsgsVec3(transform.localScale.Unity2RosScale());

            requestMsg.header.frame_id = "unity_world";
            requestMsg.header.Update();
        }
    }
}
