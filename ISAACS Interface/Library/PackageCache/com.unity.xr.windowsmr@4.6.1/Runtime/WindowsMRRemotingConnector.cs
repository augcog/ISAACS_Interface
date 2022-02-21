using System;
using System.Collections;

using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.WindowsMR;

using UnityEditor;

namespace UnityEngine.XR.WindowsMR
{
    internal class WindowsMRRemotingConnector : MonoBehaviour
    {
        string m_RemoteMachineName = "";
        bool m_EnableAudio = false;
        bool m_EnableVideo = false;
        int m_MaxBitRateKbps = 0;
        bool m_Listen = false;

        IEnumerator StartRemotingSession()
        {
            var xrManager = XRGeneralSettings.Instance?.Manager ?? null;

            if (xrManager != null && xrManager.activeLoader != null)
            {
                xrManager.StopSubsystems();
                xrManager.DeinitializeLoader();
            }

            yield return null;

            yield return TryConnect();

        }

        IEnumerator TryConnect()
        {
            WindowsMRRemoting.remoteMachineName = m_RemoteMachineName;
            WindowsMRRemoting.isAudioEnabled = m_EnableAudio;
            WindowsMRRemoting.isVideoEnabled = m_EnableVideo;
            WindowsMRRemoting.maxBitRateKbps = m_MaxBitRateKbps;

            if (m_Listen)
            {
                WindowsMRRemoting.Listen();
            }
            else
            {
                WindowsMRRemoting.Connect();
            }

            yield return new WaitForEndOfFrame();
            ConnectionState connectionState = ConnectionState.Disconnected;

            int connectionTryCount = 0;

            while (WindowsMRRemoting.TryGetConnectionState(out connectionState) && connectionTryCount < 30)
            {
                connectionTryCount++;
                if (connectionState == ConnectionState.Connecting)
                {
                    if (m_Listen)
                        Debug.Log($"Still listening for a connection request...");
                    else
                        Debug.Log($"Still connecting to {WindowsMRRemoting.remoteMachineName}...");
                    yield return new WaitForSeconds(1);
                    continue;
                }
                break;
            }

            switch (connectionState)
            {
                case ConnectionState.Connected:
                    if (m_Listen)
                        Debug.Log($"Connection request accepted...");
                    else
                        Debug.Log($"Successfully connected to {WindowsMRRemoting.remoteMachineName} after {connectionTryCount} seconds.");

                    yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
                    if (XRGeneralSettings.Instance.Manager.activeLoader != null)
                        XRGeneralSettings.Instance.Manager.activeLoader.Start();
                    break;

                case ConnectionState.Disconnected:
                    if (m_Listen)
                        Debug.Log($"Failure to get a connection request  after {connectionTryCount} seconds.");
                    else
                        Debug.LogError($"Unable to connect to {WindowsMRRemoting.remoteMachineName} after {connectionTryCount} seconds.");
                    ConnectionFailureReason failureReason = ConnectionFailureReason.None;
                    WindowsMRRemoting.TryGetConnectionFailureReason(out failureReason);
                    Debug.LogError($"Remoting Failure Reason {failureReason}");
                    break;
            }
        }

        public void StartRemotingConnection(string remoteMachineName, bool enableAudio, bool enableVideo, int maxBitRateKbps, bool listen = false)
        {
            m_RemoteMachineName = remoteMachineName;
            m_EnableAudio = enableAudio;
            m_EnableVideo = enableVideo;
            m_MaxBitRateKbps = maxBitRateKbps;
            m_Listen = listen;
            StartCoroutine(StartRemotingSession());
        }

        public void StopRemotingConnection()
        {
            var xrManager = XRGeneralSettings.Instance?.Manager ?? null;
            if (xrManager != null)
            {
                xrManager.StopSubsystems();
                xrManager.DeinitializeLoader();
            }
            WindowsMRRemoting.Disconnect();
        }
    }
}
