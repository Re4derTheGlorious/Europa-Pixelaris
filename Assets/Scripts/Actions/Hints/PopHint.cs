using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PopHint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
        GameObject.Find("Canvas").GetComponent<InterfaceHandler>().hint.GetComponent<Hint>().Enable(GameObject.Find("Map/Center").GetComponent<MapHandler>().activeProvince.growthMods.GetFinanses());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameObject.Find("Canvas").GetComponent<InterfaceHandler>().hint.GetComponent<Hint>().Disable();
    }
}

