using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using UnityEngine.XR.ARSubsystems;
using System.Runtime.ExceptionServices;

namespace UnityEngine.XR.WindowsMR
{
    /// <summary>
    /// Provides access to the underlying Microsoft [SpatialAnchorStore](https://docs.microsoft.com/en-us/uwp/api/windows.perception.spatial.spatialanchorstore?view=winrt-19041)
    /// allowing a user to persist, reload, track and unpersist anchors between the [SpatialAnchorStore](https://docs.microsoft.com/en-us/uwp/api/windows.perception.spatial.spatialanchorstore?view=winrt-19041)
    /// and the a curerntly running instance of <see cref="XRAnchorSubsystem"/>.
    ///
    /// The [SpatialAnchorStore](https://docs.microsoft.com/en-us/uwp/api/windows.perception.spatial.spatialanchorstore?view=winrt-19041) contains a snapshot of the data at the time
    /// of it's creation. If you update the store outside of the running application then you will need to
    /// make sure to destroy and recreate this.
    ///
    /// Getting an instance of this class will allocate an instance of the [SpatialAnchorStore](https://docs.microsoft.com/en-us/uwp/api/windows.perception.spatial.spatialanchorstore?view=winrt-19041)
    /// and retain it. You must dispose of this allocted resource or you may leak memory.
    ///
    /// </summary>
    public sealed class XRAnchorStore : IDisposable
    {
        private static readonly TrackableId defaultId = default(TrackableId);

