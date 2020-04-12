using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionHandler : MonoBehaviour
{
    public int delay = 1;
    private bool started = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeSinceLevelLoad > 1f && !started)
        {
            started = true;
            SceneManager.LoadSceneAsync(PlayerPrefs.GetString("TransitionMode"), LoadSceneMode.Additive).completed += (a) =>
            {
                
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(PlayerPrefs.GetString("TransitionMode")));
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("Scene_Load"));
            };
        }
    }
}
