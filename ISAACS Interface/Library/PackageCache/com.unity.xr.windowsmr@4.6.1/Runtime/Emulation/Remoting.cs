using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

using UnityEngine.XR.WindowsMRInternals;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_2019_3_OR_NEWER && !UNITY_2020_2_OR_NEWER
using UnityEngineInternal.XR.WSA;
#endif


namespace UnityEngine.XR.WindowsMR
{
    // Must match ConnectionFlags in native code
    /// <summary>Windows Mixed Reality Remoting Connection flags</summary>
    public enum ConnectionFlags
    {
        /// <summary>No Audio or Video available when connected</summary>
        None = 0,
        /// <summary>Audio available when connected</summary>
        EnableAudio = 1 << 0,
        /// <summary>Video available when connected</summary>
        EnableVideo = 1 << 1,
    }

    // Do not change this enum, it matches what's inside a DLL we call into
    /// <summary>Remoting connection failure reasons</summary>
    public enum ConnectionFailureReason
    {
        /// <summary>No failure</summary>
        None = 0,
        /// <summary>Unknown failure</summary>
        Unknown = 1,
        /// <summary>No certificate found on the server</summary>
        NoServerCertificate = 2,
        /// <summary>Port busy or already in use</summary>
        HandshakePortBusy = 3,
        /// <summary>Unable to find a device to connect to</summary>
        HandshakeUnreachable = 4,
        /// <summary>Handshake connection failed</summary>
        HandshakeConnectionFailed = 5,
        /// <summary>User Authentication failed</summary>
        AuthenticationFailed = 6,
        /// <summary>Mismatched versions between app and Holographic Remoting Player</summary>
        RemotingVersionMismatch = 7,
        /// <summary>Incompatible transport protocols used</summary>
        IncompatibleTransportProtocols = 8,
        /// <summary>Handshake failed</summary>
        HandshakeAttemptFailed = 9,
        /// <summary>Transport protocol port busy or already in use</summary>
        TransportPortBusy = 10,
        /// <summary>Transport protocol could not find device at port specified</summary>
        TransportUnreachable = 11,
        /// <summary>Transport protocol connection failed</summary>
        TransportConnectionFailed = 12,
        /// <summary>Transport protocol version mismatch between app and Holographic Remoting Player</summary>
        ProtocolVersionsMismatch = 13,
        /// <summary>Transport protocol error</summary>
        ProtocolError = 14,
        /// <summary>Video Codec not supported</summary>
        VideoCodecNotAvailable = 15,
        /// <summary>Connection cancelled</summary>
        Canceled = 16,
        /// <summary>Connection lost</summary>
        ConnectionWasLost = 17,
        /// <summary>Device lost</summary>
        DeviceLost = 18,
        /// <summary>Manual disconnect requested</summary>
        DisconnectRequest = 19,

        /// <summary>Obsolete and should not be used.</summary>
        [Obsolete("This enumeration value is obsolete.", false)]
        Unreachable = 2,

        /// <summary>Obsolete and should not be used.</summary>
        [Obsolete("This enumeration value is obsolete.", false)]
        HandshakeFailed = 3,

        /// <summary>Obsolete and should not be used.</summary>
        [Obsolete("This enumeration value is obsolete.", false)]
        ProtocolVersionMismatch = 4,

        /// <summary>Obsolete and should not be used.</summary>
        [Obsolete("This enumeration value is obsolete.", false)]
        ConnectionLost = 5,

    }

    // Do not change this enum, it matches what's inside a DLL we call into
    /// <summary>Holographic Remoting connection state</summary>
    public enum ConnectionState
    {
        /// <summary>Remoting Disconnected</summary>
        Disconnected,
        /// <summary>Remoting Connecting</summary>
        Connecting,
        /// <summary>Remoting Connected</summary>
        Connected,
    }

    /// <summary>Windows Mixed Reality Class used to manage Remoting within the application</summary>
    public static class WindowsMRRemoting
    {
        static WindowsMRRemoting()
        {
            WindowsMRInternal.Init();

        }


