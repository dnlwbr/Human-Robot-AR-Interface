using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;


namespace HumanRobotInterface
{
    public class SetObjectAtEyeGazeHitPoint : MonoBehaviour
    {
        [Tooltip("Set position only if Visuals are not already active.")]
        [SerializeField]
        private bool onlyIfVisualsAreInactive = true;

        private GameObject Visuals;

        // Start is called before the first frame update
        void Start()
        {
            Visuals = gameObject.transform.Find("Visuals").gameObject;
        }

        public void SetPosition()
        {
            if (onlyIfVisualsAreInactive && Visuals.activeSelf)
                return;

            var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
            if (eyeGazeProvider != null)
            {
                gameObject.transform.position = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
                gameObject.transform.LookAt(CameraCache.Main.transform);
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

                EyeTrackingTarget lookedAtEyeTarget = EyeTrackingTarget.LookedAtEyeTarget;

                // Update GameObject to the current eye gaze position
                if (lookedAtEyeTarget != null)
                {
                    // Show the object at the center of the currently looked at target.
                    if (lookedAtEyeTarget.EyeCursorSnapToTargetCenter)
                    {
                        Ray rayToCenter = new Ray(CameraCache.Main.transform.position, lookedAtEyeTarget.transform.position - CameraCache.Main.transform.position);
                        RaycastHit hitInfo;
                        UnityEngine.Physics.Raycast(rayToCenter, out hitInfo);
                        gameObject.transform.position = hitInfo.point;
                    }
                    else
                    {
                        // Show the object at the hit position of the user's eye gaze ray with the target.
                        gameObject.transform.position = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
                    }
                }
                else
                {
                    // Show the object at the hit position of the user's eye gaze ray with the target.
                    gameObject.transform.position = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
                }
                Visuals.SetActive(true);
            }
        }
    }
}