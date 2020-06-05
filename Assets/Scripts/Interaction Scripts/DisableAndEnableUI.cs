using System.Collections;
using System.Collections.Generic;
using VRTK;
using UnityEngine;

public class DisableAndEnableUI : MonoBehaviour {

    public GameObject panel;
    private bool panelIsDisabled = false;

    void Update () {
        var controllerEvents = GetComponent<VRTK_ControllerEvents>();
        if (controllerEvents.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress)) {
            if (!panelIsDisabled)
            {
                panel.SetActive(false);
                panelIsDisabled = true;
            }
        }
        else if (panelIsDisabled)
        {
            panel.SetActive(true);
            panelIsDisabled = false;
        }
    }
}
