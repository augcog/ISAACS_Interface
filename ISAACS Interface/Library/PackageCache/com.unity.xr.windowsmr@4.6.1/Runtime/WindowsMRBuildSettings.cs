using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.WindowsMR
{
    /// <summary>Build specific settings for the Windows XR Plugin package.</summary>
    public class WindowsMRBuildSettings : ScriptableObject
    {
        /// <summary>Enable or disable the use of the primary window for XR display.
        ///
        /// If enabled the XR Plugin will attempt to take over the CoreWindow instance of the player for reuse.
        /// If disabled then the plugin will create a new secondary CoreWindow+View to use.
        ///
        /// Only applicable for XAML based applications.
        /// </summary>
        [SerializeField, Tooltip("Enable or disable the use of the primary application window for display when running as a WSA application.")]
        public bool UsePrimaryWindowForDisplay = true;

        /// <summary>True if we want to use holographic remoting in a UWP desktop build, false otherwise.</summary>
        [SerializeField, Tooltip("Enable holographic remoting support in UWP desktop builds. Enabling remoting will disable Primary Window and XR Plug-in Management Initialize on Start.")]
        public bool HolographicRemoting = false;
    }
}
