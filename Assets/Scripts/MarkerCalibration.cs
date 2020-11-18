using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking;
using RosSharp;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using nav_msgs = RosSharp.RosBridgeClient.MessageTypes.Nav;


namespace HumanRobotInterface
{
    public class MarkerCalibration : MonoBehaviour
    {
        public bool isCalibrated { get; private set; } = false;
        public float markerOffset { get; private set; }  // Distance from rotation centre to depth sensor

        [SerializeField]
        [Tooltip("Gameobject that visualizes the origin of the robot's odometry.")]
        private GameObject RobotOrigin = null;  // Origin of robot odometry in the unity world frame

        [SerializeField]
        [Tooltip("(Optional) Gameobject that visualizes trajectory.")]
        private GameObject trajectoryVisualization = null;

        private GameObject RosSharp;
        private RosSocket rosSocket;
        private string subscriptionId;
        private string topic = "/odom";
        private Vector3 robotCurrentPosition;
        private Quaternion robotCurrentRotation;
        private List<CalibrationElements> calibrationElements = new List<CalibrationElements>();
        private bool isInitialized;
        private bool isOriented;

        private float lastClickTime = 0;
        private float debounceDelay = 0.005f;

        private struct CalibrationElements
        {
            public readonly GameObject calibrationObject;
            public readonly Vector3 robotPosition;
            public readonly Quaternion robotRotation;
            public List<Vector3> robotOrigins { get; private set; }
            public List<float> markerOffsets { get; private set; }
            public CalibrationElements(GameObject go, Vector3 robotPos, Quaternion robotRot)
            {
                calibrationObject = go;
                robotPosition = robotPos;
                robotRotation = robotRot;
                robotOrigins = new List<Vector3>();
                markerOffsets = new List<float>();
            }
        }

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
        }

        void OnDisable()
        {
            // The CalibrationMarkerDirection game object must first be set to the correct position so that the value fits
            Transform directionMarkerTransfrom = GameObject.Find("CalibrationMarkerDirection").transform;
            Vector3 directionMarkerToCenter = trajectoryVisualization.transform.position - directionMarkerTransfrom.position;
            directionMarkerToCenter = Quaternion.Inverse(Quaternion.LookRotation(directionMarkerTransfrom.forward, Vector3.up)) * directionMarkerToCenter;
            Debug.Log("Marker to center: (x, y, z) = " + directionMarkerToCenter.ToString("F3"));

            isOriented = false;

            foreach (CalibrationElements calibrationElement in calibrationElements)
            {
                Destroy(calibrationElement.calibrationObject);
            }
            calibrationElements.Clear();

            if (!isCalibrated)
                gameObject.SetActive(false);

            trajectoryVisualization.SetActive(false);
            RobotOrigin.transform.Find("Visuals").gameObject.SetActive(false);
        }

        private void callback(nav_msgs.Odometry message)
        {
            robotCurrentPosition = Conversions.NavMsgsOdomPositionToVec3(message).Ros2Unity();
            robotCurrentRotation = Conversions.NavMsgsOdomOrientationToQuaternion(message).Ros2Unity();
            isInitialized = true;
        }

