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


		// These variables store information on the state of the left controller.
		private bool LeftIsGrabbing; /// True if left hand is grabbing another object.
		private bool LeftIndex; /// True if the left index trigger is being pressed.
		private bool LeftMiddle; /// True if the left middle trigger is being pressed.
		private bool LeftX; /// True if the left X button is being pressed.
		private bool LeftY; /// True if the left Y button is being pressed.
		private bool LeftThumb; /// True if the left thumbstick is being pressed.
		private Vector2 LeftThumbDelta; /// The distance by which the left thumbstick has been moved in the X-axis and Y-axis.
		private float LeftThumbAngle; /// The angle of rotation of the left thumbstick.
		private bool LeftThumbMoved; /// True if LeftThumbDelta{.x|.y} != 0.0f or LeftThumbAngle != 0.0f, meaning that the left thumbstick has been moved.
		private Transform LeftTransform; /// The transform (position, rotation, scale) of the left controller.
		private Vector3 LeftPosition; /// The position of the left controller.
		private Vector3 LeftLocalPosition; /// The relative position of the left controller.
		private Quaternion LeftRotation; /// The rotation of the left controller. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		private Quaternion LeftLocalRotation; /// The relative rotation of the left controller. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		private Vector3 LeftVelocity; /// How fast the left controller is moving in space.
		private Vector3 LeftAngularVelocity; /// How fast the left controller is rotating in space.


		// These variables store information on the state of the right controller.
		private bool RightIsGrabbing; /// True if the right hand is grabbing another object.
		private bool RightIndex; /// True if the right index trigger is being pressed.
		private bool RightMiddle; /// True if the right middle trigger is being pressed.
		private bool RightA; /// True if the right A button is being pressed.
		private bool RightB; /// True if the right B button is being pressed.
		private bool RightThumb; /// True if the right thumbstick is being pressed.
		private Vector2 RightThumbDelta; /// The distance by which the right thumbstick has been moved in the X-axis and Y-axis.
		private float RightThumbAngle; /// The angle of rotation of the right thumbstick.
		private bool RightThumbMoved; /// True if RightThumbDelta{.x|.y} != 0.0f or RightThumbAngle != 0.0f, meaning that the right thumbstick has been moved.
		private Transform RightTransform; /// The transform (position, rotation, scale) of the right controller.
		private Vector3 RightPosition; /// The position of the right controller.
		private Vector3 RightLocalPosition; /// The relative position of the right controller.
		private Quaternion RightRotation; /// The rotation of the right controller. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		private Quaternion RightLocalRotation; /// The relative rotation of the right controller. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		private Vector3 RightVelocity; /// How fast the right controller is moving in space.
		private Vector3 RightAngularVelocity; /// How fast the right controller is rotating in space.


		// These variables store information on the state of both controllers.
		private bool BothMiddle; /// True if both the right and the left middle triggers are being pressed.
		private Vector3 Distance; /// The absolute distance between the two controllers.
		private Vector3 LocalDistance; /// The relative distance between the two controller. Unless the local scale is different from the absolute scale, LocalDistance == Distance.
		private Vector3 VelocityDelta; /// The difference in velocity of the two controllers.
		private Vector3 AngularVelocityDelta; /// The difference in angular velocity of the two controllers.
		private float ScalingFactor; /// The dot product of VelocityDelta with Distance, indicating the absolute cartesian scaling factor.
		private float LocalScalingFactor; /// The dot product of elocityDelta with LocalDistance, indicating the relative cartesian scaling factor.


		// Unlike Update, which gets called once per frame, FixedUpdate is called accordingly to the physics engine.
		void FixedUpdate()
		{
			// These variables store the controller state, which can be queried for input information.
            var left = LeftController.GetComponent<VRTK_ControllerEvents>();
			var leftInteractGrab = LeftController.GetComponent<VRTK_InteractGrab>();
			var right = RightController.GetComponent<VRTK_ControllerEvents>();
			var rightInteractGrab = RightController.GetComponent<VRTK_InteractGrab>();

			// Receive input information from the left controller.
			LeftIsGrabbing = (leftInteractGrab.GetGrabbedObject()) ? true : false;
			LeftIndex = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerPress);
			LeftMiddle = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress);
			LeftX = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonOnePress);
			LeftY = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress);
			LeftThumb = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TouchpadPress);
			LeftThumbDelta = left.GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
			LeftThumbAngle = left.GetAxisAngle(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
			LeftThumbMoved = LeftThumbDelta.x != 0.0f || LeftThumbDelta.y != 0.0f;
			LeftTransform = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController);
			LeftPosition = LeftTransform.position;
			LeftLocalPosition = LeftTransform.localPosition;
			LeftRotation = LeftTransform.rotation;
			LeftLocalRotation = LeftTransform.localRotation;
			LeftVelocity = VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand());
			LeftAngularVelocity = VRTK_DeviceFinder.GetControllerAngularVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand());

            // Receive input information from the right controller.
			RightIsGrabbing = (rightInteractGrab.GetGrabbedObject()) ? true : false;
			RightIndex = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerPress);
			RightMiddle = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress);
			RightA = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonOnePress);
			RightB = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress);
			RightThumb = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TouchpadPress);
			RightThumbDelta = right.GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
			RightThumbAngle = right.GetAxisAngle(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
			RightThumbMoved = RightThumbDelta.x != 0.0f || RightThumbDelta.y != 0.0f;
			RightTransform = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController);
			RightPosition = RightTransform.position;
			RightLocalPosition = RightTransform.localPosition;
			RightRotation = RightTransform.rotation;
			RightLocalRotation = RightTransform.localRotation;
			RightVelocity = VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
			RightAngularVelocity = VRTK_DeviceFinder.GetControllerAngularVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
	
            // Compute information about the state of both controllers.
			BothMiddle = LeftMiddle && RightMiddle;
			Distance = LeftPosition - RightPosition;
			LocalDistance = LeftLocalPosition - RightLocalPosition;
			VelocityDelta = LeftVelocity - RightVelocity;
			AngularVelocityDelta = LeftAngularVelocity - RightAngularVelocity;
			ScalingFactor = Vector3.Dot(VelocityDelta, Distance);
			LocalScalingFactor = Vector3.Dot(VelocityDelta, LocalDistance);
		}


		/* Getter methods, left controller. TODO: correct comment style?
		*/

		/// <summary>
		/// True if left hand is grabbing an object.
		/// </summary>
		public bool GetLeftIsGrabbing()
		{
			return LeftIsGrabbing;
		}

		/// <summary>
		/// True if the left index trigger is being pressed.
		/// </summary>
		public bool GetLeftIndex()
		{
			return LeftIndex;
		}

		/// <summary>
		/// True if the left middle trigger is being pressed.
		/// </summary>
		public bool GetLeftMiddle()
		{
			return LeftMiddle;
		}

		/// <summary>
		/// True if the left X button is being pressed.
		/// </summary>
		public bool GetLeftX()
		{
			return LeftX;
		}

		/// <summary>
		/// True if the left Y button is being pressed.
		/// </summary>
		public bool GetLeftY()
		{
			return LeftY;
		}

		/// <summary>
		/// True if the left thumbstick is being pressed.
		/// </summary>
		public bool GetLeftThumb()
		{
			return LeftThumb;
		}

		/// <summary>
		/// The distance by which the left thumbstick has been moved in the X-axis and Y-axis.
		/// </summary>
		public Vector2 GetLeftThumbDelta()
		{
			return LeftThumbDelta;
		}

		/// <summary>
		/// The angle of rotation of the left thumbstick.
		/// </summary>
		public float GetLeftThumbAngle()
		{
			return LeftThumbAngle;
		}

		/// <summary>
		/// True if LeftThumbDelta{.x|.y} != 0.0f or LeftThumbAngle != 0.0f, meaning that the left thumbstick has been moved.
		/// </summary>
		public bool GetLeftThumbMoved()
		{
			return LeftThumbMoved;
		}

		/// <summary>
		/// The transform (position, rotation, scale) of the left controller.
		/// </summary>
		public Transform GetLeftTransform()
		{
			return LeftTransform;
		}

		/// <summary>
 		/// The position of the left controller.
		/// </summary>
		public Vector3 GetLeftPosition()
		{
			return LeftPosition;
		}

		/// <summary>
 		/// The relative position of the left controller.
		/// </summary>
		public Vector3 GetLeftLocalPosition()
		{
			return LeftLocalPosition;
		}

		/// <summary>
 		/// The rotation of the left controller. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		/// </summary>
		public Quaternion GetLeftRotation()
		{
			return LeftRotation;
		}

		/// <summary>
 		/// The relative rotation of the left controller. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		/// </summary>
		public Quaternion GetLeftLocalRotation()
		{
			return LeftLocalRotation;
		}

		/// <summary>
 		/// How fast the left controller is moving in space.
		/// </summary>
		public Vector3 GetLeftVelocity()
		{
			return LeftVelocity;
		}

		/// <summary>
 		/// How fast the left controller is rotating in space.
		/// </summary>
		public Vector3 GetLeftAngularVelocity()
		{
			return LeftAngularVelocity;
		}




		// Getter methods, right controller. TODO: correct comment style?

		/// <summary>
 		/// True if the right hand is grabbing an object.
		/// </summary>
		public bool GetRightIsGrabbing()
		{
			return RightIsGrabbing;
		}

		/// <summary>
 		/// True if the right index trigger is being pressed.
		/// </summary>
		public bool GetRightIndex()
		{
			return RightIndex;
		}

		/// <summary>
 		/// True if the right middle trigger is being pressed.
		/// </summary>
		public bool GetRightMiddle()
		{
			return RightMiddle;
		}

		/// <summary>
 		/// True if the right A button is being pressed.
		/// </summary>
		public bool GetRightA()
		{
			return RightA;
		}

		/// <summary>
 		/// True if the right B button is being pressed.
		/// </summary>
		public bool GetRightB()
		{
			return RightB;
		}

		/// <summary>
 		/// True if the right thumbstick is being pressed.
		/// </summary>
		public bool GetRightThumb()
		{
			return RightThumb;
		}

		/// <summary>
 		/// The distance by which the right thumbstick has been moved in the X-axis and Y-axis.
		/// </summary>
		public Vector2 GetRightThumbDelta()
		{
			return RightThumbDelta;
		}

		/// <summary>
 		/// The angle of rotation of the right thumbstick.
		/// </summary>
		public float GetRightThumbAngle()
		{
			return RightThumbAngle;
		}

		/// <summary>
 		/// True if RightThumbDelta{.x|.y} != 0.0f or RightThumbAngle != 0.0f, meaning that the right thumbstick has been moved.
		/// </summary>
		public bool GetRightThumbMoved()
		{
			return RightThumbMoved;
		}

		/// <summary>
 		/// The transform (position, rotation, scale) of the right controller.
		/// </summary>
		public Transform GetRightTransform()
		{
			return RightTransform;
		}

		/// <summary>
 		/// The position of the right controller.
		/// </summary>
		public Vector3 GetRightPosition()
		{
			return RightPosition;
		}

		/// <summary>
 		/// The relative position of the right controller.
		/// </summary>
		public Vector3 GetRightLocalPosition()
		{
			return RightLocalPosition;
		}

		/// <summary>
 		/// The rotation of the right controller. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		/// </summary>
		public Quaternion GetRightRotation()
		{
			return RightRotation;
		}

		/// <summary>
		/// The relative rotation of the right controller. Can be queried for its normalized value, eulerAngles, and w, x, y, and z components.
		/// </summary>
		public Quaternion GetRightLocalRotation()
		{
			return RightLocalRotation;
		}

		/// <summary>
 		/// How fast the right controller is moving in space.
		/// </summary>
		public Vector3 GetRightVelocity()
		{
			return RightVelocity;
		}

		/// <summary>
 		/// How fast the right controller is rotating in space.
		/// </summary>
		public Vector3 GetRightAngularVelocity()
		{
			return RightAngularVelocity;
		}




		// Getter methods, both controllers. TODO

		/// <summary>
 		/// True if both the right and the left middle triggers are being pressed.
		/// </summary>
		public bool GetBothMiddle()
		{
			return BothMiddle;
		}

		/// <summary>
 		/// The absolute distance between the two controllers.
		/// </summary>
		public Vector3 GetDistance()
		{
			return Distance;
		}

		/// <summary>
 		/// The relative distance between the two controller. Unless the local scale is different from the absolute scale, LocalDistance == Distance.
		/// </summary>
		public Vector3 GetLocalDistance()
		{
			return LocalDistance;
		}

		/// <summary>
 		/// The difference in velocity of the two controllers.
		/// </summary>
		public Vector3 GetVelocityDelta()
		{
			return VelocityDelta;
		}

		/// <summary>
 		/// The difference in angular velocity of the two controllers.
		/// </summary>
		public Vector3 GetAngularVelocityDelta()
		{
			return AngularVelocityDelta;
		}

		/// <summary>
 		/// The dot product of VelocityDelta with Distance, indicating the absolute cartesian scaling factor.
		/// </summary>
		public float GetScalingFactor()
		{
			return ScalingFactor;
		}

		/// <summary>
 		/// The dot product of elocityDelta with LocalDistance, indicating the relative cartesian scaling factor.
		/// </summary>
		public float GetLocalScalingFactor()
		{
			return LocalScalingFactor;
		}

	}
}