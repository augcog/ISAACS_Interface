namespace ISAACS_ControllerInput {

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
		using VRTK;


	/* This script is a wrapper around VRTK, to be used as a library for
	higher-level scripts needing access to user input coming from a controller.
	Initialize the ControllerState class as a singleton of the GameObject Controller.
	For further clarifications, ask Apollo.
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


		private GameObject LeftPointer; // The VRTK Pointer Component of the left controller.
		private GameObject LeftPointerRenderer; // The VRTK Pointer Renderer Component of the left controller.
		private GameObject RightPointer; // The VRTK Pointer Component of the right controller.
		private GameObject RightPointerRenderer; // The VRTK Pointer Renderer Component of the right controller.
		private GameObject RightUIPointer; // The VRTK UI Pointer Component of the right controller.

		private bool LeftPointerEnabled = true;
		private bool LeftUIEnabled = true;	
		private bool RightPointerEnabled = true;
		private bool RightUIEnabled = true;	

		void Start()
		{
            LeftPointer = LeftController.GetComponent<VRTK_Pointer>();
            LeftPointerRenderer = LeftController.GetComponent<VRTK_BezierPointerRenderer>();
            RightPointer = RightController.GetComponent<VRTK_Pointer>();
            RightPointerRenderer = RightController.GetComponent<VRTK_StraightPointerRenderer>();
            RightUIPointer = RightController.GetComponent<VRTK_UIPointer>();
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

		//TODO
		public bool LeftPointerEnabled()
		{
			return LeftPointerEnabled;
		}
		//TODO
		public bool LeftUIEnabled()
		{
			return LeftUIEnabled;
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
			if (RightController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad).x != 0.0f || RightController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad).x != 0.0f)
			{
				return true;
			}
			else if (RightController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad).x != 0.0f || RightController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad).y != 0.0f)
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

		//TODO
		public bool RightPointerEnabled()
		{
			return RightPointerEnabled;
		}
		//TODO
		public bool RightUIEnabled()
		{
			return RightUIEnabled;
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
			return LeftController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress) && RightController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress);
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




		/******************************/
		//  LEFT HAND SETTER METHODS  //
		/******************************/

		/// <summary>
 		/// TODO
		/// </summary>
		/// <returns>TODO</returns>	
		public void EnableLeftPointer()
		{
            LeftPointerRenderer.enabled = true;
            LeftPointer.enabled = true;
			// Also update the variable keeping track of whether this pointer is enabled or disabled (returned by getter methods).
			LeftPointerEnabled = true;
		}

		public void EnableLeftUI()
		{
			LeftUI.SetActive(true);
			// Also update the variable keeping track of whether left hand UI is enabled or disabled (returned by getter methods).
			LeftUIEnabled = true;
		}

		public void DisableLeftPointer()
		{
            LeftPointerRenderer.enabled = false;
            LeftPointer.enabled = false;
			// Also update the variable keeping track of whether this pointer is enabled or disabled (returned by getter methods).
			LeftPointerEnabled = false;
		}

		public void DisableLeftUI()
		{
			LeftUI.SetActive(false);
			// Also update the variable keeping track of whether the left hand UI is enabled or disabled (returned by getter methods).
			LeftUIEnabled = false;
		}




		/******************************/
		//  RIGHT HAND SETTER METHODS  //
		/******************************/

		public void EnableRightPointer()
		{
            RightUIPointer.enabled = true;
            RightPointerRenderer.enabled = true;
            RightPointer.enabled = true;
			// Also update the variable keeping track of whether this pointer is enabled or disabled (returned by the getter methods).
			RightPointerEnabled = true;	
		}

		public void EnableRightUI()
		{
			RightUI.SetActive(true);
			// Also update the variable keeping track of whether the right hand UI is enabled or disabled (returned by getter methods).
			RightUIEnabled = true;
		}

		public void DisableRightPointer()
		{
            RightUIPointer.enabled = false;
            RightPointerRenderer.enabled = false;
            RightPointer.enabled = false;
			// Also update the variable keeping track of whether this pointer is enabled or disabled (returned by the getter methods).
			RightPointerEnabled = true;	
		}

		public void DisableRightUI()
		{
			RightUI.SetActive(false);
			// Also update the variable keeping track of whether the right hand UI is enabled or disabled (returned by getter methods).
			RightUIEnabled = false;
		}
		// TODO: composite setter method

	}
}