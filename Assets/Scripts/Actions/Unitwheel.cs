using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Unitwheel : MonoBehaviour
{
    private Vector3 orginalScale;
    private float ratio;
    private GameObject layer_army_select;
    private GameObject layer_army_inf;
    private GameObject layer_army_cav;
    private GameObject layer_army_art;

    private GameObject layer_build_select;
    private GameObject layer_build_fort;

    private GameObject activeLayer;

    public Province pivotProv;
    public Classes.Army pivotArmy;

    public int mapLayer;

    // Start is called before the first frame update
    void Start()
    {
        layer_army_select = this.gameObject.transform.GetChild(0).GetChild(0).transform.gameObject;
        layer_army_inf = this.gameObject.transform.GetChild(0).GetChild(1).transform.gameObject;
        layer_army_cav = this.gameObject.transform.GetChild(0).GetChild(2).transform.gameObject;
        layer_army_art = this.gameObject.transform.GetChild(0).GetChild(3).transform.gameObject;

        layer_build_select = this.gameObject.transform.GetChild(1).GetChild(0).transform.gameObject;
        layer_build_fort = this.gameObject.transform.GetChild(1).GetChild(1).transform.gameObject;

        activeLayer = layer_army_select;
        //save scale
        orginalScale = transform.localScale;
        ratio = transform.localScale.y / (Camera.main.orthographicSize * 2);
        Resize(15);


    }

    void Resize(float zoom)
    {
        transform.localScale = orginalScale * zoom * 2 * ratio * 3;
    }

    public void Pivot(Classes.Army army)
    {
        pivotArmy = army;
        pivotProv = null;
    }
    public void Pivot(Province prov)
    {
        pivotProv = prov;
        pivotArmy = null;
    }

    public void Activate(bool active, int mode)
    {
        if (active)
        {
            if (mode == 4)
            {
                activeLayer = layer_army_select;
            }
            else if(mode == 2)
            {
                activeLayer = layer_build_select;
            }
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    public void OnMouseOver()
    {
        Vector3 vec = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        vec = vec - transform.position;
        if (Mathf.Abs(vec.x) < 0.05 * 75 && Mathf.Abs(vec.y) < 0.05 * 75)
        {
            GameObject piece;
            Vector3 shift = new Vector3(transform.localScale.x / 5, transform.localScale.y / 5, 0);
            //top right
            if (vec.x > 0 && vec.y > 0)
            {
                piece = activeLayer.transform.GetChild(0).gameObject;
                Vector3 shifted = transform.position + new Vector3(shift.x, shift.y, 0);
                piece.transform.position = shifted;

                //action
                if (Input.GetMouseButtonDown(0))
                {
                    //army
                    if (activeLayer == layer_army_select)
                    {
                        activeLayer = layer_army_inf;
                    }
                    else if (activeLayer == layer_army_inf)
                    {
                        activeLayer = layer_army_select;
                    }
                    else if (activeLayer == layer_army_cav)
                    {
                        Recruit("cav_shock");
                    }
                    else if (activeLayer == layer_army_art)
                    {
                        Recruit("art_heavy");
                    }


                    //constructions
                    else if (activeLayer == layer_build_select)
                    {
                        activeLayer = layer_build_fort;
                    }
                    else if (activeLayer == layer_build_fort)
                    {
                        activeLayer = layer_build_select;
                    }
                }
            }
            else
            {
                activeLayer.transform.GetChild(0).transform.gameObject.transform.position = transform.position;
            }
            //bottom right
            if (vec.x > 0 && vec.y < 0)
            {
                piece = activeLayer.transform.GetChild(1).gameObject;
                Vector3 shifted = transform.position + new Vector3(shift.x, -shift.y, 0);
                piece.transform.position = shifted;

                //action
                if (Input.GetMouseButtonDown(0))
                {
                    if (activeLayer == layer_army_select)
                    {
                        activeLayer = layer_army_cav;
                    }
                    else if (activeLayer == layer_army_inf)
                    {
                        Recruit("inf_skirmish");
                    }
                    else if (activeLayer == layer_army_cav)
                    {
                        activeLayer = layer_army_select;
                    }
                    else if (activeLayer == layer_army_art)
                    {
                        Recruit("art_siege");
                    }


                    //constructions
                    else if (activeLayer == layer_build_fort)
                    {
                        Construct("const_fort");
                    }
                }
            }
            else
            {
                activeLayer.transform.GetChild(1).gameObject.transform.position = transform.position;
            }
            //bottom left
            if (vec.x < 0 && vec.y < 0)
            {
                piece = activeLayer.transform.GetChild(2).gameObject;
                Vector3 shifted = transform.position + new Vector3(-shift.x, -shift.y, 0);
                piece.transform.position = shifted;

                //action
                if (Input.GetMouseButtonDown(0))
                {
                    if (activeLayer == layer_army_select)
                    {
                        activeLayer = layer_army_art;
                    }
                    else if (activeLayer == layer_army_inf)
                    {
                        Recruit("inf_light");
                    }
                    else if (activeLayer == layer_army_cav)
                    {
                        Recruit("cav_missile");
                    }
                    else if (activeLayer == layer_army_art)
                    {
                        activeLayer = layer_army_select;
                    }

                    //constructions
                    else if (activeLayer == layer_build_fort)
                    {
                        Construct("const_castle");
                    }
                }
            }
            else
            {
                activeLayer.transform.GetChild(2).gameObject.transform.position = transform.position;
            }

            //top left
            if (vec.x < 0 && vec.y > 0)
            {
                piece = activeLayer.transform.GetChild(3).gameObject;
                Vector3 shifted = transform.position + new Vector3(-shift.x, shift.y, 0);
                piece.transform.position = shifted;

                //action
                if (Input.GetMouseButtonDown(0))
                {
                    if (activeLayer == layer_army_select)
                    {
                        Activate(false, 0);
                    }
                    else if (activeLayer == layer_army_inf)
                    {
                        Recruit("inf_heavy");
                    }
                    else if (activeLayer == layer_army_cav)
                    {
                        Recruit("cav_light");
                    }
                    else if (activeLayer == layer_army_art)
                    {
                        Recruit("art_field");
                    }
                    else if (activeLayer == layer_build_select)
                    {
                        Activate(false, 0);
                    }

                    //constructions
                    else if (activeLayer == layer_build_fort)
                    {
                        //Do nothing
                    }
                }
            }
            else
            {
                activeLayer.transform.GetChild(3).gameObject.transform.position = transform.position;
            }
        }
    }

    private void Recruit(string type)
    {
        if (pivotArmy != null)
        {
            pivotArmy.recruiter.Recruit(type);
            GameObject.Find("Canvas").GetComponent<InterfaceHandler>().RefreshInterface();
        }
        else if (pivotProv != null)
        {
            pivotProv.recruiter.Recruit(type);
            GameObject.Find("Canvas").GetComponent<InterfaceHandler>().RefreshInterface();
        }
    }
    private void Construct(string type)
    {
        if (pivotProv != null)
        {
            pivotProv.constructor.Construct(type);
            GameObject.Find("Canvas").GetComponent<InterfaceHandler>().RefreshInterface();
        }
    }
    void OnMouseExit()
    {
        activeLayer.transform.GetChild(0).transform.gameObject.transform.position = transform.position;
        activeLayer.transform.GetChild(1).transform.gameObject.transform.position = transform.position;
        activeLayer.transform.GetChild(2).transform.gameObject.transform.position = transform.position;
        activeLayer.transform.GetChild(3).transform.gameObject.transform.position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        layer_army_select.SetActive(false);
        layer_army_inf.SetActive(false);
        layer_army_cav.SetActive(false);
        layer_army_art.SetActive(false);

        layer_build_select.SetActive(false);
        layer_build_fort.SetActive(false);

        activeLayer.SetActive(true);

        if (Camera.main.orthographicSize < 15)
        {
            Resize(Camera.main.orthographicSize);
        }

        if (pivotArmy != null)
        {
            Vector3 pos = pivotArmy.rep.transform.position;
            pos.z = mapLayer;
            transform.position = pos;
        }
        else if (pivotProv != null)
        {
            Vector3 pos = MapTools.LocalToScale(pivotProv.center);
            pos.z = mapLayer;
            transform.position = pos;
        }
    }
}
