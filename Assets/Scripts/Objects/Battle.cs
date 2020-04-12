using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Battle
{
    public class Grid
    {
        public Grid(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public int x = 0;
        public int y = 0;
        public bool impassable = false;
        public bool cover = false;
        public bool highGround = false;
        public Classes.Unit contester;
    }
    public int stage = 0;
    public GameObject representation;

    //sides stats
    public int attackerCasualities = 0;
    public int defenderCasualities = 0;
    public int attackerRouted = 0;
    public int defenderRouted = 0;
    public int attackerEngaged = 0;
    public int defenderEngaged = 0;
    public int attackerReserves = 0;
    public int defenderReserves = 0;

    //setting
    public List<Classes.Army> attackers;
    public List<Classes.Army> defenders;
    public Province location;

    //reserves
    public List<Classes.Unit> attackerReserveUnits;
    public List<Classes.Unit> defenderReserveUnits;
    public List<Classes.Unit> attSkirmishers = new List<Classes.Unit>();
    public List<Classes.Unit> defSkirmishers = new List<Classes.Unit>();
    public List<Classes.Unit> attLight = new List<Classes.Unit>();
    public List<Classes.Unit> defLight = new List<Classes.Unit>();
    public List<Classes.Unit> attHeavy = new List<Classes.Unit>();
    public List<Classes.Unit> defHeavy = new List<Classes.Unit>();
    public List<Classes.Unit> attArt = new List<Classes.Unit>();
    public List<Classes.Unit> defArt = new List<Classes.Unit>();

    //battlefield
    public Grid[,] frontLine;
    public Grid[,] leftFlank;
    public Grid[,] rightFlank;
    public Grid[,] attackerBack;
    public Grid[,] defenderBack;


    public Battle(Classes.Army attacker, Classes.Army defender, Province location)
    {
        attackers = new List<Classes.Army>();
        defenders = new List<Classes.Army>();

        attackers.Add(attacker);
        defenders.Add(defender);

        this.location = location;

        representation = Object.Instantiate(Resources.Load("Prefabs/Battle") as GameObject, GameObject.Find("Map/Battles").transform);
        Vector3 loc = MapTools.LocalToScale(attacker.location.center);
        loc.z = -5;
        representation.transform.position = loc;
    }

    //setting
    public void StartBattle()
    {
        foreach (Classes.Army attacker in attackers)
        {
            attacker.SetEngaged(true);
        }
        foreach (Classes.Army defender in defenders)
        {
            defender.SetEngaged(true);
        }

        RefreshReserves();
        SetBattlefield();
    }
    public void EndBattle()
    {
        foreach (Classes.Army attacker in attackers)
        {
            attacker.SetEngaged(false);
        }
        foreach (Classes.Army defender in defenders)
        {
            defender.SetEngaged(false);
            defender.Route();
        }

        Object.Destroy(representation);
    }
    public void JoinArmy()
    {

    }
    public void SetBattlefield()
    {
        //determine battlefield
        int width = (int)Mathf.Max(attackers.ElementAt(0).owner.mods.GetMod("engagement_width"), defenders.ElementAt(0).owner.mods.GetMod("engagement_width"));
        int flankWidth = 0;
        int covers = 0;
        int steeps = 0;
        int impasses = 0;

        //set to even values
        if (width % 2 != 0)
        {
            width++;
        }
        if (width > 20)
        {
            width = 20;
        }
        else if (width < 6)
        {
            width = 6;
        }
        flankWidth = (int)(width / 3);
        if (flankWidth % 2 != 0)
        {
            flankWidth++;
        }
        if (flankWidth > 8)
        {
            flankWidth = 8;
        }
        else if (flankWidth < 2)
        {
            flankWidth = 2;
        }

        //Determine obstructions
        int totalVolume = width * 8 + flankWidth * 8;
        if ((int)location.mods.GetMod("terrain_plain") == 1)
        {
            covers = (int)(totalVolume * 0.1f);
            steeps = (int)(totalVolume * 0.1f);
            impasses = (int)(totalVolume * 0.1f);
        }
        else if ((int)location.mods.GetMod("terrain_forest") == 1)
        {
            covers = (int)(totalVolume * 0.6f);
            steeps = (int)(totalVolume * 0.1f);
            impasses = (int)(totalVolume * 0.2f);
        }
        else if ((int)location.mods.GetMod("terrain_hills") == 1)
        {
            covers = (int)(totalVolume * 0.2f);
            steeps = (int)(totalVolume * 0.6f);
            impasses = (int)(totalVolume * 0.3f);
        }
        else if ((int)location.mods.GetMod("terrain_mountains") == 1)
        {
            covers = (int)(totalVolume * 0.1f);
            steeps = (int)(totalVolume * 0.1f);
            impasses = (int)(totalVolume * 0.5f);
        }

        //set battlefield
        frontLine = new Grid[width, 4];
        leftFlank = new Grid[flankWidth, 4];
        rightFlank = new Grid[flankWidth, 4];
        attackerBack = new Grid[width, 2];
        defenderBack = new Grid[width, 2];

        //fill grids
        do
        {
            for (int x = 0; x < frontLine.GetLength(0); x++)
            {
                for (int y = 0; y < frontLine.GetLength(1); y++)
                {
                    frontLine[x, y] = new Grid(x, y);
                }
            }
            for (int x = 0; x < leftFlank.GetLength(0); x++)
            {
                for (int y = 0; y < leftFlank.GetLength(1); y++)
                {
                    leftFlank[x, y] = new Grid(x, y);
                    rightFlank[x, y] = new Grid(x, y);
                }
            }
            for (int x = 0; x < attackerBack.GetLength(0); x++)
            {
                for (int y = 0; y < attackerBack.GetLength(1); y++)
                {
                    attackerBack[x, y] = new Grid(x, y);
                    defenderBack[x, y] = new Grid(x, y);
                }
            }

            //impassables
            int insideTries = 0;
            for (int i = 0; i < impasses; i++)
            {
                Grid grid = RandomGrid();
                if (!grid.impassable)
                {
                    grid.impassable = true;
                }
                else
                {
                    i--;
                }
                if (insideTries > 10000)
                {
                    break;
                }
            }
        }
        while (!IsTerrainValid());
        //steeps
        int tries = 0;
        for (int i = 0; i < steeps; i++)
        {
            Grid grid = RandomGrid();
            if (!grid.highGround && !grid.impassable)
            {
                grid.highGround = true;
            }
            else
            {
                i--;
            }
            if (tries > 10000)
            {
                break;
            }
        }
        //covers
        tries = 0;
        for (int i = 0; i < steeps; i++)
        {
            Grid grid = RandomGrid();
            if (!grid.highGround && !grid.impassable && !grid.cover)
            {
                grid.cover = true;
            }
            else
            {
                i--;
            }
            if (tries > 10000)
            {
                break;
            }
        }
    }
    public void RollOutSkirmishers()
    {
        //determine skirmish size
        int attSkirmishSize = 0;
        int defSkirmishSize = 0;
        int skirmishSize = 0;
        foreach (Grid g in frontLine)
        {
            if (g.contester != null)
            {
                if (g.contester.attacking)
                {
                    attSkirmishSize++;
                }
                else
                {
                    defSkirmishSize++;
                }
            }
        }
        skirmishSize = Mathf.Max(defSkirmishSize, attSkirmishSize);

        //determine available space
        List<Grid> attAvailable = new List<Grid>();
        List<Grid> defAvailable = new List<Grid>();
        for (int x = 0; x<frontLine.GetLength(0); x++)
        {
            if(frontLine[x, 0].contester == null)
            {
                attAvailable.Add(frontLine[x, 0]);
            }
            if(frontLine[x, frontLine.GetLength(1) - 1].contester == null)
            {
                defAvailable.Add(frontLine[x, frontLine.GetLength(1) - 1]);
            }
        }

        //rollout attackers
        while (attAvailable.Count > 0)
        {
            int i = Random.Range(0, attAvailable.Count);
            if (attSkirmishers.Count > 0)
            {
                attAvailable.ElementAt(i).contester = attSkirmishers.ElementAt(0);
                attSkirmishers.ElementAt(0).position = attAvailable.ElementAt(i);
                attSkirmishers.RemoveAt(0);
                break;
            }
            else if (attSkirmishSize < skirmishSize) {
                if (attLight.Count > 0)
                {
                    attAvailable.ElementAt(i).contester = attLight.ElementAt(0);
                    attLight.ElementAt(0).position = attAvailable.ElementAt(i);
                    attLight.RemoveAt(0);
                    break;
                }
                else if (attHeavy.Count > 0)
                {
                    attAvailable.ElementAt(i).contester = attHeavy.ElementAt(0);
                    attHeavy.ElementAt(0).position = attAvailable.ElementAt(i);
                    attHeavy.RemoveAt(0);
                    break;
                }
                else if (attArt.Count > 0)
                {
                    attAvailable.ElementAt(i).contester = attArt.ElementAt(0);
                    attArt.ElementAt(0).position = attAvailable.ElementAt(i);
                    attArt.RemoveAt(0);
                    break;
                }
            }
            break;
        }

        //rollout defenders
        while (defAvailable.Count > 0)
        {
            int i = Random.Range(0, defAvailable.Count);
            if (defSkirmishers.Count > 0)
            {
                defAvailable.ElementAt(i).contester = defSkirmishers.ElementAt(0);
                defSkirmishers.ElementAt(0).position = defAvailable.ElementAt(i);
                defSkirmishers.RemoveAt(0);
                break;
            }
            else if (defSkirmishSize < skirmishSize)
            {
                if (defLight.Count > 0)
                {
                    defAvailable.ElementAt(i).contester = defLight.ElementAt(0);
                    defLight.ElementAt(0).position = defAvailable.ElementAt(i);
                    defLight.RemoveAt(0);
                    break;
                }
                else if (defHeavy.Count > 0)
                {
                    defAvailable.ElementAt(i).contester = defHeavy.ElementAt(0);
                    defHeavy.ElementAt(0).position = defAvailable.ElementAt(i);
                    defHeavy.RemoveAt(0);
                    break;
                }
                else if (defArt.Count > 0)
                {
                    defAvailable.ElementAt(i).contester = defArt.ElementAt(0);
                    defArt.ElementAt(0).position = defAvailable.ElementAt(i);
                    defArt.RemoveAt(0);
                    break;
                }
            }
            break;
        }
    }
    public void RetrieveUnits(Grid[,] field)
    {
        foreach(Grid g in field)
        {
            if (g.contester != null)
            {
                g.contester.position = null;
                g.contester = null;
            }
        }
        RefreshReserves();
    }
    public void RollOutCombatants()
    {
        //determine front
        List<Classes.Unit> attFront = new List<Classes.Unit>();
        foreach (Classes.Unit u in attHeavy)
        {
            if (u.type == "inf_heavy")
            {
                attFront.Add(u);
            }
        }
        foreach (Classes.Unit u in attLight)
        {
            if (u.type == "inf_light")
            {
                attFront.Add(u);
            }
        }
        foreach (Classes.Unit u in attSkirmishers)
        {
            if (u.type == "inf_skirmish")
            {
                attFront.Add(u);
            }
        }
        List<Classes.Unit> defFront = new List<Classes.Unit>();
        foreach (Classes.Unit u in defHeavy)
        {
            if (u.type == "inf_heavy")
            {
                defFront.Add(u);
            }
        }
        foreach (Classes.Unit u in defLight)
        {
            if (u.type == "inf_light")
            {
                defFront.Add(u);
            }
        }
        foreach (Classes.Unit u in defSkirmishers)
        {
            if (u.type == "inf_skirmish")
            {
                defFront.Add(u);
            }
        }
        attFront = attFront.Distinct().ToList();
        defFront = defFront.Distinct().ToList();

        for (int x = 1; x<frontLine.GetLength(0)/2+1; x++)
        {
            for(int i = 0; i<2; i++)
            {
                int trueX = 0;
                if (i == 0)
                {
                    trueX = frontLine.GetLength(0) / 2 - x;
                }
                else
                {
                    trueX = frontLine.GetLength(0) / 2 + x-1;
                }

                if (attFront.Count > 0)
                {
                    frontLine[trueX, 0].contester = attFront.ElementAt(0);
                    attFront.ElementAt(0).position = frontLine[trueX, 0];
                    attFront.RemoveAt(0);
                }
                if (defFront.Count > 0)
                {
                    frontLine[trueX, frontLine.GetLength(1) - 1].contester = defFront.ElementAt(0);
                    defFront.ElementAt(0).position = frontLine[trueX, frontLine.GetLength(1)-1];
                    defFront.RemoveAt(0);
                }
            }
        }
        
    }
    public void RollOutSupports()
    {
        for (int x = 1; x < frontLine.GetLength(0) / 2 + 1; x++)
        {
            for (int i = 0; i < 2; i++)
            {
                int trueX = 0;
                if (i == 0)
                {
                    trueX = frontLine.GetLength(0) / 2 - x;
                }
                else
                {
                    trueX = frontLine.GetLength(0) / 2 + x - 1;
                }

                if (attSkirmishers.Count > 0)
                {
                    if(frontLine[trueX, 0].contester == null)
                    {
                        frontLine[trueX, 0].contester = attSkirmishers.ElementAt(0);
                        attSkirmishers.ElementAt(0).position = frontLine[trueX, 0];
                        attSkirmishers.RemoveAt(0);
                    }
                }
                else if (defSkirmishers.Count > 0)
                {
                    if (frontLine[trueX, frontLine.GetLength(1)-1].contester == null)
                    {
                        frontLine[trueX, frontLine.GetLength(1) - 1].contester = defSkirmishers.ElementAt(0);
                        defSkirmishers.ElementAt(0).position = frontLine[trueX, frontLine.GetLength(1) - 1];
                        defSkirmishers.RemoveAt(0);
                    }
                }
            }
        }
    }
    public Grid RandomGrid()
    {
        int x = 0;
        int y = 0;
        switch (Random.Range(0, 5))
        {
            case 0:
                x = Random.Range(0, frontLine.GetLength(0));
                y = Random.Range(0, frontLine.GetLength(1));
                return frontLine[x, y];
            case 1:
                x = Random.Range(0, rightFlank.GetLength(0));
                y = Random.Range(0, rightFlank.GetLength(1));
                return rightFlank[x, y];
            case 2:
                x = Random.Range(0, leftFlank.GetLength(0));
                y = Random.Range(0, leftFlank.GetLength(1));
                return leftFlank[x, y];
            case 3:
                x = Random.Range(0, attackerBack.GetLength(0));
                y = Random.Range(0, attackerBack.GetLength(1));
                return attackerBack[x, y];
            case 4:
                x = Random.Range(0, defenderBack.GetLength(0));
                y = Random.Range(0, defenderBack.GetLength(1));
                return defenderBack[x, y];
        }
        return null;
    }
    public void RefreshReserves()
    {
        //add
        attackerReserves = 0;
        attackerReserveUnits = new List<Classes.Unit>();
        foreach (Classes.Army a in attackers)
        {
            foreach (Classes.Unit u in a.units)
            {
                if (!attackerReserveUnits.Contains(u))
                {
                    if (u.position == null && u.routed == false)
                    {
                        attackerReserveUnits.Add(u);
                        attackerReserves += u.manpower;
                        u.attacking = true;
                    }
                }
            }
        }
        defenderReserves = 0;
        defenderReserveUnits = new List<Classes.Unit>();
        foreach (Classes.Army a in defenders)
        {
            foreach (Classes.Unit u in a.units)
            {
                if (!defenderReserveUnits.Contains(u))
                {
                    if (u.position == null && u.routed == false)
                    {
                        defenderReserveUnits.Add(u);
                        defenderReserves += u.manpower;
                        u.attacking = false;
                    }
                }
            }
        }

        //sort out
        foreach (Classes.Unit u in attackerReserveUnits)
        {
            if (u.type.Equals("inf_skirmish") || u.type.Equals("cav_missile"))
            {
                attSkirmishers.Add(u);
            }
            else if (u.type.Equals("inf_light") || u.type.Equals("cav_light"))
            {
                attLight.Add(u);
            }
            else if (u.type.Equals("inf_heavy") || u.type.Equals("cav_shock"))
            {
                attHeavy.Add(u);
            }
            else if (u.type.Equals("art_field") || u.type.Equals("art_heavy") || u.type.Equals("art_siege"))
            {
                attArt.Add(u);
            }
        }
        foreach (Classes.Unit u in defenderReserveUnits)
        {
            if (u.type.Equals("inf_skirmish") || u.type.Equals("cav_missile"))
            {
                defSkirmishers.Add(u);
            }
            else if (u.type.Equals("inf_light") || u.type.Equals("cav_light"))
            {
                defLight.Add(u);
            }
            else if (u.type.Equals("inf_heavy") || u.type.Equals("cav_shock"))
            {
                defHeavy.Add(u);
            }
            else if (u.type.Equals("art_field") || u.type.Equals("art_heavy") || u.type.Equals("art_siege"))
            {
                defArt.Add(u);
            }
        }
    }
    public bool IsTerrainValid()
    {

        return true;
    }

    //resolving
    public bool Tick(Classes.TimeAndPace time)
    {
        if (stage == 0)
        {
            //preparations
            if (time.hour == 0)
            {
                stage++;
            }
        }
        if (stage == 1)
        {
            //skirmishes
            ResetVariables();
            EngageUnits();
            UnitsMovement();
            ApplyDamage();
            RollOutSkirmishers();
            if (time.hour == 23) {
                if (CheckForSkirmishEnd())
                {
                    RetrieveUnits(frontLine);
                    RollOutCombatants();
                }
            }
        }
        else if (stage == 2)
        {
            //battle
            
            ResetVariables();
            EngageUnits();
            UnitsMovement();
            ApplyDamage();
            if (time.hour == 23) { 
                CheckForBattleEnd();
            }
        }
        else if (stage == 3)
        {
            //disengagement
            ResetVariables();
            EngageUnits();
            UnitsMovement();
            ApplyDamage();
            stage++;
        }
        else if (stage == 4)
        {
            stage++;
        }

        return false;
    }
    public bool CheckForSkirmishEnd()
    {


        if(Random.Range(0, 1f) > 0.5)
        {
            stage++;
            Debug.Log("Skirmish end");
            return true;
        }
        else if(GetUnits(frontLine, true).Count+GetUnits(frontLine, false).Count < 4)
        {
            stage++;
            Debug.Log("Skirmish end");
            return true;
        }
        return false;
    }
    public void CheckForBattleEnd()
    {
        bool attackerRouted = true;
        bool defenderRouted = true;
        foreach(Classes.Unit u in GetUnits(frontLine, true))
        {
            if (!u.routing)
            {
                attackerRouted = false;
                break;
            }
        }
        foreach (Classes.Unit u in GetUnits(frontLine, false))
        {
            if (!u.routing)
            {
                defenderRouted = false;
                break;
            }
        }
        if (attackerRouted)
        {
            //defender victory
            Debug.Log("Def win");
            stage++;
        }
        else if(defenderRouted)
        {
            //attacker victory
            Debug.Log("Att win");
            stage++;
        }
    }

    public void UnitsMovement()
    {
        //frontline
        List<Classes.Unit> attUnits = GetUnits(frontLine, true);
        List<Classes.Unit> defUnits = GetUnits(frontLine, false);

        foreach (Classes.Unit u in attUnits)
        {
            MoveUnit(u, frontLine);
        }
        foreach (Classes.Unit u in defUnits)
        {
            MoveUnit(u, frontLine);
        }
    }
    public void MoveUnit(Classes.Unit u, Grid[,] field)
    {
        //set direction
        int direction = 1;
        if (!u.attacking)
        {
            direction = -1;
        }
        int x = u.position.x;
        int y = u.position.y;

        //routing
        if (u.routing)
        {
            if (y - direction < 0 || y - direction >= field.GetLength(1))
            {
                u.position.contester = null;
                u.position = null;
                u.routed = true;
                if (u.attacking)
                {
                    attackerRouted += u.manpower;
                }
                else
                {
                    defenderRouted += u.manpower;
                }
                return;
            }
            else if (y - direction >= 0 && y - direction < field.GetLength(1))
            {
                if (field[x, y - direction].contester == null && !field[x, y - direction].impassable)
                {
                    MoveTo(x, y-direction, frontLine, u);
                    Debug.Log(1);
                    return;
                }
                else if (!field[x, y - direction].impassable && !field[x, y - direction].contester.routing && !field[x, y - direction].contester.moved)
                {
                    //switch units
                    MoveTo(x, y-direction, frontLine, u);
                    Debug.Log(2);
                    return;
                }
            }
        }

        //regular movement
        else if (!u.moved)
        {
            if (u.dmgReceived + u.dmgDealt == 0)
            {
                //move toward front
                if (u.attacking && y + 1 < field.GetLength(1) / 2)
                {
                    if (field[x, y + 1].contester == null)
                    {
                        MoveTo(x, y + 1, frontLine, u);
                        return;
                    }
                }
                else if (!u.attacking && y - 1 >= field.GetLength(1) / 2)
                {
                    if (field[x, y - 1].contester == null)
                    {
                        MoveTo(x, y - 1, frontLine, u);
                        return;
                    }
                }
                //move toward center
                if (x >= field.GetLength(0) / 2)
                {
                    if (field[x - 1, y].contester == null)
                    {
                        MoveTo(x - 1, y, frontLine, u);
                        return;
                    }
                }
                else
                {
                    if (field[x + 1, y].contester == null)
                    {
                        MoveTo(x + 1, y, frontLine, u);
                        return;
                    }
                }
            }
        }
    }
    public void MoveTo(int x, int y, Grid[,] field, Classes.Unit u)
    {
        if(field[x, y].contester != null)
        {
            field[u.position.x, u.position.y].contester = field[x, y].contester;
            field[x, y].contester.position = field[u.position.x, u.position.y];
            field[x, y].contester = u;
            u.position = field[x, y];
            u.moved = true;
            field[u.position.x, u.position.y].contester.moved = true;
        }
        else
        {
            u.position.contester = null;
            u.position = field[x, y];
            field[x, y].contester = u;
            u.moved = true;
        }
    }
    public void FindTarget(Classes.Unit u, Grid[,] field)
    {
        //set direction
        int direction = 1;
        if (!u.attacking)
        {
            direction = -1;
        }
        int x = u.position.x;
        int y = u.position.y;

        //long range

        //ranged
        if (u.ranged || u.longRanged)
        {
            if (field[x, y + 2 * direction].contester != null)
            {
                if (field[x, y + 2 * direction].contester.attacking != u.attacking)
                {
                    if (field[x, y + 2 * direction].contester.ranged || field[x, y + 2 * direction].contester.longRanged)
                    {
                        ResolveCombat(u, field[x, y + 2 * direction].contester, true);
                    }
                    else
                    {
                        ResolveCombat(u, field[x, y + 2 * direction].contester, false);
                    }
                    return;
                }
            }
            if (x + 1 < field.GetLength(0) && field[x + 1, y + 2 * direction].contester != null)
            {
                if (field[x + 1, y + 2 * direction].contester.attacking != u.attacking)
                {
                    if (field[x + 1, y + 2 * direction].contester.ranged || field[x + 1, y + 2 * direction].contester.longRanged)
                    {
                        ResolveCombat(u, field[x + 1, y + 2 * direction].contester, true);
                    }
                    else
                    {
                        ResolveCombat(u, field[x + 1, y + 2 * direction].contester, false);
                    }
                    return;
                }
            }
            if (x - 1 >= 0 && field[x - 1, y + 2 * direction].contester != null)
            {
                if (field[x - 1, y + 2 * direction].contester.attacking != u.attacking)
                {
                    if (field[x - 1, y + 2 * direction].contester.ranged || field[x - 1, y + 2 * direction].contester.longRanged)
                    {
                        ResolveCombat(u, field[x - 1, y + 2 * direction].contester, true);
                    }
                    else
                    {
                        ResolveCombat(u, field[x - 1, y + 2 * direction].contester, false);
                    }
                    return;
                }
            }
        }

        //melee
        if (frontLine[x, y + direction].contester != null)
        {
            if (frontLine[x, y + direction].contester.attacking != u.attacking)
            {
                ResolveCombat(u, field[x, y + direction].contester, true);
                return;
            }
        }
        if (x + 1 < frontLine.GetLength(0) && frontLine[x + 1, y + direction].contester != null)
        {
            if (frontLine[x + 1, y + direction].contester.attacking != u.attacking)
            {
                ResolveCombat(u, field[x + 1, y + direction].contester, true);
                return;
            }
        }
        if (x - 1 >= 0 && frontLine[x - 1, y + direction].contester != null)
        {
            if (frontLine[x - 1, y + direction].contester.attacking != u.attacking)
            {
                ResolveCombat(u, field[x - 1, y + direction].contester, true);
                return;
            }
        }
    }
    public void EngageUnits()
    {
        //frontline
        List<Classes.Unit> attUnits = GetUnits(frontLine, true);
        List<Classes.Unit> defUnits = GetUnits(frontLine, false);

        foreach (Classes.Unit u in attUnits)
        {
            FindTarget(u, frontLine);
        }
        foreach (Classes.Unit u in defUnits)
        {
            FindTarget(u, frontLine);
        }
    }
    public void ResolveCombat(Classes.Unit attacker, Classes.Unit target, bool counterable)
    {
        //count damage
        float dmgMod = attacker.GetCombatStrength(stage) / target.GetCombatStrength(stage);
        int result = (int)Mathf.Round(dmgMod*1);
        if (Random.Range(0, 1f) > 0.5f)
        {
            result = (int)Mathf.Round(result * 1.1f)+1;
        }

        //add damage
        attacker.lastTarget = target;
        attacker.dmgDealt += result;
        target.dmgReceived += result;

        //morale efffect

        //counter attack
        if (counterable)
        {
            ResolveCombat(target, attacker, false);
        }
    }
    public void ResetVariables()
    {
        //frontline
        List<Classes.Unit> attUnits = GetUnits(frontLine, true);
        List<Classes.Unit> defUnits = GetUnits(frontLine, false);

        foreach (Classes.Unit u in attUnits)
        {
            u.dmgDealt = 0;
            u.dmgReceived = 0;
            u.moved = false;
        }
        foreach (Classes.Unit u in defUnits)
        {
            u.dmgDealt = 0;
            u.dmgReceived = 0;
            u.moved = false;
        }

        //backline

        //flanks
    }
    public void ApplyDamage()
    {
        //frontline
        List<Classes.Unit> units = GetUnits(frontLine, true);
        units.AddRange(GetUnits(frontLine, false));

        foreach (Classes.Unit u in units)
        {
            int dmg = u.dmgReceived;

            //check for route
            if (dmg >= u.manpower / 2)
            {
                u.routing = true;
            }
            else if (u.morale < 0.5f)
            {
                if(Random.Range(0f, 1f) > u.morale){
                    u.routing = true;
                }
            }

            //apply dead
            if (u.manpower - dmg > 0)
            {
                if (u.attacking)
                {
                    attackerCasualities += dmg;
                }
                else
                {
                    defenderCasualities += dmg;
                }
                u.manpower -= dmg;
            }
            else
            {
                if (u.attacking)
                {
                    attackerCasualities += u.manpower;
                }
                else
                {
                    defenderCasualities += u.manpower;
                }
                u.manpower = 0;
                u.routed = true;
                u.position.contester = null;
                u.position = null;
            }

            //Debug.Log(u.dmgDealt + " / " + u.dmgReceived);
            //apply morale
            if (u.dmgDealt < u.dmgReceived)
            {
                //unit losing
                u.morale -= 0.05f;
            }
        }

        //backline

        //flanks

    }
    public List<Classes.Unit> GetUnits(Grid[,] field, bool attacker)
    {
        List<Classes.Unit> units = new List<Classes.Unit>();
        foreach (Grid grid in frontLine)
        {
            if (grid.contester != null)
            {
                if (grid.contester.attacking && attacker)
                {
                    units.Add(grid.contester);
                }
                else if (!grid.contester.attacking && !attacker)
                {
                    units.Add(grid.contester);
                }
            }
        }
        return units;
    }
}
