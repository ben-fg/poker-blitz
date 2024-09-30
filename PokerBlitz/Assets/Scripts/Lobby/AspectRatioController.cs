using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AspectRatioController : MonoBehaviour
{
    private float targetAspect = 16f / 9f;

    void Start()
    {
        float windowAspect = Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        //If the screen is wider than the target aspect ratio, add horizontal black bars
        if (scaleHeight < 1.0f)
        {
            Rect rect = Camera.main.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            Camera.main.rect = rect;
        }
        //If the screen is taller than the target aspect ratio, add vertical black bars
        else
        {
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = Camera.main.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            Camera.main.rect = rect;
        }
    }

    void OnPreCull()
    {
        //Set the clear flag colour to black
        GL.Clear(true, true, Color.black);
    }
}
