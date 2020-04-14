using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Linq;

public class Unitframe : MonoBehaviour, IPointerDownHandler
{
    private GameObject bar_0;
    private GameObject bar_1;
    private GameObject bar_2;
    private GameObject bar_3;
    private GameObject bar_4;
    private GameObject bar_5;
    public TextMeshProUGUI text;
    public RawImage icon;
    private GameObject fade;

    private Unit unit;
    public List<Army> armies = new List<Army>();

    public Classes.Recruiter recruiter;
    public bool goTop = false;
    public bool actAll = false;
    public bool goBottom = false;

    // Start is called before the first frame update
    void Awake()
    {
        bar_0 = this.gameObject.transform.GetChild(0).GetChild(0).gameObject;
        bar_1 = this.gameObject.transform.GetChild(0).GetChild(1).gameObject;
        bar_2 = this.gameObject.transform.GetChild(0).GetChild(2).gameObject;
        bar_3 = this.gameObject.transform.GetChild(0).GetChild(3).gameObject;
        bar_4 = this.gameObject.transform.GetChild(0).GetChild(4).gameObject;
        bar_5 = this.gameObject.transform.GetChild(0).GetChild(5).gameObject;
        icon = this.gameObject.transform.GetChild(1).GetComponent<RawImage>();
        text = this.gameObject.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        fade = this.gameObject.transform.GetChild(3).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (unit != null)
        {
            if (recruiter==null)
            {
                string newText = "";
                newText += unit.manpower;
                newText += " ";
                newText += unit.unitName;
                text.text = newText;
                icon.color = Color.white;
                SetBar(unit.morale);
            }
            else
            {
                string newText = "";
                newText += "Gathering: " + unit.unitName;
                text.text = newText;
                icon.color = Color.gray;

                if (recruiter.recruitementQueue.Count == 0)
                {
                    recruiter = null;
                    SetFade(0);
                }
                else if (recruiter.recruitementQueue.ElementAt(0)==unit)
                {
                    SetBar(recruiter.recruitementProgress);
                }
                else if (recruiter.recruitementQueue.Contains(unit))
                {
                    SetBar(0);
                }
                else
                {
                    recruiter = null;
                    SetFade(0);
                }
            }
        }
        else if(armies.Count == 1)
        {
            //icon
            SetArmyIcon();

            //text
            string newText = "";
            newText += armies.ElementAt(0).CurrentManpower() + "/" + armies.ElementAt(0).TotalManpower() +" in "+ armies.ElementAt(0).location.provName;
            text.text = newText;
            if (armies.ElementAt(0).IsActive())
            {
                SetFade(0f);
            }
            else
            {
                SetFade(0.2f);
            }

            //bar
            SetBar(armies.ElementAt(0).ArmyMorale());
        }
        else if(armies.Count>1)
        {
            string newText = "";
            int currentMan = 0;
            int totalMan = 0;
            float morale = 0;
            bool act = true;
            foreach(Army a in armies)
            {
                currentMan += a.CurrentManpower();
                totalMan += a.TotalManpower();
                morale += a.ArmyMorale() * a.CurrentManpower();
                if (!a.IsActive())
                {
                    act = false;
                }
            }
            if (act)
            {
                SetFade(0);
            }
            else
            {
                SetFade(0.2f);
            }
            newText += currentMan + "/" + totalMan;
            text.text = newText;

            //bar
            morale /= currentMan;
            SetBar(armies.ElementAt(0).ArmyMorale());
        }
    }

    private void HideBars()
    {
        bar_5.SetActive(false);
        bar_4.SetActive(false);
        bar_3.SetActive(false);
        bar_2.SetActive(false);
        bar_1.SetActive(false);
        bar_0.SetActive(false);
    }

    public void ClearOwners()
    {
        unit = null;
        armies.Clear();
        string newText = "";
        text.text = newText;
        icon.texture = Resources.Load("Icons/Default_Icon") as Texture2D;
        icon.color = Color.white;
        goTop = false;
        goBottom = false;
        actAll = false;
        HideBars();
    }

    public void SetArmy(List<Army> armies)
    {
        this.armies.Clear();
        goBottom = false;
        goTop = false;
        actAll = false;
        foreach (Army a in armies)
        {
            this.armies.Add(a);
            SetFade(0);
        }
        unit = null;
        Update();
    }

    public void SwitchActive()
    {
        if (armies.ElementAt(0).IsActive()){
            GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.Remove(armies.ElementAt(0));
        }
        else
        {
            GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.Add(armies.ElementAt(0));
        }

    }
    public void ActivateAll()
    {
        bool act = true;
        foreach (Army a in transform.parent.GetComponent<ArmyInterface>().armies)
        {
            if (!a.IsActive())
            {
                act = false;
                break;
            }
        }
        foreach (Army a in transform.parent.GetComponent<ArmyInterface>().armies)
        {
            a.SetActive(!act);
        }
    }

    public void SetBar(float percent)
    {
        if (percent >= 1)
        {
            HideBars();
            bar_5.SetActive(true);
        }
        else if (percent >= 0.8f)
        {
            HideBars();
            bar_4.SetActive(true);
        }
        else if (percent >= 0.6f)
        {
            HideBars();
            bar_3.SetActive(true);
        }
        else if (percent >= 0.4f)
        {
            HideBars();
            bar_2.SetActive(true);
        }
        else if (percent >= 0.2f)
        {
            HideBars();
            bar_1.SetActive(true);
        }
        else
        {
            HideBars();
            bar_0.SetActive(true);
        }
    }

