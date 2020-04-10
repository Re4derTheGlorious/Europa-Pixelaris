using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;

public class ModIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string type;
    private float value;
    private string tip;

    private string path = "Assets/Resources/mods.csv";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void SetMod(string type, float value)
    {
        this.type = type;
        this.value = value;

        //read icon and tooltip
        StreamReader reader = new StreamReader(path);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            if(GameObject.Find("Map/Center").GetComponent<MapHandler>().GetFields(line, ";")[0].Equals(type))
            {
                gameObject.GetComponent<RawImage>().texture = Resources.Load("Icons/"+GameObject.Find("Map/Center").GetComponent<MapHandler>().GetFields(line, ";")[2]) as Texture2D;
                tip = GameObject.Find("Map/Center").GetComponent<MapHandler>().GetFields(line, ";")[1];
                reader.Close();
                return;
            }
            else
            {
                tip = "Fluffy fluff";
            }
        }
        gameObject.GetComponent<RawImage>().texture = Resources.Load("Icons/Default_Icon") as Texture2D;
        reader.Close();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject.Find("Canvas").GetComponent<InterfaceHandler>().hint.GetComponent<Hint>().Enable(type + "\n" + tip);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        GameObject.Find("Canvas").GetComponent<InterfaceHandler>().hint.GetComponent<Hint>().Disable();
    }
}