        public void VisualsIsSelected(GameObject caller)
        {
            // Workaround due to bug that triggers OnSelected() twice
            if (Time.time - lastClickTime < debounceDelay)
            {
                return;
            }
            lastClickTime = Time.time;

            if (!isInitialized)
            {
                Debug.Log("Marker is selected, but the robot position is not yet initialized.");
                return;
            }

            if (caller.name == "CalibrationMarkerDirection")
            {
                RobotOrigin.transform.forward = Quaternion.Inverse(robotCurrentRotation) * caller.transform.forward;
                isOriented = true;
                Vector3 robotCenter = caller.transform.position + Quaternion.LookRotation(caller.transform.forward, Vector3.up) * (new Vector3(-0.124f, 0.473f, -0.071f));
                RobotOrigin.transform.position = robotCenter - RobotOrigin.transform.rotation * robotCurrentPosition;
                isCalibrated = true;
                // Wenn erneut ausgewählt, nachdem Calibriert wurde, dann wird es auf default zurück gesetzt. Ist das erwünscht?
                Debug.Log("Direction marker selected.");
                VisualizeOrigin();
                VisualizeCentre();
            }
            else if (caller.name == "CalibrationMarkerKinect" && isOriented)
            {
                GameObject visuals = caller.transform.Find("Visuals").gameObject;
                GameObject calibrationObject = Instantiate(visuals, visuals.transform.position, visuals.transform.rotation);
                calibrationElements.Add(new CalibrationElements(calibrationObject, robotCurrentPosition, robotCurrentRotation));
                Debug.Log("Camera marker selected.");
                CalculateOrigins();
                Calibrate();
                VisualizeOrigin();
                VisualizeCentre();
            }
            else
            {
                // Prevent the original game objects from being deleted
                if (caller.name == "Visuals" || !isOriented)
                    return;

                var itemToRemove = calibrationElements.Single(element => ReferenceEquals(element.calibrationObject, caller));
                // Remove Elements in robotOrigins and markerOffsets from higher indices then itemToRemove
                int indexToRemove = calibrationElements.IndexOf(itemToRemove);
                for (int i = indexToRemove + 1; i < calibrationElements.Count; i++)
                {
                    calibrationElements[i].robotOrigins.RemoveAt(indexToRemove);
                    calibrationElements[i].markerOffsets.RemoveAt(indexToRemove);
                }
                calibrationElements.Remove(itemToRemove);
                //Destroy(caller);
                caller.GetComponent<HitBehaviorDestroyOnSelect>().enabled = true;
                caller.GetComponent<HitBehaviorDestroyOnSelect>().TargetSelected();  // Destroy object
                Calibrate();
                VisualizeOrigin();
                VisualizeCentre();
                Debug.Log("Marker destroyed.");
            }
        }

        private void CalculateOrigins()
        {
            if (calibrationElements.Count < 2)
                return;

            for (int i = 0; i < calibrationElements.Count - 1; i++)
            {
                // Get a "forward vector" for each rotation
                Vector3 robotInitialForward = calibrationElements[i].robotRotation * Vector3.forward;
                Vector3 robotTargetForward = calibrationElements.Last().robotRotation * Vector3.forward;

                // Get a numeric angle for each vector, on the X-Z plane (relative to world forward)
                float angleInitial = Mathf.Atan2(robotInitialForward.x, robotInitialForward.z) * Mathf.Rad2Deg;
                float angleTarget = Mathf.Atan2(robotTargetForward.x, robotTargetForward.z) * Mathf.Rad2Deg;

                // Get the total angle of rotation (in radians)
                //float theta = Quaternion.Angle(robotInitialRotation, robotTargetRotation) * Mathf.Deg2Rad; // unsigned
                float robotRotationAngle = Mathf.DeltaAngle(angleInitial, angleTarget);  // signed

                if (Mathf.Abs(robotRotationAngle) >= 10)  // check whether robot has moved less than 1cm in one direction
                {
                    //Where:
                    // - p0 = Initial marker position
                    // - p1 = Target marker position
                    // - planeNormal = the relative 'up' vector of the plane

                    // Marker position relative to the first element in order to isolate rotation
                    Vector3 relativeLastMarkerPosition = calibrationElements.Last().calibrationObject.transform.position;
                    relativeLastMarkerPosition -= RobotOrigin.transform.rotation * (calibrationElements.Last().robotPosition - calibrationElements[i].robotPosition);

                    //Find the vector between p0 and p1
                    Vector3 markerPositionDiff = relativeLastMarkerPosition - calibrationElements[i].calibrationObject.transform.position;

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
                    float sideA2 = dist2 / (2f * (1f - Mathf.Cos(robotRotationAngle * Mathf.Deg2Rad)));

                    //Find the height of the triangle using Pythagoras' theorem
                    float offset = Mathf.Sqrt(sideA2 - 0.25f * dist2);

                    //Find the midpoint between p0 and p1
                    Vector3 midpoint = (calibrationElements[i].calibrationObject.transform.position + relativeLastMarkerPosition) * 0.5f;

                    //Find the direction of the centre from the midpoint (use the plane's normal to calculate the vector perpendicular to p01)
                    Vector3 dir = Vector3.Cross(Mathf.Sign(robotRotationAngle) * Vector3.up, markerPositionDiff / Mathf.Sqrt(dist2)).normalized;

                    //Combine and offset to find the centre
                    Vector3 centre = midpoint + dir * offset;

                    // Calculate origin of robot odometry in the unity world frame
                    Vector3 origin = centre - RobotOrigin.transform.rotation * calibrationElements[i].robotPosition;

                    // Add origin and offset to list
                    calibrationElements.Last().robotOrigins.Add(origin);
                    calibrationElements.Last().markerOffsets.Add(offset);
                }
                else
                {
                    calibrationElements.Last().robotOrigins.Add(new Vector3(float.NaN, float.NaN, float.NaN));
                    calibrationElements.Last().markerOffsets.Add(float.NaN);
                }
            }
        }

