using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using TMPro;

public class MainMenu : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetFloat("UI_scale", -123) == -123)
        {
            PlayerPrefs.SetFloat("UI_scale", 1f);
        }
        if (PlayerPrefs.GetInt("autosave_frequency", -123) == -123)
        {
            PlayerPrefs.SetInt("autosave_frequency", 2);
        }
        if (PlayerPrefs.GetFloat("vol_master", -123) == -123)
        {
            PlayerPrefs.SetFloat("vol_master", 1);
        }
        if (PlayerPrefs.GetFloat("vol_music", -123) == -123)
        {
            PlayerPrefs.SetFloat("vol_music", 1);
        }
        if (PlayerPrefs.GetFloat("vol_audio", -123) == -123)
        {
            PlayerPrefs.SetFloat("vol_audio", 1);
        }

        GameObject.Find("UI_Fade").GetComponent<Fade>().FadeOut(0);
        if (Directory.EnumerateFiles(new SaveManager().location_path).Where(x => new SaveManager().GetSaveFile(x).IsValid()).Count() == 0){
            transform.Find("Continue_Button").GetComponent<Button>().interactable = false;
        }
        else
        {
            transform.Find("Continue_Button").GetComponent<Button>().interactable = true;
        }

        GameObject.Find("Version").GetComponent<TextMeshProUGUI>().text = Application.version;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Button_Exit()
    {
        GameObject.Find("UI_Fade").GetComponent<Fade>().FadeIn(1);
        InvokeRepeating("WaitThenQuit", 0f, 0.1f);
    }

    public void Button_Start()
    {
        StartGame();
    }

    public void Button_Continue()
    {
        if (GetLatestSave() != null)
        {
            PlayerPrefs.SetString("SaveName", GetLatestSave());
            StartGame();
        }
    }

    public string GetLatestSave()
    {
        return Directory.EnumerateFiles(new SaveManager().location_path).OrderByDescending(x => Directory.GetCreationTime(x)).Where(x=>new SaveManager().GetSaveFile(x).IsValid()).ElementAt(0);
    }
    public void StartGame()
    {
        transform.Find("UI_Fade").gameObject.SetActive(true);
        PlayerPrefs.SetString("TransitionMode", "Scene_Map");
        GameObject.Find("UI_Fade").GetComponent<Fade>().FadeIn(1);
        InvokeRepeating("WaitThenStart", 0f, 0.1f);
    }
    private void WaitThenStart()
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
}
