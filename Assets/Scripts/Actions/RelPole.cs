using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelPole : MonoBehaviour
{
    private GameObject flag_war;
    private GameObject flag_ally;
    private GameObject flag_trade;
    private GameObject flag_acces;
    private RawImage symbol;


    // Start is called before the first frame update
    void Start()
    {
        flag_war = transform.GetChild(3).GetChild(0).gameObject;
        flag_ally = transform.GetChild(3).GetChild(1).gameObject;
        flag_trade = transform.GetChild(3).GetChild(2).gameObject;
        flag_acces = transform.GetChild(3).GetChild(3).gameObject;

        symbol = transform.GetChild(0).GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetNation(Nation natA, Nation natB)
    {
        Start();

        RefreshFlags(natA, natB);
        SetSymbol(natB.id);
    }

    public void SetSymbol(int id)
    {
        symbol.texture = Resources.Load("Symbols/Symb_" + id) as Texture2D;
    }

    public void RefreshFlags(Nation natA, Nation natB)
    {
        if (natA.rel.IsTrading(natB))
        {
            flag_trade.SetActive(true);
        }
        else
        {
            flag_war.SetActive(false);
        }

        if (natA.rel.IsAtWar(natB))
        {
            flag_war.SetActive(true);
        }
        else
        {
            flag_war.SetActive(false);
        }

        if (natA.rel.IsAllied(natB))
        {
            flag_ally.SetActive(true);
        }
        else
        {
            flag_ally.SetActive(false);
        }

        if (natA.rel.IsAccesible(natB))
        {
            flag_acces.SetActive(true);
        }
        else
        {
            flag_acces.SetActive(false);
        }
    }

}
