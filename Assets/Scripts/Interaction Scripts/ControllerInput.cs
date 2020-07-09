namespace ISAACS_ControllerInput {

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
	using VRTK;


	/** This script is a wrapper around VRTK, to be used as a library for
	higher-level scripts needing access to user input coming from a controller.
	Initialize the ControllerState class as a singleton of the GameObject Controller.
	For further clarifications, please ask Apollo.
	*/

	public class ControllerInput : MonoBehaviour
	{
		[Header("Left Hand")]

		[Tooltip("The left controller GameObject, where VRTK scripts are attached. By default at 'Controller/LeftController'.")]
		public GameObject LeftController;
		[Tooltip("The UI attached to the left hand. By default at 'VRTK SDK Manager/Oculus SDK/Tracking Space/LeftHandAnchor/Left UI Panel'.")]
		public GameObject LeftUI;


		[Header("Right Hand")]

		[Tooltip("The right controller GameObject, where VRTK scripts are attached. By default at 'Controller/RightController'.")]
		public GameObject RightController;
		[Tooltip("The UI attached to the right hand. By default at 'VRTK SDK Manager/Oculus SDK/Tracking Space/RightHandAnchor/Right UI Sphere'.")]
		public GameObject RightUI;


		private VRTK_Pointer LeftPointer;                          // The VRTK Pointer Component (script) of the left controller.
		private VRTK_BezierPointerRenderer LeftPointerRenderer;    // The VRTK Pointer Renderer Component (script) of the left controller.
		private VRTK_Pointer RightPointer;                         // The VRTK Pointer Component (script) of the right controller.
		private VRTK_StraightPointerRenderer RightPointerRenderer; // The VRTK Pointer Renderer Component (script) of the right controller.
		private VRTK_UIPointer RightUIPointer;                     // The VRTK UI Pointer Component (script) of the right controller.

		private bool boolLeftPointerEnabled;  // Whether the left controller's pointer is enabled or disabled.
		private bool boolRightPointerEnabled; // Whether the right controller's pointer is enabled or disabled.


		// Awake is called on initialization, BEFORE Start.
		void Awake()
		{
            LeftPointer = LeftController.GetComponent<VRTK_Pointer>();
            LeftPointerRenderer = LeftController.GetComponent<VRTK_BezierPointerRenderer>();
            RightPointer = RightController.GetComponent<VRTK_Pointer>();
            RightPointerRenderer = RightController.GetComponent<VRTK_StraightPointerRenderer>();
            RightUIPointer = RightController.GetComponent<VRTK_UIPointer>();

			LeftPointerRenderer.enabled = false;
			LeftPointer.enabled = false;
			RightUIPointer.enabled = false;
			RightPointerRenderer.enabled = false;
			RightPointer.enabled = false;

			LeftUI.SetActive(false);
			RightUI.SetActive(false);
		}



		/******************************/
		//  LEFT HAND GETTER METHODS  //
		/******************************/

		/// <summary>
		/// Whether the left hand is currently grabbing an object.
		/// </summary>
        /// <returns>True if the left hand is currently grabbing an object.</returns>
		public bool LeftIsGrabbing()
		{
			return (LeftController.GetComponent<VRTK_InteractGrab>().GetGrabbedObject()) ? true : false;
		}

		/// <summary>
		/// Whether the left trigger (index finger) is held down. 
		/// </summary>
		/// <returns>True if the left trigger is held down.</returns>
		public bool LeftTrigger()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerPress);
		}

		/// <summary>
		/// Whether the left grip (middle finger) is held down. 
		/// </summary>
		/// <returns>True if the left grip is held down.</returns>
		public bool LeftGrip()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress);
		}

		/// <summary>
		/// Whether the left X button is held down.
		/// </summary>
		/// <returns>True if the left X button is held down.</returns>
		public bool LeftX()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonOnePress);
		}

		/// <summary>
		/// Whether the left Y button is held down.
		/// </summary>
		/// <returns>True if the left Y button is held down.</returns>
		public bool LeftY()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress);
		}

		/// <summary>
		/// Whether the left thumbstick is held down (that is pressed, but not necessarily rotated around).
		/// </summary>
		/// <returns>True if the left thumbstick is held down.</returns>
		public bool LeftStick()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TouchpadPress);
		}

		/// <summary>
		/// The distance by which the left thumbstick has been moved in the x and y axes.
		/// </summary>
		/// <returns>A 2-dimensional vector, equal to the thumbstick's displacement in the [0] x direction (float), and [1] y direction (float).</returns>
		public Vector2 LeftStickDelta()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
		}

		/// <summary>
		/// The angle of rotation of the left thumbstick.
		/// </summary>
		/// <returns>A float equal to the left thumbstick's conterclockwise angular diplacement in degrees.</returns>
		public float LeftStickAngle()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().GetAxisAngle(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
		}

		/// <summary>
		/// Whether the left sthumbstick has been moved from its origin.
		/// </summary>
		/// <returns>True if the left thumbstick has been moved from its origin.</returns>
		public bool LeftStickMoved()
		{
			if (LeftController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad).x != 0.0f)
			{
				return true;
			}
			else if (LeftController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad).y != 0.0f)
			{
				return true;
			}
			return false;	
		}

		/// <summary>
		/// The transform (position, rotation, scale) of the left controller.
		/// </summary>
		/// <returns>The transform component of the left controller.</returns>
		public Transform LeftTransform()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController);
		}

		/// <summary>
 		/// The position of the left controller in space.
		/// </summary>
 		/// <returns>A 3-dimensional vector, equal to the left controller's displacement in the [0] x direction (float), [1] y direction (float), and [2] z direction (float).</returns>
		public Vector3 LeftPosition()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).position;
		}

		/// <summary>
 		/// The relative position of the left controller in space.
		/// </summary>
 		/// <returns>A 3-dimensional vector, equal to the left controller's relative displacement in the [0] x direction (float), [1] y direction (float), and [2] z direction (float).</returns>
		public Vector3 LeftLocalPosition()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).localPosition;
		}

		/// <summary>
 		/// The rotation of the left controller in space. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		/// </summary>
 		/// <returns>A quaternion representing the left controller's conterclockwise angular displacement.</returns>
		public Quaternion LeftRotation()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).rotation;
		}

		/// <summary>
 		/// The relative rotation of the left controller in space. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		/// </summary>
 		/// <returns>A quaternion representing the left controller's relative conterclockwise angular displacement.</returns>
		public Quaternion LeftLocalRotation()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).localRotation;
		}

		/// <summary>
 		/// How fast the left controller is moving in space.
		/// </summary>
 		/// <returns>A 3-dimensional vector, equal to the left controller's velocity in the [0] x direction (float), [1] y direction (float), and [2] z direction (float).</returns>
		public Vector3 LeftVelocity()
		{
			return VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand());
		}

		/// <summary>
 		/// How fast the left controller is rotating in space.
		/// </summary>
 		/// <returns>A 3-dimensional vector, equal to the left controller's counterclockwise angular velocity in the [0] x direction (float), [1] y direction (float), and [2] z direction (float).</returns>
		public Vector3 LeftAngularVelocity()
		{
			return VRTK_DeviceFinder.GetControllerAngularVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand());
		}

		/// <summary>
		/// Whether the left controller's pointer is enabled or disabled.
		/// </summary>
		/// <returns>True if the left controller's pointer is enabled.</returns>
		public bool LeftPointerEnabled()
		{
			return boolLeftPointerEnabled;
		}

		/// <summary>
		/// Whether the UI attached to the left hand is enabled or disabled.
		/// </summary>
		/// <returns>True if the UI attached to the left hand is enabled.</returns>
		public bool LeftUIEnabled()
		{
			return LeftUI.activeSelf;
		}

		/// <summary>
		/// The transform (position, rotation, scale) of the UI attached to the left hand.
		/// </summary>
		/// <returns>The transform component of the UI attached to the left hand.</returns>
		public Transform LeftUITransform()
		{
			if (LeftUI.activeSelf)
			{
				return LeftUI.transform;
			}
			else
			{
				return null;
			}
		}



		/*******************************/
		//  RIGHT HAND GETTER METHODS  //
		/*******************************/
		
		/// <summary>
		/// Whether the right hand is currently grabbing an object.
		/// </summary>
        /// <returns>True if the right hand is currently grabbing an object.</returns>
		public bool RightIsGrabbing()
		{
			return (RightController.GetComponent<VRTK_InteractGrab>().GetGrabbedObject()) ? true : false;;
		}

		/// <summary>
		/// Whether the right trigger (index finger) is held down. 
		/// </summary>
		/// <returns>True if the right trigger is held down.</returns>
		public bool RightTrigger()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerPress);
		}

		/// <summary>
		/// Whether the right grip (middle finger) is held down. 
		/// </summary>
		/// <returns>True if the right grip is held down.</returns>
		public bool RightGrip()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress);
		}

		/// <summary>
		/// Whether the right A button is held down.
		/// </summary>
		/// <returns>True if the right A button is held down.</returns>
		public bool RightA()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonOnePress);
		}

		/// <summary>
		/// Whether the right B button is held down.
		/// </summary>
		/// <returns>True if the right B button is held down.</returns>
		public bool RightB()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress);
		}

		/// <summary>
		/// Whether the right thumbstick is held down (that is pressed, but not necessarily rotated around).
		/// </summary>
		/// <returns>True if the right thumbstick is held down.</returns>
		public bool RightStick()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TouchpadPress);
		}

		/// <summary>
		/// The distance by which the right thumbstick has been moved in the x and y axes.
		/// </summary>
		/// <returns>A 2-dimensional vector, equal to the thumbstick's displacement in the [0] x direction (float), and [1] y direction (float).</returns>
		public Vector2 RightStickDelta()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
		}

		/// <summary>
		/// The angle of rotation of the right thumbstick.
		/// </summary>
		/// <returns>A float equal to the right thumbstick's conterclockwise angular diplacement in degrees.</returns>
		public float RightStickAngle()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().GetAxisAngle(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
		}

		/// <summary>
		/// Whether the right sthumbstick has been moved from its origin.
		/// </summary>
		/// <returns>True if the right thumbstick has been moved from its origin.</returns>
		public bool RightStickMoved()
		{
			if (RightController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad).x != 0.0f)
			{
				return true;
			}
			else if ( RightController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad).y != 0.0f)
			{
				return true;
			}
			return false;	
		}

		/// <summary>
		/// The transform (position, rotation, scale) of the right controller.
		/// </summary>
		/// <returns>The transform component of the right controller.</returns>
		public Transform RightTransform()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController);
		}

		/// <summary>
 		/// The position of the right controller in space.
		/// </summary>
 		/// <returns>A 3-dimensional vector, equal to the right controller's displacement in the [0] x direction (float), [1] y direction (float), and [2] z direction (float).</returns>
		public Vector3 RightPosition()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).position;
		}

		/// <summary>
 		/// The relative position of the right controller in space.
		/// </summary>
 		/// <returns>A 3-dimensional vector, equal to the right controller's relative displacement in the [0] x direction (float), [1] y direction (float), and [2] z direction (float).</returns>
		public Vector3 RightLocalPosition()
		
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).localPosition;
		}

		/// <summary>
 		/// The rotation of the right controller in space. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		/// </summary>
 		/// <returns>A quaternion representing the right controller's conterclockwise angular displacement.</returns>
		public Quaternion RightRotation()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).rotation;
		}

		/// <summary>
 		/// The relative rotation of the right controller in space. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		/// </summary>
 		/// <returns>A quaternion representing the right controller's relative conterclockwise angular displacement.</returns>
		public Quaternion RightLocalRotation()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).localRotation;
		}

		/// <summary>
 		/// How fast the right controller is moving in space.
		/// </summary>
 		/// <returns>A 3-dimensional vector, equal to the right controller's velocity in the [0] x direction (float), [1] y direction (float), and [2] z direction (float).</returns>
		public Vector3 RightVelocity()
		{
			return VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
		}

		/// <summary>
 		/// How fast the right controller is rotating in space.
		/// </summary>
 		/// <returns>A 3-dimensional vector, equal to the right controller's counterclockwise angular velocity in the [0] x direction (float), [1] y direction (float), and [2] z direction (float).</returns>
		public Vector3 RightAngularVelocity()
		{
			return VRTK_DeviceFinder.GetControllerAngularVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
		}

		/// <summary>
		/// Whether the right controller's pointer is enabled or disabled.
		/// </summary>
		/// <returns>True if the right controller's pointer is enabled.</returns>
		public bool RightPointerEnabled()
		{
			return boolRightPointerEnabled;
		}

		/// <summary>
		/// Whether the UI attached to the right hand is enabled or disabled.
		/// </summary>
		/// <returns>True if the UI attached to the right hand is enabled.</returns>
		public bool RightUIEnabled()
		{
			return RightUI.activeSelf;
		}

		/// <summary>
		/// The transform (position, rotation, scale) of the UI attached to the right hand.
		/// </summary>
		/// <returns>The transform component of the UI attached to the right hand.</returns>
		public Transform RightUITransform()
		{
			if (RightUI.activeSelf)
			{
				return RightUI.transform;
			}
			else
			{
				return null;
			}
		}



		/******************************/
		//  COMPOSITE GETTER METHODS  //
		/******************************/

		/// <summary>
 		/// Whether both the right grip and the left grip are held down together (commonly used as scaling control).
		/// </summary>
 		/// <returns>True if both the right grip and the left grip are held down together.</returns>
		public bool BothGrip()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress)
			    && RightController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress);
		}

		/// <summary>
 		/// The absolute distance between the two controllers in space.
		/// </summary>
		/// <returns>A 3-dimensional vector, equal to the absolute distance between the controller's [0] x positions, [1] y positions (float), and [2] z positions (float).</returns>	
		public Vector3 Distance()
		{
			Vector3 LeftPosition = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).position;
			Vector3 RightPosition = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).position;
			return LeftPosition - RightPosition;
		}

		/// <summary>
 		/// The relative distance between the two controller. Unless the local scale is different from the absolute scale, LocalDistance == Distance.
		/// </summary>
		/// <returns>A 3-dimensional vector, equal to the relative distance between the controller's [0] x positions, [1] y positions (float), and [2] z positions (float).</returns>	
		public Vector3 LocalDistance()
		{
			Vector3 LeftLocalPosition = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).localPosition;
			Vector3 RightLocalPosition = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).localPosition;
			return LeftLocalPosition - RightLocalPosition;
		}

		/// <summary>
 		/// The difference in velocity of the two controllers.
		/// </summary>
		/// <returns>A 3-dimensional vector, equal to the difference between the controller's [0] velocities in the x direction, [1] velocities in the y direction (float), and [2] velocities in the z direction (float).</returns>	
		public Vector3 VelocityDelta()
		{
			Vector3 LeftVelocity = VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand());
			Vector3 RightVelocity = VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
			return LeftVelocity - RightVelocity;	
		}

		/// <summary>
 		/// The difference in angular velocity of the two controllers.
		/// </summary>
		/// <returns>A 3-dimensional vector, equal to the difference between the controller's [0] angular velocities in the x direction, [1] angular velocities in the y direction (float), and [2] angular velocities in the z direction (float).</returns>	
		public Vector3 AngularVelocityDelta()
		{
			Vector3 LeftAngularVelocity = VRTK_DeviceFinder.GetControllerAngularVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand());	
			Vector3 RightAngularVelocity = VRTK_DeviceFinder.GetControllerAngularVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
			return LeftAngularVelocity - RightAngularVelocity;	
		}

		/// <summary>
 		/// The dot product of the difference in velocity between the two controllers and their distance. Can be used as a world scaling factor.
		/// </summary>
		/// <returns>A float that approximates a reasonable (neither too fast, nor too slow) world scaling factor.</returns>	
		public float ScalingFactor()
		{
			Vector3 LeftVelocity = VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand());
			Vector3 RightVelocity = VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
			Vector3 VelocityDelta = LeftVelocity - RightVelocity; 

			Vector3 LeftPosition = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).position;
			Vector3 RightPosition = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).position;
			Vector3 Distance = LeftPosition - RightPosition;

			return Vector3.Dot(VelocityDelta, Distance);
		}

		/// <summary>
 		/// The dot product of the difference in velocity between the two controllers and their local distance. Can be used as a local scaling factor.
		/// </summary>
		/// <returns>A float that approximates a reasonable (neither too fast, nor too slow) local scaling factor.</returns>	
		public float LocalScalingFactor()
		{
			Vector3 LeftVelocity = VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand());
			Vector3 RightVelocity = VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
			Vector3 VelocityDelta = LeftVelocity - RightVelocity; 

			Vector3 LeftLocalPosition = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).localPosition;
			Vector3 RightLocalPosition = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).localPosition;
			Vector3 localDistance = LeftLocalPosition - RightLocalPosition;

			return Vector3.Dot(VelocityDelta, localDistance);
		}

		/// <summary>
		/// Whether both controllers' pointers are enabled or disabled.
		/// </summary>
		/// <returns>True if both controllers' pointers are enabled.</returns>
		public bool BothPointerEnabled()
		{
			return boolLeftPointerEnabled && boolRightPointerEnabled;
		}

		/// <summary>
		/// Whether both UIs attached to the user's hands are enabled or disabled.
		/// </summary>
		/// <returns>True if both UIs attached to the user's hands are enabled.</returns>
		public bool BothUIsEnabled()
		{
			return LeftUI.activeSelf && RightUI.activeSelf;
		}



		/******************************/
		//  LEFT HAND SETTER METHODS  //
		/******************************/

		/// <summary>
 		/// If not already enabled, enables the left controller's pointer.
		/// </summary>
		public void EnableLeftPointer()
		{
			if (!boolLeftPointerEnabled)
			{
				LeftPointerRenderer.enabled = true;
				LeftPointer.enabled = true;
				boolLeftPointerEnabled = true;
			}	
		}

		/// <summary>
 		/// If not already enabled, enables the UI attached to the left hand.
		/// </summary>
		public void EnableLeftUI()
		{
			if (!LeftUI.activeSelf)
			{
				LeftUI.SetActive(true);
			}	
		}

		/// <summary>
 		/// If not already disabled, disables the left controller's pointer.
		/// </summary>
		public void DisableLeftPointer()
		{
			if (boolLeftPointerEnabled)
			{
				LeftPointerRenderer.enabled = false;
				LeftPointer.enabled = false;
				boolLeftPointerEnabled = false;
			}	
		}

		/// <summary>
 		/// If not already disabled, disables the UI attached to the left hand.
		/// </summary>
		public void DisableLeftUI()
		{
			if (LeftUI.activeSelf)
			{
				LeftUI.SetActive(false);
			}	
		}



		/*******************************/
		//  RIGHT HAND SETTER METHODS  //
		/*******************************/

		/// <summary>
 		/// If not already enabled, enables the right controller's pointer.
		/// </summary>
		public void EnableRightPointer()
		{
			if (!boolRightPointerEnabled)
			{
				RightUIPointer.enabled = true;
				RightPointerRenderer.enabled = true;
				RightPointer.enabled = true;
				boolRightPointerEnabled = true;	
			}	
		}

		/// <summary>
 		/// If not already enabled, enables the UI attached to the right hand.
		/// </summary>
		public void EnableRightUI()
		{
			if (!RightUI.activeSelf)
			{
				RightUI.SetActive(true);
			}	
		}

		/// <summary>
 		/// If not already disabled, disables the right controller's pointer.
		/// </summary>
		public void DisableRightPointer()
		{
			if (boolRightPointerEnabled)
			{
				RightUIPointer.enabled = false;
				RightPointerRenderer.enabled = false;
				RightPointer.enabled = false;
				boolRightPointerEnabled = false;	
			}	
		}

		/// <summary>
 		/// If not already disabled, disables the UI attached to the right hand.
		/// </summary>
		public void DisableRightUI()
		{
			if (RightUI.activeSelf)
			{
				RightUI.SetActive(false);
			}	
		}



		/******************************/
		//  COMPOSITE SETTER METHODS  //
		/******************************/

		/// <summary>
 		/// Checks the controllers' pointers one-by-one, and accordingly enables them.
		/// </summary>
		public void EnableBothPointers()
		{
			// If not already enabled, enables the left controller's pointer.
			if (!boolLeftPointerEnabled)
			{
				LeftPointerRenderer.enabled = true;
				LeftPointer.enabled = true;
				boolLeftPointerEnabled = true;
			}	
			// If not already enabled, enables the right controller's pointer.
			if (!boolRightPointerEnabled)
			{
				RightUIPointer.enabled = true;
				RightPointerRenderer.enabled = true;
				RightPointer.enabled = true;
				boolRightPointerEnabled = true;	
			}	
		}

		/// <summary>
 		/// Checks the UIs attached to the user's hands one-by-one, and accordingly enables them.
		/// </summary>
		public void EnableBothUIs()
		{
			// If not already enabled, enables the UI attached to the left hand.
			if (!LeftUI.activeSelf)
			{
				LeftUI.SetActive(true);
			}	
			// If not already enabled, enables the UI attached to the right hand.
			if (!RightUI.activeSelf)
			{
				RightUI.SetActive(true);
			}	
		}

		/// <summary>
 		/// Checks the controllers' pointers one-by-one, and accordingly disables them.
		/// </summary>
		public void DisableBothPointers()
		{
			// If not already disabled, disables the left controller's pointer.
			if (boolLeftPointerEnabled)
			{
				LeftPointerRenderer.enabled = false;
				LeftPointer.enabled = false;
				boolLeftPointerEnabled = false;
			}	
			// If not already disabled, disables the right controller's pointer.
			if (boolRightPointerEnabled)
			{
				RightUIPointer.enabled = false;
				RightPointerRenderer.enabled = false;
				RightPointer.enabled = false;
				boolRightPointerEnabled = false;	
			}	
		}

		/// <summary>
 		/// Checks the UIs attached to the user's hands one-by-one, and accordingly disables them.
		/// </summary>
		public void DisableBothUIs()
		{
			// If not already disabled, disables the UI attached to the left hand.
			if (LeftUI.activeSelf)
			{
				LeftUI.SetActive(false);
			}	
			// If not already disabled, disables the UI attached to the right hand.
			if (RightUI.activeSelf)
			{
				RightUI.SetActive(false);
			}	
		}

	}
}