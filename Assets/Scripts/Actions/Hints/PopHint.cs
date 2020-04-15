using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

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

    void OnDisable()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MapTools.GetHint().Enable(MapTools.GetMap().activeProvince.growthMods.GetFinanses(), transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MapTools.GetHint().Disable();
    }
}

