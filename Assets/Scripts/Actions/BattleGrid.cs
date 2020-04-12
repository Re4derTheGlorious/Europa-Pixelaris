using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleGrid : MonoBehaviour
{
    public Texture2D textImpass;
    public Texture2D textUnavailable;
    public Texture2D textEmpty;
    public Texture2D textInf;
    public Texture2D textCav;
    public Texture2D textArt;
    public Texture2D textMiss;

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
        GetComponent<RawImage>().texture = textImpass;
        GetComponent<RawImage>().color = Color.white;
        GetComponent<StaticHint>().hintText = "This part of the battlefield is inpassable";
    }
    public void SetUnit(Classes.Unit unit)
    {
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
        Color newColor = Color.clear;
        if (unit.morale<=0.5f)
        {
            newColor = Color.yellow;
        }
        else if(unit.routing)
        {
            newColor = Color.white;
        }
        else if (unit.attacking)
        {
            newColor = Color.red;
        }
        else
        {
            newColor = Color.blue;
        }

        float a = ((float)unit.manpower / unit.MaxManpower()) * 0.75f + 0.25f;
        newColor.a = a;
        GetComponent<RawImage>().color = newColor;

        //hint
        string hintText = unit.unitName + " Contested by " + unit.manpower + " "+unit.owner.owner.name+" "+unit.type+"\n";
        hintText += unit.morale + " morale\n\n";
        hintText += "Last tick:\n";
        if (unit.lastTarget != null)
        {
            hintText += "Attacked " + unit.lastTarget.unitName + " for " + unit.dmgDealt;
        }
        GetComponent<StaticHint>().hintText = hintText;
    }
    public void SetEmpty()
    {
        GetComponent<RawImage>().texture = textEmpty;
        GetComponent<RawImage>().color = Color.white;
        GetComponent<StaticHint>().hintText = "This part of the battlefield is currently uncontested";
    }
    public void SetUnavailable()
    {
        GetComponent<RawImage>().texture = textUnavailable;
        GetComponent<RawImage>().color = Color.gray;
        GetComponent<StaticHint>().hintText = "This part of the battlefield is inaccesibile due to exceded engagement width";
    }
}
