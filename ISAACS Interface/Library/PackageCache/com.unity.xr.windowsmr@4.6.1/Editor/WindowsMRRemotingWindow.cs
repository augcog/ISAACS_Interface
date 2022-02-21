using System;
using System.IO;

using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Management;
using UnityEngine.XR.WindowsMR;

using UnityEditor.XR.Management;

namespace UnityEditor.XR.WindowsMR
{

    [Serializable]
    class RemoteSettings : ScriptableObject
    {
        [SerializeField]
        public string m_RemoteMachineName = "";
        [SerializeField]
        public bool m_EnableAudio = true;
        [SerializeField]
        public bool m_EnableVideo = true;
        [SerializeField]
        public int m_MaxBitRateKbps = 99999;

        [SerializeField]
        [FormerlySerializedAs("autoConnectOnPlay")]
        public bool m_AutoConnectOnPlay = true;

        [SerializeField]
        public bool m_shouldListen = false;
    }


    /// <summary>
    /// Remoting Window Class, GUI content and callbacks to WMRRemoting apis.
    /// </summary>
    public class WindowsMRRemotingWindow : EditorWindow
    {
        /// <summary>
        /// Initializes the Remoting Window class
        /// </summary>
        [MenuItem("Window/XR/Windows XR Plugin Remoting")]
        public static void Init()
        {
            GetWindow<WindowsMRRemotingWindow>(false);
        }

        static GUIContent s_ConnectionStatusText = EditorGUIUtility.TrTextContent("Connection Status");
        static GUIContent s_EmulationModeText = EditorGUIUtility.TrTextContent("Emulation Mode");
        static GUIContent s_RemoteMachineText = EditorGUIUtility.TrTextContent("Remote Machine");
        static GUIContent s_EnableVideoText = EditorGUIUtility.TrTextContent("Enable Video");
        static GUIContent s_EnableAudioText = EditorGUIUtility.TrTextContent("Enable Audio");
        static GUIContent s_MaxBitrateText = EditorGUIUtility.TrTextContent("Max Bitrate (kbps)");
        static GUIContent s_ShouldListenText = EditorGUIUtility.TrTextContent("Listen for connection");
        static GUIContent s_AutoConnectOnPlay = new GUIContent("Connect on Play", null, "When enabled, will auto connect remoting when the Editor enters play mode.");
        static GUIContent s_AutoListenOnPlay = new GUIContent("Listen on Play", null, "When enabled, will automatically create a listening remoting server when the Editor enters play mode.");
        static GUIContent s_ConnectionButtonConnectText = EditorGUIUtility.TrTextContent("Connect");
        static GUIContent s_ConnectionButtonListenText = EditorGUIUtility.TrTextContent("Listen");
        static GUIContent s_ConnectionButtonDisconnectText = EditorGUIUtility.TrTextContent("Disconnect");

        static GUIContent s_ConnectionStateDisconnectedText = EditorGUIUtility.TrTextContent("Disconnected");
        static GUIContent s_ConnectionStateConnectingText = EditorGUIUtility.TrTextContent("Connecting");
        static GUIContent s_ConnectionStateListeningText = EditorGUIUtility.TrTextContent("Listening");
        static GUIContent s_ConnectionStateConnectedText = EditorGUIUtility.TrTextContent("Connected");

        static GUIContent s_RemotingSettingsReminder = EditorGUIUtility.TrTextContent("The Editor uses player settings from the 'Standalone' platform for play mode and a remoting connection can be established without 'Windows Mixed Reality' enabled.");
        static GUIContent s_AutoRemotingConnectionHelp = EditorGUIUtility.TrTextContent("Remoting will connect/disconnect automatically when you enter/exit play mode in editor.");
        static GUIContent s_ManualRemotingConnectionHelp = EditorGUIUtility.TrTextContent("Remoting connection is manually handled by user. Connection will automatically be disconnected when exiting play mode.");


        GUIContent labelContent;
        GUIContent buttonContent;
        ConnectionState connectionState;
        ConnectionState previousConnectionState = ConnectionState.Disconnected;

        static GUIContent[] s_ModeStrings = new GUIContent[]
        {
            EditorGUIUtility.TrTextContent("None"),
            EditorGUIUtility.TrTextContent("Remote to Device")
        };

        static WindowsMRRemotingConnector s_Connector = null;

        static WindowsMRRemotingConnector GetConnector()
        {
            if (s_Connector == null)
            {
                var go = GameObject.Find("~wmrconnector");
                if (go == null)
                {
                    go = new GameObject("~wmrconnector");
                    go.hideFlags = HideFlags.HideAndDontSave;
                }
                s_Connector = go.AddComponent<WindowsMRRemotingConnector>();
            }
            return s_Connector;
        }

        bool m_IsListening = false;

        static RemoteSettings s_RemoteSettings = null;

        static string s_SettingsPath => Path.Combine(Application.dataPath, "../UserSettings");
        const string s_SettingsFileName = "WindowsMRRemoteSettings.asset";

