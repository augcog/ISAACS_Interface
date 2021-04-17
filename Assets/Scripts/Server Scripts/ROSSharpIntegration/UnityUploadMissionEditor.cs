
using UnityEditor;
using UnityEngine;

namespace RosSharp.RosBridgeClient.Actionlib
{
    [CustomEditor(typeof(UnityUploadMissionActionClient))]
    public class UnityUploadMissionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Send Goal"))
            {
                ((UnityUploadMissionActionClient)target).RegisterGoal();
                ((UnityUploadMissionActionClient)target).uploadMissionActionClient.SendGoal();
            }

            if (GUILayout.Button("Cancel Goal"))
            {
                ((UnityUploadMissionActionClient)target).uploadMissionActionClient.CancelGoal();
            }
        }
    }
}
