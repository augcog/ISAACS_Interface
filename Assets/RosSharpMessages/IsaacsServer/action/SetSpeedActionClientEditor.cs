using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RosSharp.RosBridgeClient.Actionlib
{
    [CustomEditor(typeof(UnitySetSpeedActionClient))]
    public class SetSpeedActionClientEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Send Goal"))
            {
                ((UnitySetSpeedActionClient)target).RegisterGoal();
                ((UnitySetSpeedActionClient)target).setSpeedActionClient.SendGoal();
            }

            if (GUILayout.Button("Cancel Goal"))
            {
                ((UnitySetSpeedActionClient)target).setSpeedActionClient.CancelGoal();
            }
        }
    }
}
