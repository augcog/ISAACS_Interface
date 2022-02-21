using System.Collections;
using UnityEngine;

using UnityEngine.XR.WindowsMR;
using UnityEngine.XR.Management;

/// <summary>
/// Sample component to configure and manage a remote connection to a HoloLens v2 device.
/// </summary>
public class RemotingConnect : MonoBehaviour
{

    /// <summary>
    /// Enabled audio to/from the connected device. This is especially important for user voice input.
    /// </summary>
    public bool enableAudio = true;

    /// <summary>
    /// Enable the streaming of video to/from device.
    /// </summary>
    public bool enableVideo = true;

    /// <summary>
    /// Maximum data rate you want to use for transmission.
    /// </summary>
    public int maxBitRate = 9999;

    /// <summary>
    /// The name of the HoloLensV2 device to connect to.
    /// </summary>
    public string machineName;

    /// <summary>
    /// The number of connection retries we will attempt.
    /// </summary>
    public int connectionRetryCount = 30;

    /// <summary>
    /// Connect to the remote device.
    /// </summary>
    public void Connect()
    {
        if (WindowsMRRemoting.TryGetConnectionState(out var connectionState) && connectionState == ConnectionState.Connected)
            return;

        // Connection can take a long time, especially since we need to also spin up
        // XR SDK once connected. For that reason we do most of the work in a CoRoutine.
        StartCoroutine(TryConnect());
    }

    /// <summary>
    /// Disconnect from the remove device.
    /// </summary>
    public void Disconnect()
    {
        if (WindowsMRRemoting.TryGetConnectionState(out var connectionState) && connectionState != ConnectionState.Connected)
            return;

        // Disconnecting can also take some time given that we need to shut down the XR SDK session.
        StartCoroutine(TryDisconnect());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Attempting to connect...");

            Connect();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Disconnect();

            Debug.Log("Attempting to disconnect...");
        }
    }

    IEnumerator TryConnect()
    {
        // Setup the connection state information required to make a connection
        // to the remote device.
        WindowsMRRemoting.isAudioEnabled = enableAudio;
        WindowsMRRemoting.isVideoEnabled = enableVideo;
        WindowsMRRemoting.maxBitRateKbps = maxBitRate;
        WindowsMRRemoting.remoteMachineName = machineName;

        WindowsMREmulation.mode = WindowsMREmulationMode.Remoting;

        Debug.Log($"Connecting to {machineName}...");

        WindowsMRRemoting.Connect();

        yield return new WaitForEndOfFrame();
        ConnectionState connectionState = ConnectionState.Disconnected;

        int connectionTryCount = 0;

        // If connection fails, we'll try again, up to the number of times specified
        while (WindowsMRRemoting.TryGetConnectionState(out connectionState) && connectionTryCount < connectionRetryCount)
        {
            connectionTryCount++;
            if (connectionState == ConnectionState.Connecting)
            {
                Debug.Log($"Still connecting to {machineName}...");
                yield return new WaitForSeconds(1);
                continue;
            }
            break;
        }

        switch (connectionState)
        {
            case ConnectionState.Connected:
                Debug.Log($"Successfully connected to {machineName} after {connectionTryCount} seconds.");

                // We've connected, so now we need to spin up XR SDK.
                yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
                if (XRGeneralSettings.Instance.Manager.activeLoader != null)
                    XRGeneralSettings.Instance.Manager.activeLoader.Start();
                break;

            case ConnectionState.Disconnected:
                Debug.LogError($"Unable to connect to {machineName} after {connectionTryCount} seconds");
                ConnectionFailureReason failureReason = ConnectionFailureReason.None;
                WindowsMRRemoting.TryGetConnectionFailureReason(out failureReason);
                Debug.LogError($"Connection Failure Reason {failureReason}");
                break;
        }
    }

    IEnumerator TryDisconnect()
    {

        ConnectionState connectionState = ConnectionState.Disconnected;
        if (WindowsMRRemoting.TryGetConnectionState(out connectionState) && connectionState == ConnectionState.Connected)
        {
            // If we have a live XR SDK session, we need to shut that before we can disconnect the remote session.
            if (XRGeneralSettings.Instance.Manager.activeLoader != null)
            {
                XRGeneralSettings.Instance.Manager.activeLoader.Stop();
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            }

            WindowsMRRemoting.Disconnect();
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }
}
