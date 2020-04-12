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

    private bool inputBlocked = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!inputBlocked)
        {
            //get click position
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            x = (int)MapTools.ScaleToLocal(pos).x;
            y = (int)MapTools.ScaleToLocal(pos).y;

            //get color
            Color color = MapTools.GetMap().map_template.GetPixel(x, y);

            Province prov = MapTools.IdToProv(MapTools.ColToId(color));


            if (!MapTools.GetInterface().GetActiveInterface().Equals("none"))
            {
                MapTools.GetInterface().activeInterface.MouseInput(prov);
                MapTools.GetInterface().activeInterface.KeyboardInput(prov);
            }
            else 
            {
                CameraInput();
                BasicInput(prov);
            }

        }
        else
        {
            inputBlocked = false;
        }
    }

    public void StartSelection(float x, float y)
    {
        if (MapTools.GameStarted())
        {
            Camera.main.GetComponent<CameraHandler>().StopZooming();
            selection = new Rect(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0, 0);
        }
    }
    public void UpdateSelection(float x, float y)
    {
        if (MapTools.GameStarted())
        {
            Camera.main.GetComponent<CameraHandler>().StopZooming();
            if (selection.size.x != 0 || selection.size.y != 0)
            {
                isSelecting = true;
            }
            selection.size = new Vector2(x - selection.x, y - selection.y);
        }
    }
    public void FinalizeSelection()
    {
        if (MapTools.GameStarted())
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

                foreach (Classes.Army a in MapTools.GetSave().GetActiveNation().armies)
                {
                    if (selection.Contains(a.rep.transform.position))
                    {
                        if (!MapTools.GetMap().activeArmies.Contains(a))
                        {
                            MapTools.GetMap().activeArmies.Add(a);
                        }
                    }
                }
                if (MapTools.GetMap().activeArmies.Count > 0)
                {
                    MapTools.GetInterface().EnableInterface("army");
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

    //Inputs
    public void CameraInput()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            Camera.main.GetComponent<CameraHandler>().OnMouseScroll();
        }
    }
    public void BasicInput(Province prov)
    {
        //Keyboard
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            MapTools.GetSave().GetTime().Faster();
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            MapTools.GetSave().GetTime().Slower();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            MapTools.GetSave().GetTime().Pause();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {

            MapTools.GetInterface().EnableInterface("menu");
        }

        //Mouse
        if (!EventSystem.current.IsPointerOverGameObject())
        {
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
                if (MapTools.GetMap().activeArmies.Count > 0)
                {
                    MapTools.GetMap().activeArmies.Clear();
                    MapTools.GetInterface().EnableInterface("none");
                }
                if (MapTools.GetMap().activeProvince != null)
                {
                    MapTools.GetMap().activeArmies.Clear();
                    MapTools.GetInterface().EnableInterface("none");
                }

                //select province
                if (MapTools.GameStarted())
                {
                    if (!MapTools.GetInterface().GetActiveInterface().Equals("army"))
                    {
                        if (prov == null)
                        {
                            MapTools.GetInterface().EnableInterface("none");
                            MapTools.GetMap().activeProvince = null;
                            MapTools.GetMap().activeArmies.Clear();
                        }
                        else
                        {
                            if (MapTools.GetInterface().GetActiveInterface().Equals("diplomacy"))
                            {
                                MapTools.GetInterface().EnableInterface("none");
                                MapTools.GetInterface().EnableInterface("diplomacy", prov.owner, null, null);
                            }
                            else if (!IsSelecting())
                            {
                                MapTools.GetMap().activeProvince = prov;
                                MapTools.GetInterface().EnableInterface("province", null, prov, null);
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
                if (MapTools.GetMap().activeArmies.Count > 0)
                {
                    foreach (Classes.Army a in MapTools.GetMap().activeArmies)
                    {
                        if (a.owner == MapTools.GetSave().GetActiveNation())
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
                    MapTools.GetMap().activeProvince = null;
                    MapTools.GetInterface().EnableInterface("none", null, null, null);
                }
            }
        }
    }
}
