using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.WindowsMR
{
    /// <summary>Runtime settings for this XR Plugin.</summary>
    public class WindowsMRSettings : ScriptableObject
    {
        /// <summary>Options for Depth Buffer Format</summary>
        public enum DepthBufferOption
        {
            /// <summary>16 bit depth buffer format</summary>
            DepthBuffer16Bit,
            /// <summary>24 bit depth buffer format</summary>
            DepthBuffer24Bit
        }

        /// <summary>If using a shared depth buffer, this is the type of the depth buffer we should use.</summary>
        [SerializeField, Tooltip("Set the size of the depth buffer")]
        public DepthBufferOption DepthBufferFormat;

        /// <summary>True if we want to use a shared depth buffer, false otherwise.</summary>
        [SerializeField, Tooltip("Enable depth buffer sharing")]
        public bool UseSharedDepthBuffer;

#if !UNITY_EDITOR
        internal static WindowsMRSettings s_Settings;

        /// <summary>Unity Awake callback for initialization of the object.</summary>
        public void Awake()
        {
            s_Settings = this;
        }
#endif
    }
}
