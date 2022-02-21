using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.WindowsMR
{
    /// <summary>
    /// Extensions to <see cref="XRAnchorSubsystem" /> for getting an instance of
    /// <see cref="XRAnchorStore" /> to allow a user to interact with the  Microsoft [SpatialAnchorStore](https://docs.microsoft.com/en-us/uwp/api/windows.perception.spatial.spatialanchorstore?view=winrt-19041).
    /// </summary>
    public static class XRAnchorSubsystemExtensions
    {
        internal static class NativeApi
        {
#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
        [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
#endif
            public static extern IntPtr UnityWindowsMR_refPoints_tryGetAnchorStore();
        }

        /// <summary>
        /// Request an instance of <see cref="XRAnchorStore"/>.
        /// </summary>
        /// <param name="anchorSubsytem">Instance of a running <see cref="XRAnchorSubsystem"/></param>
        /// <returns>A [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1?view=netcore-3.1) that may return an instance of <see cref="XRAnchorStore"/> at some time in the future.</returns>
        public static Task<XRAnchorStore> TryGetAnchorStoreAsync(this XRAnchorSubsystem anchorSubsytem)
        {
            return Task<XRAnchorStore>.Run( () => {
                IntPtr storePtr = NativeApi.UnityWindowsMR_refPoints_tryGetAnchorStore();
                return new XRAnchorStore(storePtr);
            });
        }

    }
}
