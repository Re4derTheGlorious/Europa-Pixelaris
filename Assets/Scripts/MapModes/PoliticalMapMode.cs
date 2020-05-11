using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PoliticalMapMode : MapMode
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
            MapTools.GetInterface().button_map_political.onClick.Invoke();
        }
    }
    public override void Disable()
    {

    }
    public override void Refresh()
    {

    }
}
