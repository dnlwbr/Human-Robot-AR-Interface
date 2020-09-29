using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HumanRobotInterface
{
    public class StayAtHitPoint : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            gameObject.transform.localPosition = new Vector3(0, 0, -1 * gameObject.transform.localScale.z / 2);
        }
    }
}
