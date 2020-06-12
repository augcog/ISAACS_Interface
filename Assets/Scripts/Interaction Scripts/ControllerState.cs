namespace ISAACS {

    using System.Collections;
    using System.Collections.Generic;
	using VRTK;
    using UnityEngine;

	public class ControllerState : MonoBehaviour {

		// Remember to set these controllers to actual GameObjects through the Unity GUI.
		public GameObject LeftController;
		public GameObject RightController;
		public GameObject controller_left;
		public GameObject controller_right;

		// True indicates that the button is being pressed, False that it's released.
		private bool LeftIsGrabbing; // Whether the left hand is grabbing another object.
		private bool LeftIndex;
		private bool LeftMiddle;
		private bool LeftX;
		private bool LeftY;
		private bool LeftThumb;
		private Vector2 LeftThumbDelta; // The distance by which the left stick has been moved in the X-axis and Y-axis.
		private float LeftThumbAngle; // The angle of rotation of the left stick.
		private Transform LeftOrigin; // The origin ("position") of the left controller.
		private Vector3 LeftVelocity; // How fast the left controller is moving in space.
		private Vector3 LeftAngularVelocity; // How fast the left controller is moving in space.

		// True indicates that the button is being pressed, False that it's released.
		private bool RightIsGrabbing; // Whether the right hand is grabbing another object.
		private bool RightIndex;
		private bool RightMiddle;
		private bool RightA;
		private bool RightB;
		private bool RightThumb;
		private Vector2 RightThumbDelta; // The distance by which the right stick has been moved in the X-axis and Y-axis.
		private float RightThumbAngle; // The angle of rotation of the right stick.
		private Transform RightOrigin; // The origin ("position") of the right controller.
		private Vector3 RightVelocity; // How fast the right controller is moving in space.
		private Vector3 RightAngularVelocity; // How fast the right controller is moving in space.


		// Update is called once per frame.
		void Update () {

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
			LeftOrigin = VRTK_DeviceFinder.GetControllerOrigin(VRTK_DeviceFinder.GetControllerReferenceLeftHand());
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
			RightOrigin = VRTK_DeviceFinder.GetControllerOrigin(VRTK_DeviceFinder.GetControllerReferenceRightHand());
			RightVelocity = VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
			RightAngularVelocity = VRTK_DeviceFinder.GetControllerAngularVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());

		}


		// Getter methods, left controller.
		public bool GetLeftIsGrabbing()
		{
			return LeftIsGrabbing;
		}
		
		public bool GetLeftIndex()
		{
			return LeftIndex;
		}

		public bool GetLeftMiddle()
		{
			return LeftMiddle;
		}
		
		public bool GetLeftX()
		{
			return LeftX;
		}

		public bool GetLeftY()
		{
			return LeftY;
		}

		public bool GetLeftThumb()
		{
			return LeftThumb;
		}

		public Vector2 GetLeftThumbDelta()
		{
			return LeftThumbDelta;
		}

		public float GetLeftThumbAngle()
		{
			return LeftThumbAngle;
		}

		public Transform GetLeftOrigin()
		{
			return LeftOrigin;
		}

		public Vector3 GetLeftVelocity()
		{
			return LeftVelocity;
		}

		public Vector3 GetLeftAngularVelocity()
		{
			return LeftAngularVelocity;
		}


		// Getter methods, right controller.
		public bool GetRightIsGrabbing()
		{
			return RightIsGrabbing;
		}
		
		public bool GetRightIndex()
		{
			return RightIndex;
		}

		public bool GetRightMiddle()
		{
			return RightMiddle;
		}
		
		public bool GetRightA()
		{
			return RightA;
		}

		public bool GetRightB()
		{
			return RightB;
		}

		public bool GetRightThumb()
		{
			return RightThumb;
		}

		public Vector2 GetRightThumbDelta()
		{
			return RightThumbDelta;
		}

		public float GetRightThumbAngle()
		{
			return RightThumbAngle;
		}

		public Transform GetRightOrigin()
		{
			return RightOrigin;
		}

		public Vector3 GetRightVelocity()
		{
			return RightVelocity;
		}

		public Vector3 GetRightAngularVelocity()
		{
			return RightAngularVelocity;
		}

	}
}