        private void Calibrate()
        {
            Vector3 meanOrigin = CalculateMeanOrigin();
            float meanOffset = CalculateMeanOffset();

            if (meanOrigin.IsValidVector() && !float.IsNaN(meanOffset))  // returns false if n was 0
            {
                RobotOrigin.transform.position = meanOrigin;
                markerOffset = meanOffset;
                isCalibrated = true;
            }
            else
            {
                isCalibrated = false;
            }
        }

        private Vector3 CalculateMeanOrigin()
        {
            Vector3 meanOrigin = Vector3.zero;
            int n = 0;
            foreach (CalibrationElements calibrationElement in calibrationElements)
            {
                foreach (Vector3 origins in calibrationElement.robotOrigins)
                {
                    if (origins.IsValidVector())
                    {
                        meanOrigin += origins;
                        n++;
                    }
                }
            }
            meanOrigin /= n;
            return meanOrigin;
        }

        private float CalculateMeanOffset()
        {
            float meanOffset = 0;
            int n = 0;
            foreach (CalibrationElements calibrationElement in calibrationElements)
            {
                foreach (float offset in calibrationElement.markerOffsets)
                {
                    if (!float.IsNaN(offset))
                    {
                        meanOffset += offset;
                        n++;
                    }
                }
            }
            meanOffset /= n;
            return meanOffset;
        }

        private void VisualizeCentre()
        {
            if (isCalibrated)
            {
                if (trajectoryVisualization)
                {
                    Debug.Log("Show rotation centre");
                    trajectoryVisualization.SetActive(true);
                    trajectoryVisualization.transform.position = robotCurrentPosition.Robot2UnityFrame(RobotOrigin.transform);
                    trajectoryVisualization.transform.rotation = robotCurrentRotation.Robot2UnityFrame(RobotOrigin.transform);
                    Vector3 trajectoryScale = new Vector3(2 * markerOffset, trajectoryVisualization.transform.GetChild(0).localScale.y, 2 * markerOffset);
                    trajectoryVisualization.transform.GetChild(0).localScale = trajectoryScale;
                }
            }
            else
            {
                trajectoryVisualization.SetActive(false);
            }
        }

        private void VisualizeOrigin()
        {
            if (isCalibrated)
            {
                Debug.Log("Show origin of robot's odometry");
                RobotOrigin.transform.Find("Visuals").gameObject.SetActive(true);
            }
            else
            {
                RobotOrigin.transform.Find("Visuals").gameObject.SetActive(false);
            }
        }
    }
}
