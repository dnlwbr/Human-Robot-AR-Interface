using Microsoft.MixedReality.Toolkit.UI;
using RosSharp;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using RosSharp.RosBridgeClient.MessageTypes.Actionlib;
using hri_msgs = RosSharp.RosBridgeClient.MessageTypes.HriRobotArm;


namespace HumanRobotInterface
{
    public class RobotArm : MonoBehaviour
    {
        private GameObject RosSharp;
        private RosSocket rosSocket;

        private RecordActionClient recordActionClient;
        private hri_msgs.RecordGoal goal;
        private string actionName = "/hri_robot_arm/Record";

        private float lastClickTime = 0;
        private float debounceDelay = 0.005f;

        [SerializeField]
        private GameObject indicatorObjectOrbs;
        [SerializeField]
        private GameObject indicatorObjectBar;
        private IProgressIndicator indicatorOrbs;
        private IProgressIndicator indicatorBar;

        float barProgress = 0;


        // Start is called before the first frame update
        void Start()
        {
            indicatorOrbs = indicatorObjectOrbs.GetComponent<IProgressIndicator>();
            indicatorBar = indicatorObjectBar.GetComponent<IProgressIndicator>();
        }

        void OnEnable()
        {
            RosSharp = GameObject.Find("RosSharp");
            rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
            recordActionClient = new RecordActionClient(actionName, rosSocket);
            recordActionClient.Initialize();
            goal = new hri_msgs.RecordGoal();
        }

        void OnDisable()
        {
            recordActionClient.Terminate();
        }

        public void StartRecord()
        {
            // Workaround due to bug that triggers OnSelected() twice
            if (Time.time - lastClickTime < debounceDelay)
            {
                return;
            }
            lastClickTime = Time.time;

            // gameObject.GetComponent<BoundingBoxSubscriber>().enabled = false;  // Already disabled when keyboard is opened
            FillMsg();
            ToggleIndicator(indicatorBar);
            recordActionClient.action.action_goal.goal = goal;
            recordActionClient.SendGoal();
        }

        private void FillMsg()
        {
            goal.bbox.center.position = Conversions.Vec3ToGeoMsgsPoint(transform.position.Unity2Ros());
            goal.bbox.center.orientation = Conversions.QuaternionToGeoMsgsQuaternion(transform.rotation.Unity2Ros());

            goal.bbox.size = Conversions.Vec3ToGeoMsgsVec3(transform.localScale.Unity2RosScale());

            goal.header.frame_id = "unity_world";
            goal.header.Update();
        }

        private async void ToggleIndicator(IProgressIndicator indicator)
        {
            await indicator.AwaitTransitionAsync();

            switch (indicator.State)
            {
                case ProgressIndicatorState.Closed:
                    await indicator.OpenAsync();
                    break;

                case ProgressIndicatorState.Open:
                    await indicator.CloseAsync();
                    break;
            }
        }

        void Update()
        {
            if (indicatorBar.State == ProgressIndicatorState.Open)
            {
                barProgress = (float) recordActionClient.action.action_feedback.feedback.progress / 100;
                indicatorBar.Progress = barProgress;
                if (barProgress == 1)
                {
                    ToggleIndicator(indicatorBar);
                    ToggleIndicator(indicatorOrbs);
                }
            }
            if (recordActionClient.action.action_result.status.status == GoalStatus.SUCCEEDED ||
                recordActionClient.action.action_result.status.status == GoalStatus.ABORTED)
            {
                ToggleIndicator(indicatorOrbs);
                recordActionClient.action = new hri_msgs.RecordAction();
                gameObject.GetComponent<BoundingBoxSubscriber>().enabled = true;
            }
        }
    }
}
