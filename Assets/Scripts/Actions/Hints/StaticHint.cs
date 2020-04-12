using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StaticHint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string hintText = "";
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
        MapTools.GetHint().Enable(hintText, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MapTools.GetHint().Disable();
    }
}
