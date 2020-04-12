using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RelFrame : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    RawImage symbol;
    NationSymbolClick clicker;
    public Classes.Nation nat;

    // Start is called before the first frame update
    void Start()
    {
        symbol = transform.GetChild(0).GetComponent<RawImage>();
        clicker = transform.GetChild(2).GetComponent<NationSymbolClick>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSymbol(int id)
    {
        Start();
        symbol.texture = Resources.Load("Symbols/Symb_" + id) as Texture2D;
        clicker.SetNation(MapTools.IdToNat(id));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (transform.parent.parent.parent.GetComponent<DiplomacyInterface>() != null) {
            string hintText = transform.parent.parent.parent.GetComponent<DiplomacyInterface>().owner.name+" relation towards "+nat.name+"\n";
            hintText += transform.parent.parent.parent.GetComponent<DiplomacyInterface>().owner.rel.GetRelationToward(nat).GetFinanses();
            hintText += "\n\nTheir treaties being:\n\n";
            hintText += transform.parent.parent.parent.GetComponent<DiplomacyInterface>().owner.rel.GetTreatiesWith(nat);
            MapTools.GetHint().Enable(hintText, transform.position);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        MapTools.GetHint().Disable();
    }
}
