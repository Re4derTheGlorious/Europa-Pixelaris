using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
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
        gameObject.SetActive(false);

        GameObject.Find("UI_Fade").GetComponent<Fade>().FadeOut(0);
    }
    public override void Enable()
    {
        GameObject.Find("UI_Fade").GetComponent<Fade>().FadeIn(0.5f);


        Camera.main.GetComponent<CameraHandler>().enabled = false;

        transform.Find("Contents/Settings").gameObject.SetActive(false);
        transform.Find("Contents/Saves").gameObject.SetActive(false);
        transform.Find("Contents/Buttons").gameObject.SetActive(true);
        transform.Find("Contents/Details").gameObject.SetActive(false);
        transform.Find("Contents").gameObject.SetActive(true);
        gameObject.SetActive(true);

    }
    public override void Refresh()
    {
        
    }
    public override void KeyboardInput(Province prov)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Button_Return();
        }
    }
    public override void MouseInput(Province prov)
    {

    }

    public void SavePrefs()
    {
        //Video
        PlayerPrefs.SetFloat("UI_scale", float.Parse(transform.Find("Contents/Settings/Contents/Video/UI_Scale/Slider").gameObject.GetComponent<Slider>().value.ToString("F2")));

        //audio
        PlayerPrefs.SetFloat("vol_music", transform.Find("Contents/Settings/Contents/Audio/Volume_Music/Slider").gameObject.GetComponent<Slider>().value);
        PlayerPrefs.SetFloat("vol_audio", transform.Find("Contents/Settings/Contents/Audio/Volume_Audio/Slider").gameObject.GetComponent<Slider>().value);
        PlayerPrefs.SetFloat("vol_master", transform.Find("Contents/Settings/Contents/Audio/Volume_Master/Slider").gameObject.GetComponent<Slider>().value);

        //gameplay
        PlayerPrefs.SetInt("autosave_frequency", (int)transform.Find("Contents/Settings/Contents/Gameplay/Autosave_Freq/Slider").gameObject.GetComponent<Slider>().value);
    }
    public void LoadPrefs()
    {
        
        //Video
        transform.Find("Contents/Settings/Contents/Video/UI_Scale/Slider").gameObject.GetComponent<Slider>().value = PlayerPrefs.GetFloat("UI_scale");
        Slider_Scale(transform.Find("Contents/Settings/Contents/Video/UI_Scale/Slider").gameObject.GetComponent<Slider>());


        //audio
        transform.Find("Contents/Settings/Contents/Audio/Volume_Music/Slider").gameObject.GetComponent<Slider>().value = PlayerPrefs.GetFloat("vol_music");
        transform.Find("Contents/Settings/Contents/Audio/Volume_Audio/Slider").gameObject.GetComponent<Slider>().value = PlayerPrefs.GetFloat("vol_audio");
        transform.Find("Contents/Settings/Contents/Audio/Volume_Master/Slider").gameObject.GetComponent<Slider>().value = PlayerPrefs.GetFloat("vol_master");

        //gameplay
        transform.Find("Contents/Settings/Contents/Gameplay/Autosave_Freq/Slider").gameObject.GetComponent<Slider>().value = PlayerPrefs.GetInt("autosave_frequency");
        Slider_Freq(transform.Find("Contents/Settings/Contents/Gameplay/Autosave_Freq/Slider").gameObject.GetComponent<Slider>());
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
            PlayerPrefs.SetString("TransitionMode", "Scene_Map");
            GameObject.Find("UI_Fade").GetComponent<Fade>().FadeIn(1);

            transform.Find("Contents").gameObject.SetActive(false);
            InvokeRepeating("WaitThenLoad", 0, 0.1f);
        }
        else
        {
            GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cannot Load");
        }
    }
    private void WaitThenLoad()
    {
        if (GameObject.Find("UI_Fade").GetComponent<RawImage>().color.a >= 1)
        {
            SceneManager.LoadSceneAsync("Scene_Load", LoadSceneMode.Single);
        }
    }
    private void WaitThenQuit()
    {
        if (GameObject.Find("UI_Fade").GetComponent<RawImage>().color.a >= 1)
        {
            Application.Quit();
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
    public void ShowDetails()
    {
        if (saveSelected != null)
        {
            saveSelected.ShowDetails(transform.Find("Contents/Details"));
            transform.Find("Contents/Details").gameObject.SetActive(true);
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
    public void Slider_Scale(Slider slider)
    {
        slider.transform.parent.Find("Text").GetComponent<TextMeshProUGUI>().text = "UI Scale: " + slider.value.ToString("F2");
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

        transform.Find("Contents").gameObject.SetActive(false);
        GameObject.Find("UI_Fade").GetComponent<Fade>().FadeIn(1);
        InvokeRepeating("WaitThenQuit", 0, 0.1f);
    }
    public void Button_Menu()
    {
        SaveGame();
        GameObject.Find("UI_Fade").GetComponent<Fade>().FadeIn(1);
        transform.Find("Contents").gameObject.SetActive(false);
        PlayerPrefs.SetString("TransitionMode", "Scene_Menu");
        InvokeRepeating("WaitThenLoad", 0, 0.1f);
    }
    public void Button_Settings()
    {
        LoadPrefs();
        transform.Find("Contents/Buttons").gameObject.SetActive(false);
        transform.Find("Contents/Settings").gameObject.SetActive(true);
        transform.Find("Contents/Saves").gameObject.SetActive(false);

        Button_Audio();
    }

    public void Button_Return()
    {
        if (IsSet())
        {
            transform.Find("Contents/Buttons").gameObject.SetActive(true);
            transform.Find("Contents/Settings").gameObject.SetActive(false);
            transform.Find("Contents/Saves").gameObject.SetActive(false);
            transform.Find("Contents/Details").gameObject.SetActive(false);

            saveSelected = null;
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
            MapTools.GetInterface().ApplyScale();
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
