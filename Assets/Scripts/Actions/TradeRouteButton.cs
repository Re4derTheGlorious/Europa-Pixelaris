using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TradeRouteButton : MonoBehaviour, IPointerDownHandler
{
    public bool isActive;
    public Classes.TradeRoute owner;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Activate()
    {
        foreach (GameObject gj in GameObject.FindGameObjectsWithTag("TradeRoute"))
        {
            gj.GetComponent<RawImage>().color = Color.white;
            gj.GetComponent<TradeRouteButton>().isActive = false;

        }
        GetComponent<RawImage>().color = Color.yellow;
        isActive = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Activate();
    }
}
