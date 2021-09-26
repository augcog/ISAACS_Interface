
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ISAACS;

public class SpeedSlider : MonoBehaviour
{

    Slider mySlider;

    void Awake()
    {
        mySlider = GetComponent<Slider>(); // <-- you get access to the button component here
        mySlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }

    public void ValueChangeCheck()
    {
        Debug.Log(mySlider.value);
        WorldProperties.GetSelectedDrone().setSpeed(mySlider.value);
    }

}
