using System;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.XR;

namespace UnityEngine.XR.WindowsMR
{
    /// <summary>Extension methods for the XRMeshingSubsystem that provide Windows MR XR Plugin specific APIs on top of the standard subsystem feature set.</summary>
    public static class WindowsMRExtensions
    {
        [ StructLayout( LayoutKind.Sequential )]
        struct WMRPlane
        {
            public float d;
            public float nx;
            public float ny;
            public float nz;
        }

        static WMRPlane[] wmrp = new WMRPlane[6];
        /// <summary>
        /// Set the Frustum's Bounding Volume
        /// </summary>
        /// <param name="meshing">MeshSubystem reference</param>
        /// <param name="planes">Array of planes for each face of the bounding Frustum</param>
        public static void SetBoundingVolumeFrustum(this XRMeshSubsystem meshing, Plane[] planes)
        {
            for (int i = 0; i < 6; i++)
            {
                wmrp[i].d = planes[i].distance;
                wmrp[i].nx = planes[i].normal.x;
                wmrp[i].ny = planes[i].normal.y;
                wmrp[i].nz = planes[i].normal.z;
            }
            NativeApi.SetBoundingVolumeFrustum(wmrp, 6);
        }


        [ StructLayout( LayoutKind.Sequential )]
        struct WMROrientedBox
        {
            public float cx;
            public float cy;
            public float cz;
            public float ex;
            public float ey;
            public float ez;
            public float ox;
            public float oy;
            public float oz;
            public float ow;
        }

        /// <summary>
        /// Set the OB Bounding Volume
        /// </summary>
        /// <param name="meshing">MeshSubystem reference</param>
        /// <param name="center">Center position of the box</param>
        /// <param name="extents">Extents of the box</param>
        /// <param name="orientation">Orientation/Rotation of the box</param>
        public static void SetBoundingVolumeOrientedBox(this XRMeshSubsystem meshing, Vector3 center, Vector3 extents, Quaternion orientation)
        {
            WMROrientedBox obb;
            obb.cx = center.x;
            obb.cy = center.y;
            obb.cz = center.z;

            obb.ex = extents.x;
            obb.ey = extents.y;
            obb.ez = extents.z;

            obb.ox = orientation.x;
            obb.oy = orientation.y;
            obb.oz = orientation.z;
            obb.ow = orientation.w;
            NativeApi.SetBoundingVolumeOrientedBox(obb);
        }

        [ StructLayout( LayoutKind.Sequential )]
        struct WMRSphere
        {
            public float cx;
            public float cy;
            public float cz;
            public float r;
        }

        /// <summary>
        /// Set the Sphere Bounding Volume
        /// </summary>
        /// <param name="meshing">MeshSubsystem reference</param>
        /// <param name="center">Center point for the sphere</param>
        /// <param name="radius">Radius of the sphere</param>
        public static void SetBoundingVolumeSphere(this XRMeshSubsystem meshing, Vector3 center, float radius)
        {
            WMRSphere sbb;
            sbb.cx = center.x;
            sbb.cy = center.y;
            sbb.cz = center.z;
            sbb.r = radius;
            NativeApi.SetBoundingVolumeSphere(sbb);
        }

        /// <summary>Mesh data for a surface</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MeshingData
        {
            /// <summary>Mesh data version</summary>
            public int version;

            /// <summary>Surface info object</summary>
            [MarshalAs(UnmanagedType.IUnknown)]
            public System.Object surfaceInfo;

            /// <summary>Mesh object</summary>
            [MarshalAs(UnmanagedType.IUnknown)]
            public System.Object surfaceMesh;
        }

        /// <summary>
        /// Get mesh data
        /// </summary>
        /// <param name="meshing">MeshSubsystem reference</param>
        /// <param name="meshId">Id of the mesh to aquire data for</param>
        /// <param name="data">Mesh data out</param>
        public static void GetMeshingDataForMesh(this XRMeshSubsystem meshing, UnityEngine.XR.MeshId meshId, out MeshingData data)
        {
            NativeApi.GetMeshingDataForMesh(meshId, out data);
        }

        /// <summary>
        /// Release mesh data
        /// </summary>
        /// <param name="meshing">MeshSubsystem reference</param>
        /// <param name="data">Data to release</param>
        public static void ReleaseMeshingData(this XRMeshSubsystem meshing, ref MeshingData data)
        {
            NativeApi.ReleaseMeshingData(ref data);
        }

        static class NativeApi
        {
        #if UNITY_EDITOR
            [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
        #else
        #if ENABLE_DOTNET
            [DllImport("WindowsMRXRSDK.dll")]
        #else
            [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
        #endif
        #endif
            public static extern void GetMeshingDataForMesh(UnityEngine.XR.MeshId id, [Out] out MeshingData data);


        #if UNITY_EDITOR
            [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
        #else
        #if ENABLE_DOTNET
            [DllImport("WindowsMRXRSDK.dll")]
        #else
            [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
        #endif
        #endif
            public static extern void ReleaseMeshingData([In,Out] ref MeshingData data);


        #if UNITY_EDITOR
            [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
        #else
        #if ENABLE_DOTNET
            [DllImport("WindowsMRXRSDK.dll")]
        #else
            [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
        #endif
        #endif
            public static extern void SetBoundingVolumeFrustum([In] WMRPlane[] planes, int size);

        #if UNITY_EDITOR
            [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
        #else
        #if ENABLE_DOTNET
            [DllImport("WindowsMRXRSDK.dll")]
        #else
            [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
        #endif
        #endif
           public static extern void SetBoundingVolumeOrientedBox(WMROrientedBox planes);

        #if UNITY_EDITOR
            [DllImport("Packages/com.unity.xr.windowsmr/Runtime/Plugins/x64/WindowsMRXRSDK.dll", CharSet = CharSet.Auto)]
        #else
        #if ENABLE_DOTNET
            [DllImport("WindowsMRXRSDK.dll")]
        #else
            [DllImport("WindowsMRXRSDK", CharSet=CharSet.Auto)]
        #endif
        #endif
            public static extern void SetBoundingVolumeSphere(WMRSphere planes);

        }

    }
}
