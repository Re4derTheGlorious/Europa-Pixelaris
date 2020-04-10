using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

public class MainMenu : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        if (Directory.EnumerateFiles(new SaveManager().location_path).Where(x => new SaveManager().GetSaveFile(x).IsValid()).Count() == 0){
            transform.Find("Continue_Button").GetComponent<Button>().interactable = false;
        }
        else
        {
            transform.Find("Continue_Button").GetComponent<Button>().interactable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Button_Exit()
    {
        Application.Quit();
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
        transform.Find("Fade").gameObject.SetActive(true);
        InvokeRepeating("FadeInAndStart", 0f, 0.01f);
    }
    private void FadeInAndStart()
    {
        Color newColor = transform.Find("Fade").GetComponent<RawImage>().color;
        newColor.a += (float)(0.01);
        transform.Find("Fade").GetComponent<RawImage>().color = newColor;
        if (newColor.a >= 0.95)
        {
            CancelInvoke();
            SceneManager.LoadScene("Scene_Map", LoadSceneMode.Single);
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
}
