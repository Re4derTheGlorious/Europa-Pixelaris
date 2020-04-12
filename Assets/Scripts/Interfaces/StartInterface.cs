using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

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

    public override void KeyboardInput(Province prov)
    {

    }
    public override void MouseInput(Province prov)
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        { 
            if (Input.GetMouseButtonDown(0))
            {
                if (prov != null)
                {
                    GameObject.Find("UI_Nation").GetComponent<TextMeshProUGUI>().text = prov.owner.name;
                    MapTools.GetSave().GetActiveNation() = prov.owner;
                    MapTools.GetInterface().SetSymbol(MapTools.GetSave().GetActiveNation().id);
                    MapTools.GetMap().HighlightBorder(MapTools.GetSave().GetActiveNation().id, Color.yellow);
                }
                else
                {
                    GameObject.Find("UI_Nation").GetComponent<TextMeshProUGUI>().text = "Nation";
                    MapTools.GetSave().GetActiveNation() = null;
                    MapTools.GetInterface().SetSymbol(-1);
                    MapTools.GetMap().HighlightBorder(-1, Color.black);
                }
            }
        }

        MapTools.GetInput().CameraInput();
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
        gameObject.SetActive(true);
        MapTools.GetFade().FadeOut(0);
    }
    public override void Refresh()
    {

    }

    public void Button_Start()
    {
        if (MapTools.GetSave().GetActiveNation() != null)
        {
            GameObject.Find("UI_Top/Symbol").GetComponent<Button>().interactable = true;
            GameObject.Find("Canvas").GetComponent<InterfaceHandler>().SetSymbol(GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetActiveNation().id);
            GameObject.Find("Canvas").transform.Find("UI_Time").gameObject.SetActive(true);
            GameObject.Find("Canvas").transform.Find("UI_Mapmode").gameObject.SetActive(true);

            GameObject.Find("Map").transform.Find("Armies").gameObject.SetActive(true);

            Camera.main.GetComponent<CameraHandler>().ZoomTo(MapTools.LocalToScale(MapTools.GetSave().GetActiveNation().capital.center), 7);
            GameObject.Find("Canvas").GetComponent<InterfaceHandler>().EnableInterface("none");
        }
    }
}
