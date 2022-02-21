using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;

using UnityEngine;
using UnityEngine.XR.Management;

using UnityEngine.XR.WindowsMR;

namespace UnityEditor.XR.WindowsMR
{
    /// <summary>
    /// Small utility class for reading, updating and writing boot config.
    /// </summary>
    class BootConfig
    {
        static readonly string kXrBootSettingsKey = "xr-boot-settings";
        Dictionary<string, string> bootConfigSettings;

        private BuildReport buildReport;
        private string bootConfigPath;

        public BootConfig(BuildReport report)
        {
            buildReport = report;
        }

        public void ReadBootConfg()
        {
            bootConfigSettings = new Dictionary<string, string>();

            string buildTargetName = BuildPipeline.GetBuildTargetName(buildReport.summary.platform);
            string xrBootSettings = UnityEditor.EditorUserBuildSettings.GetPlatformSettings(buildTargetName, kXrBootSettingsKey);
            if (!String.IsNullOrEmpty(xrBootSettings))
            {
                // boot settings string format
                // <boot setting>:<value>[;<boot setting>:<value>]*
                var bootSettings = xrBootSettings.Split(';');
                foreach (var bootSetting in bootSettings)
                {
                    var setting = bootSetting.Split(':');
                    if (setting.Length == 2 && !String.IsNullOrEmpty(setting[0]) && !String.IsNullOrEmpty(setting[1]))
                    {
                        bootConfigSettings.Add(setting[0], setting[1]);
                    }
                }
            }

        }

        public void SetValueForKey(string key, string value, bool replace = false)
        {
            if (bootConfigSettings.ContainsKey(key))
            {
                bootConfigSettings[key] = value;
            }
            else
            {
                bootConfigSettings.Add(key, value);
            }
        }

        public void ClearEntryForKey(string key)
        {
            if (bootConfigSettings.ContainsKey(key))
            {
                bootConfigSettings.Remove(key);
            }
        }

        public void WriteBootConfig()
        {
            // boot settings string format
            // <boot setting>:<value>[;<boot setting>:<value>]*
            bool firstEntry = true;
            var sb = new System.Text.StringBuilder();
            foreach (var kvp in bootConfigSettings)
            {
                if (!firstEntry)
                {
                    sb.Append(";");
                }
                sb.Append($"{kvp.Key}:{kvp.Value}");
                firstEntry = false;
            }

            string buildTargetName = BuildPipeline.GetBuildTargetName(buildReport.summary.platform);
            EditorUserBuildSettings.SetPlatformSettings(buildTargetName, kXrBootSettingsKey, sb.ToString());
        }
    }

    /// <summary>Build Processor class used to handle XR Plugin specific build actions.</summary>
    /// The settings instance type the build processor will use.
    public class WindowsMRBuildProcessor : XRBuildHelper<WindowsMRSettings>
    {
        private static List<BuildTarget> s_ValidBuildTargets = new List<BuildTarget>(){
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
            BuildTarget.WSAPlayer,
        };

        /// <summary> The key used to get the build settings object.</summary>
        public override string BuildSettingsKey { get { return Constants.k_SettingsKey; } }

        private bool IsCurrentBuildTargetVaild(BuildReport report)
        {
            return report.summary.platformGroup == BuildTargetGroup.WSA ||
                (report.summary.platformGroup == BuildTargetGroup.Standalone && s_ValidBuildTargets.Contains(report.summary.platform));
        }

        private bool HasLoaderEnabledForTarget(BuildTargetGroup buildTargetGroup)
        {
            if (buildTargetGroup != BuildTargetGroup.Standalone && buildTargetGroup != BuildTargetGroup.WSA)
                return false;

            XRGeneralSettings settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(buildTargetGroup);
            if (settings == null)
                return false;

            bool loaderFound = false;
            var loaders = settings.Manager.activeLoaders;
            for (int i = 0; i < loaders.Count; ++i)
            {
                if (loaders[i] as WindowsMRLoader != null)
                {
                    loaderFound = true;
                    break;
                }
            }

            return loaderFound;
        }

