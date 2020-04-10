using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Hint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = Input.mousePosition;
        pos.x += GetComponent<RectTransform>().sizeDelta.x / 2+1;
        pos.y -= GetComponent<RectTransform>().sizeDelta.y / 2+1;
        GetComponent<RectTransform>().position = pos;
        
    }

    public void Enable(string text)
    {
        if (!gameObject.activeSelf)
        {
            transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
            this.gameObject.SetActive(true);
        }
    }
    public void Disable()
    {
        this.gameObject.SetActive(false);
    }
}
