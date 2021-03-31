using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking;
using RosSharp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using nav_msgs = RosSharp.RosBridgeClient.MessageTypes.Nav;


namespace HumanRobotInterface
{
    public class MarkerCalibration : Subscriber<nav_msgs.Odometry>
    {
        public bool isCalibrated { get; private set; } = false;
        public Pose base_footprint2Kinect { get; private set; }

        [SerializeField]
        [Tooltip("Gameobject that visualizes the origin of the robot's odometry.")]
        private GameObject RobotOrigin = null;  // Origin of robot odometry in the unity world frame

        [SerializeField]
        [Tooltip("(Optional) Gameobject that visualizes trajectory.")]
        private GameObject footprintVisualization = null;

        private Vector3 robotCurrentPosition;
        private Quaternion robotCurrentRotation;
        private List<CalibrationElements> calibrationElements = new List<CalibrationElements>();
        private Transform base_footprint;
        private float markerOffset; // Distance from rotation centre to depth sensor
        private bool isInitialized;
        private bool isOriented;
        private bool isRobotPoseCalibrated;

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
        protected override void Start()
        {
            base.Start();
            base_footprint = gameObject.transform.root;
        }

        void OnEnable()
        {
            isInitialized = false;  // Must be placed here instead of in OnDisable, otherwise callback function will change the value again
        }

        void OnDisable()
        {
            isOriented = false;

            foreach (CalibrationElements calibrationElement in calibrationElements)
            {
                Destroy(calibrationElement.calibrationObject);
            }
            calibrationElements.Clear();

            if (!isRobotPoseCalibrated)
            {
                gameObject.SetActive(false);
                isCalibrated = false;
            }
            else
            {
                SetTFProperty();

                Vector3 directionMarker2base_footprint = -GameObject.Find("CalibrationMarkerDirection").transform.localPosition;
                Debug.Log(
                    "#######################################################" + System.Environment.NewLine +
                    "directionMarker -> base_footprint:" + System.Environment.NewLine +
                    "-------------------------------------------------------" + System.Environment.NewLine +
                    "Unity: (x, y, z) = " + directionMarker2base_footprint.ToString("F4") + System.Environment.NewLine +
                    "ROS:   (x, y, z) = " + directionMarker2base_footprint.Unity2Ros().ToString("F4") + System.Environment.NewLine +
                    "#######################################################");
            }
                
            footprintVisualization.SetActive(false);
            RobotOrigin.transform.Find("Visuals").gameObject.SetActive(false);
        }

        protected override void callback(nav_msgs.Odometry message)
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
                Debug.LogWarning("Marker is selected, but the robot position is not yet initialized.");
                return;
            }

            if (caller.name == "CalibrationMarkerDirection")
            // If selected again after calibration, it will be reset to default. Is this desired?
            {
                Vector3 directionMarkerForward = Vector3.ProjectOnPlane(caller.transform.forward, Vector3.up);

                RobotOrigin.transform.rotation = Quaternion.Inverse(robotCurrentRotation) * Quaternion.LookRotation(directionMarkerForward, Vector3.up);
                isOriented = true;

                Vector3 directionMarker2base_footprint = new Vector3(-0.0362f, 0, 0); // --> mid of marker
                directionMarker2base_footprint += new Vector3(0, 0, -caller.GetComponent<QRCodeTarget>().boardThickness/1000); // --> back of marker
                directionMarker2base_footprint += new Vector3(0, -0.0862f, 0); // --> bottom of marker
                directionMarker2base_footprint += new Vector3(0, 0, -0.04f); // --> spine (spine is 8x8cm)
                directionMarker2base_footprint += new Vector3(0, -0.004f, 0); // --> mounting of spine (4mm thick)
                //directionMarker2base_footprint += new Vector3(0, -0.0021f, 0); // --> mid of torso plate (torso plate is 4.2mm thick)
                //directionMarker2base_footprint += new Vector3(0, -0.589f, 0.085f); // --> base_footprint (Inaccurate: torso_plate to base_footprint tf matches urdf model but not the real robot)
                directionMarker2base_footprint += new Vector3(0, -0.586f, 0.075f); // --> base_footprint  (measured + head_base_frame used for forward direction)
                base_footprint.position = caller.transform.position + Quaternion.LookRotation(directionMarkerForward, Vector3.up) * directionMarker2base_footprint;
                RobotOrigin.transform.position = base_footprint.position - RobotOrigin.transform.rotation * robotCurrentPosition;
                base_footprint.rotation = Quaternion.LookRotation(directionMarkerForward, Vector3.up);  // Has to be in the end, because it changes caller.transform
                isRobotPoseCalibrated = true;

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

                // check whether positions are too similar or opposite to each other
                if (15 <= Mathf.Abs(robotRotationAngle)  && Mathf.Abs(robotRotationAngle) <= 165)
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

                    // Set y coordinate of origin to the value obtained from the directionMarker (directionMarker's height above base_footprint)
                    origin.y = RobotOrigin.transform.position.y;

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
                base_footprint.position = robotCurrentPosition.Robot2UnityPosition(RobotOrigin.transform);
                base_footprint.rotation = robotCurrentRotation.Robot2UnityTwist(RobotOrigin.transform);
                isRobotPoseCalibrated = true;
            }
            else
            {
                isRobotPoseCalibrated = false;
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
            if (isRobotPoseCalibrated)
            {
                if (footprintVisualization)
                {
                    Debug.Log("Show rotation centre");
                    footprintVisualization.SetActive(true);
                    Vector3 trajectoryScale = new Vector3(2 * markerOffset, footprintVisualization.transform.GetChild(0).localScale.y, 2 * markerOffset);
                    footprintVisualization.transform.GetChild(0).localScale = trajectoryScale;
                }
            }
            else
            {
                footprintVisualization.SetActive(false);
            }
        }

        private void VisualizeOrigin()
        {
            if (isRobotPoseCalibrated)
            {
                Debug.Log("Show origin of robot's odometry");
                RobotOrigin.transform.Find("Visuals").gameObject.SetActive(true);
            }
            else
            {
                RobotOrigin.transform.Find("Visuals").gameObject.SetActive(false);
            }
        }

        private void SetTFProperty()
        {
            if (isRobotPoseCalibrated)
            {
                Transform camera_base = gameObject.transform.Find("Visuals").Find("Depth");
                base_footprint2Kinect = new Pose(
                    base_footprint.InverseTransformPoint(camera_base.position),
                    Quaternion.Inverse(base_footprint.rotation) * camera_base.rotation);
                isCalibrated = true;
            }
            else
            {
                isCalibrated = false;
            }
        }
    }
}
