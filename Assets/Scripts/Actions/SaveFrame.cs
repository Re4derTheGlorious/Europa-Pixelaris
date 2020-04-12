using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveFrame : MonoBehaviour
{
    public MenuInterface menuInterface;
    public string path;
    private SaveFile save;
    DateTime modDate;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Set(SaveFile save, DateTime modDate, string name, MenuInterface inter, string path)
    {
        menuInterface = inter;
        this.path = path;
        this.save = save;
        this.modDate = modDate;

        transform.Find("Text_Date").gameObject.GetComponent<TextMeshProUGUI>().text = modDate.ToString();
        transform.Find("Text_Name").gameObject.GetComponent<TextMeshProUGUI>().text = name;

        transform.Find("Symbol").gameObject.GetComponent<RawImage>().texture = Resources.Load("Symbols/Symb_" + save.activeNation_as) as Texture2D;

        if (!save.IsValid())
        {
            GetComponent<Button>().interactable = false;
        }
    }
    public void ShowDetails(Transform frame)
    {
        frame.Find("Nation").GetComponent<TextMeshProUGUI>().text = MapTools.IdToNat(save.activeNation_as).name;
        frame.Find("Date").GetComponent<TextMeshProUGUI>().text = save.GetTime().day+" "+save.GetTime().GetMonthName()+" "+(save.GetTime().year+save.GetTime().startYear);


        frame.Find("Created").GetComponent<TextMeshProUGUI>().text = "Created on " + modDate.Date.Day + "." + modDate.Date.Month + "." + modDate.Date.Year;
        frame.Find("Version").GetComponent<TextMeshProUGUI>().text = save.GetVersion();

    }

    public bool IsCompatible()
    {
        if (!save.GetChecksum().Equals(new SaveManager().CalculateChecksum()))
        {
            return false;
        }
        else if (save.GetVersion() != Application.version)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void Button_Action()
    {
        if (menuInterface.saveSelected != null)
        {
            menuInterface.saveSelected.GetComponent<Image>().color = Color.white;
        }
        GetComponent<Image>().color = Color.gray;
        menuInterface.saveSelected = this;

        transform.parent.parent.parent.parent.Find("Buttons/Load").GetComponent<Button>().interactable = true;

        menuInterface.ShowDetails();
    }
}
