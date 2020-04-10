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

        transform.Find("Text_Date").gameObject.GetComponent<TextMeshProUGUI>().text = modDate.ToString();
        transform.Find("Text_Name").gameObject.GetComponent<TextMeshProUGUI>().text = name;

        transform.Find("Symbol").gameObject.GetComponent<RawImage>().texture = Resources.Load("Symbols/Symb_" + save.activeNation_as) as Texture2D;

        if (!save.IsValid())
        {
            GetComponent<Button>().interactable = false;
        }
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
    }
}
