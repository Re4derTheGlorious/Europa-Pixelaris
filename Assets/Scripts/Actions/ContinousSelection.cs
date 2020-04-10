using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ContinousSelection : MonoBehaviour,  IPointerExitHandler, IPointerEnterHandler
{
    private bool pointerOver = false;

    public bool alsoClearHint = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (pointerOver)
        {
            if (Input.GetMouseButton(0))
            {
                if (GameObject.Find("Map/Center").GetComponent<MapHandler>().input.IsSelecting())
                {
                    GameObject.Find("Map/Center").GetComponent<MapHandler>().input.UpdateSelection(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (GameObject.Find("Map/Center").GetComponent<MapHandler>().input.IsSelecting())
                {
                    GameObject.Find("Map/Center").GetComponent<MapHandler>().input.UpdateSelection(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
                    GameObject.Find("Map/Center").GetComponent<MapHandler>().input.FinalizeSelection();
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerOver = true;
        if (alsoClearHint)
        {
            GameObject.Find("Canvas").GetComponent<InterfaceHandler>().hint.GetComponent<Hint>().Disable();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerOver = false;
    }
}
