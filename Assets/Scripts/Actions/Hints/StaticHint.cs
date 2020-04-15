using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StaticHint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string hintText = "";
    private float delay = 0.25f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDelay(float d)
    {
        delay = d;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MapTools.GetHint().Enable(hintText, transform.position, delay);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MapTools.GetHint().Disable();
    }
}
