using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WealthTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameObject.Find("UI_Start") == null)
        { 
            GameObject.Find("Canvas").GetComponent<InterfaceHandler>().hint.GetComponent<Hint>().Enable(GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetActiveNation().expBook.GetFinanses());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameObject.Find("UI_Start") == null)
        {
            GameObject.Find("Canvas").GetComponent<InterfaceHandler>().hint.GetComponent<Hint>().Disable();
        }
    }
}
