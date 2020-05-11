using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TerrainMapMode : MapMode
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
            MapTools.GetInterface().button_map_terrain.onClick.Invoke();
        }

        GameObject.Find("Map/Center").transform.Find("Features").gameObject.SetActive(true);
    }
    public override void Disable()
    {
        GameObject.Find("Map/Center").transform.Find("Features").gameObject.SetActive(false);
    }
    public override void Refresh()
    {

    }
}
