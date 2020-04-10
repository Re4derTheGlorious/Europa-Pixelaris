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
        GameObject.Find("Canvas").GetComponent<InterfaceHandler>().hint.GetComponent<Hint>().Enable(hintText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameObject.Find("Canvas").GetComponent<InterfaceHandler>().hint.GetComponent<Hint>().Disable();
    }
}
