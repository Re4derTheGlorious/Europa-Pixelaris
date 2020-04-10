using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
