using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.WindowsMR
{
    /// <summary>Windows Mixed Reality Types</summary>
    public static class NativeTypes
    {
        /// <summary>Custom enum to map native Windows.Perception.Spatial.SpatialLocatability to something we can actually reference.</summary>
        public enum SpatialLocatability
        {
            /// <summary>No Spatial Locatability data is available</summary>
            Unavailable = 0,
            /// <summary>Orientation data only</summary>
            OrientationOnly = 1,
            /// <summary>In the process of activating Positional data</summary>
            PositionalTrackingActivating = 2,
            /// <summary>Positional data available</summary>
            PositionalTrackingActive = 3,
            /// <summary>Positional data active, but inhibited</summary>
            PositionalTrackingInhibited = 4,
        }

    }

    internal static class Native
    {

        internal enum UnitySubsystemErrorCode
        {
            kUnitySubsystemErrorCodeSuccess,
            kUnitySubsystemErrorCodeFailure,
            kUnitySubsystemErrorCodeInvalidArguments,
            kUnitySubsystemErrorCodeNotSupported,
            kUnitySubsystemErrorCodeOutOfMemory
        }

#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#else
#if ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
        [DllImport("WindowsMRXRSDK", CharSet = CharSet.Auto)]
#endif
#endif
        internal static extern UnitySubsystemErrorCode CreateHolographicSession();

#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#else
#if ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
        [DllImport("WindowsMRXRSDK", CharSet = CharSet.Auto)]
#endif
#endif
        internal static extern UnitySubsystemErrorCode StartHolographicSession();

#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#else
#if ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
        [DllImport("WindowsMRXRSDK", CharSet = CharSet.Auto)]
#endif
#endif
        internal static extern void StopHolographicSession();

#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#else
#if ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
        [DllImport("WindowsMRXRSDK", CharSet = CharSet.Auto)]
#endif
#endif
        internal static extern void DestroyHolographicSession();

        [StructLayout(LayoutKind.Sequential)]
        internal struct UserDefinedSettings
        {
            public ushort depthBufferType;
            public ushort sharedDepthBuffer;
        }

#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#else
#if ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
        [DllImport("WindowsMRXRSDK", CharSet = CharSet.Auto)]
#endif
#endif
        internal static extern void SetUserDefinedSettings(UserDefinedSettings settings);

#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#else
#if ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
        [DllImport("WindowsMRXRSDK", CharSet = CharSet.Auto)]
#endif
#endif
        internal static extern NativeTypes.SpatialLocatability GetSpatialLocatability();


#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#else
#if ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
        [DllImport("WindowsMRXRSDK", CharSet = CharSet.Auto)]
#endif
#endif
        internal static extern IntPtr GetHolographicSpace();

#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#else
#if ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
        [DllImport("WindowsMRXRSDK", CharSet = CharSet.Auto)]
#endif
#endif
        internal static extern IntPtr GetOriginSpatialCoordinateSystem();

#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#else
#if ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
        [DllImport("WindowsMRXRSDK", CharSet = CharSet.Auto)]
#endif
#endif
        internal static extern IntPtr GetCurrentHolographicRenderFrame();

#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#else
#if ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
        [DllImport("WindowsMRXRSDK", CharSet = CharSet.Auto)]
#endif
#endif
        internal static extern IntPtr GetCurrentHolographicSimulationFrame();
    }
}
