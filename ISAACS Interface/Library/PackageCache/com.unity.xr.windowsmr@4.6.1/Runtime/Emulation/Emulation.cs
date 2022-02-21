using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.WindowsMRInternals;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WINRT
using System.Runtime.InteropServices;
#endif

namespace UnityEngine.XR.WindowsMR
{
    /// <summary>Modes available for Emulation. Primarily used by the Emulation Window.</summary>
    public enum WindowsMREmulationMode
    {
        /// <summary>No Emulation</summary>
        None,
        /// <summary>Remoting enabled</summary>
        Remoting
    }

    /// <summary>Windows Mixed Reality Emulation class. Used to Get/Set the emulation state of the application.</summary>
    public static class WindowsMREmulation
    {
        /// <summary>Value used to Get/Set the WindowsMREmulationMode</summary>
        public static WindowsMREmulationMode mode
        {
            get
            {
                return UnityWindowsMR_Emulation_GetMode();
            }
            set
            {
                UnityWindowsMR_Emulation_SetMode(value);
            }
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WINRT
        [DllImport("WindowsMRXRSDK")]
        static extern void UnityWindowsMR_Emulation_SetMode(WindowsMREmulationMode mode);

        [DllImport("WindowsMRXRSDK")]
        static extern WindowsMREmulationMode UnityWindowsMR_Emulation_GetMode();
#else
        static void UnityWindowsMR_Emulation_SetMode(WindowsMREmulationMode mode)
        {
        }

        static WindowsMREmulationMode UnityWindowsMR_Emulation_GetMode()
        {
            return WindowsMREmulationMode.None;
        }
#endif
    }
}
