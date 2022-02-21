# Windows MR XR SDK Plug-in Remoting Sample

This sample demonstrates how to connect to a remote HoloLensV2 device using the new Windows MR XR SDK Plug-in.

## Pre-requisites to using this sample.

Before you can test remoting, you need to make sure that your project is properly setup for it.

* You must disable automatic startup of XR SDK from XR Plug-in Management. To do this, navigate to `Project Settings` -> `XR Plug-in Management`, select the target platform you are going to run on and disable `Initialize XR on Startup`.
* Make sure that Windows Mixed Reality is enabled. navigate to `Project Settings` -> `XR Plug-in Management`, select the target platform you are going to run on and toggle on `Windows Mixed Reality`.
* Make sure that remoting is enabled. Navigate to `Project Settings` -> `XR Plug-in Management` -> `Windows Mixed Reality`, select the target platform you are going to run on and then enable `Holographic Remoting` under the `Build Settings` section.
* For UWP you must disable primary window on startup. Navigate to `Project Settings` -> `XR Plug-in Management` -> `Windows Mixed Reality`, select the target platform you are going to run on and then disable `Force Primary Window on Startup` under the `Build Settings` section.

## Running the test scene

When you run the test scene, remoting will not connect automatically. The sample script is setup to connect and disconnect only on user input. To connect press the 'C' key. To disconnect press the 'D' key.
