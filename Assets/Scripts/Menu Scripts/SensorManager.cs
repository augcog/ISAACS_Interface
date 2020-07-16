using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ISAACS;

/// <summary>
/// A manager for all sensors that are present in the worldspace.
/// </summary>
public class SensorManager : MonoBehaviour {

    public Button leftButton;
    public Button rightButton;
    public Text sensorText;
    public GameObject togglePrefab;
    public GameObject sensorMenu;

    private List<ROSSensorConnectionInterface> sensorList = new List<ROSSensorConnectionInterface>();
    private Dictionary<string, bool> subscriberDict = new Dictionary<string, bool>();
    private ROSSensorConnectionInterface selectedSensor;
    private int selectedSensorPos;

    void Awake()
    {
        leftButton.onClick.AddListener(() => { ShowNextSensor(); });
        rightButton.onClick.AddListener(() => { ShowPreviousSensor(); });
    }

    // function 1: new drone selected: update all the sensors
    // UpdateUI function: paramaters: List of Sensors
    // store the list of sensors as current sensors
    // selected sensor is the first sensor
    // update the ui with the first sensor's subscribers.

    public void initializeSensorUI(List<ROSSensorConnectionInterface> allSensors)
    {
        sensorList.Clear();
        deleteOldSensors();

        if (allSensors.Count == 0)
        {
            Debug.Log("No sensors");
            sensorText.text = "No attached sensors";
            return;
        }

        foreach (ROSSensorConnectionInterface sensor in allSensors)
        {
            Debug.Log("Adding sensor to UI: " + sensor.GetSensorName());
            sensorList.Add(sensor);
        }
        
        Debug.Log("Selecting sensor: " + sensorList[0].GetSensorName());
        selectedSensor = sensorList[0];
        selectedSensorPos = 0;
        updateSensorUI(selectedSensor);

    }

    public void deleteOldSensors()
    {
        GameObject[] previousSensors = GameObject.FindGameObjectsWithTag("TOGGLESENSORS");
        foreach (GameObject prev in previousSensors)
        {
            Debug.Log("Deleting " + prev.name);
            Destroy(prev);
        }
        previousSensors = null;
    }

    /// <summary>
    /// update the ui with the current selected sensor, upper bound of 6 subscribers per sensor
    /// </summary>
    public void updateSensorUI(ROSSensorConnectionInterface inputSensor)
    {
        deleteOldSensors();

        sensorText.text = inputSensor.GetSensorName();
        subscriberDict = new Dictionary<string, bool>(inputSensor.GetSensorSubscribers());
        int subscribercount = 0;

        foreach (string subscriber in subscriberDict.Keys)
        {
            Debug.Log("Creating button for :" + subscriber);

            //Instantiating and positioning toggles
            float ypos = 60 - (subscribercount * 40);
            var position = new Vector3(-19, ypos, -14);
            GameObject toggleUI = Instantiate(togglePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            toggleUI.transform.parent = sensorMenu.transform;
            toggleUI.transform.localPosition = position;
            toggleUI.transform.localScale = Vector3.one *1.3f;
            toggleUI.transform.localRotation = Quaternion.identity;
            toggleUI.tag = "TOGGLESENSORS";

            //making toggles w/ correct labels and subscribers
            Toggle thisToggle = toggleUI.GetComponent<Toggle>();
            Text toggleName = toggleUI.GetComponentInChildren<Text>();
            toggleName.text = subscriber;
            thisToggle.isOn = subscriberDict[subscriber];
            thisToggle.onValueChanged.AddListener(delegate { ToggleValueChanged(thisToggle); });
            subscribercount++;
            
        }
    }

    /// <summary>
    /// Subscribes/unsubscribes sensor when toggle THISTOGGLE is pressed
    /// </summary>
    void ToggleValueChanged(Toggle thisToggle)
    {
        Text selectedSubscriberName = thisToggle.GetComponentInChildren<Text>();

        if (thisToggle.isOn)
        {
            selectedSensor.Subscribe(selectedSubscriberName.text);
            Debug.Log("Subscribing: " + thisToggle.GetComponentInChildren<Text>().text);
        }

        else
        {
            selectedSensor.Unsubscribe(selectedSubscriberName.text);
            Debug.Log("Unsubscribing: " + thisToggle.GetComponentInChildren<Text>().text);
        }
    }

    /// <summary>
    /// Cycle sensor/ display next sensor
    /// </summary>
    public void ShowNextSensor()
    {
        if (selectedSensorPos == sensorList.Count-1)
        {
            selectedSensorPos = 0;
        }
        else
        {
            selectedSensorPos += 1 ;
        }

        selectedSensor = sensorList[selectedSensorPos];
        updateSensorUI(selectedSensor);
    }

    public void ShowPreviousSensor()
    {
        if (selectedSensorPos == 0)
        {
            selectedSensorPos = sensorList.Count - 1;
        }
        else
        {
            selectedSensorPos -= 1;
        }

        selectedSensor = sensorList[selectedSensorPos];
        updateSensorUI(selectedSensor);
    }

    public ROSSensorConnectionInterface getSelectedSensor()
    {
        return selectedSensor;
    }

    public Dictionary<string,bool> getSubscriberDict()
    {
        return subscriberDict;
    }
}