        private WindowsMRPackageSettings PackageSettingsForBuildTargetGroup(BuildTargetGroup buildTargetGroup)
        {
            if (!HasLoaderEnabledForTarget(buildTargetGroup))
                return null;

            UnityEngine.Object settingsObj = null;
            EditorBuildSettings.TryGetConfigObject(BuildSettingsKey, out settingsObj);
            WindowsMRPackageSettings settings = settingsObj as WindowsMRPackageSettings;

            if (settings == null)
            {
                var assets = AssetDatabase.FindAssets("t:WindowsMRPackageSettings");
                if (assets.Length == 1)
                {
                    string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                    settings = AssetDatabase.LoadAssetAtPath(path, typeof(WindowsMRPackageSettings)) as WindowsMRPackageSettings;
                    if (settings != null)
                    {
                        EditorBuildSettings.AddConfigObject(BuildSettingsKey, settings, true);
                    }

                }
            }

            return settings;
        }

        /// <summary>Get the XR Plugin build settings for the specific build platform.</summary>
        /// <param name="buildTargetGroup">The build platform we want to get settings for.</param>
        /// <returns>An instance of WindowsMRBuildSettings, or null if there are none for the current build platform.</returns>
        public WindowsMRBuildSettings BuildSettingsForBuildTargetGroup(BuildTargetGroup buildTargetGroup)
        {
            WindowsMRPackageSettings settings = PackageSettingsForBuildTargetGroup(buildTargetGroup);

            if (settings != null)
            {
                WindowsMRBuildSettings targetSettings = settings.GetBuildSettingsForBuildTargetGroup(buildTargetGroup);
                return targetSettings;
            }

            return null;
        }

        /// <summary>Get a generic object reference for runtime settings for the build platform</summary>
        /// <param name="buildTargetGroup">The build platform we want to get settings for.</param>
        /// <returns>An object instance of the saved settings, or null if there are none.</returns>
        public override UnityEngine.Object SettingsForBuildTargetGroup(BuildTargetGroup buildTargetGroup)
        {
            WindowsMRPackageSettings settings = PackageSettingsForBuildTargetGroup(buildTargetGroup);

            if (settings != null)
            {
                WindowsMRSettings targetSettings = settings.GetSettingsForBuildTargetGroup(buildTargetGroup);
                return targetSettings;
            }

            return null;
        }

        const string k_ForcePrimaryWindowHolographic = "force-primary-window-holographic";
        const string k_VrEnabled = "vr-enabled";
        const string k_WmrLibrary = "xrsdk-windowsmr-library";
        const string k_WmrLibraryName = "WindowsMRXRSDK";
        const string k_EarlyBootHolographic = "early-boot-windows-holographic";

        /// <summary>OnPostprocessBuild override to provide XR Plugin specific build actions.</summary>
        /// <param name="report">The build report.</param>
        public override void OnPostprocessBuild(BuildReport report)
        {
            if (!IsCurrentBuildTargetVaild(report))
                return;

            if (!HasLoaderEnabledForTarget(report.summary.platformGroup))
                return;

            BootConfig bootConfig = new BootConfig(report);
            bootConfig.ReadBootConfg();

            bootConfig.ClearEntryForKey(k_ForcePrimaryWindowHolographic);
            bootConfig.ClearEntryForKey(k_VrEnabled);
            bootConfig.ClearEntryForKey(k_WmrLibrary);
            bootConfig.ClearEntryForKey(k_WmrLibraryName);
            bootConfig.ClearEntryForKey(k_EarlyBootHolographic);

            bootConfig.WriteBootConfig();

            base.OnPostprocessBuild(report);
        }

        private readonly string[] runtimePluginNames = new string[]
        {
            "WindowsMRXRSDK.dll",
        };

        /// <summary>
        /// Used to determine whether or not plugins used for WMR runtime should be included in the final build.
        /// </summary>
        /// <param name="path">The path to the plugin.</param>
        /// <returns>True if the plugin should be included, false otherwise.</returns>
        [Obsolete("This API is obsolete and will be removed in a future version of the package.", false)]
        public bool ShouldIncludeRuntimePluginsInBuild(string path)
        {
            throw new NotImplementedException("This API is no longer supported and should not be used.");
        }

        internal bool ShouldIncludeRuntimePluginsInBuild(string path, BuildTargetGroup buildTargetGroup)
        {
            return HasLoaderEnabledForTarget(buildTargetGroup);
        }

        private readonly string spatializerPluginName = "AudioPluginMsHRTF.dll";
        private readonly string spatializerReadableName = "MS HRTF Spatializer";

