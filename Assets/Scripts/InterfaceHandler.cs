using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;

public class InterfaceHandler : MonoBehaviour
{
    TextMeshProUGUI text_wealth;
    TextMeshProUGUI text_manpower;
    GameObject text_time;
    GameObject text_nation;
    public GameObject frame_mapmode;
    public GameObject frame_time;
    GameObject icons_towns;
    GameObject icons_trade;
    GameObject arrows_trade;
    GameObject rect_selection;
    GameObject interface_top;
    public GameObject wheel_units;

    public GameObject selectorPrefab;
    public GameObject hubPrefab;

    //layers
    public GameObject layer_features;
    public GameObject layer_units;

    //mapmodes
    public List<MapMode> mapModes;
    public MapMode activeMapMode;
    public MapMode map_terrain;
    public MapMode map_trade;
    public MapMode map_diplo;
    public MapMode map_political;

    //interfaces
    public Interface activeInterface;
    public GameObject interface_trade;
    public GameObject interface_army;
    public GameObject interface_province;
    public GameObject interface_diplo;
    public GameObject interface_combat;
    public GameObject interface_menu;
    public GameObject interface_start;
    public List<Interface> interfaces;

    void Start()
    {
        ApplyScale();

        interface_top = GameObject.Find("UI_Top");
        rect_selection = GameObject.Find("Select_Rect");
        rect_selection.SetActive(false);
        wheel_units = GameObject.Find("Unitwheel");
        wheel_units.SetActive(false);
        arrows_trade = GameObject.Find("Map/Arrows/TradeRoutes");
        icons_towns = GameObject.Find("Map/Towns");
        icons_trade = GameObject.Find("Map/Trade");
        text_wealth = interface_top.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>();
        text_manpower = interface_top.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
        text_time = GameObject.Find("UI_Time");
        frame_mapmode = GameObject.Find("UI_Mapmode");
        frame_mapmode.SetActive(false);
        frame_time = GameObject.Find("UI_Time");
        frame_time.SetActive(false);
        text_nation = GameObject.Find("UI_Nation");

        //mapmodes
        mapModes = new List<MapMode>();
        map_political = new PoliticalMapMode();
        map_trade = new TradeMapMode();
        map_diplo = new DiploMapMode();
        map_terrain = new TerrainMapMode();
        mapModes.Add(map_political);
        mapModes.Add(map_trade);
        mapModes.Add(map_diplo);
        mapModes.Add(map_terrain);

        EnableMapMode("terrain");

        //interfaces
        interfaces = new List<Interface>();
        interface_combat = GameObject.Find("UI_Combat_Interface");
        interface_trade = GameObject.Find("UI_Trade_Interface");
        interface_army = GameObject.Find("UI_Army_Interface");
        interface_province = GameObject.Find("UI_Province_Interface");
        interface_diplo = GameObject.Find("UI_Diplo_Interface");
        interface_menu = GameObject.Find("UI_Menu_Interface");
        interface_start = transform.Find("UI_Start_Interface").gameObject;
        interfaces.Add(interface_combat.GetComponent<CombatInterface>());
        interfaces.Add(interface_trade.GetComponent<TradeInterface>());
        interfaces.Add(interface_army.GetComponent<ArmyInterface>());
        interfaces.Add(interface_province.GetComponent<ProvinceInterface>());
        interfaces.Add(interface_diplo.GetComponent<DiplomacyInterface>());
        interfaces.Add(interface_menu.GetComponent<MenuInterface>());
        interfaces.Add(interface_start.GetComponent<StartInterface>());
        interfaces.Add(GetComponent<NoneInterface>());
        interface_combat.SetActive(false);
        interface_trade.SetActive(false);
        interface_army.SetActive(false);
        interface_province.SetActive(false);
        interface_diplo.SetActive(false);
        interface_menu.SetActive(false);

        activeInterface = interface_start.GetComponent<StartInterface>(); ;
        activeInterface.Enable();
    }

    void Update()
    {
        //refresh wealth and power
        if (MapTools.GetSave().GetActiveNation() != null && !GetActiveInterface().Equals("start")) {
            text_wealth.GetComponent<TextMeshProUGUI>().text = MapTools.GetSave().GetActiveNation().wealth.ToString("F0");
            text_manpower.GetComponent<TextMeshProUGUI>().text = MapTools.GetSave().GetActiveNation().man.ToString();
        }

        //refresh time
        int year = MapTools.GetSave().GetTime().startYear + MapTools.GetSave().GetTime().year;
        int day = MapTools.GetSave().GetTime().day;
        int hour = MapTools.GetSave().GetTime().hour;
        string month = MapTools.GetSave().GetTime().GetMonthName();
        if (hour > 23)
        {
            hour = 23;
        }
        frame_time.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = ""+day+" "+month+"\n"+year;
        frame_time.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = ""+hour;
        frame_time.transform.GetChild(0).GetComponent<RawImage>().texture = Resources.Load("Pacers/Pacer_"+ MapTools.GetSave().GetTime().GetPaceAbsolute()) as Texture2D;
        if (MapTools.GetSave().GetTime().IsPaused())
        {
            frame_time.transform.GetChild(0).GetComponent<RawImage>().color = Color.gray;
        }
        else
        {
            frame_time.transform.GetChild(0).GetComponent<RawImage>().color = Color.white;
        }

        //selection rectangle
        if(MapTools.GetInput().IsSelecting())
        {
            Vector3 newSize = new Vector2(0, 0);
            newSize = MapTools.GetInput().selection.size;
            newSize.z = newSize.y;
            newSize.y = 1;
            rect_selection.transform.localScale = newSize/10;

            Vector3 newPos = MapTools.GetInput().selection.position;
            newPos.x += newSize.x / 2;
            newPos.y += newSize.z / 2;
            newPos.z = rect_selection.transform.position.z;
            rect_selection.transform.position = newPos;

            rect_selection.SetActive(true);
        }
        else
        {
            rect_selection.SetActive(false);
        }
    }

 
    public void ApplyScale()
    {
        GetComponent<CanvasScaler>().scaleFactor = PlayerPrefs.GetFloat("UI_scale");
    }

