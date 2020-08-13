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

    bool first = true;
    bool second = true;


    // Use this for initialization
    void Start () {
      

    }

    // Update is called once per frame
    void Update () {
		
	}

    /// <summary>
    /// called by OnReceiveMessage to set the videoPlayer to display given image data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="videoPlayer"></param>
	public void SetFrame(ImageMsg data, string videoPlayer)
    {
        Debug.Log(videoPlayer);
        if (videoPlayer == "")
        {
            return;
        }

        print(GameObject.Find(videoPlayer).ToString());
        print("Encoding: " + data.GetEncoding());
            
        GameObject.Find(videoPlayer).GetComponent<RawImage>().enabled = true;
        GameObject.Find(videoPlayer).GetComponent<RectTransform>().sizeDelta = new Vector2((int)data.GetWidth(), (int)data.GetHeight());
        Texture2D tex = new Texture2D((int)data.GetWidth(), (int)data.GetHeight(), TextureFormat.Alpha8, false);
        tex.LoadRawTextureData(data.GetImage());
        tex.Apply();
        GameObject.Find(videoPlayer).GetComponent<RawImage>().texture = tex;

        /*
        if (first || second)
        {
            first = false;
            second = false;
            byte[] _bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes("C:\\Users\vrab\\Desktop\\" + videoPlayer, _bytes);
            Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + "C:\\Users\vrab\\Desktop\\");

        }
        */
    }
}
