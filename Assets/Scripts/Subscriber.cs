using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace HumanRobotInterface
{
    public abstract class Subscriber<T> : MonoBehaviour where T : Message, new()
    {
        [SerializeField]
        private string topic;
        [SerializeField, Tooltip("The rate (in ms in between messages) at which to throttle the topics.")]
        private float timeStep = 0;
        [SerializeField, Tooltip("Number of messages that will be buffered up before beginning to throw away the oldest ones.")]
        private int queueLength = 1;

        private GameObject RosSharp;
        private RosSocket rosSocket;
        private readonly int SecondsTimeout = 1;

        // Start is called before the first frame update
        protected virtual void Start()
        {
            RosSharp = GameObject.Find("RosSharp");
            rosSocket = RosSharp.GetComponent<RosConnector>().RosSocket;
            //new Thread(Subscribe).Start();
            Subscribe();
        }

        private void Subscribe()
        {

            if (!RosSharp.GetComponent<RosConnector>().IsConnected.WaitOne(SecondsTimeout * 1000))
                Debug.LogWarning("Failed to subscribe: RosConnector not connected");

            rosSocket.Subscribe<T>(topic, callback, (int)(timeStep * 1000), queueLength);
        }

        protected abstract void callback(T message);
    }
}
