#if JENKINS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.TestTools;
using UnityEngine.XR.Management;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.Management;
#endif

[PrebuildSetup(typeof(TestPrebuildSetup))]
public class TestBaseSetup
{
    public static GameObject m_Camera;
    public static GameObject m_Light;
    public static GameObject m_Cube;

    public static GameObject m_XrManager;
    public static GameObject m_TrackingRig;
    public static TrackedPoseDriver m_TrackHead;

    public TestSetupHelpers m_TestSetupHelpers;

    internal static ScriptableObject GetInstanceOfTypeWithNameFromAssetDatabase(string typeName)
    {
        var assets = AssetDatabase.FindAssets(String.Format("t:{0}", typeName));
        if (assets.Any())
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);
            var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject));
            return asset as ScriptableObject;
        }
        return null;
    }

    protected bool IsLoaderEnabledForTarget(BuildTargetGroup buildTargetGroup, string loaderTypeName)
    {
        if (buildTargetGroup != BuildTargetGroup.Standalone && buildTargetGroup != BuildTargetGroup.WSA)
            return false;

#if UNITY_EDITOR
        XRGeneralSettings settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);
#else
        XRGeneralSettings settings  = XRGeneralSettings.Instance;
#endif

        if (settings == null || settings.Manager == null)
            return false;

        var instance = TestBaseSetup.GetInstanceOfTypeWithNameFromAssetDatabase(loaderTypeName);
        if (instance == null || !(instance is XRLoader))
            return false;

        XRLoader loader = instance as XRLoader;
        return settings.Manager.activeLoaders.Contains(loader);
    }

    protected bool EnableLoader(string loaderTypeName, bool enable, BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone)
    {
#if UNITY_EDITOR
        XRGeneralSettings settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);
#else
        XRGeneralSettings settings  = XRGeneralSettings.Instance;
#endif

        if (settings == null || settings.Manager == null)
            return false;

        var instance = TestBaseSetup.GetInstanceOfTypeWithNameFromAssetDatabase(loaderTypeName);
        if (instance == null || !(instance is XRLoader))
            return false;

        XRLoader loader = instance as XRLoader;
        bool ret = false;

        if (enable)
            ret = settings.Manager.TryAddLoader(loader);
        else
            ret = settings.Manager.TryRemoveLoader(loader);

        return ret;

    }

    [SetUp]
    public void XrSdkTestBaseSetup()
    {
        m_Cube = new GameObject("Cube");
        m_TestSetupHelpers = new TestSetupHelpers();

        m_TestSetupHelpers.TestStageSetup(TestStageConfig.BaseStageSetup);
    }

    [TearDown]
    public void XrSdkTestBaseTearDown()
    {
        m_TestSetupHelpers.TestStageSetup(TestStageConfig.CleanStage);
    }

    public class TestPrebuildSetup : IPrebuildSetup
    {
        public void Setup()
        {
#if UNITY_EDITOR
            // Configure StandAlone build
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            EditorUserBuildSettings.wsaUWPBuildType = WSAUWPBuildType.D3D;
            EditorUserBuildSettings.wsaSubtarget = WSASubtarget.AnyDevice;
            EditorUserBuildSettings.allowDebugging = true;

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
#endif
        }
    }
}
#endif //JENKINS
