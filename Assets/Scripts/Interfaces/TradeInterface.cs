using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Pathfinding;

public class TradeInterface : Interface
{
    private GameObject selectors;
    private GameObject frames;
    private Classes.TradeRoute route;
    private Nation nat;

    void Start()
    {
        selectors = transform.Find("Routes").GetChild(0).gameObject;
        frames = transform.Find("Routes").GetChild(1).gameObject;
    }

    void Update()
    {
        foreach(Transform selector in selectors.transform)
        {
            if (selector.gameObject.GetComponent<TradeRouteButton>().isActive)
            {
                if (selector.gameObject.GetComponent<TradeRouteButton>().owner != route)
                {
                    route = selector.gameObject.GetComponent<TradeRouteButton>().owner;
                    Refresh();
                }
            }
        }
    }

    public override void MouseInput(Province prov)
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            //for developer to setup trade goods
            //if (prov)
            //{
            //    if (Input.GetMouseButtonDown(0))
            //    {
            //        if (prov.tradeGood.name.Equals("Rice"))
            //        {
            //            prov.tradeGood.name = "Timber";
            //        }
            //        else if (prov.tradeGood.name.Equals("Timber"))
            //        {
            //            prov.tradeGood.name = "Iron";
            //        }
            //        else if (prov.tradeGood.name.Equals("Iron"))
            //        {
            //            prov.tradeGood.name = "Chinaware";
            //        }
            //        else if (prov.tradeGood.name.Equals("Chinaware"))
            //        {
            //            prov.tradeGood.name = "Silk";
            //        }
            //        else if (prov.tradeGood.name.Equals("Silk"))
            //        {
            //            prov.tradeGood.name = "Gold";
            //        }
            //        else
            //        {
            //            prov.tradeGood.name = "Rice";
            //        }
            //        prov.RefreshIcon("trade");
            //    }
            //    if (Input.GetMouseButtonDown(1))
            //    {
            //        MapTools.GetMap().ProvinceBackuper();
            //    }
            //}

