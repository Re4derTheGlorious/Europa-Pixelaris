using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Radio_Button : MonoBehaviour, IPointerDownHandler
{
    public GameObject buttonGroup;
    public Texture2D active;
    public Texture2D inactive;
    public Color activeColor;
    public bool isActive = false;

    public bool textureTransition = true;
    public bool colorTransition = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Disable()
    {
        if (textureTransition)
        {
            gameObject.GetComponent<RawImage>().texture = inactive;
        }
        else if(colorTransition)
        {
            gameObject.GetComponent<RawImage>().color = Color.white;
        }
        isActive = false;
    }

    public void Enable()
    {
        if (textureTransition)
        {
            gameObject.GetComponent<RawImage>().texture = active;
        }
        else if(colorTransition)
        {
            gameObject.GetComponent<RawImage>().color = activeColor;
        }
        isActive = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        foreach(Transform button in buttonGroup.transform)
        {
            if(button.gameObject != gameObject)
            {
                button.gameObject.GetComponent<Radio_Button>().Disable();
            }
        }
        Enable();
    }
}
