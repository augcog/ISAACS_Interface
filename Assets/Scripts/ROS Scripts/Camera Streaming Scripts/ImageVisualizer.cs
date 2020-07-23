using ROSBridgeLib.sensor_msgs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageVisualizer : MonoBehaviour {

    //attached gameobject with texture2D that will change per frame
    //GameObject s;
    Texture2D originalTexture;

    string encoding;
    uint width;
    uint height;
    ImageMsg image;
    RawImage m_RawImage;


    // Use this for initialization
    void Start () {
      

    }

    // Update is called once per frame
    void Update () {
		
	}

	public void SetFrame(ImageMsg data)
    {
        Texture2D tex = new Texture2D((int)data.GetWidth(), (int)data.GetHeight(), TextureFormat.RGB24, false);
        GameObject.Find("CameraManager").GetComponent<RectTransform>().sizeDelta = new Vector2((int)data.GetWidth(), (int)data.GetHeight());
        tex.LoadRawTextureData(data.GetImage());
        tex.Apply();
        GameObject.Find("CameraManager").GetComponent<RawImage>().texture = tex;
    }


}
