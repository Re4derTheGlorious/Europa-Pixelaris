using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        field_front = transform.Find("Battlefield/Frontline");
        field_left = transform.Find("Battlefield/Left Flank");
        field_right = transform.Find("Battlefield/Right Flank");
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

    }

    public override void Refresh()
    {
        //show stage
        if (battle.stage == 1)
        {
            transform.Find("Stage/Stage1").gameObject.GetComponent<RawImage>().color = Color.green;
        }
        else if (battle.stage == 2)
        {
            transform.Find("Stage/Stage1").gameObject.GetComponent<RawImage>().color = Color.gray;
            transform.Find("Stage/Stage2").gameObject.GetComponent<RawImage>().color = Color.green;
        }
        else if(battle.stage == 3)
        {
            transform.Find("Stage/Stage1").gameObject.GetComponent<RawImage>().color = Color.gray;
            transform.Find("Stage/Stage2").gameObject.GetComponent<RawImage>().color = Color.gray;
            transform.Find("Stage/Stage3").gameObject.GetComponent<RawImage>().color = Color.green;
        }

        //refresh info
        transform.Find("Defender Info/Casualities").GetComponent<TextMeshProUGUI>().text = "Casualities: " + battle.defenderCasualities;
        transform.Find("Attacker Info/Casualities").GetComponent<TextMeshProUGUI>().text = "Casualities: " + battle.attackerCasualities;
        transform.Find("Defender Info/In Reserves").GetComponent<TextMeshProUGUI>().text = "In reserve: " + battle.defenderReserves;
        transform.Find("Attacker Info/In Reserves").GetComponent<TextMeshProUGUI>().text = "In reserve: " + battle.attackerReserves;
        transform.Find("Defender Info/Engaged").GetComponent<TextMeshProUGUI>().text = "In combat: " + battle.defenderEngaged;
        transform.Find("Attacker Info/Engaged").GetComponent<TextMeshProUGUI>().text = "In combat: " + battle.attackerEngaged;
        transform.Find("Defender Info/Routed").GetComponent<TextMeshProUGUI>().text = "Routed: " + battle.defenderRouted;
        transform.Find("Attacker Info/Routed").GetComponent<TextMeshProUGUI>().text = "Routed: " + battle.attackerRouted;

        //refresh units positions
        OverlayUnits();
    }
    public override void Set(Classes.Nation nat = null, Province prov = null, Classes.Army arm = null, Classes.TradeRoute route = null, List<Classes.Army> armies = null, List<Classes.Unit> units = null, Battle battle = null)
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
        Start();
        gameObject.SetActive(true);
    }

    public void ResetStage()
    {
        transform.Find("Stage/Stage1").gameObject.GetComponent<RawImage>().color = Color.white;
        transform.Find("Stage/Stage2").gameObject.GetComponent<RawImage>().color = Color.white;
        transform.Find("Stage/Stage3").gameObject.GetComponent<RawImage>().color = Color.white;
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
                if (y < battle.attackerBack.GetLength(1))
                {
                    if (battle.defenderBack[x, y].contester != null)
                    {
                        GetTile(field_att_back, x, y, battle.attackerBack).GetComponent<BattleGrid>().SetUnit(battle.attackerBack[x, y].contester);
                    }
                }
                else
                {
                    if (battle.defenderBack[x, y- battle.defenderBack.GetLength(1)].contester != null)
                    {
                        GetTile(field_def_back, x, y- battle.defenderBack.GetLength(1), battle.defenderBack).GetComponent<BattleGrid>().SetUnit(battle.defenderBack[x, y- battle.defenderBack.GetLength(1)].contester);
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
