using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuInterface : Interface
{
    public SaveFrame saveSelected;

    public override bool IsSet()
    {
        return !transform.Find("Contents/Buttons").gameObject.activeSelf;
    }
    public override void Set(Classes.Nation nat = null, Province prov = null, Classes.Army arm = null, Classes.TradeRoute route = null, List<Classes.Army> armies = null, List<Classes.Unit> units = null, Battle battle = null)
    {

    }
    public override void Disable()
    {
        Camera.main.GetComponent<CameraHandler>().enabled = true;
        CancelInvoke();
        transform.Find("Contents").gameObject.SetActive(false);
        InvokeRepeating("FadeOut", 0f, 0.01f);
    }
    public override void Enable()
    {
        Camera.main.GetComponent<CameraHandler>().enabled = false;

        transform.Find("Contents/Settings").gameObject.SetActive(false);
        transform.Find("Contents/Saves").gameObject.SetActive(false);
        transform.Find("Contents/Buttons").gameObject.SetActive(true);

        CancelInvoke();
        transform.Find("Contents").gameObject.SetActive(true);
        gameObject.SetActive(true);
        InvokeRepeating("FadeIn", 0f, 0.01f);
    }
    public override void Refresh()
    {
        
    }


    public void SavePrefs()
    {
        //audio
        PlayerPrefs.SetFloat("vol_music", transform.Find("Contents/Settings/Contents/Audio/Volume_Music/Slider").gameObject.GetComponent<Slider>().value);
        PlayerPrefs.SetFloat("vol_audio", transform.Find("Contents/Settings/Contents/Audio/Volume_Audio/Slider").gameObject.GetComponent<Slider>().value);
        PlayerPrefs.SetFloat("vol_master", transform.Find("Contents/Settings/Contents/Audio/Volume_Master/Slider").gameObject.GetComponent<Slider>().value);

        //gameplay
        PlayerPrefs.SetInt("autosave_frequency", (int)transform.Find("Contents/Settings/Contents/Gameplay/Autosave_Freq/Slider").gameObject.GetComponent<Slider>().value);
    }
    public void LoadPrefs()
    {
        //audio
        transform.Find("Contents/Settings/Contents/Audio/Volume_Music/Slider").gameObject.GetComponent<Slider>().value = PlayerPrefs.GetFloat("vol_music");
        transform.Find("Contents/Settings/Contents/Audio/Volume_Audio/Slider").gameObject.GetComponent<Slider>().value = PlayerPrefs.GetFloat("vol_audio");
        transform.Find("Contents/Settings/Contents/Audio/Volume_Master/Slider").gameObject.GetComponent<Slider>().value = PlayerPrefs.GetFloat("vol_master");

        //gameplay
        transform.Find("Contents/Settings/Contents/Gameplay/Autosave_Freq/Slider").gameObject.GetComponent<Slider>().value = PlayerPrefs.GetInt("autosave_frequency");
    }

    public void SaveGame()
    {
        SaveManager sm = new SaveManager();
        if (!sm.SaveGame())
        {
            GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cannot save");
        }
    }
    public void LoadGame()
    {
        SaveFile saveFile = new SaveManager().GetSaveFile(saveSelected.path);
        if (saveFile!=null && saveFile.IsValid())
        {
            PlayerPrefs.SetString("LoadSave", saveSelected.path);
            SceneManager.LoadSceneAsync("Scene_Map", LoadSceneMode.Single);
        }
        else
        {
            GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cannot Load");
        }
    }


    public void LoadSaves()
    {
        foreach(Transform trans in transform.Find("Contents/Saves/Contents/Viewport/Content"))
        {
            Destroy(trans.gameObject);
        }
        transform.Find("Contents/Saves/Contents/Viewport/Content").DetachChildren();

        foreach (string file in Directory.EnumerateFiles(new SaveManager().location_path).OrderByDescending(x => Directory.GetCreationTime(x)))
        {
            if (file.EndsWith(new SaveManager().save_extension))
            {
                GameObject newFrame = Instantiate(Resources.Load("UI/SaveFrame/SaveFrame") as GameObject, transform.Find("Contents/Saves/Contents/Viewport/Content"));
                newFrame.GetComponent<SaveFrame>().Set(new SaveManager().GetSaveFile(file), Directory.GetCreationTime(file), Path.GetFileName(file), this, file);
            }
        }
    }

    private void FadeIn()
    {
        Color newColor = transform.Find("Fade").GetComponent<RawImage>().color;
        newColor.a += (float)(0.01);
        transform.Find("Fade").GetComponent<RawImage>().color = newColor;
        if (newColor.a > 0.5)
        {
            CancelInvoke();
        }
    }
    private void FadeOut()
    {
        Color newColor = transform.Find("Fade").GetComponent<RawImage>().color;
        newColor.a -= (float)(0.01);
        transform.Find("Fade").GetComponent<RawImage>().color = newColor;
        if (newColor.a <= 0)
        {
            CancelInvoke();
            gameObject.SetActive(false);
        }
    }

    public void Slider_Freq(Slider slider)
    {
        slider.transform.parent.Find("Text").GetComponent<TextMeshProUGUI>().text = "Autosaves: ";
        if (slider.value==0)
        {
            slider.transform.parent.Find("Text").GetComponent<TextMeshProUGUI>().text += "Never";
        }
        else if (slider.value == 1)
        {
            slider.transform.parent.Find("Text").GetComponent<TextMeshProUGUI>().text += "Daily";
        }
        else if (slider.value == 2)
        {
            slider.transform.parent.Find("Text").GetComponent<TextMeshProUGUI>().text += "Monthly";
        }
        else if (slider.value == 3)
        {
            slider.transform.parent.Find("Text").GetComponent<TextMeshProUGUI>().text += "Yearly";
        }
    }

    public void Button_Save()
    {
        SaveGame();
    }
    public void Button_Load()
    {
        transform.Find("Contents/Saves/Buttons/Load").GetComponent<Button>().interactable = false;

        transform.Find("Contents/Buttons").gameObject.SetActive(false);
        transform.Find("Contents/Settings").gameObject.SetActive(false);
        transform.Find("Contents/Saves").gameObject.SetActive(true);

        LoadSaves();
    }
    public void Button_Exit()
    {
        SaveGame();
        Application.Quit();
    }
    public void Button_Menu()
    {
        SaveGame();
        SceneManager.LoadScene("Scene_Menu", LoadSceneMode.Single);
    }
    public void Button_Settings()
    {
        LoadPrefs();
        transform.Find("Contents/Buttons").gameObject.SetActive(false);
        transform.Find("Contents/Settings").gameObject.SetActive(true);
        transform.Find("Contents/Saves").gameObject.SetActive(false);

        Button_Gameplay();
    }

    public void Button_Return()
    {
        if (IsSet())
        {
            transform.Find("Contents/Buttons").gameObject.SetActive(true);
            transform.Find("Contents/Settings").gameObject.SetActive(false);
            transform.Find("Contents/Saves").gameObject.SetActive(false);
        }
        else
        {
            GameObject.Find("Canvas").GetComponent<InterfaceHandler>().EnableInterface("none");
        }
    }
    public void Button_Apply()
    {
        if (transform.Find("Contents/Settings").gameObject.activeSelf)
        { 
            SavePrefs();
            Button_Return();
        }
        else if(transform.Find("Contents/Saves").gameObject.activeSelf)
        {
            if (saveSelected != null)
            {
                LoadGame();
            }
        }
    }

    public void Button_Audio()
    {
        transform.Find("Contents/Settings/Contents/Audio").gameObject.SetActive(true);
        transform.Find("Contents/Settings/Contents/Video").gameObject.SetActive(false);
        transform.Find("Contents/Settings/Contents/Gameplay").gameObject.SetActive(false);
        transform.Find("Contents/Settings/Contents/Controls").gameObject.SetActive(false);

    }
    public void Button_Video()
    {
        transform.Find("Contents/Settings/Contents/Audio").gameObject.SetActive(false);
        transform.Find("Contents/Settings/Contents/Video").gameObject.SetActive(true);
        transform.Find("Contents/Settings/Contents/Gameplay").gameObject.SetActive(false);
        transform.Find("Contents/Settings/Contents/Controls").gameObject.SetActive(false);
    }
    public void Button_Gameplay()
    {
        transform.Find("Contents/Settings/Contents/Audio").gameObject.SetActive(false);
        transform.Find("Contents/Settings/Contents/Video").gameObject.SetActive(false);
        transform.Find("Contents/Settings/Contents/Gameplay").gameObject.SetActive(true);
        transform.Find("Contents/Settings/Contents/Controls").gameObject.SetActive(false);
    }
    public void Button_Controls()
    {
        transform.Find("Contents/Settings/Contents/Audio").gameObject.SetActive(false);
        transform.Find("Contents/Settings/Contents/Video").gameObject.SetActive(false);
        transform.Find("Contents/Settings/Contents/Gameplay").gameObject.SetActive(false);
        transform.Find("Contents/Settings/Contents/Controls").gameObject.SetActive(true);
    }
}
