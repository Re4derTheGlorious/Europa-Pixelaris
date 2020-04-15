using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Hint : MonoBehaviour
{
    private Vector2 pivot;
    private float delay;
    private float enabledOn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.GetChild(0).GetComponent<CanvasGroup>().alpha > 0)
        {
            int xDir = 1;
            int yDir = 1;
            if (Camera.main.ScreenToViewportPoint(Input.mousePosition).x > 0.5)
            {
                xDir = -1;
            }
            if (Camera.main.ScreenToViewportPoint(Input.mousePosition).y < 0.5)
            {
                yDir = -1;
            }
            Vector2 newPos = pivot;
            float sizeFactor = 1/PlayerPrefs.GetFloat("UI_scale");
            newPos.x += xDir * transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x/2 / sizeFactor;
            newPos.y -= yDir * transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y/2 / sizeFactor;
            transform.GetChild(0).GetComponent<RectTransform>().position = newPos;
        }
    }

    public void Enable(string text, Vector2 newPivot, float delay = 0.5f)
    {
        CancelInvoke();
        this.delay = delay;
        enabledOn = Time.time;
        InvokeRepeating("FadeIn", 0, 0.01f);

        //Position
        pivot = newPivot;
        int xDir = 1;
        int yDir = 1;
        if (Camera.main.ScreenToViewportPoint(Input.mousePosition).x > 0.5)
        {
            xDir = -1;
        }
        if (Camera.main.ScreenToViewportPoint(Input.mousePosition).y < 0.5)
        {
            yDir = -1;
        }
        Vector2 newPos = pivot;
        transform.GetChild(0).GetComponent<RectTransform>().position = pivot;

        transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = text;
    }
    public void Disable()
    {
        CancelInvoke();
        InvokeRepeating("FadeOut", 0, 0.01f);
    }

    private void FadeIn()
    {
        if (Time.time >= enabledOn + delay) { 
            transform.GetChild(0).GetComponent<CanvasGroup>().alpha += (float)(0.05);

            if (transform.GetChild(0).GetComponent<CanvasGroup>().alpha >= 1)
            {
                CancelInvoke();
            }
        }
    }
    private void FadeOut()
    {
        transform.GetChild(0).GetComponent<CanvasGroup>().alpha -= (float)(0.05);

        if (transform.GetChild(0).GetComponent<CanvasGroup>().alpha <= 0)
        {
            CancelInvoke();
        }
    }
}
