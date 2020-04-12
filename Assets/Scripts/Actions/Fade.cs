using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fade: MonoBehaviour
{
    private float target = 0;


    public void FadeIn(float target)
    {
        CancelInvoke();
        this.target = target;
        InvokeRepeating("FadeIn", 0, 0.01f);
    }

    private void FadeIn()
    {
        Color newColor = GetComponent<RawImage>().color;
        newColor.a += (float)(0.01);
        GetComponent<RawImage>().color = newColor;
        if (newColor.a > target)
        {
            CancelInvoke();
        }
    }

    public void FadeOut(float target)
    {
        CancelInvoke();
        this.target = target;
        InvokeRepeating("FadeOut", 0, 0.01f);
    }

    private void FadeOut()
    {
        Color newColor = GetComponent<RawImage>().color;
        newColor.a -= (float)(0.01);
        GetComponent<RawImage>().color = newColor;
        if (newColor.a <= 0)
        {
            CancelInvoke();
        }
    }
}
