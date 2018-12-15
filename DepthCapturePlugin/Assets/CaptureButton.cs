using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class CaptureButton : MonoBehaviour
{
    DepthCapture capture_;
    public GameObject quad;

    // Use this for initialization
    IEnumerator Start()
    {
        Debug.Log("Start");

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            capture_ = new DepthCapture();

            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

            capture_.Configure();
            capture_.Start();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnClick()
    {
        Debug.Log("OnClick");

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            int width = 0, height = 0;
            float[] pixels = null;
            capture_.AcquireNextFrame((pVideoData, videoWidth, videoHeight, pDepthData, depthWidth, depthHeight) =>
            {
                width = depthWidth;
                height = depthHeight;
                pixels = new float[width * height];
                Marshal.Copy(pDepthData, pixels, 0, width * height);
            });

            var texture = new Texture2D(width, height);
            for (var y = 0; y < (int)height; y++)
            {
                for (var x = 0; x < (int)width; x++)
                {
                    var v = pixels[y * width + x];
                    Color color;
                    if (float.IsNaN(v))
                    {
                        color = new Color(0f, 1f, 0f);
                    }
                    else
                    {
                        color = new Color(v, v, v);
                    }
                    texture.SetPixel(x, y, color);
                }
            }

            quad.GetComponent<Renderer>().material.mainTexture = texture;
            texture.Apply();
        }
    }

    void OnDestroy()
    {
        Debug.Log("OnDestroy");

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            capture_.Stop();
            capture_.Dispose();
        }
    }
}