        /// <summary>Enable Audio when remoting</summary>
        public static bool isAudioEnabled
        {
            get
            {
                return UnityWindowsMR_Remoting_IsAudioEnabled();
            }
            set
            {
                UnityWindowsMR_Remoting_SetAudioEnabled(value);
            }
        }

        /// <summary>Enable Video when remoting</summary>
        public static bool isVideoEnabled
        {
            get
            {
                return UnityWindowsMR_Remoting_IsVideoEnabled();
            }
            set
            {
                UnityWindowsMR_Remoting_SetVideoEnabled(value);
            }
        }

        /// <summary>The maximum bit rate (in Kbps) for sending data between application and device when Remoting</summary>
        public static int maxBitRateKbps
        {
            get
            {
                return UnityWindowsMR_Remoting_GetMaxBitRateKbps();
            }
            set
            {
                UnityWindowsMR_Remoting_SetMaxBitRateKbps(value);
            }
        }

        /// <summary>IP address of the device to connect to</summary>
        public static string remoteMachineName
        {
            get
            {
                var sb = new System.Text.StringBuilder(256);
                UnityWindowsMR_Remoting_GetRemoteMachineName(sb, sb.Capacity);
                return sb.ToString();
            }
            set
            {
                UnityWindowsMR_Remoting_SetRemoteMachineName(value);
            }
        }

        /// <summary>Active remoting connection established</summary>
        public static bool isConnected
        {
            get
            {
                return UnityWindowsMR_Remoting_IsConnected();
            }
        }

        private static void EnableRemoteSpeech()
        {
#if UNITY_2019_3_OR_NEWER && !UNITY_2020_2_OR_NEWER
#pragma warning disable 0618
            RemoteSpeechAccess.EnableRemoteSpeech(UnityEngine.XR.WSA.RemoteDeviceVersion.V2);
#pragma warning restore 0618
#endif
        }

        /// <summary>
        /// Setup a listening port for a server to attempt to connect
        /// to the client application on the device.
        /// </summary>
        public static void Listen()
        {
            EnableRemoteSpeech();
            // throw exception on failed connection?
            Debug.Log("Remoting listen returned: " + UnityWindowsMR_Remoting_TryListen());
        }

        /// <summary>
        /// Callback server API used to request validation of the token sent from the client. Passed to <see cref="Listen"/>
        /// to be used later to request that a token be validated as authentic.
        ///
        /// See <see href="https://docs.microsoft.com/en-us/windows/mixed-reality/develop/platform-capabilities-and-apis/holographic-remoting-secure-connection">Microsoft documentation</see>
        /// about secure Holographic Remoting Connections for how a secure connection is setup and used.
        /// </summary>
        /// <param name="token">Token value passed to the <see cref="Listen"/> as part of the <see cref="SecureListenData.token" />.</param>
        /// <param name="tokenToCheck">Shared secret that needs to be validated.</param>
        public delegate bool ValidateToken([MarshalAs(UnmanagedType.LPWStr)]string token, [MarshalAs(UnmanagedType.LPWStr)]string tokenToCheck);

        /// <summary>
        /// Used to pass vertificate and authentication validation to the remoting runtime when
        /// remoting is setup using the secure <see cref="Listn"/> API.
        ///
        /// See <see href="https://docs.microsoft.com/en-us/windows/mixed-reality/develop/platform-capabilities-and-apis/holographic-remoting-secure-connection">Microsoft documentation</see>
        /// about secure Holographic Remoting Connections for how a secure connection is setup and used.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct SecureListenData
        {
            /// <summary>
            /// Byte array that contains the encrypted certificate information that the secure <see cref="Listen" />
            /// API will use to validate it's authenticity with the client.
            /// </summary>
            [MarshalAs(UnmanagedType.LPArray)]
            public byte[] certificate;

            /// <summary>
            /// Count of bytes in the <see cref="certificate"/> array.
            /// </summary>
            public uint certificateByteCount;

            /// <summary>
            /// The subject of the certificate. Used to securely retrieve the certificate information
            /// from the <see cref="certificate"/>.
            /// 
            /// Must match the subject name that was used to create the certificate data passed in <see cref="certificate" />.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            public string certificateSubject;

            /// <summary>
            /// The password used to securely retrieve the certificate information
            /// from the <see cref="certificate"/>.
            /// 
            /// Must match the password that was used to create the certificate data passed in <see cref="certificate" />.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            public string certificatePassword;

            /// <summary>
            /// The Realm of the authentication provider set up with the secure <see cref="Listen"/> API.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            public string realm;

            /// <summary>
            /// Shared token used to authenticate the id of the account opening the secure connection.
            /// 
            /// Not required if the a validation callback delegate is provided to the <see cref="Listen"/> API call.
            /// 
            /// If this is empty, and the validation callback is not used then a secure connection will not
            /// be established and the Listen call will fallback to non-secure listen mode.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            public string token;
        }

