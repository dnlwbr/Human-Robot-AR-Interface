using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using RosSharp;
using RosSharp.RosBridgeClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using RosSharp.RosBridgeClient.MessageTypes.Actionlib;
using hri_msgs = RosSharp.RosBridgeClient.MessageTypes.HriRobotArm;
using geometry_msgs = RosSharp.RosBridgeClient.MessageTypes.Geometry;


namespace HumanRobotInterface
{
    public class RobotArm : MonoBehaviour
    {
        private GameObject RosSharp;
        private RosSocket rosSocket;

        private RecordActionClient recordActionClient;
        private hri_msgs.RecordGoal goal;
        private string actionName = "/hri_robot_arm/Record";

        [SerializeField]
        private GameObject indicatorObjectOrbs;
        [SerializeField]
        private GameObject indicatorObjectBar;
        private IProgressIndicator indicatorOrbs;
        private IProgressIndicator indicatorBar;

        float barProgress = 0;
        bool isBussy = false;
        List<geometry_msgs.Point> gaze_points;
        private AudioSource audioSource;

        // Start is called before the first frame update
        void Start()
        {
            indicatorOrbs = indicatorObjectOrbs.GetComponent<IProgressIndicator>();
            indicatorBar = indicatorObjectBar.GetComponent<IProgressIndicator>();
            audioSource = GetComponent<AudioSource>();
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
            isBussy = false;
        }

        public void StartRecord()
        {
            // Set dynamic values before entering the class name
            FillMsg();

            // Check whether classen name has been provided
            string class_name = gameObject.GetComponent<MixedRealityKeyboard>().Text;
            if (class_name.Length > 0)
            {
                SetMsgClass(class_name);
                ToggleIndicator(indicatorBar);
                recordActionClient.action.action_goal.goal = goal;
                recordActionClient.SendGoal();
                Debug.Log("Goal has been sent with the class \"" + class_name + "\".");
            }
            else
            {
                Debug.Log("No class name has been provided.");
            }
        }

        public void StartGazeTracking()
        {
            if (isBussy)
                return;

            isBussy = true;

            // Set initial gaze point
            Vector3 hitPosition = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
            goal.gaze_point = Conversions.Vec3ToGeoMsgsPoint(hitPosition.Unity2Ros());

            // Track Gaze
            StartCoroutine(TrackGaze());
        }

        private IEnumerator TrackGaze()
        {
            gaze_points = new List<geometry_msgs.Point>();
            Vector3 hitPosition;
            //yield return new WaitForSeconds(3);
            audioSource.Play();
            while (audioSource.isPlaying)
            {
                hitPosition = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
                hitPosition = hitPosition.Unity2Ros();
                gaze_points.Add(Conversions.Vec3ToGeoMsgsPoint(hitPosition));
                yield return null;
            }

            // Open Keyboard
            gameObject.GetComponent<KeyboardHandler>().OpenKeyboard();
        }


        private void FillMsg()
        {
            if (gameObject.GetComponent<BoundingBoxSubscriber>())
            {
                // Point Cloud
                goal.segmented_cloud = gameObject.GetComponent<BoundingBoxSubscriber>().pointCloud;

                // Bounding Box
                goal.bbox.center.position = Conversions.Vec3ToGeoMsgsPoint(transform.position.Unity2Ros());
                goal.bbox.center.orientation = Conversions.QuaternionToGeoMsgsQuaternion(transform.rotation.Unity2Ros());

                goal.bbox.size = Conversions.Vec3ToGeoMsgsVec3(transform.localScale.Unity2RosScale());

                // Gaze
                Vector3 hitPosition = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
                goal.gaze_point = Conversions.Vec3ToGeoMsgsPoint(hitPosition.Unity2Ros());
            }
            else
            {
                // Gaze Points
                goal.gaze_points = gaze_points.ToArray();
            }

            // Header
            goal.header.frame_id = "unity_world";
            goal.header.Update();
        }

        private void SetMsgClass(string class_name)
        {
            // Class
            goal.class_name = class_name;
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
                if (barProgress == 1)   // if 100 %
                {
                    ToggleIndicator(indicatorBar);
                    // Empty percentage in order to start without showing the previous progress at the beginning of the next run
                    indicatorObjectBar.transform.Find("ProgressText").gameObject.GetComponent<TextMeshPro>().text = "";
                    ToggleIndicator(indicatorOrbs);
                }
            }
            if (recordActionClient.action.action_result.status.status == GoalStatus.SUCCEEDED ||
                recordActionClient.action.action_result.status.status == GoalStatus.ABORTED)
            {
                ToggleIndicator(indicatorOrbs);
                recordActionClient.action = new hri_msgs.RecordAction();
                if (gameObject.GetComponent<BoundingBoxSubscriber>())
                    gameObject.GetComponent<BoundingBoxSubscriber>().enabled = true;
                isBussy = false;
            }
        }
    }
}
