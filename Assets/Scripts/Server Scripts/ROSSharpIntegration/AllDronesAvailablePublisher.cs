using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;

namespace RosSharp.RosBridgeClient
{
    public class AllDronesAvailablePublisher : UnityPublisher<MessageTypes.IsaacsServer.AllDronesAvailableRequest>
    {
        private MessageTypes.IsaacsServer.AllDronesAvailableRequest message;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void FixedUpdate()
        {
            UpdateMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.IsaacsServer.AllDronesAvailableRequest();
            //TODO: What to do here?
        }

        private void UpdateMessage()
        {
            //TODO: What to do here?
            Publish(message);
        }
    }
}
