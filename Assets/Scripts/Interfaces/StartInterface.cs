using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartInterface : Interface
{
    public bool quickStart = false;

    void Update()
    {
        if (quickStart)
        {
            Button_Start();
        }
    }

    public override bool IsSet()
    {
        return false;
    }
    public override void Set(Classes.Nation nat = null, Province prov = null, Classes.Army arm = null, Classes.TradeRoute route = null, List<Classes.Army> armies = null, List<Classes.Unit> units = null, Battle battle = null)
    {

    }
    public override void Disable()
    {
        
        gameObject.SetActive(false);
    }
    public override void Enable()
    {
    }
    public override void Refresh()
    {

    }

    public void Button_Start()
    {
        if (GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetActiveNation() != null)
        {
            GameObject.Find("Canvas").GetComponent<InterfaceHandler>().SetSymbol(GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetActiveNation().id);
            GameObject.Find("Canvas").transform.Find("UI_Time").gameObject.SetActive(true);
            GameObject.Find("Canvas").transform.Find("UI_Mapmode").gameObject.SetActive(true);

            GameObject.Find("Map").transform.Find("Armies").gameObject.SetActive(true);

            Camera.main.GetComponent<CameraHandler>().ZoomTo(GameObject.Find("Map/Center").GetComponent<MapHandler>().LocalToScale(GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetActiveNation().capital.center), 7);
            GameObject.Find("Canvas").GetComponent<InterfaceHandler>().EnableInterface("none");
        }
    }
}
