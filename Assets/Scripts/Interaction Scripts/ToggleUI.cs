/* Attach this script to the RightController GameObject.
 * Please remember to specify which public GameObjects to use, inside Unity.
 * By convention, each Unity GameObject should share the same name as its corresponding C# GameObject.
 * 
 * This script automatically shows and hides UI elements, according to the user's actions.
 * For example, if the user holds down the left middle-finger trigger (to teleport somwehere),
 * then the right laser pointer (used to set waypoints) will disappear.
 * Similarly, if the user holds down the right middle-finger trigger (to set waypoints),
 * then the left bezier pointer (used to teleport) will disappear.
 * 
 * For questions, ask Apollo 
 */

namespace ISAACS
{
    using System.Collections;
    using System.Collections.Generic;
    using VRTK;
    using UnityEngine;

    public class ToggleUI : MonoBehaviour
    {
        public GameObject LeftController;
        public GameObject controller_left;
        public GameObject controller_right;
        public GameObject leftUIPanel; /* The panel showing possible actions for the current agent. */
        private enum pressedState { INIT, LEFT, RIGHT, BOTH };
        private static pressedState pressed;

        void Update()
        {
            var right = GetComponent<VRTK_ControllerEvents>();
            var left = LeftController.GetComponent<VRTK_ControllerEvents>();

            if (right.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress))
            {
                if (left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress))
                {
                    /* If both middle-finger triggers have not already been pressed. */
                    if (pressed != pressedState.BOTH)
                    {
                        /* Disable the laser pointer used for setting waypoints. */
                        GetComponent<VRTK_StraightPointerRenderer>().enabled = false;
                        GetComponent<VRTK_Pointer>().enabled = false;
                        controller_right.GetComponent<ControllerInteractions>().enabled = false;
                        /* Disable the bezier pointer used for teleportation. */
                        LeftController.GetComponent<VRTK_BezierPointerRenderer>().enabled = false;
                        LeftController.GetComponent<VRTK_Pointer>().enabled = false;
                        /* Hide both controllers to give the user a better view for scaling. */
                        controller_right.SetActive(false);
                        controller_left.SetActive(false);
                        leftUIPanel.SetActive(false);
                        /* Let the program know that both triggers have already been pressed. */
                        pressed = pressedState.BOTH;
                    }
                }
                /* If only the right trigger is currently being pressed,
                 * and it was not previously pressed. */
                else if (pressed != pressedState.RIGHT)
                {
                    if (pressed == pressedState.BOTH)
                    {
                        controller_right.SetActive(true);
                        controller_left.SetActive(true);

                        controller_right.GetComponent<ControllerInteractions>().enabled = true;
                        GetComponent<VRTK_StraightPointerRenderer>().enabled = true;
                        GetComponent<VRTK_Pointer>().enabled = true;

                        LeftController.GetComponent<VRTK_BezierPointerRenderer>().enabled = true;
                        LeftController.GetComponent<VRTK_Pointer>().enabled = true;

                        leftUIPanel.SetActive(true);

                    }
                    else if (pressed == pressedState.LEFT)
                    {
                        /* Hide the right UI panel to give the user a better view for placing waypoints. */
                        leftUIPanel.SetActive(true);
                    }
                    pressed = pressedState.RIGHT;
                }
            }
            else if (left.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress))
            {
                if (pressed != pressedState.LEFT)
                {
                    if (pressed == pressedState.BOTH)
                    {
                        controller_right.SetActive(true);
                        controller_left.SetActive(true);

                        controller_right.GetComponent<ControllerInteractions>().enabled = true;
                        GetComponent<VRTK_StraightPointerRenderer>().enabled = true;
                        GetComponent<VRTK_Pointer>().enabled = true;

                        LeftController.GetComponent<VRTK_BezierPointerRenderer>().enabled = true;
                        LeftController.GetComponent<VRTK_Pointer>().enabled = true;
                    }
                    else if (pressed == pressedState.RIGHT)
                    {
                        leftUIPanel.SetActive(false);
                    }
                    else
                    {
                        leftUIPanel.SetActive(false);
                    }
                    pressed = pressedState.LEFT;
                }
            }
            /* If the user release both triggers. */
            else if (pressed != pressedState.INIT)
            {
                if (pressed == pressedState.BOTH)
                {
                    controller_right.SetActive(true);
                    controller_left.SetActive(true);

                    controller_right.GetComponent<ControllerInteractions>().enabled = true;
                    GetComponent<VRTK_StraightPointerRenderer>().enabled = true;
                    GetComponent<VRTK_Pointer>().enabled = true;

                    LeftController.GetComponent<VRTK_BezierPointerRenderer>().enabled = true;
                    LeftController.GetComponent<VRTK_Pointer>().enabled = true;

                    leftUIPanel.SetActive(true);
                }
                if (pressed == pressedState.LEFT)
                {
                    leftUIPanel.SetActive(true);
                }
                pressed = pressedState.INIT;
            }
        }
    }
}