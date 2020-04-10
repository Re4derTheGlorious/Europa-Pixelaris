using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        foreach(Province prov in GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetProvinces())
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
