using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NegotiationsHint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
        if (transform.parent.parent.GetComponent<DiplomacyInterface>().neg != null)
        {
            if (transform.parent.parent.GetComponent<DiplomacyInterface>().neg.NegValue() != 0)
            {
                string newText = "Their willignes to accept this deal:\n";
                newText += transform.parent.parent.GetComponent<DiplomacyInterface>().neg.NegValue();
                newText += "\n\n" + transform.parent.parent.GetComponent<DiplomacyInterface>().neg.NegValueBreakdown();


                MapTools.GetHint().Enable(newText, transform.position);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MapTools.GetHint().Disable();
    }
}