        internal static void LoadSettings()
        {
            if (s_RemoteSettings == null)
            {
                s_RemoteSettings = ScriptableObject.CreateInstance(typeof(RemoteSettings)) as RemoteSettings;
            }

            if (s_RemoteSettings == null)
                return;

            string packageInitPath = Path.Combine(s_SettingsPath, s_SettingsFileName);

            if (File.Exists(packageInitPath))
            {
                using (StreamReader sr = new StreamReader(packageInitPath))
                {
                    string settings = sr.ReadToEnd();
                    JsonUtility.FromJsonOverwrite(settings, s_RemoteSettings);
                }
            }
        }


        internal static void SaveSettings()
        {
            if (s_RemoteSettings == null)
                return;

            string packageInitPath = Path.Combine(s_SettingsPath, s_SettingsFileName);

            if (!Directory.Exists(s_SettingsPath))
                Directory.CreateDirectory(s_SettingsPath);

            using (StreamWriter sw = new StreamWriter(packageInitPath))
            {
                string settings = JsonUtility.ToJson(s_RemoteSettings, true);
                sw.Write(settings);
            }
        }

        public void Awake()
        {
            titleContent = EditorGUIUtility.TrTextContent("Windows Mixed Reality");
            LoadSettings();
        }

        void OnEnable()
        {
            LoadSettings();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public void OnDisable()
        {
            SaveSettings();
        }

        public void OnDestroy()
        {
            if (WindowsMRRemoting.isConnected)
            {
                Debug.LogWarning("Remoting window closed while connected. Closing remoting connection.");
                Disconnect();
            }
            SaveSettings();
        }

        void OnLostFocus()
        {
            SaveSettings();
        }

        static void Disconnect()
        {
            if (WindowsMREmulation.mode == WindowsMREmulationMode.Remoting && WindowsMRRemoting.isConnected)
            {
                var connector = GetConnector();
                if (EditorApplication.isPlaying && s_RemoteSettings.m_AutoConnectOnPlay && connector)
                {
                    connector.StopRemotingConnection();
                    GameObject.Destroy(s_Connector);
                    s_Connector = null;
                }
                else
                {
                    WindowsMRRemoting.Disconnect();
                }
            }
        }

        static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        {
            var connector = GetConnector();

            switch (stateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    LoadSettings();
                    break;

                case PlayModeStateChange.ExitingEditMode:
                    SaveSettings();
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    LoadSettings();
                    if (WindowsMREmulation.mode == WindowsMREmulationMode.Remoting && connector && s_RemoteSettings.m_AutoConnectOnPlay)
                    {
                        if (!s_RemoteSettings.m_shouldListen && string.IsNullOrEmpty(s_RemoteSettings.m_RemoteMachineName))
                        {
                            Debug.LogError("Atempting to initiate remoting connection with no valid machine name set.");
                            return;
                        }
                        connector.StartRemotingConnection(
                            s_RemoteSettings.m_RemoteMachineName,
                            s_RemoteSettings.m_EnableAudio,
                            s_RemoteSettings.m_EnableVideo,
                            s_RemoteSettings.m_MaxBitRateKbps,
                            s_RemoteSettings.m_shouldListen);
                    }

                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    Disconnect();
                    break;
            }
        }

        void DrawEmulationModeOnGUI()
        {
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            EditorGUI.BeginChangeCheck();
            WindowsMREmulationMode previousMode = WindowsMREmulation.mode;
            WindowsMREmulationMode currentMode = (WindowsMREmulationMode)EditorGUILayout.Popup(s_EmulationModeText, (int)previousMode, s_ModeStrings);
            if (EditorGUI.EndChangeCheck())
            {
                if (previousMode == WindowsMREmulationMode.Remoting)
                    WindowsMRRemoting.Disconnect();
                WindowsMREmulation.mode = currentMode;
            }
            EditorGUI.EndDisabledGroup();
        }

        void UpdateConnectionStatus()
        {
            if (!WindowsMRRemoting.TryGetConnectionState(out connectionState))
            {
                Debug.Log("Failed to get connection state! Exiting remoting-window drawing.");
                return;
            }

            if (previousConnectionState == ConnectionState.Connecting && connectionState == ConnectionState.Disconnected && !s_RemoteSettings.m_shouldListen)
            {
                ConnectionFailureReason failureReason;
                WindowsMRRemoting.TryGetConnectionFailureReason(out failureReason);

                Debug.Log("Remoting Failure Reason: " + failureReason);
            }

            if (previousConnectionState != connectionState)
            {
                previousConnectionState = connectionState;
                Repaint();
            }

            switch (connectionState)
            {
                case ConnectionState.Disconnected:
                default:
                    m_IsListening = false;
                    labelContent = s_ConnectionStateDisconnectedText;
                    buttonContent = s_ConnectionButtonConnectText;
                    break;

                case ConnectionState.Connecting:
                    m_IsListening = s_RemoteSettings.m_shouldListen;
                    labelContent = s_RemoteSettings.m_shouldListen ? s_ConnectionStateListeningText : s_ConnectionStateConnectingText;
                    buttonContent = s_ConnectionButtonDisconnectText;
                    Repaint();
                    break;

                case ConnectionState.Connected:
                    m_IsListening = false;
                    labelContent = s_ConnectionStateConnectedText;
                    buttonContent = s_ConnectionButtonDisconnectText;
                    break;
            }
        }

        void HandleManualConnect()
        {
            if (!s_RemoteSettings.m_AutoConnectOnPlay)
            {
                if (connectionState != ConnectionState.Connected)
                {
                    bool shouldDisableButton = false;

                    if (!s_RemoteSettings.m_shouldListen)
                        shouldDisableButton = (string.IsNullOrEmpty(s_RemoteSettings.m_RemoteMachineName) ||
                                EditorApplication.isPlayingOrWillChangePlaymode ||
                                connectionState != ConnectionState.Disconnected);

                    EditorGUILayout.Space();
                    EditorGUI.BeginDisabledGroup(shouldDisableButton);
                    if (GUILayout.Button(
                            m_IsListening ? s_ConnectionButtonDisconnectText :
                                    s_RemoteSettings.m_shouldListen ? s_ConnectionButtonListenText : s_ConnectionButtonConnectText))
                    {
                        var connector = GetConnector();
                        if (connector)
                        {
                            if (!WindowsMRRemoting.TryGetConnectionState(out connectionState))
                            {
                                Debug.LogError("Failed to get connection state! Connection attempt terminated");
                                return;
                            }

                            WindowsMRRemoting.remoteMachineName = s_RemoteSettings.m_RemoteMachineName;
                            WindowsMRRemoting.isAudioEnabled = s_RemoteSettings.m_EnableAudio;
                            WindowsMRRemoting.isVideoEnabled = s_RemoteSettings.m_EnableVideo;
                            WindowsMRRemoting.maxBitRateKbps = s_RemoteSettings.m_MaxBitRateKbps;

                            if (s_RemoteSettings.m_shouldListen)
                            {
                                if (m_IsListening)
                                {
                                    WindowsMRRemoting.Disconnect();
                                }
                                else
                                {
                                    Debug.Log("Attempting to start listening for remoting connection...");
                                    WindowsMRRemoting.Listen();
                                }
                            }
                            else
                            {
                                Debug.Log("Attempting to start remoting...");
                                WindowsMRRemoting.Connect();
                            }
                            Repaint();
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    EditorGUILayout.Space();
                    EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode || connectionState != ConnectionState.Connected);
                    if (GUILayout.Button(s_ConnectionButtonDisconnectText))
                    {
                        var connector = GetConnector();
                        if (connector)
                        {
                            Debug.Log("Attempting to stop remoting...");
                            WindowsMRRemoting.Disconnect();
                            Repaint();
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
        }

        void DrawConnectionState()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(s_ConnectionStatusText, "Button");
            float iconSize = EditorGUIUtility.singleLineHeight;
            Rect iconRect = GUILayoutUtility.GetRect(iconSize, iconSize, GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(labelContent);
            EditorGUILayout.EndHorizontal();
        }

        void DrawSettings()
        {

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(WindowsMRRemoting.isConnected);
            s_RemoteSettings.m_RemoteMachineName = EditorGUILayout.TextField(s_RemoteMachineText, s_RemoteSettings.m_RemoteMachineName);
            s_RemoteSettings.m_EnableAudio = EditorGUILayout.Toggle(s_EnableVideoText, s_RemoteSettings.m_EnableAudio);
            s_RemoteSettings.m_EnableVideo = EditorGUILayout.Toggle(s_EnableAudioText, s_RemoteSettings.m_EnableVideo);
            s_RemoteSettings.m_MaxBitRateKbps = EditorGUILayout.IntSlider(s_MaxBitrateText, s_RemoteSettings.m_MaxBitRateKbps, 1024, 99999);
            s_RemoteSettings.m_shouldListen = EditorGUILayout.Toggle(s_ShouldListenText, s_RemoteSettings.m_shouldListen);

            if (!s_RemoteSettings.m_shouldListen && m_IsListening)
                WindowsMRRemoting.Disconnect();

            EditorGUILayout.Space();

            s_RemoteSettings.m_AutoConnectOnPlay = EditorGUILayout.Toggle(s_RemoteSettings.m_shouldListen ? s_AutoListenOnPlay : s_AutoConnectOnPlay, s_RemoteSettings.m_AutoConnectOnPlay);
            if (s_RemoteSettings.m_AutoConnectOnPlay)
            {
                EditorGUILayout.HelpBox(s_AutoRemotingConnectionHelp);
            }
            else
            {
                EditorGUILayout.HelpBox(s_ManualRemotingConnectionHelp);
            }
            EditorGUI.EndDisabledGroup();

        }

        void DrawRemotingOnGUI()
        {
            EditorGUILayout.HelpBox(s_RemotingSettingsReminder);

            DrawSettings();

            UpdateConnectionStatus();

            HandleManualConnect();

            DrawConnectionState();
        }

        void OnGUI()
        {
            DrawEmulationModeOnGUI();
            if (WindowsMREmulation.mode == WindowsMREmulationMode.Remoting)
                DrawRemotingOnGUI();
        }
    }
}
