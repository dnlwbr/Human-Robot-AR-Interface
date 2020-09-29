using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;


namespace HumanRobotInterface
{
    public class FollowEyeGazeHitPoint : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        public void SetPosition()
        {
            var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
            if (eyeGazeProvider != null)
            {
                gameObject.transform.position = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
                gameObject.transform.LookAt(CameraCache.Main.transform);

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
            }
        }
    }
}