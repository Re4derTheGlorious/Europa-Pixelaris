using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NationSymbolClick : MonoBehaviour
{
    private Classes.Nation nat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetNation(Classes.Nation nat)
    {
        this.nat = nat;
    }

    public Classes.Nation GetNation()
    {
        if (nat == null)
        {
            return MapTools.GetSave().GetActiveNation();
        }
        return nat;
    }

    public void Button_Symbol()
    {
        Classes.Nation targetNation = GetNation();

        if (MapTools.GetInterface().GetActiveInterface().Equals("diplomacy"))
        {
            MapTools.GetInterface().activeInterface.Set(targetNation, null, null);
        }
        else
        {
            MapTools.GetInterface().EnableInterface("diplomacy", targetNation, null, null);
            MapTools.GetInterface().activeInterface.Set(targetNation, null, null);
        }
    }
}
