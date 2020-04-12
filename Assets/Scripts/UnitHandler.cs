using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using Pathfinding;
using UnityEngine.EventSystems;

public class UnitHandler : MonoBehaviour
{
    public List<Classes.Army> owners = new List<Classes.Army>();

    private Vector3 orginalScale;
    private float ratio;
    // Start is called before the first frame update
    void Start()
    {

        //save scale
        orginalScale = transform.localScale;
        ratio = transform.localScale.y / 100;
        Resize(15);

        //Set Highlight Colors
        SetColor();
    }

    public void RelocateTo(Province prov) {

        Vector3 newPos = MapTools.LocalToScale(prov.center);
        newPos.z = transform.position.z;
        transform.position = newPos;
    }

    // Update is called once per frame
    void Update()
    {
        //refresh owners
        for(int i = 0; i<owners.Count; i++)
        {
            if (owners.ElementAt(i).rep == null)
            {
                owners.Remove(owners.ElementAt(i));
                i--;
            }
        }

        //fixed size
        if (Camera.main.orthographicSize < 15)
        {
            Resize(Camera.main.orthographicSize);
        }

        if (owners.Count>0)
        {
            //active?
            
            if(IsActive())
            {
                transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(1).gameObject.SetActive(false);
            }

            //refresh bar
            float morale = owners.ElementAt(0).ArmyMorale();
            if (morale >= 1)
            {
                SetBar(0);
            }
            else if (morale >= 0.8)
            {
                SetBar(1);
            }
            else if (morale >= 0.6)
            {
                SetBar(2);
            }
            else if (morale >= 0.4)
            {
                SetBar(3);
            }
            else if (morale >= 0.2)
            {
                SetBar(4);
            }
            else
            {
                SetBar(5);
            }

            //set shadow
            if (IsAlone())
            {
                transform.GetChild(4).gameObject.SetActive(false);
            }
            else
            {
                transform.GetChild(4).gameObject.SetActive(true);
            }

            //set manpower
            int mp = 0;
            foreach(Classes.Army o in owners)
            {
                mp += o.CurrentManpower();
            }
            transform.GetChild(0).GetChild(6).gameObject.GetComponent<TextMeshPro>().text = "" + mp;
        }
    }

    private void SetBar(int level)
    {
        transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
        transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
        transform.GetChild(0).GetChild(3).gameObject.SetActive(false);
        transform.GetChild(0).GetChild(4).gameObject.SetActive(false);
        transform.GetChild(0).GetChild(5).gameObject.SetActive(false);

        transform.GetChild(0).GetChild(level).gameObject.SetActive(true);
    }

    void Resize(float zoom)
    {
        transform.localScale = orginalScale * zoom*2 * ratio * 3;
    }

    public void ChangeOwner(List<Classes.Army> newOwners)
    {
        owners = newOwners;
        SetColor();
    }
    private void SetColor()
    {
        if (owners!=null)
        {
            transform.GetChild(2).GetComponent<MeshRenderer>().material.color = owners.ElementAt(0).owner.color;
        }
    }
    public void SetUnitsActive(bool active)
    {
        if (active)
        {
            foreach (Classes.Army o in owners)
            {
                MapTools.GetMap().activeArmies.Add(o);
            }
        }
        else
        {
            foreach (Classes.Army o in owners)
            {
                MapTools.GetMap().activeArmies.Remove(o);
            }
        }
        ActOrRefInterface();
    }
    public ArrowPlacer AddArrow(Vector2 start, Vector2 end)
    {
        ArrowPlacer arr = Instantiate(Resources.Load("Prefabs/ArrowPointer") as GameObject, GameObject.Find("Map/Arrows/Movement").transform).GetComponent<ArrowPlacer>();

        arr.Place(MapTools.LocalToScale(start), MapTools.LocalToScale(end));
        return arr;
    }
    public bool IsActive()
    {
        bool isActive = false;
        foreach (Classes.Army o in owners)
        {
            if (MapTools.GetMap().activeArmies.Contains(o))
            {
                isActive = true;
                break;
            }
        }
        return isActive;
    }
    public bool IsAlone()
    {
        return owners.Count < 2;
    }
    public void ActOrRefInterface()
    {
        if (!MapTools.GetInterface().GetActiveInterface().Equals("army"))
        {
            MapTools.GetInterface().EnableInterface("army");
        }
        else
        {
            MapTools.GetInterface().activeInterface.Set(armies: owners);

        }
    }
    void OnMouseDown()
    {
        //rect selection
        if (Input.GetMouseButton(0))
        {
            MapTools.GetInput().StartSelection(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        }
    }

    void OnMouseUp()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonUp(0))
            {
                //single selection
                foreach (Classes.Army o in owners)
                {
                    if (MapTools.GetSave().GetActiveNation() == o.owner)
                    {
                        if (MapTools.GetMap().activeArmies.Contains(o))
                        {
                                //GameObject.Find("Canvas").GetComponent<InterfaceHandler>().wheel_units.GetComponent<Unitwheel>().Pivot(owners.ElementAt(0));
                                //GameObject.Find("Canvas").GetComponent<InterfaceHandler>().wheel_units.GetComponent<Unitwheel>().Activate(true, 4);

                            SetUnitsActive(false);
                            return;
                        }
                        else if (MapTools.GetMap().activeArmies.Count > 0)
                        {
                            MapTools.GetMap().activeArmies.Clear();
                            SetUnitsActive(true);
                            ActOrRefInterface();
                            return;
                        }
                        else
                        {
                            SetUnitsActive(true);
                            ActOrRefInterface();
                            return;
                        }
                    }
                }
            }
        }
    }

    void OnMouseOver()
    {
        

        if (Input.GetMouseButtonUp(0))
        {
            //rect selection
            if (MapTools.GetInput().IsSelecting())
            {
                MapTools.GetInput().UpdateSelection(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
                MapTools.GetInput().FinalizeSelection();
            }
        }

        //rect selection
        if (Input.GetMouseButton(0))
        {
            MapTools.GetInput().UpdateSelection(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        }

        //Camera
        MapTools.GetInput().CameraInput();
    }

    void OnMouseEnter()
    {
        
    }

    void OnMouseExit()
    {
        
    }
}
