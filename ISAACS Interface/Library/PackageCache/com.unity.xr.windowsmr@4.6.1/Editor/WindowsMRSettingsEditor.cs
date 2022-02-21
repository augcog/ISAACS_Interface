using System;

using UnityEngine;
using UnityEngine.XR.WindowsMR;

using UnityEditor;
using UnityEditor.XR.Management;

namespace UnityEditor.XR.WindowsMR
{
    /// <summary>Custom editor settings support for this XR Plugin.</summary>
    [CustomEditor(typeof(WindowsMRPackageSettings))]
    public class SettingsEditor : UnityEditor.Editor
    {
        const string k_DepthBufferFormat = "DepthBufferFormat";
        const string k_SharedDepthBuffer = "UseSharedDepthBuffer";
        const string k_ForcePrimaryWindowHolographic = "UsePrimaryWindowForDisplay";
        const string k_HolographicRemoting = "HolographicRemoting";

        static GUIContent s_DepthBufferFormatLabel = new GUIContent("Depth Buffer Format");
        static GUIContent s_SharedDepthBufferLabel = new GUIContent("Shared Depth Buffer");
        static GUIContent s_HolographicRemotingLabel = new GUIContent("Holographic Remoting");
        static GUIContent s_forcePrimaryWindowHolographicLabel = new GUIContent("Use Primary Window");
        static GUIContent s_ShowBuildSettingsLabel = new GUIContent("Build Settings");
        static GUIContent s_ShowRuntimeSettingsLabel = new GUIContent("Runtime Settings");

        bool m_ShowBuildSettings = true;
        bool m_ShowRuntimeSettings = true;

        /// <summary>
        /// GUI for WindowsMRSettingsEditor class.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (serializedObject == null || serializedObject.targetObject == null)
                return;

            WindowsMRPackageSettings settings = serializedObject.targetObject as WindowsMRPackageSettings;

            BuildTargetGroup buildTargetGroup = EditorGUILayout.BeginBuildTargetSelectionGrouping();

            if (buildTargetGroup == BuildTargetGroup.Standalone || buildTargetGroup == BuildTargetGroup.WSA)
            {
                var mgmtsettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);

                serializedObject.Update();

                m_ShowBuildSettings = EditorGUILayout.Foldout(m_ShowBuildSettings, s_ShowBuildSettingsLabel);
                if (m_ShowBuildSettings)
                {
                    var serializedSettingsObject = new SerializedObject(settings.GetBuildSettingsForBuildTargetGroup(buildTargetGroup));
                    serializedSettingsObject.Update();

                    SerializedProperty holographicRemoting = serializedSettingsObject.FindProperty(k_HolographicRemoting);
                    SerializedProperty forcePrimaryWindowHolographic = serializedSettingsObject.FindProperty(k_ForcePrimaryWindowHolographic);
                    EditorGUI.indentLevel++;

                    if (buildTargetGroup == BuildTargetGroup.WSA)
                    {
                        EditorGUI.BeginDisabledGroup(holographicRemoting.boolValue);

                        if (holographicRemoting.boolValue && forcePrimaryWindowHolographic.boolValue)
                        {
                            forcePrimaryWindowHolographic.boolValue = false;
                            Debug.LogWarning("Holographic remoting has been enabled. This requires use of primary window to be disabled.");
                        }

                        EditorGUILayout.PropertyField(forcePrimaryWindowHolographic, s_forcePrimaryWindowHolographicLabel);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.PropertyField(holographicRemoting, s_HolographicRemotingLabel);
                    EditorGUI.indentLevel--;


                    if (mgmtsettings != null)
                    {
                        if (holographicRemoting.boolValue == mgmtsettings.InitManagerOnStart)
                        {
                            mgmtsettings.InitManagerOnStart = !holographicRemoting.boolValue;
                            if (!mgmtsettings.InitManagerOnStart)
                                Debug.LogWarning("Holographic remoting has been enabled. This requires XR Plug-in Management Initialize on Startup to be disabled.");
                            else
                                Debug.LogWarning("Holographic remoting has been disabled. XR Plug-in Management Initialize on Startup has been re-enabled.");
                        }
                    }

                    serializedSettingsObject.ApplyModifiedProperties();
                }


                EditorGUILayout.Space();

                m_ShowRuntimeSettings = EditorGUILayout.Foldout(m_ShowRuntimeSettings, s_ShowRuntimeSettingsLabel);

                if (m_ShowRuntimeSettings)
                {
                    var serializedSettingsObject = new SerializedObject(settings.GetSettingsForBuildTargetGroup(buildTargetGroup));
                    serializedSettingsObject.Update();

                    SerializedProperty depthBufferFormat = serializedSettingsObject.FindProperty(k_DepthBufferFormat);
                    SerializedProperty sharedDepthBuffer = serializedSettingsObject.FindProperty(k_SharedDepthBuffer);

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(depthBufferFormat, s_DepthBufferFormatLabel);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(sharedDepthBuffer, s_SharedDepthBufferLabel);
                    EditorGUI.indentLevel--;
                    serializedSettingsObject.ApplyModifiedProperties();
                }
                serializedObject.ApplyModifiedProperties();

            }
            else
            {
                EditorGUILayout.HelpBox("Settings for this package are unsupported for this target platform.", MessageType.Info);
            }
            EditorGUILayout.EndBuildTargetSelectionGrouping();

        }
    }
}
