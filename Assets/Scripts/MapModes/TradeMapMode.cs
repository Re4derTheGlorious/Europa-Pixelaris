using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeMapMode : MapMode
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Enable()
    {
        if (MapTools.GetInterface().activeMapMode != this)
        {
            MapTools.GetInterface().button_map_trade.onClick.Invoke();
        }

        foreach (Province prov in GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetProvinces())
        {
            prov.RefreshIcon("trade");
        }
    }
    public override void Disable()
    {
        foreach (Province prov in GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetProvinces())
        {
            prov.RefreshIcon("none");
        }
    }
    public override void Refresh()
    {

    }
}
