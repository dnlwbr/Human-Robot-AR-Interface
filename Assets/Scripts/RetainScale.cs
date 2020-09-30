using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HumanRobotInterface
{
    public class RetainScale : MonoBehaviour
    {
        private Vector3 scaleAtStart;

        // Start is called before the first frame update
        void Start()
        {
            scaleAtStart = Vector3.Scale(transform.parent.localScale, transform.localScale);
        }

        // Update is called once per frame
        void Update()
        {
            transform.localScale = new Vector3(
                scaleAtStart.x / transform.parent.localScale.x,
                scaleAtStart.y / transform.parent.localScale.y,
                scaleAtStart.z / transform.parent.localScale.z); 
        }
    }
}