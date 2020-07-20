using ROSBridgeLib.sensor_msgs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageVisualizer : MonoBehaviour {

    //attached gameobject with texture2D that will change per frame
    public GameObject imageTarget;

    string encoding;
    uint width;
    uint height;
    ImageMsg image;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetFrame(ImageMsg data)
    {
        Debug.Log(data.GetEncoding());
    }

    
}
