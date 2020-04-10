using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NationSymbolClick : MonoBehaviour, IPointerDownHandler
{
    public Classes.Nation nat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameObject.Find("UI_Start") == null)
        {
            GameObject.Find("Map/Center").GetComponent<MapHandler>().activeProvince = null;
            if (GameObject.Find("Canvas").GetComponent<InterfaceHandler>().interface_province.gameObject.activeSelf)
            {
                GameObject.Find("Canvas").GetComponent<InterfaceHandler>().interface_province.SetActive(false);
            }

            if (nat == null)
            {
                GameObject.Find("Canvas").GetComponent<InterfaceHandler>().EnableInterface("diplomacy", GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetActiveNation(), null, null);
            }
            else
            {
                GameObject.Find("Canvas").GetComponent<InterfaceHandler>().EnableInterface("diplomacy", nat, null, null);
            }
        }
    }
}