        /// <summary>
        /// Setup a listening port for a server to attempt to connect
        /// to the client application on the device.
        ///
        /// Will used the passed in data to create a secure network connection
        /// and to authenticate the client.
        /// 
        /// If <see cref="SecureListenData.token"/> is empty, and the validation callback is not used then a secure connection will not
        /// be established and the Listen call will fallback to non-secure listen mode.
        /// </summary>
        /// <param name="data">Instance of <see cref="SecureListenData"/> with information for validation and authentication.</param>
        /// <param name="validateToken">The <see cref="ValidationToken" /> callback used to request validation of the authentication token sent from the client.</param>
        public static void Listen(SecureListenData data, ValidateToken validateToken)
        {
            EnableRemoteSpeech();
            // throw exception on failed connection?
            Debug.Log("Remoting listen returned: " + UnityWindowsMR_Remoting_TryListenSecure(data, validateToken));

        }

        /// <summary>Attempt to connect to the Holographic Remoting Player on the device specified by the remoteMachineName</summary>
        public static void Connect()
        {
            EnableRemoteSpeech();

            // throw exception on failed connection?
            Debug.Log("Remoting connect returned: " + UnityWindowsMR_Remoting_TryConnect());
        }

        /// <summary>
        /// Attempts to create a secure connection to the server.
        ///
        /// See <see href="https://docs.microsoft.com/en-us/windows/mixed-reality/develop/platform-capabilities-and-apis/holographic-remoting-secure-connection">Microsoft documentation</see>
        /// about secure Holographic Remoting Connections for how a secure connection is setup and used.
        /// </summary>
        /// <param name="token">Shared secret used to authenticate the client with the server.</param>
        public static void Connect(string token)
        {
            EnableRemoteSpeech();

            // throw exception on failed connection?
            Debug.Log("Remoting connect returned: " + UnityWindowsMR_Remoting_TryConnectSecure(token));
        }

        /// <summary>Disconnect any active remoting connections</summary>
        public static void Disconnect()
        {
            // throw exception on failed disconnection?
            UnityWindowsMR_Remoting_TryDisconnect();

#if UNITY_2019_3_OR_NEWER && !UNITY_2020_2_OR_NEWER
#pragma warning disable 0618
            RemoteSpeechAccess.DisableRemoteSpeech();
#pragma warning restore 0618
#endif
        }

        /// <summary>Try to get the current state of the remoting connection</summary>
        /// <param name="connectionState">The current state of the connection</param>
        /// <returns>True if the connection state was able to be acquired</returns>
        public static bool TryGetConnectionState(out ConnectionState connectionState)
        {
            connectionState = ConnectionState.Disconnected;
            return UnityWindowsMR_Remoting_TryGetConnectionState(ref connectionState);
        }

