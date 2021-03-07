using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class SetSpeedPublisher : UnityPublisher<MessageTypes.isaacs_server.set_speed>
    {
        public int id;
        public bool success;
        public string msg;

        private MessageTypes.isaacs_server.set_speed message;


        /*namespace RosSharp.RosBridgeClient
    {
        public class FloatPublisher : UnityPublisher<MessageTypes.Std.Float32>
        {
            public float messageData;

            private MessageTypes.Std.Float32 message;

            protected override void Start()
            {
                base.Start();
                InitializeMessage();
            }

            private void InitializeMessage()
            {
                message = new MessageTypes.Std.Float32
                {
                    data = messageData
                };
            }

            private void Update()
            {
                message.data = messageData;
                Publish(message);
            }
        }
    }*/
    }
}