        /// <summary>
        /// Used to determine whether or not the MicrosoftHRTFAudioSpatializer plugin should be included in the final build.
        /// </summary>
        /// <param name="path">The path to the plugin.</param>
        /// <returns>True if the plugin should be included, false otherwise.</returns>
        [Obsolete("This API is obsolete and will be removed in a future version of the package.", false)]
        public bool ShouldIncludeSpatializerPluginsInBuild(string path)
        {
            throw new NotImplementedException("This API is no longer supported and should not be used.");
        }

        internal bool Internal_ShouldIncludeSpatializerPluginsInBuild(string path)
        {
            string currentSpatializerPluginName = AudioSettings.GetSpatializerPluginName();

            if (string.Compare(spatializerReadableName, currentSpatializerPluginName, true) == 0)
                return true;

            return false;
        }

        private readonly string[] remotingPluginNames = new string[]
        {
            "Microsoft.Holographic.AppRemoting.dll",
            "PerceptionDevice.dll",
            "UnityRemotingWMR.dll"
        };

        /// <summary>
        /// Used to determine whether or not the WMR remoting plugin should be included in the final build.
        /// </summary>
        /// <param name="path">The path to the plugin.</param>
        /// <returns>True if the plugin should be included, false otherwise.</returns>
        [Obsolete("This API is obsolete and will be removed in a future version of the package.", false)]
        public bool ShouldIncludeRemotingPluginsInBuild(string path)
        {
            throw new NotImplementedException("This API is no longer supported and should not be used.");
        }

        internal bool ShouldIncludeRemotingPluginsInBuild(string path, BuildTargetGroup buildTargetGroup)
        {
            WindowsMRBuildSettings buildSettings = BuildSettingsForBuildTargetGroup(buildTargetGroup) as WindowsMRBuildSettings;
            if (buildSettings == null)
                return false;

            return buildSettings.HolographicRemoting;
        }

        /// <summary>OnPreprocessBuild override to provide XR Plugin specific build actions.</summary>
        /// <param name="report">The build report.</param>
        public override void OnPreprocessBuild(BuildReport report)
        {
            if (IsCurrentBuildTargetVaild(report) && HasLoaderEnabledForTarget(report.summary.platformGroup))
            {
                base.OnPreprocessBuild(report);

                BootConfig bootConfig = new BootConfig(report);
                bootConfig.ReadBootConfg();

                bootConfig.SetValueForKey(k_VrEnabled, "1", true);
                bootConfig.SetValueForKey(k_WmrLibrary, k_WmrLibraryName, true);
                if (report.summary.platformGroup == BuildTargetGroup.WSA)
                {
                    bootConfig.SetValueForKey(k_EarlyBootHolographic, "1", true);

                    bool usePrimaryWindow = true;
                    WindowsMRBuildSettings buildSettings = BuildSettingsForBuildTargetGroup(report.summary.platformGroup);
                    if (buildSettings != null)
                    {
                        usePrimaryWindow = buildSettings.UsePrimaryWindowForDisplay;
                    }

                    bootConfig.SetValueForKey(k_ForcePrimaryWindowHolographic, usePrimaryWindow ? "1" : "0", true);
                }


                bootConfig.WriteBootConfig();

            }


            var allPlugins = PluginImporter.GetAllImporters();
            foreach (var plugin in allPlugins)
            {
                if (plugin.isNativePlugin)
                {
                    foreach (var pluginName in remotingPluginNames)
                    {
                        if (plugin.assetPath.Contains(pluginName))
                        {
                            plugin.SetIncludeInBuildDelegate((path) => { return ShouldIncludeRemotingPluginsInBuild(path, report.summary.platformGroup); });
                            break;
                        }
                    }

                    foreach (var pluginName in runtimePluginNames)
                    {
                        if (plugin.assetPath.Contains(pluginName))
                        {
                            plugin.SetIncludeInBuildDelegate((path) => { return ShouldIncludeRuntimePluginsInBuild(path, report.summary.platformGroup); });
                            break;
                        }
                    }

                    if (plugin.assetPath.Contains(spatializerPluginName))
                    {
                        plugin.SetIncludeInBuildDelegate(Internal_ShouldIncludeSpatializerPluginsInBuild);
                    }
                }
            }
        }
    }
}