        /// <summary>Try to get the failure reason of the remoting connection</summary>
        /// <param name="connectionFailureReason">The reason the remoting connection failed</param>
        /// <returns>True if the connection failure reason was able to be acquired</returns>
        public static bool TryGetConnectionFailureReason(out ConnectionFailureReason connectionFailureReason)
        {
            connectionFailureReason = ConnectionFailureReason.None;
            return UnityWindowsMR_Remoting_TryGetConnectionFailureReason(ref connectionFailureReason);
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WINRT
        [DllImport("WindowsMRXRSDK")]
        public static extern void UnityWindowsMR_Remoting_SetVideoEnabled(bool video);

        [DllImport("WindowsMRXRSDK")]
        public static extern bool UnityWindowsMR_Remoting_IsVideoEnabled();

        [DllImport("WindowsMRXRSDK")]
        public static extern void UnityWindowsMR_Remoting_SetAudioEnabled(bool audio);

        [DllImport("WindowsMRXRSDK")]
        public static extern bool UnityWindowsMR_Remoting_IsAudioEnabled();

        [DllImport("WindowsMRXRSDK")]
        public static extern void UnityWindowsMR_Remoting_SetMaxBitRateKbps(int kbps);

        [DllImport("WindowsMRXRSDK")]
        public static extern int UnityWindowsMR_Remoting_GetMaxBitRateKbps();

        [DllImport("WindowsMRXRSDK")]
        public static extern void UnityWindowsMR_Remoting_SetRemoteMachineName([MarshalAs(UnmanagedType.LPWStr)] string clientMachineName);

        [DllImport("WindowsMRXRSDK")]
        public static extern void UnityWindowsMR_Remoting_GetRemoteMachineName([MarshalAs(UnmanagedType.LPWStr)]System.Text.StringBuilder sb, int capacity);

        [DllImport("WindowsMRXRSDK")]
        public static extern bool UnityWindowsMR_Remoting_IsConnected();

        [DllImport("WindowsMRXRSDK")]
        public static extern bool UnityWindowsMR_Remoting_TryGetConnectionState(ref ConnectionState connectionState);

        [DllImport("WindowsMRXRSDK")]
        public static extern bool UnityWindowsMR_Remoting_TryGetConnectionFailureReason(ref ConnectionFailureReason connectionFailureReason);

        [DllImport("WindowsMRXRSDK")]
        public static extern bool UnityWindowsMR_Remoting_TryConnect();

        [DllImport("WindowsMRXRSDK")]
        public static extern bool UnityWindowsMR_Remoting_TryConnectSecure([MarshalAs(UnmanagedType.LPWStr)]string token);

        [DllImport("WindowsMRXRSDK")]
        public static extern bool UnityWindowsMR_Remoting_TryListen();

        [DllImport("WindowsMRXRSDK")]
        public static extern bool UnityWindowsMR_Remoting_TryListenSecure(SecureListenData data, ValidateToken validateToken);

        [DllImport("WindowsMRXRSDK")]
        public static extern bool UnityWindowsMR_Remoting_TryDisconnect();
#else
        static void UnityWindowsMR_Remoting_SetVideoEnabled(bool video)
        {
        }

        static bool UnityWindowsMR_Remoting_IsVideoEnabled()
        {
            return false;
        }

        static void UnityWindowsMR_Remoting_SetAudioEnabled(bool audio)
        {
        }

        static bool UnityWindowsMR_Remoting_IsAudioEnabled()
        {
            return false;
        }

        static void UnityWindowsMR_Remoting_SetMaxBitRateKbps(int kbps)
        {
        }

        static int UnityWindowsMR_Remoting_GetMaxBitRateKbps()
        {
            return -1;
        }

        static void UnityWindowsMR_Remoting_SetRemoteMachineName(string clientMachineName)
        {
        }

        static void UnityWindowsMR_Remoting_GetRemoteMachineName(System.Text.StringBuilder sb, int capacity)
        {
        }

        static bool UnityWindowsMR_Remoting_IsConnected()
        {
            return false;
        }

        static bool UnityWindowsMR_Remoting_TryGetConnectionState(ref ConnectionState connectionState)
        {
            return false;
        }

        static bool UnityWindowsMR_Remoting_TryGetConnectionFailureReason(ref ConnectionFailureReason connectionFailureReason)
        {
            return false;
        }

        static bool UnityWindowsMR_Remoting_TryConnect()
        {
            return false;
        }

        static bool UnityWindowsMR_Remoting_TryConnectSecure(string token)
        {
            return false;
        }

        static bool UnityWindowsMR_Remoting_TryListen()
        {
            return false;
        }

        static bool UnityWindowsMR_Remoting_TryListenSecure(SecureListenData data, ValidateToken validateToken)
        {
            return false;
        }

        static bool UnityWindowsMR_Remoting_TryDisconnect()
        {
            return false;
        }
#endif
    }
}
