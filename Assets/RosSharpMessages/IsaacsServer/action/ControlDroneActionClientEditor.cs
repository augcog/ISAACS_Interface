using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RosSharp.RosBridgeClient.Actionlib
{
    [CustomEditor(typeof(UnityControlDroneActionClient))]
    public class ControlDroneActionClientEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Send Goal"))
            {
                ((UnityControlDroneActionClient)target).RegisterGoal();
                ((UnityControlDroneActionClient)target).controlDroneActionClient.SendGoal();
            }

            if (GUILayout.Button("Cancel Goal"))
            {
                ((UnityControlDroneActionClient)target).controlDroneActionClient.CancelGoal();
            }
        }
    }
}
