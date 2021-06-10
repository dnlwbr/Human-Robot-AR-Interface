using Microsoft.MixedReality.Toolkit.UI;
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
        [SerializeField]
        private GameObject indicatorObject;
        private IProgressIndicator indicator;

        private GameObject RosSharp;
        private RosSocket rosSocket;

        private hri_msgs.RecordRequest requestMsg;

        private float lastClickTime = 0;
        private float debounceDelay = 0.005f;


        // Start is called before the first frame update
        void Start()
        {
            RosSharp = GameObject.Find("RosSharp");
            rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
            requestMsg = new hri_msgs.RecordRequest();
            indicator = indicatorObject.GetComponent<IProgressIndicator>();
        }

        public void StartRecord()
        {
            // Workaround due to bug that triggers OnSelected() twice
            if (Time.time - lastClickTime < debounceDelay)
            {
                return;
            }
            lastClickTime = Time.time;

            gameObject.GetComponent<BoundingBoxSubscriber>().enabled = false;
            FillMsg();
            ToggleIndicator(indicator);
            rosSocket.CallService<hri_msgs.RecordRequest, hri_msgs.RecordResponse>("/hri_robot_arm/Record", ServiceCallHandler, requestMsg);
        }

        private void ServiceCallHandler(hri_msgs.RecordResponse response)
        {
            Debug.Log("Completed: " + response.completed);
            ToggleIndicator(indicator);
            gameObject.GetComponent<BoundingBoxSubscriber>().enabled = true;
        }

        private void FillMsg()
        {
            requestMsg.bbox.center.position = Conversions.Vec3ToGeoMsgsPoint(transform.position.Unity2Ros());
            requestMsg.bbox.center.orientation = Conversions.QuaternionToGeoMsgsQuaternion(transform.rotation.Unity2Ros());

            requestMsg.bbox.size = Conversions.Vec3ToGeoMsgsVec3(transform.localScale.Unity2RosScale());

            requestMsg.header.frame_id = "unity_world";
            requestMsg.header.Update();
        }

        private async void ToggleIndicator(IProgressIndicator indicator)
        {
            await indicator.AwaitTransitionAsync();

            switch (indicator.State)
            {
                case ProgressIndicatorState.Closed:
                    await indicator.OpenAsync();
                    Debug.Log("Start");
                    break;

                case ProgressIndicatorState.Open:
                    await indicator.CloseAsync();
                    Debug.Log("Stop");
                    break;
            }
        }
    }
}
