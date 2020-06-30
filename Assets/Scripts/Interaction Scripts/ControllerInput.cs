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
		// TODO.
		public GameObject LeftController;
		public GameObject RightController;

		/// <summary>
		/// True if left hand is grabbing an object.
		/// </summary>
		public bool GetLeftIsGrabbing()
		{
			return (LeftController.GetComponent<VRTK_InteractGrab>().GetGrabbedObject()) ? true : false;
		}

		/// <summary>
		/// True if the left index trigger is being pressed.
		/// </summary>
		public bool GetLeftIndex()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerPress);
		}

		/// <summary>
		/// True if the left middle trigger is being pressed.
		/// </summary>
		public bool GetLeftMiddle()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress);
		}

		/// <summary>
		/// True if the left X button is being pressed.
		/// </summary>
		public bool GetLeftX()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonOnePress);
		}

		/// <summary>
		/// True if the left Y button is being pressed.
		/// </summary>
		public bool GetLeftY()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress);
		}

		/// <summary>
		/// True if the left thumbstick is being pressed.
		/// </summary>
		public bool GetLeftThumb()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TouchpadPress);
		}

		/// <summary>
		/// The distance by which the left thumbstick has been moved in the X-axis and Y-axis.
		/// </summary>
		public Vector2 GetLeftThumbDelta()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
		}

		/// <summary>
		/// The angle of rotation of the left thumbstick.
		/// </summary>
		public float GetLeftThumbAngle()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().GetAxisAngle(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
		}

		/// <summary>
		/// True if LeftThumbDelta{.x|.y} != 0.0f or LeftThumbAngle != 0.0f, meaning that the left thumbstick has been moved.
		/// </summary>
		public bool GetLeftThumbMoved()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad).x != 0.0f || LeftController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad).y != 0.0f;
		}

		/// <summary>
		/// The transform (position, rotation, scale) of the left controller.
		/// </summary>
		public Transform GetLeftTransform()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController);
		}

		/// <summary>
 		/// The position of the left controller.
		/// </summary>
		public Vector3 GetLeftPosition()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).position;
		}

		/// <summary>
 		/// The relative position of the left controller.
		/// </summary>
		public Vector3 GetLeftLocalPosition()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).localPosition;
		}

		/// <summary>
 		/// The rotation of the left controller. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		/// </summary>
		public Quaternion GetLeftRotation()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).rotation;
		}

		/// <summary>
 		/// The relative rotation of the left controller. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		/// </summary>
		public Quaternion GetLeftLocalRotation()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).localRotation;
		}

		/// <summary>
 		/// How fast the left controller is moving in space.
		/// </summary>
		public Vector3 GetLeftVelocity()
		{
			return VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand());
		}

		/// <summary>
 		/// How fast the left controller is rotating in space.
		/// </summary>
		public Vector3 GetLeftAngularVelocity()
		{
			return VRTK_DeviceFinder.GetControllerAngularVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand());
		}




		// Getter methods, right controller. TODO: correct comment style?

		/// <summary>
 		/// True if the right hand is grabbing an object.
		/// </summary>
		public bool GetRightIsGrabbing()
		{
			return (RightController.GetComponent<VRTK_InteractGrab>().GetGrabbedObject()) ? true : false;;
		}

		/// <summary>
 		/// True if the right index trigger is being pressed.
		/// </summary>
		public bool GetRightIndex()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerPress);
		}

		/// <summary>
 		/// True if the right middle trigger is being pressed.
		/// </summary>
		public bool GetRightMiddle()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress);
		}

		/// <summary>
 		/// True if the right A button is being pressed.
		/// </summary>
		public bool GetRightA()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonOnePress);
		}

		/// <summary>
 		/// True if the right B button is being pressed.
		/// </summary>
		public bool GetRightB()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress);
		}

		/// <summary>
 		/// True if the right thumbstick is being pressed.
		/// </summary>
		public bool GetRightThumb()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TouchpadPress);
		}

		/// <summary>
 		/// The distance by which the right thumbstick has been moved in the X-axis and Y-axis.
		/// </summary>
		public Vector2 GetRightThumbDelta()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
		}

		/// <summary>
 		/// The angle of rotation of the right thumbstick.
		/// </summary>
		public float GetRightThumbAngle()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().GetAxisAngle(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
		}

		/// <summary>
 		/// True if RightThumbDelta{.x|.y} != 0.0f or RightThumbAngle != 0.0f, meaning that the right thumbstick has been moved.
		/// </summary>
		public bool GetRightThumbMoved()
		{
			return RightController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad).x != 0.0f || RightController.GetComponent<VRTK_ControllerEvents>().GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad).y != 0.0f;
		}

		/// <summary>
 		/// The transform (position, rotation, scale) of the right controller.
		/// </summary>
		public Transform GetRightTransform()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController);
		}

		/// <summary>
 		/// The position of the right controller.
		/// </summary>
		public Vector3 GetRightPosition()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).position;
		}

		/// <summary>
 		/// The relative position of the right controller.
		/// </summary>
		public Vector3 GetRightLocalPosition()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).localPosition;
		}

		/// <summary>
 		/// The rotation of the right controller. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		/// </summary>
		public Quaternion GetRightRotation()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).rotation;
		}

		/// <summary>
		/// The relative rotation of the right controller. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		/// </summary>
		public Quaternion GetRightLocalRotation()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).localRotation;
		}

		/// <summary>
 		/// How fast the right controller is moving in space.
		/// </summary>
		public Vector3 GetRightVelocity()
		{
			return VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
		}

		/// <summary>
 		/// How fast the right controller is rotating in space.
		/// </summary>
		public Vector3 GetRightAngularVelocity()
		{
			return VRTK_DeviceFinder.GetControllerAngularVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
		}




		// Getter methods, both controllers. TODO

		/// <summary>
 		/// True if both the right and the left middle triggers are being pressed.
		/// </summary>
		public bool GetBothMiddle()
		{
			return LeftController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress) && RightController.GetComponent<VRTK_ControllerEvents>().IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress);
		}

		/// <summary>
 		/// The absolute distance between the two controllers.
		/// </summary>
		public Vector3 GetDistance()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).position - VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).position;
		}

		/// <summary>
 		/// The relative distance between the two controller. Unless the local scale is different from the absolute scale, LocalDistance == Distance.
		/// </summary>
		public Vector3 GetLocalDistance()
		{
			return VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).localPosition - VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).localPosition;
		}

		/// <summary>
 		/// The difference in velocity of the two controllers.
		/// </summary>
		public Vector3 GetVelocityDelta()
		{
			return VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand()) - VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
		}

		/// <summary>
 		/// The difference in angular velocity of the two controllers.
		/// </summary>
		public Vector3 GetAngularVelocityDelta()
		{
			return VRTK_DeviceFinder.GetControllerAngularVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand()) - VRTK_DeviceFinder.GetControllerAngularVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
		}

		/// <summary>
 		/// The dot product of VelocityDelta with Distance, indicating the absolute cartesian scaling factor.
		/// </summary>
		public float GetScalingFactor()
		{
			Vector3 VelocityDelta = VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand()) - VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
			Vector3 Distance = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).position - VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).position;
			return Vector3.Dot(VelocityDelta, Distance);
		}

		/// <summary>
 		/// The dot product of elocityDelta with LocalDistance, indicating the relative cartesian scaling factor.
		/// </summary>
		public float GetLocalScalingFactor()
		{
			Vector3 VelocityDelta = VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand()) - VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
			Vector3 localDistance = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).localPosition - VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).localPosition;
			return Vector3.Dot(VelocityDelta, localDistance);
		}

	}
}