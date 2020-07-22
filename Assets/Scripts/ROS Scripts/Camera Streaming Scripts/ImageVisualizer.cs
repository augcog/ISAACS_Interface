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
        Texture2D thisTexture = new Texture2D((int) data.GetWidth(), (int) data.GetHeight());
        Debug.Log(data.GetEncoding());
        //encoding type is RGB8
        byte[] pixelsR8 = data.GetImage();
        Color32[] pixelsRGBA32 = new Color32[pixelsR8.Length];
        for (int i = pixelsR8.Length - 1; i != -1; i--)
        {
            byte value = pixelsR8[i];
            pixelsRGBA32[i] = new Color32(value, value, value, 255);//simplest R8 to RGBA32 conversion
        }
        thisTexture.SetPixels32(pixelsRGBA32);//updates textureRGBA32 data in CPU memory
        thisTexture.Apply();//sends textureRGBA32 data from CPU memory to GPU (without this rendering wont change a bit)
        imageTarget.GetComponent<Renderer>().material.mainTexture = thisTexture;
    }

    
}