            if (Input.GetMouseButtonDown(0))
            {
                MapTools.GetInterface().EnableMapMode("trade");
                MapTools.GetInterface().interface_trade.GetComponent<TradeInterface>().AddToRoute(prov);
            }
            if (Input.GetMouseButtonDown(1))
            {
                MapTools.GetInterface().EnableMapMode("trade");
                MapTools.GetInterface().interface_trade.GetComponent<TradeInterface>().RemoveFromRoute(prov);
            }
        }
    }
    public override void KeyboardInput(Province prov)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MapTools.GetInterface().EnableInterface("none");
        }
    }


    public override void Disable()
    {
        nat = null;
        gameObject.SetActive(false);
    }

    public override bool IsSet()
    {
        return route != null;
    }

    public override void Enable()
    {
        GameObject.Find("Canvas").GetComponent<InterfaceHandler>().EnableMapMode("trade");
        gameObject.SetActive(true);
        LoadEconomySelectors();
    }

    public override void Refresh()
    {
        ClearFrames();
        RefreshSelectors();

        GameObject newFrame = null;

        //initial
        newFrame = Instantiate(Resources.Load("Prefabs/UI_Routeframe") as GameObject, frames.transform);
        newFrame.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Initial Value: " + route.ValueAt(route.route.Count).ToString("F2");
        newFrame.transform.GetChild(1).gameObject.SetActive(false);

        //route
        for (int i = route.route.Count - 1; i >= 0; i--)
        {
            if (route.route[i] != null)
            {
                newFrame = Instantiate(Resources.Load("Prefabs/UI_Routeframe") as GameObject, frames.transform);
                newFrame.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = route.route.ElementAt(i).provName + " for " + (route.ValueAt(i) - route.ValueAt(i + 1)).ToString("F2") + " loss";
            }
        }

        //final
        newFrame = Instantiate(Resources.Load("Prefabs/UI_Routeframe") as GameObject, frames.transform);
        newFrame.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Final Gain: " + route.ValueAt(0).ToString("F2");

        //validity
        if (route.IsValid())
        {

        }
        else
        {

        }
    }

    public override void Set(Nation nat = null, Province prov = null, Army arm = null, Classes.TradeRoute route = null, List<Army> armies = null, List<Unit> units = null, Battle battle = null)
    {
        Start();
        this.nat = nat;
        this.route = nat.tradeRoutes.ElementAt(0);
        foreach(Classes.TradeRoute rt in nat.tradeRoutes)
        {
            if (rt.rep != null)
            {
                if (rt.rep.GetComponent<TradeRouteButton>().isActive)
                {
                    this.route = rt;
                }
            }
            else
            {

            }
        }
        Refresh();
    }

    public void AddToRoute(Province prov)
    {
        if (prov != null)
        {
            if (route.route.Count == 0)
            {
                if (prov.owner != route.owner)
                {
                    GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Route must start in your own province");
                }
                else if (prov.mods.GetMod("trade_hub") != 1)
                {
                    GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Route must start in province with trade hub");
                }
                else
                {
                    route.StartRoute(prov);
                    GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Route started in " + prov.provName);
                    Refresh();
                }
            }
            else if (route.route.Count >= 1)
            {
                if (prov.owner == route.owner)
                {
                    GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Route must end in a foreign province");
                }
                else if (prov.mods.GetMod("trade_hub") != 1)
                {
                    GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Route must end in province with trade hub");
                }
                else
                {
                    List<Province> path = SetPathTo(prov);
                    if (path.Count > 1)
                    {
                        route.SetRoute(path);
                        GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Route finished succesfuly");
                        Refresh();
                    }
                    else
                    {
                        GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cant find route");
                    }
                }
            }
        }
    }

    public List<Province> SetPathTo(Province prov)
    {
        Vector2 start = MapTools.LocalToScale(route.route.ElementAt(0).center);
        Vector2 end = MapTools.LocalToScale(prov.center);

        var p = GetComponent<Seeker>().StartPath(start, end, OnPathComplete);

        p.BlockUntilCalculated();

        List<Province> path = new List<Province>();
        foreach (Vector3 vec in p.vectorPath)
        {
            path.Add(MapTools.ScaleToProv(vec));
        }
        return path;
    }

    public void OnPathComplete(Path p)
    {

    }

    public void RemoveFromRoute(Province prov)
    {
        if (route.route.Contains(prov))
        {
            route.ClearRoute();
            GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Route cleared");
            Refresh();
        }
    }

    public void ClearFrames()
    {
        foreach (Transform obj in frames.transform)
        {
            Destroy(obj.gameObject);
        }
        frames.transform.DetachChildren();
    }

    public void RefreshSelectors()
    {
        ClearSelectors();
        foreach (Classes.TradeRoute rt in nat.tradeRoutes)
        {
            TradeRouteButton newRt = rt.Representate(selectors).GetComponent<TradeRouteButton>();
            newRt.owner = rt;
            if (rt == route)
            {
                newRt.Activate();
            }
        }
    }

    public void ClearSelectors()
    {
        foreach (Transform obj in selectors.transform)
        {
            Destroy(obj.gameObject);
        }
        selectors.transform.DetachChildren();
    }

    public void LoadEconomySelectors()
    {
        if (transform.Find("Economy/Selectors/Viewport/Content/").childCount == 0)
        {
            foreach(Classes.TradeGood good in MapTools.GetSave().GetEconomy().tradeGoods)
            {
                GameObject icon = Instantiate(Resources.Load("Prefabs/UI_InteractableIcon") as GameObject, transform.Find("Economy/Selectors/Viewport/Content/"));
                Texture2D tex = Resources.Load("Icons/Trade/Trade_" + good.name) as Texture2D;
                if (tex == null)
                {
                    tex = Resources.Load("Icons/Trade/Trade_Icon") as Texture2D;
                }
                icon.GetComponent<RawImage>().texture = tex;
                icon.GetComponent<Button>().onClick.AddListener(delegate { LoadGlobalEconomy(good); });
            }
        }
    }
    public void LoadGlobalEconomy(Classes.TradeGood good)
    {
        Texture2D tex = Resources.Load("Icons/Trade/Trade_" + good.name) as Texture2D;
        if (tex == null)
        {
            tex = Resources.Load("Icons/Trade/Trade_Icon") as Texture2D;
        }
        transform.Find("Economy/GlobalEconomy/Icon").GetComponent<RawImage>().texture = tex;
        transform.Find("Economy/GlobalEconomy/Name").GetComponent<TextMeshProUGUI>().text = good.name+" Market";

        List<Economy.Pair> ranking = MapTools.GetSave().GetEconomy().ranking[good];

        foreach(Transform trans in transform.Find("Economy/GlobalEconomy/Chart/Scroll/Viewport/Content/"))
        {
            Destroy(trans.gameObject);
        }
        transform.Find("Economy/GlobalEconomy/Chart/Scroll/Viewport/Content/").DetachChildren();

        int i = 0;
        for(i = 0; i<3; i++)
        {
            transform.Find("Economy/GlobalEconomy/Chart/Top/Chart_" + (i + 1) + "/Background/Text_Key").gameObject.SetActive(false);
        }
        transform.Find("Economy/GlobalEconomy/Chart/Chart_Player").gameObject.SetActive(false);


        i = 0;
        foreach(Economy.Pair pair in ranking)
        {
            if (i < 3)
            {
                transform.Find("Economy/GlobalEconomy/Chart/Top/Chart_" + (i + 1) + "/Background/Text_Key").gameObject.SetActive(true);
                transform.Find("Economy/GlobalEconomy/Chart/Top/Chart_"+(i+1)+"/Background/Text_Key").GetComponent<TextMeshProUGUI>().text = (i+1)+"."+ranking.ElementAt(i).nat.name;
                transform.Find("Economy/GlobalEconomy/Chart/Top/Chart_" + (i + 1) + "/Background/Text_Value").GetComponent<TextMeshProUGUI>().text = "+" + ranking.ElementAt(i).value;
            }
            else
            {
                Transform newEntry = GameObject.Instantiate(Resources.Load("Prefabs/UI_Chart_Entry") as GameObject, transform.Find("Economy/GlobalEconomy/Chart/Scroll/Viewport/Content")).transform;
                newEntry.Find("Background/Text_Key").GetComponent<TextMeshProUGUI>().text = (i + 1) + "." + ranking.ElementAt(i).nat.name;
                newEntry.Find("Background/Text_Value").GetComponent<TextMeshProUGUI>().text = "+" + ranking.ElementAt(0).value;
            }

            if (pair.nat == MapTools.GetSave().GetActiveNation())
            {
                if (ranking.Count >= 3) { 
                    transform.Find("Economy/GlobalEconomy/Chart/Chart_Player").gameObject.SetActive(true);
                    transform.Find("Economy/GlobalEconomy/Chart/Chart_Player/Background/Text_Key").GetComponent<TextMeshProUGUI>().text = (i + 1) + "." + ranking.ElementAt(i).nat.name;
                    transform.Find("Economy/GlobalEconomy/Chart/Chart_Player/Background/Text_Value").GetComponent<TextMeshProUGUI>().text = "+" + ranking.ElementAt(i).value;
                }
            }

            i++;
        }
    }

}