        internal static class NativeApi
        {
#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
        [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
#endif
            public static extern void UnityWindowsMR_refPoints_DestroyAnchorStore(IntPtr ptr);


        [ StructLayout( LayoutKind.Sequential )]
        public struct Buffer
        {
            public int size;
            public IntPtr buffer;
        }

#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
        [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
#endif
            public static extern void UnityWindowsMR_refPoints_GetPersistedNames(IntPtr ptr, out Buffer buffer);


#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
        [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
#endif
            public static extern void UnityWindowsMR_refPoints_DestroyPersistedNames(Buffer buffer);


#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
            [DllImport("WindowsMRXRSDK", CharSet = CharSet.Auto)]
#endif
            public static extern bool UnityWindowsMR_refPoints_TryPersistAnchor(IntPtr ptr, TrackableId id, [MarshalAs(UnmanagedType.LPWStr)]string name);


#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
            [DllImport("WindowsMRXRSDK", CharSet = CharSet.Auto)]
#endif
            public static extern void UnityWindowsMR_refPoints_UnpersistAnchor(IntPtr ptr, [MarshalAs(UnmanagedType.LPWStr)] string name);


#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
            [DllImport("WindowsMRXRSDK", CharSet = CharSet.Auto)]
#endif
            public static extern void UnityWindowsMR_refPoints_Clear(IntPtr ptr);

#if UNITY_EDITOR
        [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
#elif ENABLE_DOTNET
        [DllImport("WindowsMRXRSDK.dll")]
#else
            [DllImport("WindowsMRXRSDK", CharSet = CharSet.Auto)]
#endif
            public static extern void UnityWindowsMR_refPoints_LoadAnchor(IntPtr ptr, [MarshalAs(UnmanagedType.LPWStr)] string name, out TrackableId id);
        }

        private bool disposed = false;
        private IntPtr storePtr;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            NativeApi.UnityWindowsMR_refPoints_DestroyAnchorStore(storePtr);
            storePtr = IntPtr.Zero;
            disposed = true;
        }

        internal XRAnchorStore(IntPtr storePtr)
        {
            this.storePtr = storePtr;
        }

        /// <inheritdoc/>
        ~XRAnchorStore()
        {
            Dispose();
        }

        private List<string> persistedNames = null;

        private void ClearPersistedNames(bool shouldClear)
        {
            if (shouldClear)
                persistedNames = null;
        }

        /// <summary>
        /// Retrieve all the currently stored, named anchors from
        /// the SpatialAnchorStore
        ///
        /// If the list of names is not alrady created, we will as the anchor store
        /// for the current list of persisted names. This list will not change unless
        /// you modify the store by calling <see cref="Clear"/>, <see cref="TryPersistAnchor"/>,
        /// <see cref="UnpersistAnchor"/>, or destroyong and re-creating an instance of
        /// <see cref="XRAnchorStore"/>.
        ///
        /// Has no impact on the set of currently tracked anchors in
        /// the running instance of <see cref="XRAnchorSubsystem"/>.
        /// </summary>
        /// <value>Read only list of anchor names previously persisted to the store.</value>
        public IReadOnlyList<string> PersistedAnchorNames
        {
            get
            {
                if (persistedNames == null)
                {
                    persistedNames = new List<string>();
                    NativeApi.Buffer buffer = new NativeApi.Buffer();
                    NativeApi.UnityWindowsMR_refPoints_GetPersistedNames(storePtr, out buffer);
                    if (buffer.size > 0)
                    {
                        byte[] byteBuffer = new byte[buffer.size];
                        Marshal.Copy(buffer.buffer, byteBuffer, 0, buffer.size);
                        NativeApi.UnityWindowsMR_refPoints_DestroyPersistedNames(buffer);
                        using (MemoryStream stream = new MemoryStream(byteBuffer))
                        {
                            using(BinaryReader reader = new BinaryReader(stream))
                            {
                                System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding();
                                int countStrings = reader.ReadInt32();
                                for (int i = 0; i < countStrings; i++)
                                {
                                    int strByteLen = reader.ReadInt32() * 2;
                                    byte[] bytes = reader.ReadBytes(strByteLen);
                                    string name = encoding.GetString(bytes, 0, strByteLen);
                                    persistedNames.Add(name);
                                }
                            }
                        }
                    }
                }

                return persistedNames.AsReadOnly();
            }
        }

        /// <summary>
        /// Take a persisted anchor from the [SpatialAnchorStore](https://docs.microsoft.com/en-us/uwp/api/windows.perception.spatial.spatialanchorstore?view=winrt-19041)
        /// with the given name and addes it to the current set of tracked <see cref="XREferencePoint"/> in the runnign instance of <see cref="XRAnchorSubsystem"/>.
        /// </summary>
        /// <param name="name">The name of the anchor to load.</param>
        /// <returns>The Id of the anchor.</returns>
        public TrackableId LoadAnchor(string name)
        {
            TrackableId ret = defaultId;
            NativeApi.UnityWindowsMR_refPoints_LoadAnchor(storePtr, name, out ret);
            return ret;
        }

        /// <summary>
        /// Given a trackable anchor with id, save the anchor to the
        /// SpatialAnchorStore with the given name.
        ///
        /// Has no impact on the set of currently tracked anchors in
        /// the running instance of <see cref="XRAnchorSubsystem"/>.
        /// </summary>
        /// <param name="id">The trackable id of a <see cref="XRAnchor"/> from the running instance of <see cref="XRAnchorSubsystem"/>.</param>
        /// <param name="name">The name you wish to assign to the anchor persisted in the [SpatialAnchorStore](https://docs.microsoft.com/en-us/uwp/api/windows.perception.spatial.spatialanchorstore?view=winrt-19041).</param>
        /// <returns>False if there is no running instance of <see cref="XRAnchorSubsystem"/>, the
        /// id or name is unknown, or if the underlying anchor store has any issues. True otherwise.</returns>
        public bool TryPersistAnchor(TrackableId id, string name)
        {
            bool ret = NativeApi.UnityWindowsMR_refPoints_TryPersistAnchor(storePtr, id, name);
            ClearPersistedNames(ret);
            return ret;
        }

        /// <summary>
        /// Given an anchor with id and name, remove the anchor from the
        /// SpatialAnchorStore.
        ///
        /// Has no impact on the set of currently tracked anchors in
        /// the running instance of <see cref="XRAnchorSubsystem"/>.
        /// </summary>
        /// <param name="name">The name of a currently persisted anchor to remove from the [SpatialAnchorStore](https://docs.microsoft.com/en-us/uwp/api/windows.perception.spatial.spatialanchorstore?view=winrt-19041).</param>
        public void UnpersistAnchor(string name)
        {
            NativeApi.UnityWindowsMR_refPoints_UnpersistAnchor(storePtr, name);
            ClearPersistedNames(true);
        }

        /// <summary>
        /// Clear all anchors from the store.
        ///
        /// Has no impact on the set of currently tracked anchors in
        /// the running instance of <see cref="XRAnchorSubsystem"/>.
        /// </summary>
        public void Clear()
        {
            NativeApi.UnityWindowsMR_refPoints_Clear(storePtr);
            ClearPersistedNames(true);
        }
    }
}
