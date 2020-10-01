using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HumanRobotInterface
{
    public enum ManipulationType
    {
        Move,
        Scale
    }

    public enum Axis
    {
        xAxis,
        yAxis,
        zAxis
    }

    public class ManipulateObjIfEyeGaze : MonoBehaviour,
        IMixedRealitySourceStateHandler,
        IMixedRealityPointerHandler,
        IMixedRealityHandJointHandler
    {
        private IMixedRealityEyeGazeProvider EyeTrackingProvider => eyeTrackingProvider ?? (eyeTrackingProvider = CoreServices.InputSystem?.EyeGazeProvider);
        private IMixedRealityEyeGazeProvider eyeTrackingProvider = null;

        #region Serialized variables
        [Header("Manipultation")]
        public ManipulationType manipulationType = ManipulationType.Move;

        [Header("Hands")]
        [Tooltip("To control whether the hand motion is used 1:1 to move a target or to use different gains to allow for smaller hand motions.")]
        [SerializeField]
        private float handmapping = 1;

        [Header("Transitioning")]
        [Tooltip("Transparency of the target itself while dragging is active.")]
        [SerializeField]
        [Range(0, 1)]
        private float transparency_inTransition = 130 / 255f;

        [Header("Audio Feedback")]
        [SerializeField]
        private AudioClip audio_OnDragStart = null;

        [SerializeField]
        private AudioClip audio_OnDragStop = null;

        [SerializeField]
        private UnityEvent OnDragStart = null;

        [SerializeField]
        private UnityEvent OnDrop = null;

        [Header("Constrained Movement")]
        [SerializeField]
        private bool freezeX = false;

        [SerializeField]
        private bool freezeY = false;

        [SerializeField]
        private bool freezeZ = false;

        public Vector2 LocalMinMax_X = new Vector2(float.NegativeInfinity, float.PositiveInfinity);
        public Vector2 LocalMinMax_Y = new Vector2(float.NegativeInfinity, float.PositiveInfinity);
        public Vector2 LocalMinMax_Z = new Vector2(float.NegativeInfinity, float.PositiveInfinity);

        [Tooltip("All axes are scaled uniformly according to master axis.")]
        public bool scaleUniformly = false;

        [HideInInspector]
        public Axis masterAxis = Axis.zAxis;
        #endregion

        #region Private variables
        private float originalTransparency = -1;
        private bool originalUseGravity = false;
        private float originalDrag = 1;

        private static bool isManipulatingUsing_Hands = false;
        private Vector3 handPos_absolute;
        private Vector3 handPos_relative;
        private static bool handIsPinching = false;
        private Handedness currEngagedHand = Handedness.None;
        private bool objIsGrabbed = false;

        private bool invertZ = true;
        private int invZ { get { return ((invertZ) ? -1 : 1); } }

        private int constrX { get { return ((freezeX) ? 0 : 1); } }
        private int constrY { get { return ((freezeY) ? 0 : 1); } }
        private int constrZ { get { return ((freezeZ) ? 0 : 1); } }
        private Vector3 constrMoveCtrl { get { return new Vector3(constrX, constrY, constrZ); } }
        #endregion

        #region Hand input handler
        void IMixedRealityHandJointHandler.OnHandJointsUpdated(InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> eventData)
        {
            MixedRealityPose pose;
            eventData.InputData.TryGetValue(TrackedHandJoint.Palm, out pose);

            if ((pose != null) && (eventData.Handedness == currEngagedHand) && isManipulatingUsing_Hands)
            {
                if (handPos_absolute == Vector3.zero)
                {
                    handPos_absolute = pose.Position;
                }
                else
                {
                    Vector3 oldHandPos = handPos_absolute;
                    handPos_relative = new Vector3(oldHandPos.x - pose.Position.x, oldHandPos.y - pose.Position.y, oldHandPos.z - pose.Position.z);
                    handPos_absolute = pose.Position;

                    if (handIsPinching)
                    {
                        RelativeMoveUpdate(handPos_relative);
                    }
                }
            }
        }

        void IMixedRealitySourceStateHandler.OnSourceDetected(SourceStateEventData eventData) { }

        void IMixedRealitySourceStateHandler.OnSourceLost(SourceStateEventData eventData)
        {
            if (IsActiveHand(eventData.InputSource.SourceName))
            {
                HandDrag_Stop();
            }
        }

        void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
        {
            if (IsActiveHand(eventData.InputSource.SourceName))
            {
                HandDrag_Stop();
            }
        }

        void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (SetActiveHand(eventData.InputSource.SourceName))
            {
                HandDrag_Start();
            }
        }

        void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData) { }
        #endregion


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private bool SetActiveHand(string sourcename)
        {
            if (currEngagedHand == Handedness.None)
            {
                if ((sourcename == "Right Hand") || (sourcename == "Mixed Reality Controller Right"))
                {
                    currEngagedHand = Handedness.Right;
                }
                else if ((sourcename == "Left Hand") || (sourcename == "Mixed Reality Controller Left"))
                {
                    currEngagedHand = Handedness.Left;
                }

                if (currEngagedHand != Handedness.None)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsActiveHand(string sourcename)
        {

            if (((currEngagedHand == Handedness.Right) && ((sourcename == "Right Hand") || (sourcename == "Mixed Reality Controller Right"))) ||
                ((currEngagedHand == Handedness.Left) && ((sourcename == "Left Hand") || (sourcename == "Mixed Reality Controller Left"))))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Start moving the target using your hands.
        /// </summary>
        private void HandDrag_Start()
        {
            if (!isManipulatingUsing_Hands)
            {
                isManipulatingUsing_Hands = true;
                handIsPinching = true;
                handPos_relative = Vector3.zero;
                handPos_absolute = Vector3.zero;
                DragAndDrop_Start();
                CoreServices.InputSystem.PushModalInputHandler(gameObject);
            }
        }

        /// <summary>
        /// Finish moving the target using your hands.
        /// </summary>
        private void HandDrag_Stop()
        {
            if (isManipulatingUsing_Hands)
            {
                isManipulatingUsing_Hands = false;
                handIsPinching = false;
                handPos_relative = Vector3.zero;
                DragAndDrop_Finish();
                CoreServices.InputSystem.PopModalInputHandler();
                currEngagedHand = Handedness.None;
            }
        }


        /// <summary>
        /// Begin with the selection and movement of the focused target.
        /// </summary>
        public void DragAndDrop_Start()
        {
            if ((EyeTrackingProvider?.GazeTarget == gameObject) && (!objIsGrabbed))
            {
                if (AudioFeedbackPlayer.Instance != null)
                {
                    AudioFeedbackPlayer.Instance.PlaySound(audio_OnDragStart);
                }

                objIsGrabbed = true;
                if (transparency_inTransition < 1)
                {
                    EyeTrackingDemoUtils.GameObject_ChangeTransparency(gameObject, transparency_inTransition, ref originalTransparency);
                }

                Rigidbody rbody = GetComponent<Rigidbody>();
                if (rbody != null)
                {
                    originalUseGravity = rbody.useGravity;
                    originalDrag = rbody.drag;

                    rbody.useGravity = false;
                    rbody.drag = float.PositiveInfinity;
                }

                OnDragStart.Invoke();
            }
        }

        /// <summary>
        /// Finalize placing the currently selected target.
        /// </summary>
        public void DragAndDrop_Finish()
        {
            if (objIsGrabbed)
            {
                if (AudioFeedbackPlayer.Instance != null)
                {
                    AudioFeedbackPlayer.Instance.PlaySound(audio_OnDragStop);
                }

                objIsGrabbed = false;

                if (transparency_inTransition < 1)
                {
                    EyeTrackingDemoUtils.GameObject_ChangeTransparency(gameObject, originalTransparency);
                }
                Rigidbody rbody = GetComponent<Rigidbody>();
                if (rbody != null)
                {
                    rbody.useGravity = originalUseGravity;
                    rbody.drag = originalDrag;
                }

                OnDrop.Invoke();
            }
        }

        /// <summary>
        /// Move the target using relative input values.
        /// </summary>
        private void RelativeMoveUpdate(Vector3 relativeMovement)
        {
            MoveTargetBy(relativeMovement);
        }

        public void MoveTargetBy(Vector3 delta)
        {
            // Check that this game object is currently selected
            if (objIsGrabbed)
            {
                // Continuous manual target movement
                if (manipulationType == ManipulationType.Scale)
                {
                    if (scaleUniformly == true)
                    {
                        // Rotate gesture based on camera rotation
                        Vector3 d;
                        // Rotate around the y axis
                        delta = Quaternion.AngleAxis(CameraCache.Main.transform.rotation.eulerAngles.y, Vector3.down) * delta;
                        // Rotate around the x axis
                        delta = Quaternion.AngleAxis(CameraCache.Main.transform.rotation.eulerAngles.x, Vector3.left) * delta;
                        // Rotate around the z axis
                        delta = Quaternion.AngleAxis(CameraCache.Main.transform.rotation.eulerAngles.z, Vector3.back) * delta * invZ;

                        if (masterAxis == Axis.xAxis)
                        {
                            d = new Vector3(-delta.x * constrX, -delta.x * constrY, -delta.x * constrZ);
                        }
                        else if (masterAxis == Axis.yAxis)
                        {
                            d = new Vector3(-delta.y * constrX, -delta.y * constrY, -delta.y * constrZ);
                        }
                        else
                        {
                            d = new Vector3(-delta.z * constrX, -delta.z * constrY, -delta.z * constrZ);
                        }
                        Vector3 oldScale = gameObject.transform.localScale;
                        gameObject.transform.localScale = oldScale + d * handmapping;
                    }
                    else
                    {
                        Vector3 d = new Vector3(-delta.x * constrX, -delta.y * constrY, -delta.z * constrZ);
                        Vector3 oldScale = gameObject.transform.localScale;
                        gameObject.transform.localScale = oldScale + d * handmapping;
                    }
                }
                else
                {
                    Vector3 d = new Vector3(-delta.x * constrX, -delta.y * constrY, -delta.z * constrZ);
                    Vector3 oldPos = gameObject.transform.position;
                    gameObject.transform.position = oldPos + d * handmapping;
                }

                ConstrainMovement();
            }
        }

        public void ConstrainMovement()
        {
            if (manipulationType == ManipulationType.Scale)
            {
                Vector3 locScale = gameObject.transform.localScale;
                float rx, ry, rz;
                rx = Mathf.Clamp(locScale.x, LocalMinMax_X.x, LocalMinMax_X.y);
                ry = Mathf.Clamp(locScale.y, LocalMinMax_Y.x, LocalMinMax_Y.y);
                rz = Mathf.Clamp(locScale.z, LocalMinMax_Z.x, LocalMinMax_Z.y);

                gameObject.transform.localScale = new Vector3(rx, ry, rz);
            }
            else
            {
                Vector3 locPos = gameObject.transform.localPosition;
                float rx, ry, rz;
                rx = Mathf.Clamp(locPos.x, LocalMinMax_X.x, LocalMinMax_X.y);
                ry = Mathf.Clamp(locPos.y, LocalMinMax_Y.x, LocalMinMax_Y.y);
                rz = Mathf.Clamp(locPos.z, LocalMinMax_Z.x, LocalMinMax_Z.y);

                gameObject.transform.localPosition = new Vector3(rx, ry, rz);
            }
        }

        void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData) { }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(ManipulateObjIfEyeGaze))]
    public class ManipulateObjIfEyeGazeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Show default inspector property editor
            DrawDefaultInspector();

            var myScript = (ManipulateObjIfEyeGaze)target;

            if (myScript.scaleUniformly)
            {
                myScript.masterAxis = (Axis) EditorGUILayout.EnumPopup("Master Axis", myScript.masterAxis);
            }
        }
    }
#endif
}