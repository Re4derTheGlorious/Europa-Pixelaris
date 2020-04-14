using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NationSymbolClick : MonoBehaviour
{
    private Nation nat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetNation(Nation nat)
    {
        this.nat = nat;
    }

    public Nation GetNation()
    {
        if (nat == null)
        {
            return MapTools.GetSave().GetActiveNation();
        }
        return nat;
    }

    public void Button_Symbol()
    {
        Nation targetNation = GetNation();

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
