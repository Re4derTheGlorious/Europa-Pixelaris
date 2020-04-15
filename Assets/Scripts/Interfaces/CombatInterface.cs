using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;

public class CombatInterface : Interface
{
    private Battle battle;
    private Transform field_front;
    private Transform field_left;
    private Transform field_right;
    private Transform field_att_back;
    private Transform field_def_back;

    void Start()
    {
        field_front = transform.Find("Battlefield/Center/Frontline");
        field_left = transform.Find("Battlefield/Center/Left Flank");
        field_right = transform.Find("Battlefield/Center/Right Flank");
        field_att_back = transform.Find("Battlefield/Attacker Backline");
        field_def_back = transform.Find("Battlefield/Defender Backline");
    }

    void Update()
    {

    }

    public override void MouseInput(Province prov)
    {

    }
    public override void KeyboardInput(Province prov)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MapTools.GetInterface().EnableInterface("none");
        }
    }
    public override void Refresh()
    {
        //show stage
        if (battle.stage == 0)
        {
            transform.Find("Stage/Stage1").gameObject.GetComponent<RawImage>().color = Color.green;
            transform.Find("Balance/Bar/Handle Slide Area/Handle").GetComponent<RawImage>().texture = Resources.Load("Icons/Inf_Skirmish") as Texture2D;

            
        }
        else if (battle.stage == 1)
        {
            transform.Find("Stage/Stage1").gameObject.GetComponent<RawImage>().color = Color.gray;
            transform.Find("Stage/Stage2").gameObject.GetComponent<RawImage>().color = Color.green;
            transform.Find("Balance/Bar/Handle Slide Area/Handle").GetComponent<RawImage>().texture = Resources.Load("Icons/Army_Engaged") as Texture2D;

            

        }
        else if(battle.stage == 2)
        {
            transform.Find("Stage/Stage1").gameObject.GetComponent<RawImage>().color = Color.gray;
            transform.Find("Stage/Stage2").gameObject.GetComponent<RawImage>().color = Color.gray;
            transform.Find("Stage/Stage3").gameObject.GetComponent<RawImage>().color = Color.green;
            transform.Find("Balance/Bar/Handle Slide Area/Handle").GetComponent<RawImage>().texture = Resources.Load("Icons/Army_Routing") as Texture2D;

        }

        OverlayUnits();
        LoadStats();
        SetBalance(battle.powerBalance, battle.leftFlankBalance, battle.rightFlankBalance);
    }
    public override void Set(Nation nat = null, Province prov = null, Army arm = null, Classes.TradeRoute route = null, List<Army> armies = null, List<Unit> units = null, Battle battle = null)
    {
        if (battle != null)
        {
            this.battle = battle;
            ResetStage();
            OverlayTerrain();
            OverlayUnits();
            Refresh();
        }
    }
    public override bool IsSet()
    {
        return battle != null;
    }
    public override void Disable()
    {
        gameObject.SetActive(false);
    }
    public override void Enable()
    {
        

        MapTools.GetSave().GetTime().SetPace(1);
        MapTools.GetSave().GetTime().Pause();

        gameObject.SetActive(true);

        Start();
    }

    public void LoadStats()
    {
        transform.Find("Battlefield/AttackerStats/Reserve/Text").GetComponent<TextMeshProUGUI>().text = "" + battle.attackerReserveSize;
        transform.Find("Battlefield/DefenderStats/Reserve/Text").GetComponent<TextMeshProUGUI>().text = "" + battle.defenderReserveSize;

        transform.Find("Battlefield/AttackerStats/Engaged/Text").GetComponent<TextMeshProUGUI>().text = "" + battle.attackerFieldedSize;
        transform.Find("Battlefield/DefenderStats/Engaged/Text").GetComponent<TextMeshProUGUI>().text = "" + battle.defenderFieldedSize;

        transform.Find("Battlefield/AttackerStats/Captured/Text").GetComponent<TextMeshProUGUI>().text = "" + battle.attackerCaptured;
        transform.Find("Battlefield/DefenderStats/Captured/Text").GetComponent<TextMeshProUGUI>().text = "" + battle.defenderCaptured;

        transform.Find("Battlefield/AttackerStats/Wounded/Text").GetComponent<TextMeshProUGUI>().text = "" + battle.attackerWounded;
        transform.Find("Battlefield/DefenderStats/Wounded/Text").GetComponent<TextMeshProUGUI>().text = "" + battle.defenderWounded;

        transform.Find("Battlefield/AttackerStats/Dead/Text").GetComponent<TextMeshProUGUI>().text = "" + battle.attackerCasualities;
        transform.Find("Battlefield/DefenderStats/Dead/Text").GetComponent<TextMeshProUGUI>().text = "" + battle.defenderCasualities;

        transform.Find("Battlefield/AttackerStats/Routed/Text").GetComponent<TextMeshProUGUI>().text = "" + battle.attackerRouted;
        transform.Find("Battlefield/DefenderStats/Routed/Text").GetComponent<TextMeshProUGUI>().text = "" + battle.defenderRouted;
    }

    public void ResetStage()
    {
        transform.Find("Stage/Stage1").gameObject.GetComponent<RawImage>().color = Color.white;
        transform.Find("Stage/Stage2").gameObject.GetComponent<RawImage>().color = Color.white;
        transform.Find("Stage/Stage3").gameObject.GetComponent<RawImage>().color = Color.white;
    }
    public void SetBalance(float balance, float leftFlank, float rightFlank)
    {
        //main
        transform.Find("Balance/Bar").GetComponent<Slider>().value = battle.powerBalance;
        transform.Find("Balance/Bar/Background/Morale").GetComponent<Slider>().value = battle.powerBalance+(1-battle.powerBalance)*(1-battle.defenderMorale);
        transform.Find("Balance/Bar/Fill Area/Fill/Morale").GetComponent<Slider>().value = 1-battle.attackerMorale;


        transform.Find("Balance/Bar/Background").GetComponent<RawImage>().color = battle.defenders.ElementAt(0).owner.color;
        transform.Find("Balance/Bar/Fill Area/Fill").GetComponent<RawImage>().color = battle.attackers.ElementAt(0).owner.color;

        //flanks
        transform.Find("Battlefield/Center/Left Bar").GetComponent<Slider>().value = battle.leftFlankBalance;
        transform.Find("Battlefield/Center/Left Bar/Background").GetComponent<RawImage>().color = battle.defenders.ElementAt(0).owner.color;
        transform.Find("Battlefield/Center/Left Bar/Fill Area/Fill").GetComponent<RawImage>().color = battle.attackers.ElementAt(0).owner.color;

        transform.Find("Battlefield/Center/Right Bar").GetComponent<Slider>().value = battle.rightFlankBalance;
        transform.Find("Battlefield/Center/Right Bar/Background").GetComponent<RawImage>().color = battle.defenders.ElementAt(0).owner.color;
        transform.Find("Battlefield/Center/Right Bar/Fill Area/Fill").GetComponent<RawImage>().color = battle.attackers.ElementAt(0).owner.color;
    }
    public void Button_Route()
    {
        battle.Route(MapTools.GetSave().GetActiveNation());
    }
    private void ResetTerrain()
    {
        foreach(Transform trans in field_front)
        {
            trans.gameObject.GetComponent<BattleGrid>().SetUnavailable();
        }
        foreach (Transform trans in field_left)
        {
            trans.gameObject.GetComponent<BattleGrid>().SetUnavailable();
        }
        foreach (Transform trans in field_right)
        {
            trans.gameObject.GetComponent<BattleGrid>().SetUnavailable();
        }
        foreach (Transform trans in field_att_back)
        {
            trans.gameObject.GetComponent<BattleGrid>().SetUnavailable();
        }
        foreach (Transform trans in field_def_back)
        {
            trans.gameObject.GetComponent<BattleGrid>().SetUnavailable();
        }
    }
    private void OverlayTerrain()
    {
        ResetTerrain();
        for (int x = 0; x < battle.frontLine.GetLength(0); x++)
        {
            //frontline
            for (int y = 0; y < battle.frontLine.GetLength(1); y++)
            {
                if (battle.frontLine[x, y].impassable)
                {
                    GetTile(field_front, x, y, battle.frontLine).GetComponent<BattleGrid>().SetImpass();
                }
                else
                {
                    GetTile(field_front, x, y, battle.frontLine).GetComponent<BattleGrid>().SetEmpty();
                }
            }
            //backlines
            if (x < battle.attackerBack.GetLength(0)) {
                for (int y = 0; y < battle.attackerBack.GetLength(1); y++)
                {
                    if (battle.attackerBack[x, y].impassable)
                    {
                        GetTile(field_def_back, x, y, battle.defenderBack).GetComponent<BattleGrid>().SetImpass();
                    }
                    else
                    {
                        GetTile(field_def_back, x, y, battle.defenderBack).GetComponent<BattleGrid>().SetEmpty();
                    }
                    if (battle.defenderBack[x, y].impassable)
                    {
                        GetTile(field_att_back, x, y, battle.attackerBack).GetComponent<BattleGrid>().SetImpass();
                    }
                    else
                    {
                        GetTile(field_att_back, x, y, battle.attackerBack).GetComponent<BattleGrid>().SetEmpty();
                    }
                }
            }
        }
        //flanks
        for (int x = 0; x < battle.leftFlank.GetLength(0); x++)
        {
            for (int y = 0; y < battle.leftFlank.GetLength(1); y++)
            {
                if (battle.leftFlank[x, y].impassable)
                {
                    GetTile(field_left, x, y, battle.leftFlank).GetComponent<BattleGrid>().SetImpass();
                }
                else
                {
                    GetTile(field_left, x, y, battle.leftFlank).GetComponent<BattleGrid>().SetEmpty();
                }
                if (battle.rightFlank[x, y].impassable)
                {
                    GetTile(field_right, x, y, battle.rightFlank).GetComponent<BattleGrid>().SetImpass();
                }
                else
                {
                    GetTile(field_right, x, y, battle.rightFlank).GetComponent<BattleGrid>().SetEmpty();
                }
            }
        }
    }
    private void OverlayUnits()
    {
        OverlayTerrain();
        for (int x = 0; x < battle.frontLine.GetLength(0); x++)
        {
            //frontline
            for (int y = 0; y < battle.frontLine.GetLength(1); y++)
            {
                //frontline
                if (battle.frontLine[x, y].contester != null)
                {
                    GetTile(field_front, x, y, battle.frontLine).GetComponent<BattleGrid>().SetUnit(battle.frontLine[x, y].contester);
                }

                //backlines
                if (x < battle.attackerBack.GetLength(0) && y < battle.attackerBack.GetLength(1))
                {
                    if (battle.attackerBack[x, y].contester != null)
                    {
                        GetTile(field_att_back, x, y, battle.attackerBack).GetComponent<BattleGrid>().SetUnit(battle.attackerBack[x, y].contester);
                    }
                    if (battle.defenderBack[x, battle.defenderBack.GetLength(1)-1-y].contester != null)
                    {
                        GetTile(field_def_back, x, battle.defenderBack.GetLength(1) - 1 - y, battle.defenderBack).GetComponent<BattleGrid>().SetUnit(battle.defenderBack[x, battle.defenderBack.GetLength(1) - 1 - y].contester);
                    }
                }
            }
        }

        //flanks
        for (int x = 0; x < battle.leftFlank.GetLength(0); x++)
        {
            for (int y = 0; y < battle.leftFlank.GetLength(1); y++)
            {
                //left
                if (battle.leftFlank[x, y].contester != null)
                {
                    GetTile(field_left, x, y, battle.leftFlank).GetComponent<BattleGrid>().SetUnit(battle.leftFlank[x, y].contester);
                }
                //right
                if (battle.rightFlank[x, y].contester != null)
                {
                    GetTile(field_right, x, y, battle.rightFlank).GetComponent<BattleGrid>().SetUnit(battle.rightFlank[x, y].contester);
                }
            }
        }
    }
    private GameObject GetTile(Transform rep, int x, int y, Battle.Grid[,] field)
    {
        int gridSize = 10;
        int fieldSize = (int)(rep.gameObject.GetComponent<RectTransform>().sizeDelta.x/gridSize);
        int shift = (fieldSize - field.GetLength(0))/2;
        return rep.GetChild(shift + x + y * fieldSize).gameObject;
    }


}