    public void SetUnit(Unit unit, Classes.Recruiter rec = null)
    {
        goTop = false;
        goBottom = false;
        actAll = false;
        if (unit!=null && unit != this.unit)
        {
            recruiter = null;
            this.unit = unit;
            SetUnitIcon();
            if (rec != null)
            {
                this.recruiter = rec;
                SetFade(0.5f);
            }
            else
            {
                SetFade(0);
            }
        }
        armies.Clear();
        Update();
    }

    public void SetUnitIcon()
    {
        if (unit.type.Equals("inf_skirmish"))
        {
            icon.texture = Resources.Load("Icons/Inf_Skirmish") as Texture2D;
        }
        else if (unit.type.Equals("inf_light"))
        {
            icon.texture = Resources.Load("Icons/Inf_Light") as Texture2D;
        }
        else if (unit.type.Equals("inf_heavy"))
        {
            icon.texture = Resources.Load("Icons/Inf_Heavy") as Texture2D;
        }
        else if (unit.type.Equals("cav_missile"))
        {
            icon.texture = Resources.Load("Icons/Cav_Missile") as Texture2D;
        }
        else if (unit.type.Equals("cav_light"))
        {
            icon.texture = Resources.Load("Icons/Cav_Light") as Texture2D;
        }
        else if (unit.type.Equals("cav_shock"))
        {
            icon.texture = Resources.Load("Icons/Cav_Shock") as Texture2D;
        }
        else if (unit.type.Equals("art_field"))
        {
            icon.texture = Resources.Load("Icons/Art_Field") as Texture2D;
        }
        else if (unit.type.Equals("art_heavy"))
        {
            icon.texture = Resources.Load("Icons/Art_Heavy") as Texture2D;
        }
        else if (unit.type.Equals("art_siege"))
        {
            icon.texture = Resources.Load("Icons/Art_Siege") as Texture2D;
        }
    }

    public void SetArmyIcon()
    {
        if (armies.ElementAt(0).IsEngaged())
        {
            icon.texture = Resources.Load("Icons/Army_Engaged") as Texture2D;
        }
        else if (!armies.ElementAt(0).IsStationed())
        {
            if (armies.ElementAt(0).IsRouting())
            {
                icon.texture = Resources.Load("Icons/Army_Routing") as Texture2D;
            }
            else
            {
                icon.texture = Resources.Load("Icons/Army_Marching") as Texture2D;
            }
        }
        else
        {
            icon.texture = Resources.Load("Icons/Army_Stationed") as Texture2D;
        }
        
    }

    public void SetFade(float amount, Color col)
    {
        Color newColor = col;
        newColor.a = amount;
        fade.GetComponent<RawImage>().color = newColor;
    }
    public void SetFade(float amount)
    {
        SetFade(amount, Color.black);
    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        if (Input.GetMouseButton(0))
        {
            //scrolls
            if (goTop)
            {
                transform.parent.GetComponent<ArmyInterface>().GoToTop();
            }
            else if (goBottom)
            {
                transform.parent.GetComponent<ArmyInterface>().GoToBottom();
            }

            //In/Activate army
            if (actAll)
            {
                ActivateAll();
            }
            else if (armies.Count == 1)
            {
                if (transform.parent.GetComponent<ArmyInterface>() == null)
                {
                    SwitchActive();
                }
            }

            else if (unit != null)
            {
                //move between frames
                if (transform.parent.parent.GetComponent<ArmyInterface>().gameObject.activeSelf && transform.parent.parent.GetComponent<ArmyInterface>().anotherInterface.gameObject.activeSelf)
                {
                    if (recruiter == null)
                    {
                        if (transform.parent.parent.GetComponent<ArmyInterface>().armies.ElementAt(0).units.Count > 1)
                        {
                            transform.parent.parent.GetComponent<ArmyInterface>().armies.ElementAt(0).units.Remove(unit);
                            transform.parent.parent.GetComponent<ArmyInterface>().anotherInterface.armies.ElementAt(0).AddUnit(unit);

                            transform.parent.parent.GetComponent<ArmyInterface>().anotherInterface.Refresh();
                            transform.parent.parent.GetComponent<ArmyInterface>().Refresh();
                        }
                        else
                        {
                            GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cant move last unit, merge armies instead");
                        }
                    }
                    else
                    {
                        GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cant move unit that is being recruited");
                    }
                }
            }
        }
        else if (Input.GetMouseButton(1))
        {
            //remove from selection
            if (armies.Count == 1)
            {
                if (transform.parent.parent.GetComponent<ArmyInterface>() != null)
                {
                    armies.ElementAt(0).SetActive(false);
                    transform.parent.parent.GetComponent<ArmyInterface>().armies.Remove(armies.ElementAt(0));
                    transform.parent.parent.GetComponent<ArmyInterface>().Refresh();
                    Destroy(gameObject);
                    transform.SetParent(null);
                }
            }
        }
    }
}
