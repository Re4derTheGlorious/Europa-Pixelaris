using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeMapMode : MapMode
{
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

        if (GameObject.Find("Armies"))
        {
            GameObject.Find("Armies").SetActive(false);
        }
        //Vector3 newPos = MapTools.GetMap().transform.Find("Armies").position;
        //newPos.z = -100;
        //MapTools.GetMap().transform.Find("Armies").position = newPos;
    }
    public override void Disable()
    {
        foreach (Province prov in GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetProvinces())
        {
            prov.RefreshIcon("none");
        }

        GameObject.Find("Map").transform.Find("Armies").gameObject.SetActive(true);
        //Vector3 newPos = MapTools.GetMap().transform.Find("Armies").position;
        //newPos.z = 0;
        //MapTools.GetMap().transform.Find("Armies").position = newPos;
    }
    public override void Refresh()
    {

    }
}
