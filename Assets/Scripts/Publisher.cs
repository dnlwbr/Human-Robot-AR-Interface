using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HumanRobotInterface
{
    public abstract class Publisher<T> : MonoBehaviour where T : Message, new()
    {
        [SerializeField]
        protected string topic;

        private GameObject RosSharp;
        private RosSocket rosSocket;
        private string publicationId;

        private float previousRealTime;

        // Start is called before the first frame update
        protected virtual void Start()
        {
            RosSharp = GameObject.Find("RosSharp");
            rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
            publicationId = rosSocket.Advertise<T>(topic);
        }

        protected void Publish(T message, int rate = 0)
        {
            if (rate == 0)
            {
                rosSocket.Publish(publicationId, message);
            }
            else
            {
                if (Time.realtimeSinceStartup - previousRealTime >= 1 / rate)
                {
                    rosSocket.Publish(publicationId, message);
                    previousRealTime = Time.realtimeSinceStartup;
                }
            }
        }
    }
}
