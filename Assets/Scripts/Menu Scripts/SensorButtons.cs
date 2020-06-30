using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ISAACS;

public class SensorButtons : MonoBehaviour {

	Button thisButton;
	Drone thisDrone = WorldProperties.selectedDrone;

    public bool leftArrow = false;
	public bool rightArrow = true;

    void Awake()
    {
        thisButton = GetComponent<Button>(); // <-- you get access to the button component here
        thisButton.onClick.AddListener(() => { OnClickEvent(); });  // <-- you assign a method to the button OnClick event here
    }

    void OnClickEvent()
    {
        if (leftArrow)
        {
            //change selected sensor to the previous one
            //change thisDrone.selectedsensor to one before, IF IT CAN, if cant, cycle through to end of the list
        }

        if (rightArrow)
        {
            //change selected sensor to the next one
            //change thisDrone.selectedsensor to next one, IF IT CAN, if cant cycle through to beginning of the list
        }
    }
}
