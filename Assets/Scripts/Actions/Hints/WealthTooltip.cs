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
        if (MapTools.GameStarted())
        {
            MapTools.GetHint().Enable(MapTools.GetSave().GetActiveNation().expBook.GetFinanses(), transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (MapTools.GameStarted())
        {
            MapTools.GetHint().Disable();
        }
    }
}
