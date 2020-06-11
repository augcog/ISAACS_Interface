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
		private bool leftIndex;
		private bool leftMiddle;
		private bool leftX;
		private bool leftY;
		private bool leftThumb;
		private float leftThumbDeltaX; // The distance by which the left stick has been moved in the X-axis.
		private float leftThumbDeltaZ; // The distance by which the left stick has been moved in the Z-axis.
		private Vector3 leftPosition; // The position of the left controller.
		private Vector3 leftVelocity; // How fast the left controller is moving in space.

		// True indicates that the button is being pressed, False that it's released.
		private bool rightIndex;
		private bool rightMiddle;
		private bool rightA;
		private bool rightB;
		private bool rightThumb;
		private float rightThumbDeltaX; // The distance by which the tight stick has been moved in the X-axis.
		private float rightThumbDeltaZ; // The distance by which the tight stick has been moved in the Z-axis.
		private Vector3 rightPosition; // The position of the left controller.
		private Vector3 rightVelocity; // How fast the left controller is moving in space.


		// Update is called once per frame.
		void Update () {

			var right = RightController.GetComponent<VRTK_ControllerEvents>();
            var left = LeftController.GetComponent<VRTK_ControllerEvents>();

			leftIndex = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerPress);
			leftMiddle = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress);
			leftX = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonOnePress);
			leftY = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress);
			leftThumb = left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TouchpadPress);

			//float moveX = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
            //float moveZ = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
            //float deltaX = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

			rightIndex = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerPress);
			rightMiddle = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress);
			rightX = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonOnePress);
			rightY = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.ButtonTwoPress);
			rightThumb = right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TouchpadPress);


		}
	}

}