    public string GetActiveInterface()
    {
        if (activeInterface == interface_army.GetComponent<Interface>())
        {
            return "army";
        }
        else if (activeInterface == interface_province.GetComponent<Interface>())
        {
            return "province";
        }
        else if (activeInterface == interface_diplo.GetComponent<Interface>())
        {
            return "diplomacy";
        }
        else if (activeInterface == interface_trade.GetComponent<Interface>())
        {
            return "trade";
        }
        else if(activeInterface == interface_combat.GetComponent<Interface>())
        {
            return "combat";
        }
        else if (activeInterface == interface_menu.GetComponent<Interface>())
        {
            return "menu";
        }
        else if (activeInterface == interface_start.GetComponent<Interface>())
        {
            return "start";
        }
        else if (activeInterface == GetComponent<NoneInterface>())
        {
            return "none";
        }

        return null;
    }

    public void EnableMapMode(string name)
    {
        //Debug.Log("Map Mode enabled: " + name);
        if (name.Equals("none") || name.Equals("political"))
        {
            activeMapMode = map_political;
            map_political.Enable();
        }
        else if (name.Equals("diplomacy"))
        {
            activeMapMode = map_diplo;
            map_diplo.Enable();
        }
        else if (name.Equals("terrain"))
        {
            activeMapMode = map_terrain;
            map_terrain.Enable();
        }
        else if (name.Equals("trade"))
        {
            activeMapMode = map_trade;
            map_trade.Enable();
        }

        //disable inactive
        foreach (MapMode m in mapModes)
        {
            if (m != activeMapMode)
            {
                m.Disable();
            }
        }
    }

    public void RefreshMapMode()
    {
        if (activeMapMode != null)
        {
            activeMapMode.Refresh();
        }
    }

    public void EnableInterface(string name, Nation nat = null, Province prov = null, Army arm = null, List<Army> armies = null, List<Unit> units = null)
    {
        //Debug.Log("Interface enabled: "+name);
        if (name.Equals("none"))
        {
            activeInterface = GetComponent<NoneInterface>();
        }
        else if(name.Equals("province"))
        {
            activeInterface = interface_province.GetComponent<ProvinceInterface>();
            activeInterface.Set(prov:prov);
            activeInterface.Enable();
        }
        else if (name.Equals("diplomacy"))
        {
            activeInterface = interface_diplo.GetComponent<DiplomacyInterface>();
            activeInterface.Set(nat:nat);
            activeInterface.Enable();
        }
        else if (name.Equals("trade"))
        {
            activeInterface = interface_trade.GetComponent<TradeInterface>();
            activeInterface.Set(MapTools.GetSave().GetActiveNation());
            activeInterface.Enable();
        }
        else if (name.Equals("army"))
        {
            activeInterface = interface_army.GetComponent<ArmyInterface>();
            activeInterface.Enable();
            activeInterface.Set(armies: MapTools.GetMap().activeArmies);
        }
        else if (name.Equals("combat"))
        {
            activeInterface = interface_combat.GetComponent<CombatInterface>();
            activeInterface.Enable();
        }
        else if (name.Equals("menu"))
        {
            activeInterface = interface_menu.GetComponent<MenuInterface>();
            activeInterface.Enable();
        }
        else if (name.Equals("start"))
        {
            activeInterface = interface_start.GetComponent<StartInterface>();
            activeInterface.Enable();
        }

        //disable inactive
        foreach (Interface i in interfaces)
        {
            if (i != activeInterface)
            {
                i.Disable();
            }
        }
    }

    public void RefreshInterface()
    {
        if (activeInterface != null)
        {
            activeInterface.Refresh();
        }
    }

    public void TickInterface()
    {
        if (activeInterface != null)
        {
            if (activeInterface.hourlyTick)
            {
                activeInterface.Refresh();
            }
            if (MapTools.GetSave().GetTime().hour == 0)
            {
                if (activeInterface.dailyTick == true)
                {
                    activeInterface.Refresh();
                }
            }
            if (MapTools.GetSave().GetTime().day == 1 && MapTools.GetSave().GetTime().hour == 0)
            {
                if (activeInterface.monthlyTick == true)
                {
                    activeInterface.Refresh();
                }
            }
        }
    }

    public void SetSymbol(int id)
    {
        transform.Find("UI_Top").GetChild(0).GetComponent<RawImage>().texture = Resources.Load("Symbols/Symb_" + id) as Texture2D;
    }

    //buttons
    public void mapButtonTerrain()
    {
        EnableMapMode("terrain");
    }
    public void mapButtonPolitical()
    {
        EnableMapMode("political");
    }
    public void mapButtonTrade()
    {
        EnableMapMode("trade");
    }
    public void mapButtonDiplo()
    {
        EnableMapMode("diplo");
    }
}
