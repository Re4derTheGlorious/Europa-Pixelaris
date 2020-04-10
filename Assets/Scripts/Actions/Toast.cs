using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Toast : MonoBehaviour
{
    private float enabledOn = 0;
    private float duration = 0;
    private Color color = Color.black;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > enabledOn + duration)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
        else if(Time.time > enabledOn + duration / 2)
        {
            Color newColor = color;
            float fade = (Time.time - enabledOn) / duration;
            newColor.a = 1 - fade;
            transform.GetChild(0).GetComponent<RawImage>().color = newColor;

            newColor = Color.red;
            newColor.a = (1-fade)*2;
            transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().color = newColor;
        }
    }

    public void Enable(string text, float duration)
    {
        transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
        enabledOn = Time.time;
        this.duration = duration;
        Color newColor = color;
        newColor.a = 0.5f;
        transform.GetChild(0).GetComponent<RawImage>().color = newColor;
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.red;
    }

    public void Enable(string text)
    {
        Enable(text, 3);
    }
}
