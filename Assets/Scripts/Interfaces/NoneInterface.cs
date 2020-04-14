using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NoneInterface : Interface
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Refresh()
    {

    }
    public override void Set(Nation nat = null, Province prov = null, Army arm = null, Classes.TradeRoute route = null, List<Army> armies = null, List<Unit> units = null, Battle battle = null)
    {

    }
    public override bool IsSet()
    {
        return true;
    }
    public override void Disable()
    {

    }
    public override void Enable()
    {

    }
    public override void MouseInput(Province prov)
    {

        if (Input.GetMouseButtonUp(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            { 
                if (!MapTools.GetInput().IsSelecting())
                {
                    if (prov != null)
                    {
                        MapTools.GetMap().activeProvince = prov;
                        MapTools.GetInterface().EnableInterface("province", null, prov, null);
                    }
                }
            }
        }
    }
    public override void KeyboardInput(Province prov)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MapTools.GetInterface().EnableInterface("menu");
        }

        MapTools.GetInput().SelectionInput();

    }

}
