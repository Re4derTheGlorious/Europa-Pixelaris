using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;
using System;

public class InputHandler : MonoBehaviour
{
    public Rect selection;
    private bool isSelecting = false;
    private int x = 0;
    private int y = 0;

    private MapHandler map;
    private InterfaceHandler inter;
    private bool inputBlocked = false;

    // Start is called before the first frame update
    void Start()
    {
        map = GameObject.Find("Map/Center").GetComponent<MapHandler>();
        inter = GameObject.Find("Canvas").GetComponent<InterfaceHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        //Keyboard
        if (!inter.GetActiveInterface().Equals("menu") && !inter.GetActiveInterface().Equals("selection"))
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                if (map.GameStarted())
                {
                    map.save.GetTime().Faster();
                }
            }
            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                if (map.GameStarted())
                {
                    map.save.GetTime().Slower();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                if (map.GameStarted())
                {
                    map.save.GetTime().Pause();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (inter.GetActiveInterface().Equals("none"))
                {
                    if (map.GameStarted())
                    {
                        inter.EnableInterface("menu");
                    }
                }
                else if (inter.GetActiveInterface().Equals("menu") && inter.activeInterface.IsSet())
                {
                    inter.activeInterface.GetComponent<MenuInterface>().Button_Return();
                }
                else
                {
                    inter.EnableInterface("none");
                }
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                if (inter.GetActiveInterface().Equals("army"))
                {
                    inter.interface_army.GetComponent<ArmyInterface>().NewArmyAction();
                }
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                if (inter.GetActiveInterface().Equals("army"))
                {
                    inter.interface_army.GetComponent<ArmyInterface>().HaltAction();
                }
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                if (map.GameStarted())
                {
                    inter.EnableInterface("trade");
                }
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                if (inter.GetActiveInterface().Equals("army"))
                {
                    inter.interface_army.GetComponent<ArmyInterface>().MergeAction();
                }
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                if (inter.GetActiveInterface().Equals("army"))
                {
                    inter.interface_army.GetComponent<ArmyInterface>().SplitAction();
                }
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Suprise Motherfucker\n:)");
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                if (inter.GetActiveInterface().Equals("army"))
                {
                    inter.interface_army.GetComponent<ArmyInterface>().ReorgAction();
                }
            }
        }
    }

    public void StartSelection(float x, float y)
    {
        if (map.GameStarted())
        {
            //isSelecting = true;
            selection = new Rect(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0, 0);
        }
    }
    public void UpdateSelection(float x, float y)
    {
        if (map.GameStarted())
        {
            if (selection.size.x != 0 || selection.size.y != 0)
            {
                isSelecting = true;
            }
            selection.size = new Vector2(x - selection.x, y - selection.y);
        }
    }
    public void FinalizeSelection()
    {
        if (map.GameStarted())
        {
            if (IsSelecting())
            {
                if (selection.size.x < 0)
                {
                    Vector2 newPos = selection.position;
                    newPos.x += selection.size.x;
                    selection.position = newPos;

                    Vector2 newSize = selection.size;
                    newSize.x = Mathf.Abs(newSize.x);
                    selection.size = newSize;
                }
                if (selection.size.y < 0)
                {
                    Vector2 newPos = selection.position;
                    newPos.y += selection.size.y;
                    selection.position = newPos;

                    Vector2 newSize = selection.size;
                    newSize.y = Mathf.Abs(newSize.y);
                    selection.size = newSize;
                }

                foreach (Classes.Army a in map.save.GetActiveNation().armies)
                {
                    if (selection.Contains(a.rep.transform.position))
                    {
                        if (!map.activeArmies.Contains(a))
                        {
                            map.activeArmies.Add(a);
                        }
                    }
                }
                if (map.activeArmies.Count > 0)
                {
                    inter.EnableInterface("army");
                }

                selection = new Rect(0, 0, 0, 0);
                isSelecting = false;
            }
        }
    }
    public bool IsSelecting()
    {
        return isSelecting;
    }

    public void BlockInput()
    {
        inputBlocked = true;
    }

    public void ProcessInput(MapHandler map)
    {
        if (!inputBlocked)
        {
            if (!inter.GetActiveInterface().Equals("menu"))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    //get click position
                    Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    x = (int)map.ScaleToLocal(pos).x;
                    y = (int)map.ScaleToLocal(pos).y;


                    //get color
                    Color color = map.map_template.GetPixel(x, y);
                    Province prov = map.IdToProv(map.ColToId(color));

                    //selection screen
                    CountrySelectionInput(map, prov);

                    CameraInput();

                    //trade
                    if (inter.GetActiveInterface().Equals("trade"))
                    {
                        TradeInput(map, prov);
                        return;
                    }


                    BasicInput(map, prov);

                }
            }
        }
        else
        {
            inputBlocked = false;
        }
    }

    //Inputs
    public void CameraInput()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            Camera.main.GetComponent<CameraHandler>().OnMouseScroll();
        }
    }
    private void BasicInput(MapHandler map, Province prov)
    {
        //Mouse
        if (Input.GetMouseButtonDown(0))
        {
            //rectangle select
            StartSelection(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        }
        else if (Input.GetMouseButton(0))
        {
            //rectangle select
            UpdateSelection(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        }
        else if (Input.GetMouseButtonUp(0))
        {

            //clear selection
            if (map.activeArmies.Count > 0)
            {
                map.activeArmies.Clear();
                inter.EnableInterface("none");
            }
            if (map.activeProvince != null)
            {
                map.activeArmies.Clear();
                inter.EnableInterface("none");
            }

            //select province
            if (map.GameStarted())
            {
                if (!inter.GetActiveInterface().Equals("army"))
                {
                    if (prov == null)
                    {
                        inter.EnableInterface("none");
                        map.activeProvince = null;
                        map.activeArmies.Clear();
                    }
                    else
                    {
                        if (inter.GetActiveInterface().Equals("diplomacy"))
                        {
                            inter.EnableInterface("none");
                            inter.EnableInterface("diplomacy", prov.owner, null, null);
                        }
                        else if(!IsSelecting())
                        {
                            map.activeProvince = prov;
                            inter.EnableInterface("province", null, prov, null);
                        }
                    }
                }
            }

            //rectangle select
            if (IsSelecting())
            {
                UpdateSelection(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
                FinalizeSelection();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {


            //movement
            if (map.activeArmies.Count > 0)
            {
                foreach (Classes.Army a in map.activeArmies)
                {
                    if (a.owner == map.save.GetActiveNation())
                    {
                        if (!a.Move(prov))
                        {
                            GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("No acces to province!");
                        }
                    }
                }
            }

            //selection
            else
            {
                map.activeProvince = null;
                inter.EnableInterface("none", null, null, null);
            }
        }
    }
    private void CountrySelectionInput(MapHandler map, Province prov)
    {
        if (inter.GetActiveInterface().Equals("start"))
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (prov != null)
                {
                    GameObject.Find("UI_Nation").GetComponent<TextMeshProUGUI>().text = prov.owner.name;
                    map.save.GetActiveNation() = prov.owner;
                    inter.SetSymbol(map.save.GetActiveNation().id);
                    map.HighlightBorder(map.save.GetActiveNation().id, Color.yellow);
                }
                else
                {
                    GameObject.Find("UI_Nation").GetComponent<TextMeshProUGUI>().text = "Nation";
                    map.save.GetActiveNation() = null;
                    inter.SetSymbol(-1);
                    map.HighlightBorder(-1, Color.black);
                }
            }
        }
    }
    private void TradeInput(MapHandler map, Province prov)
    {
        if (Input.GetMouseButtonDown(0))
        {
            inter.interface_trade.GetComponent<TradeInterface>().AddToRoute(prov);
        }
        if (Input.GetMouseButtonDown(1))
        {
            inter.interface_trade.GetComponent<TradeInterface>().RemoveFromRoute(prov);
        }
    }
    private void Army(MapHandler map, Province prov)
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    if (map.ScaleToProv(Camera.main.ScreenToWorldPoint(Input.mousePosition)).owner == map.activeNation)
        //    {
        //        inter.wheel_units.GetComponent<Unitwheel>().Pivot(map.ScaleToProv(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
        //        inter.wheel_units.GetComponent<Unitwheel>().Activate(true, 4);
        //    }
        //}
        //else if (Input.GetMouseButtonDown(1))
        //{
        //    inter.wheel_units.GetComponent<Unitwheel>().Activate(false, 0);
        //}
    }
}
