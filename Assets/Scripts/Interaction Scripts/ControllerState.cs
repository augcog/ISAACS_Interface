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
		private bool leftIsGrabbing; // Whether the left hand is grabbing another object.
		private bool leftIndex;
		private bool leftMiddle;
		private bool leftX;
		private bool leftY;
		private bool leftThumb;
		private Vector2 leftThumbDelta; // The distance by which the left stick has been moved in the X-axis and Y-axis.
		private float leftThumbAngle; // The angle of rotation of the left stick.
		private Transform leftOrigin; // The origin ("position") of the left controller.
		private Vector3 leftVelocity; // How fast the left controller is moving in space.
		private Vector3 leftAngularVelocity; // How fast the left controller is moving in space.

		// True indicates that the button is being pressed, False that it's released.
		private bool rightIsGrabbing; // Whether the right hand is grabbing another object.
		private bool rightIndex;
		private bool rightMiddle;
		private bool rightA;
		private bool rightB;
		private bool rightThumb;
		private Vector2 rightThumbDelta; // The distance by which the right stick has been moved in the X-axis and Y-axis.
		private float rightThumbAngle; // The angle of rotation of the right stick.
		private Transform rightOrigin; // The origin ("position") of the right controller.
		private Vector3 rightVelocity; // How fast the right controller is moving in space.
		private Vector3 rightAngularVelocity; // How fast the right controller is moving in space.


		// Update is called once per frame.
		void Update () {

			// These variables store the controller state, which can be queried for input information.
            var left = LeftController.GetComponent<VRTK_ControllerEvents>();
			var leftInteractGrab = LeftController.GetComponent<VRTK_InteractGrab>();
			var right = RightController.GetComponent<VRTK_ControllerEvents>();
			var rightInteractGrab = RightController.GetComponent<VRTK_InteractGrab>();

			// Receive input information from the left controller.
			leftIsGrabbing = leftInteractGrab.GetGrabbedObject != null;
			leftIndex = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerPress);
			leftMiddle = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress);
			leftX = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonOnePress);
			leftY = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress);
			leftThumb = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TouchpadPress);
			leftThumbDelta = left.GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
			leftThumbAngle = left.GetAxisAngle(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
			leftOrigin = VRTK_DeviceFinder.GetControllerOrigin(VRTK_DeviceFinder.GetControllerReferenceLeftHand());
			leftVelocity = VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand());
			leftAngularVelocity = VRTK_DeviceFinder.GetControllerAngularVelocity(VRTK_DeviceFinder.GetControllerReferenceLeftHand());

            // Receive input information from the right controller.
			rightIsGrabbing = rightInteractGrab.GetGrabbedObject != null;
			rightIndex = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerPress);
			rightMiddle = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress);
			rightA = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonOnePress);
			rightB = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress);
			rightThumb = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TouchpadPress);
			rightThumbDelta = right.GetAxis(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
			rightThumbAngle = right.GetAxisAngle(VRTK_ControllerEvents.Vector2AxisAlias.Touchpad);
			rightOrigin = VRTK_DeviceFinder.GetControllerOrigin(VRTK_DeviceFinder.GetControllerReferenceRightHand());
			rightVelocity = VRTK_DeviceFinder.GetControllerVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());
			rightAngularVelocity = VRTK_DeviceFinder.GetControllerAngularVelocity(VRTK_DeviceFinder.GetControllerReferenceRightHand());

			// TODO: Add getter methods.

		}
	}
}
