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

    // function 1: new drone selected: update all the sensors
    // UpdateUI function: paramaters: List of Sensors
    // store the list of sensors as current sensors
    // selected sensor is the first sensor
    // update the ui with the first sensor's subscribers.

    // function 2: update the ui with the current selected sensor
    // we get the list of subscribers: sensor.GetSensorSubscribers()
    // this list of subscribers: eg: mesh, rad_level_1, rad_level3
    // generate len(list) number of toggles  with the text of each being the subcsriber string
    // upper bound of 6 subscibers per sensor

    // function 3: toggles pressed
    // when a toggle is switched off for subscriber "s" : sensor.Unsubscribe(string s);
    // when a toggle is switched on for subscriber "s" : sensor.Subscribe(string s);

    // function 4: cycle sensor/ display next sensor
    // we can have a queue of all the sensors
    // add the current sensor to the end
    // make the current sensor the first element in the queue
    // call function 2 to update the ui wuth current selected sensor


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
