using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BattleGrid : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Texture2D textImpass;
    public Texture2D textUnavailable;
    public Texture2D textEmpty;
    public Texture2D textInf;
    public Texture2D textCav;
    public Texture2D textArt;
    public Texture2D textMiss;

    private Unit unit;

    private GameObject floater;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetImpass()
    {
        unit = null;
        GetComponent<RawImage>().texture = textImpass;
        GetComponent<RawImage>().color = Color.white;
        GetComponent<StaticHint>().hintText = "This part of the battlefield is inpassable";
    }
    public void SetUnit(Unit unit)
    {
        this.unit = unit;
        this.unit.rep = this;

        //texture
        if (unit.type.Equals("inf_skirmish"))
        {
            GetComponent<RawImage>().texture = textMiss;
        }
        else if (unit.type.Equals("inf_light") || unit.type.Equals("inf_heavy"))
        {
            GetComponent<RawImage>().texture = textInf;
        }
        else if (unit.type.Equals("cav_light") || unit.type.Equals("cav_shock") || unit.type.Equals("cav_missile"))
        {
            GetComponent<RawImage>().texture = textCav;
        }
        else if (unit.type.Equals("art_field") || unit.type.Equals("art_heavy") || unit.type.Equals("art_siege"))
        {
            GetComponent<RawImage>().texture = textArt;
        }

        //color
        Color newColor = unit.owner.owner.color;
        if (unit.routing)
        {
            newColor = Color.white;
        }
        else if (unit.morale<=0.5f)
        {
            newColor = Color.yellow;
        }


        float a = ((float)unit.manpower / unit.MaxManpower()) * 0.75f + 0.25f;
        newColor.a = a;
        GetComponent<RawImage>().color = newColor;

        //hint
        SetHint();

        if (unit.launchFloater)
        {
            LaunchFloater();
            unit.launchFloater = false;
        }
    }
    public void SetEmpty()
    {
        unit = null;
        GetComponent<RawImage>().texture = textEmpty;
        GetComponent<RawImage>().color = Color.white;
        GetComponent<StaticHint>().hintText = "This part of the battlefield is currently uncontested";
    }
    public void SetUnavailable()
    {
        unit = null;
        GetComponent<RawImage>().texture = textUnavailable;
        GetComponent<RawImage>().color = Color.gray;
        GetComponent<StaticHint>().hintText = "This part of the battlefield is inaccesibile due to exceded engagement width";
    }
    private void SetHint()
    {
        if (unit.strengthBook != null)
        {
            string hintText = " Contested by " + unit.manpower + " " + unit.owner.owner.name + " " + unit.type + "\n\n";

            hintText += "Combat Strength: " + unit.strengthBook.TotalValue() + "\n";
            hintText += unit.strengthBook.GetFinanses();

            hintText += "\n\nMorale: " + unit.morale + "\n";
            hintText += "Losing morale due to:\n";
            hintText += unit.moraleLoss.GetFinanses();

            GetComponent<StaticHint>().hintText = hintText;
            GetComponent<StaticHint>().SetDelay(1f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (unit != null)
        {
            //position
            if (unit.prevPosition != null)
            {
                Vector2 pos = new Vector2(unit.rep.GetComponent<RectTransform>().position.x, unit.rep.GetComponent<RectTransform>().position.y) - new Vector2(unit.position.x - unit.prevPosition.x, unit.prevPosition.y - unit.position.y) * unit.rep.GetComponent<RectTransform>().rect.width * PlayerPrefs.GetFloat("UI_scale");
                PlaceArrow(pos, unit.rep.GetComponent<RectTransform>().position, Color.blue);
            }

            //targeting
            
            foreach (Unit u in unit.targetedBy)
            {
                PlaceArrow(GetComponent<RectTransform>().position, u.rep.GetComponent<RectTransform>().position, Color.red);
            }

            //targeted by
            if (unit.targeting != null)
            {
                if (unit.targetedBy.Contains(unit.targeting))
                {
                    PlaceArrow(GetComponent<RectTransform>().position, unit.targeting.rep.GetComponent<RectTransform>().position, Color.yellow, true);
                }
                else
                {
                    PlaceArrow(GetComponent<RectTransform>().position, unit.targeting.rep.GetComponent<RectTransform>().position, Color.green);
                }
            }
        }
    }

    private void PlaceArrow(Vector2 start, Vector2 end, Color color, bool twoSided = false)
    {
        GameObject arr = Instantiate(Resources.Load("Prefabs/UI_Line") as GameObject, GameObject.Find("UI_Lines").transform);
        arr.GetComponent<UnityEngine.UI.Extensions.UILineRenderer>().Points[0] = Camera.main.ScreenToViewportPoint(start);
        arr.GetComponent<UnityEngine.UI.Extensions.UILineRenderer>().Points[1] = Camera.main.ScreenToViewportPoint(end);
        arr.GetComponent<UnityEngine.UI.Extensions.UILineRenderer>().color = color; ;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach(Transform arr in GameObject.Find("UI_Lines").transform)
        {
            Destroy(arr.gameObject);
        }
    }

    public void LaunchFloater()
    {
        floater = Instantiate(Resources.Load("Prefabs/Floater") as GameObject, transform);

        if (unit.manpower == 0)
        {
            floater.GetComponent<RawImage>().texture = Resources.Load("Icons/Combat_Cas") as Texture2D;
        }

        InvokeRepeating("FloaterFade", 0, 0.005f);
    }

    public void FloaterFade()
    {
        Color newColor = floater.GetComponent<RawImage>().color;
        newColor = new Color(newColor.r, newColor.g, newColor.b, newColor.a - newColor.a / 100);
        floater.GetComponent<RawImage>().color = newColor;

        if (newColor.a < 0.01)
        {
            Destroy(floater);
            CancelInvoke();
        }
    }
}
