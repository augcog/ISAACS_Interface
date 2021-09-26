using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ISAACS;

namespace RosSharp.RosBridgeClient
{
    public class SetSpeedPublisher : UnityPublisher<MessageTypes.IsaacsServer.SetSpeedRequest>
    {
        public Slider slider;

        private MessageTypes.IsaacsServer.SetSpeedRequest message;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
            slider.onValueChanged.AddListener(delegate { UpdateMessage(); });
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.IsaacsServer.SetSpeedRequest();
            message.id = (uint) WorldProperties.GetSelectedDrone().id;
        }

        private void UpdateMessage()
        {
            //message.header.Update();
            message.id = (uint)WorldProperties.GetSelectedDrone().id;
            message.speed = slider.value;
            Debug.Log("Toggle changed to " + message.speed + "... Sending to server.");
            Publish(message);

        }


    }
}
