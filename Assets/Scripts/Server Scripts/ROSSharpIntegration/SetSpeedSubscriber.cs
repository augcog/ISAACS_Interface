using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RosSharp.RosBridgeClient
{

    public class SetSpeedSubscriber : UnitySubscriber<MessageTypes.IsaacsServer.SetSpeedResponse>
    {
        private bool isMessageReceived;
        private bool success;

        protected override void Start()
        {
            base.Start();
        }

        private void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }

        protected override void ReceiveMessage(MessageTypes.IsaacsServer.SetSpeedResponse msg)
        {
            success = msg.success;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            if (success) { Debug.Log("Speed changed"); }
            else
            {
                Debug.Log("invalid speed, not changed.");
            }
            isMessageReceived = false;
        }
    }
}
