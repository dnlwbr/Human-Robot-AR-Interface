using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HumanRobotInterface
{
    public class BaseReadjust : MonoBehaviour
    {
        [SerializeField]
        private MarkerCalibration calibrationMarker;

        [SerializeField]
        [Tooltip("(Optional) Gameobject that visualizes trajectory.")]
        private GameObject footprintVisualization = null;

        [SerializeField]
        [Tooltip("Button that triggers activation to untoggled it if necessary.")]
        private Interactable interactable;

        private Vector3 translation;
        private Quaternion rotation;


        void OnEnable()
        {
            if (calibrationMarker.isCalibrated)
            {
                // Get TF's translation in the frame of calibrationMarker
                translation = calibrationMarker.transform.InverseTransformVector(transform.TransformVector(-calibrationMarker.base_footprint2Kinect.position));

                // Get rotation from marker to base_footprint
                rotation = transform.rotation * Quaternion.Inverse(calibrationMarker.transform.rotation);

                if (footprintVisualization)
                {
                    footprintVisualization.SetActive(true);
                }
            }
            else
            {
                //interactable.SetState(InteractableStates.InteractableStateEnum.Toggled, false);
            }
        }

        void OnDisable()
        {
            if (footprintVisualization)
            {
                footprintVisualization.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {
            /*
            transform.position = calibrationMarker.transform.TransformPoint(translation);
            transform.rotation = rotation * calibrationMarker.transform.rotation;
            calibrationMarker.transform.localPosition = calibrationMarker.base_footprint2Kinect.position;
            */
        }
    }
}
