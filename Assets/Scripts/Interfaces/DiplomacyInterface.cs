using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class DiplomacyInterface : Interface
{
    public Classes.Nation owner;
    private TextMeshProUGUI nameText;
    private GameObject symbol;
    private GameObject rels;

    //relflags
    private Radio_Button button_trade;
    private Radio_Button button_war;
    private Radio_Button button_allied;
    private Radio_Button button_acces;
    private Radio_Button button_all;

    //negflags
    private Radio_Button button_neg_trade;
    private Radio_Button button_neg_hostile;
    private Radio_Button button_neg_coop;
    private Radio_Button button_neg_influence;
    private Radio_Button button_neg_rel;

    private string activeTag;

    //frames
    private GameObject frame_rel;
    private GameObject frame_neg;
    private Transform neg_offers;
    private Transform neg_offered;

    //negotiations
    public Classes.Negotiations neg;
    private TextMeshProUGUI summary;

    // Start is called before the first frame update
    void Start()
    {
        nameText = transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        symbol = transform.GetChild(0).gameObject;
        rels = transform.GetChild(2).GetChild(0).gameObject;

        button_all = transform.GetChild(2).GetChild(1).GetChild(4).gameObject.GetComponent<Radio_Button>();
        button_trade = transform.GetChild(2).GetChild(1).GetChild(3).gameObject.GetComponent<Radio_Button>();
        button_acces = transform.GetChild(2).GetChild(1).GetChild(2).gameObject.GetComponent<Radio_Button>();
        button_allied = transform.GetChild(2).GetChild(1).GetChild(1).gameObject.GetComponent<Radio_Button>();
        button_war = transform.GetChild(2).GetChild(1).GetChild(0).gameObject.GetComponent<Radio_Button>();

        button_neg_rel = transform.GetChild(3).GetChild(4).gameObject.GetComponent<Radio_Button>();
        button_neg_trade = transform.GetChild(3).GetChild(3).gameObject.GetComponent<Radio_Button>();
        button_neg_influence = transform.GetChild(3).GetChild(2).gameObject.GetComponent<Radio_Button>();
        button_neg_coop = transform.GetChild(3).GetChild(1).gameObject.GetComponent<Radio_Button>();
        button_neg_hostile = transform.GetChild(3).GetChild(0).gameObject.GetComponent<Radio_Button>();

        frame_rel = transform.GetChild(2).gameObject;
        frame_neg = transform.GetChild(4).gameObject;
        frame_neg.SetActive(false);

        neg_offers = frame_neg.transform.Find("Offers");
        neg_offered = frame_neg.transform.Find("Offered");

        summary = transform.GetChild(4).GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override bool IsSet()
    {
        return owner != null;
    }

    public override void MouseInput(Province prov)
    {

    }
    public override void KeyboardInput(Province prov)
    {

    }

    public override void Disable()
    {
        ExitNegotiations();
        owner = null;
        gameObject.SetActive(false);
    }

    public override void Enable()
    {
        gameObject.SetActive(true);
    }

    public override void Set(Classes.Nation nat = null, Province prov = null, Classes.Army arm = null, Classes.TradeRoute route = null, List<Classes.Army> armies = null, List<Classes.Unit> units = null, Battle battle = null)
    {
        this.owner = nat;
        Refresh();
    }

    public void SetSymbol(int id)
    {
        symbol.transform.GetChild(0).gameObject.GetComponent<RawImage>().texture = Resources.Load("Symbols/Symb_" + id) as Texture2D;
        symbol.transform.GetChild(2).gameObject.GetComponent<NationSymbolClick>().nat = MapTools.IdToNat(id);
    }

    public void FillRels()
    {
        ClearRels();
        List<Classes.Nation> relations = new List<Classes.Nation>();
        if (button_war.isActive)
        {
            foreach(Classes.War war in owner.rel.wars)
            {
                foreach (Classes.Nation n in war.GetEnemiesOf(owner))
                {
                    relations.Add(n);
                }
            }
            if (relations.Count > 1)
            {
                relations = relations.Distinct().ToList();
            }
        }
        else if (button_allied.isActive)
        {
            relations = owner.rel.allied;
        }
        else if (button_acces.isActive)
        {
            relations = owner.rel.grantsAcces;
        }
        else if (button_trade.isActive)
        {
            relations = owner.rel.grantsTrade;
        }
        else if (button_all.isActive)
        {
            relations = GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetNations();
            relations.Remove(owner);
        }

        foreach (Classes.Nation rel in relations)
        {
            GameObject newRel = Instantiate(Resources.Load("Prefabs/UI_RelFrame") as GameObject, rels.transform);
            newRel.GetComponent<RelFrame>().nat = rel;
            newRel.GetComponent<RelFrame>().SetSymbol(rel.id);
        }
    }

    public void ClearRels()
    {
        foreach(Transform obj in rels.transform)
        {
            Destroy(obj.gameObject);
        }
        rels.transform.DetachChildren();
    }

    public override void Refresh()
    {
        if (owner == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            SetSymbol(owner.id);
            nameText.text = owner.name;

            FillRels();
            RefreshButtons();

            gameObject.SetActive(true);
        }
    }

    //Negotiations
    public void EnterNegotiations(Offer topic)
    {
        ResetButtons();
        if (GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetActiveNation() != owner) {
            neg = new Classes.Negotiations(GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetActiveNation(), owner, topic);
        }
    }
    public void ExitNegotiations()
    {
        if (neg != null)
        {
            neg = null;
            ResetButtons();
        }
        ResetButtons();
        ShowOnly(activeTag);
        frame_neg.transform.Find("Accept").GetComponent<RawImage>().color = Color.white;
    }
    public void ResetButtons()
    {
        foreach(Transform trans in neg_offered)
        {
            trans.SetParent(neg_offers);
        }
        foreach(Transform trans in neg_offers)
        {
            trans.gameObject.GetComponent<Button>().interactable = true;
        }
    }
    public void RefreshButtons()
    {
        ResetButtons();

        if (neg != null)
        {
            //move offers to the right
            foreach (Transform trans in neg_offered)
            {
                trans.SetParent(neg_offers);
            }
            if (neg.topic != null)
            {
                neg.topic.transform.SetParent(neg_offered);
                neg.topic.gameObject.GetComponent<Button>().interactable = true;
            }
            foreach (Offer off in neg.offer)
            {
                off.transform.SetParent(neg_offered);
                off.gameObject.GetComponent<Button>().interactable = true;
            }

            //disactivate exclusives
            foreach (Transform trans in neg_offers)
            {
                if (trans.GetComponent<Offer>().exclusive)
                {
                    trans.gameObject.GetComponent<Button>().interactable = false;
                }
            }
        }

        //show only in selected tab
        ShowOnly(activeTag);

        //Show offer value
        if (neg != null)
        {
            if (neg.NegValue() > 0)
            {
                frame_neg.transform.Find("Accept").GetComponent<RawImage>().color = Color.green;
            }
            else
            {
                frame_neg.transform.Find("Accept").GetComponent<RawImage>().color = Color.red;
            }
        }
    }

    public void RefreshSummary()
    {
        string newText = "Summary:\n";
        newText += neg.NegSummary();
        newText += "\n\nTheir willingness to sign this treaty is " + neg.NegValue();
        summary.text = newText;
    }
    public void ShowOnly(string tag)
    {
        foreach(Transform obj in frame_neg.transform.GetChild(0).transform)
        {
            if (obj.gameObject.tag.Equals(tag))
            {
                obj.gameObject.SetActive(true);
            }
            else
            {
                obj.gameObject.SetActive(false);
            }
        }
    }

    //buttons
    public void ButtonNegFinal()
    {
        if (neg.topic.needsAgreement)
        {
            if (neg.NegValue() > 0)
            {
                neg.NegFinalize();
            }
            else
            {
                GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("They are not willing to make this deal");
                return;
            }
        }
        else
        {
            neg.NegFinalize();
        }
        ExitNegotiations();
    }
    public void ButtonNegRel()
    {
        frame_rel.SetActive(true);
        frame_neg.SetActive(false);
        ExitNegotiations();
    }
    public void ButtonNegHostile()
    {
        frame_rel.SetActive(false);
        frame_neg.SetActive(true);
        activeTag = "Neg_Hostile";
        ShowOnly("Neg_Hostile");
    }
    public void ButtonNegCoop()
    {
        frame_rel.SetActive(false);
        frame_neg.SetActive(true);
        activeTag = "Neg_Coop";
        ShowOnly("Neg_Coop");
    }
    public void ButtonNegInfluence()
    {
        frame_rel.SetActive(false);
        frame_neg.SetActive(true);
        activeTag = "Neg_Influence";
        ShowOnly("Neg_Influence");
    }
    public void ButtonNegTrade()
    {
        frame_rel.SetActive(false);
        frame_neg.SetActive(true);
        activeTag = "Neg_Trade";
        ShowOnly("Neg_Trade");
    }
    public void ButtonOffer(Offer off)
    {
        if (neg == null)
        {
            EnterNegotiations(off);
        }
        else if(off == neg.topic)
        {
            ExitNegotiations();
        }
        else
        {
            if (neg.offer.Contains(off))
            {
                neg.offer.Remove(off);
            }
            else
            {
                neg.offer.Add(off);
            }
        }
        RefreshButtons();
    }

}
