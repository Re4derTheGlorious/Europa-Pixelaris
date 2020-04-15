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
    private bool selectionEmpty = true;
    private bool selectionStarted = false;
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

            if (prov == null && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)))
            {
                for(int offX = 0; offX < 2; offX++)
                {
                    for(int offY = 0; offY < 2; offY++)
                    {
                        color = MapTools.GetMap().map_template.GetPixel(x+ (int)Mathf.Pow(-1, offX), y+ (int)Mathf.Pow(-1, offY));
                        prov = MapTools.IdToProv(MapTools.ColToId(color));
                        if (prov != null)
                        {
                            break;
                        }
                    }
                }
            }

            CameraInput();
            BasicInput(prov);

            if (MapTools.GetInterface().GetActiveInterface()!=null)
            {
                MapTools.GetInterface().activeInterface.Inputs(prov);
                
            }

        }
        else
        {
            inputBlocked = false;
        }
    }

    public void StartSelection(float x, float y)
    {
        Camera.main.GetComponent<CameraHandler>().StopZooming();
        selection = new Rect(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0, 0);
        selectionStarted = true;
        selectionEmpty = true;
    }
    public void UpdateSelection(float x, float y)
    {
        if (selectionStarted)
        {
            Camera.main.GetComponent<CameraHandler>().StopZooming();
            if (selection.size.x != 0 || selection.size.y != 0)
            {
                selectionEmpty = false;
            }
            selection.size = new Vector2(x - selection.x, y - selection.y);
        }
    }
    public void FinalizeSelection()
    {
        if (selectionStarted && !selectionEmpty)
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

            foreach (Army a in MapTools.GetSave().GetActiveNation().armies)
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
        }
        CancelSelection();
    }
    public void CancelSelection()
    {
        selection = new Rect(0, 0, 0, 0);
        selectionEmpty = true;
        selectionStarted = false;
    }
    public bool IsSelecting()
    {
        return selectionStarted && !selectionEmpty;
    }

    public void BlockInput()
    {
        inputBlocked = true;
    }

    //Inputs
    public void CameraInput()
    {
        Camera.main.GetComponent<CameraHandler>().CameraInput();
    }
    public void SelectionInput()
    {
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
                //rectangle select
                if (IsSelecting())
                {
                    UpdateSelection(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
                    FinalizeSelection();
                }
            }
        }
    }
    public void BasicInput(Province prov)
    {
        //Keyboard
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameObject.Find("UI_Hint").GetComponent<Hint>().Disable();
        }
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            MapTools.GetSave().GetTime().Faster();
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            MapTools.GetSave().GetTime().Slower();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MapTools.GetSave().GetTime().Pause();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            MapTools.GetInterface().EnableInterface("trade");
        }
    }
}
