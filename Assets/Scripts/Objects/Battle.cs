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
        public Unit contester;
    }
    public class SkirmishRound
    {
        public Classes.FinanseBook defenderCasualities = new Classes.FinanseBook();
        public Classes.FinanseBook attackerCasualities = new Classes.FinanseBook();
        public string name = "";
        public bool attackerInitiates = false;

        public SkirmishRound()
        {

        }
    }
    public int stage = 0;
    public int attackerInitiative = 0;
    public int defenderInitiative = 0;
    public GameObject representation;

    //sides stats
    public int attackerCasualities = 0;
    public int defenderCasualities = 0;
    public Classes.FinanseBook attackerTotal = new Classes.FinanseBook();
    public Classes.FinanseBook defenderTotal = new Classes.FinanseBook();
    public int attackerRouted = 0;
    public int defenderRouted = 0;
    public int attackerFieldedSize = 0;
    public int defenderFieldedSize = 0;
    public int attackerReserveSize = 0;
    public int defenderReserveSize = 0;
    public int attackerCaptured = 0;
    public int defenderCaptured = 0;
    public int attackerWounded = 0;
    public int defenderWounded = 0;
    public float powerBalance = 0.5f;
    public float attackerMorale = 1;
    public float defenderMorale = 1;


    //setting
    public List<Army> defenders;
    public List<Army> attackers;
    public Province location;

    //reserves
    public List<Unit> attackerReserve;
    public List<Unit> defenderReserve;
    public List<Unit> attackerFielded;
    public List<Unit> defenderFielded;


    //battlefield
    public Grid[,] frontLine;
    public Grid[,] leftFlank;
    public Grid[,] rightFlank;
    public Grid[,] attackerBack;
    public Grid[,] defenderBack;


    public Battle(Army attacker, Army defender, Province location)
    {
        attackers = new List<Army>();
        defenders = new List<Army>();
        defenderFielded = new List<Unit>();
        attackerFielded = new List<Unit>();
        defenderReserve = new List<Unit>();
        attackerReserve = new List<Unit>();


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
        foreach (Army attacker in attackers)
        {
            attacker.SetEngaged(true);
            attackerReserve = new List<Unit>();
            attackerReserve.AddRange(attacker.units);
            attackerReserveSize += attacker.CurrentManpower();
        }
        foreach (Army defender in defenders)
        {
            defender.SetEngaged(true);
            defenderReserve = new List<Unit>();
            defenderReserve.AddRange(defender.units);
            defenderReserveSize += defender.CurrentManpower();

        }

        CountAllStats();
        SetBattlefield();
        Tick(0);
    }
    public void EndBattle()
    {
        foreach (Army attacker in attackers)
        {
            attacker.SetEngaged(false);
        }
        foreach (Army defender in defenders)
        {
            defender.SetEngaged(false);
            defender.Route();
        }

        Object.Destroy(representation);
    }
    public bool Tick(int hour)
    {
        CountAllStats();

        if (stage == 0)
        {
            UnitsMovement();
            UnitsRollout();
            ApplyDamage();

            if (hour == 23)
            {
                stage++;
            }
        }
        else if(stage == 1)
        {

        }
        else if(stage == 2)
        {

        }

        return false;
    }
    public void ApplyDamage()
    {
        foreach(Unit u in attackerReserve)
        {
            int dmg = (int)System.Math.Round(u.dmgReceived);
            if (dmg > u.manpower)
            {
                dmg = u.manpower;
            }

            attackerCasualities += dmg;

            u.manpower -= dmg;
        }
        foreach (Unit u in defenderReserve)
        {
            int dmg = (int)System.Math.Round(u.dmgReceived);
            if (dmg > u.manpower)
            {
                dmg = u.manpower;
            }

            defenderCasualities += dmg;

            u.manpower -= dmg;
        }
    }
    public void EngageUnits(Unit attacker, Unit defender)
    {
        float attackerDamage = attacker.GetCombatStrength(stage);
        float defenderDamage = defender.GetCombatStrength(stage);
        float ratio = attackerDamage / defenderDamage;
        attackerDamage *= ratio;

        attacker.dmgDealt = attackerDamage;
        defender.dmgDealt = defenderDamage;
        defender.dmgReceived = attackerDamage;
        attacker.dmgReceived = defenderDamage;
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
        if (width > 40)
        {
            width = 40;
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
        if (flankWidth > 12)
        {
            flankWidth = 12;
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
        frontLine = new Grid[width, 6];
        leftFlank = new Grid[flankWidth, 4];
        rightFlank = new Grid[flankWidth, 4];
        attackerBack = new Grid[width-2, 2];
        defenderBack = new Grid[width-2, 2];

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

    public void UnitsRollout()
    {
        Rollout_Frontline();
        Rollout_Backline();
        Rollout_Flanks();
    }
    public void Rollout_Frontline()
    {
        List<Unit> attRoll = attackerReserve.Where(x => x.type.Equals("inf_skirmish")).OrderBy((x) => (x.manpower)).ToList<Unit>();
        attRoll.AddRange(attackerReserve.Where(x => x.type.Equals("inf_light")).OrderBy((x) => (x.manpower)).ToList<Unit>());
        attRoll.AddRange(attackerReserve.Where(x => x.type.Equals("inf_heavy")).OrderBy((x) => (x.manpower)).ToList<Unit>());

        List<Unit> defRoll = defenderReserve.Where(x => x.type.Equals("inf_skirmish")).OrderBy((x) => (x.manpower)).ToList<Unit>();
        defRoll.AddRange(defenderReserve.Where(x => x.type.Equals("inf_light")).OrderBy((x) => (x.manpower)).ToList<Unit>());
        defRoll.AddRange(defenderReserve.Where(x => x.type.Equals("inf_heavy")).OrderBy((x) => (x.manpower)).ToList<Unit>());

        int frontLength = frontLine.GetLength(0);
        int i = 0;
        int attY = frontLine.GetLength(1) - 1;
        for (int x = frontLength / 2; x < frontLength; x += (int)System.Math.Pow(-1, i) * i)
        {
            if (frontLine[x, 0].contester == null && defRoll.Count > 0)
            {
                PlaceUnit(frontLine[x, 0], defRoll.ElementAt(defRoll.Count - 1));
                defenderReserve.Remove(frontLine[x, 0].contester);
                defRoll.Remove(frontLine[x, 0].contester);
                defenderFielded.Add(frontLine[x, 0].contester);
                defenderFieldedSize += frontLine[x, 0].contester.manpower;
                defenderReserveSize -= frontLine[x, 0].contester.manpower;
            }
            if (frontLine[x, attY].contester == null && attRoll.Count > 0)
            {
                PlaceUnit(frontLine[x, attY], attRoll.ElementAt(attRoll.Count - 1));
                attackerReserve.Remove(frontLine[x, attY].contester);
                attRoll.Remove(frontLine[x, attY].contester);
                attackerFielded.Add(frontLine[x, attY].contester);
                attackerFieldedSize += frontLine[x, attY].contester.manpower;
                attackerReserveSize -= frontLine[x, attY].contester.manpower;
            }
            i++;
        }
    }
    public void Rollout_Backline()
    {
        List<Unit> attRoll = attackerReserve.Where(x => x.type.Equals("art_siege")).OrderBy((x) => (x.manpower)).ToList<Unit>();
        attRoll.AddRange(attackerReserve.Where(x => x.type.Equals("art_heavy")).OrderBy((x) => (x.manpower)).ToList<Unit>());
        attRoll.AddRange(attackerReserve.Where(x => x.type.Equals("art_field")).OrderBy((x) => (x.manpower)).ToList<Unit>());

        List<Unit>  defRoll = defenderReserve.Where(x => x.type.Equals("art_siege")).OrderBy((x) => (x.manpower)).ToList<Unit>();
        defRoll.AddRange(defenderReserve.Where(x => x.type.Equals("art_heavy")).OrderBy((x) => (x.manpower)).ToList<Unit>());
        defRoll.AddRange(defenderReserve.Where(x => x.type.Equals("art_field")).OrderBy((x) => (x.manpower)).ToList<Unit>());

        int frontLength = attackerBack.GetLength(0);
        int i = 0;
        for (int y = 0; y < attackerBack.GetLength(1); y++)
        {
            i = 0;
            for (int x = frontLength / 2; x < frontLength; x += (int)System.Math.Pow(-1, i) * i)
            {
                if (defenderBack[x, defenderBack.GetLength(1) - y - 1].contester == null && defRoll.Count > 0)
                {
                    PlaceUnit(defenderBack[x, defenderBack.GetLength(1) - y - 1], defRoll.ElementAt(defRoll.Count - 1));
                    defenderReserve.Remove(defenderBack[x, defenderBack.GetLength(1) - y - 1].contester);
                    defRoll.Remove(defenderBack[x, defenderBack.GetLength(1) - y - 1].contester);
                    defenderFielded.Add(defenderBack[x, defenderBack.GetLength(1) - y - 1].contester);
                    defenderFieldedSize += defenderBack[x, defenderBack.GetLength(1) - y - 1].contester.manpower;
                    defenderReserveSize -= defenderBack[x, defenderBack.GetLength(1) - y - 1].contester.manpower;
                }
                if (attackerBack[x, y].contester == null && attRoll.Count > 0)
                {
                    PlaceUnit(attackerBack[x, y], attRoll.ElementAt(attRoll.Count - 1));
                    attackerReserve.Remove(attackerBack[x, y].contester);
                    attRoll.Remove(attackerBack[x, y].contester);
                    attackerFielded.Add(attackerBack[x, y].contester);
                    attackerFieldedSize += attackerBack[x, y].contester.manpower;
                    attackerReserveSize -= attackerBack[x, y].contester.manpower;
                }
                i++;
            }
        }
    }
    public void Rollout_Flanks()
    {
        List<Unit> attRoll = attackerReserve.Where(x => x.type.Equals("cav_missile")).OrderBy((x) => (x.manpower)).ToList<Unit>();
        attRoll.AddRange(attackerReserve.Where(x => x.type.Equals("cav_light")).OrderBy((x) => (x.manpower)).ToList<Unit>());
        attRoll.AddRange(attackerReserve.Where(x => x.type.Equals("cav_shock")).OrderBy((x) => (x.manpower)).ToList<Unit>());

        List<Unit> defRoll = defenderReserve.Where(x => x.type.Equals("cav_missile")).OrderBy((x) => (x.manpower)).ToList<Unit>();
        defRoll.AddRange(defenderReserve.Where(x => x.type.Equals("cav_light")).OrderBy((x) => (x.manpower)).ToList<Unit>());
        defRoll.AddRange(defenderReserve.Where(x => x.type.Equals("cav_shock")).OrderBy((x) => (x.manpower)).ToList<Unit>());

        int frontLength = leftFlank.GetLength(0);
        int attY = leftFlank.GetLength(1) - 1;
        int i = 0;
        for (int x = i; i < frontLength;)
        {
            if ((x + i) % 2 == 0)
            {
                if (leftFlank[frontLength - 1 - x, 0].contester == null && defRoll.Count > 0)
                {
                    PlaceUnit(leftFlank[frontLength - x - 1, 0], defRoll.ElementAt(defRoll.Count - 1));
                    defenderReserve.Remove(leftFlank[frontLength - x - 1, 0].contester);
                    defRoll.Remove(leftFlank[frontLength - x - 1, 0].contester);
                    defenderFielded.Add(leftFlank[frontLength - x - 1, 0].contester);
                    defenderFieldedSize += leftFlank[frontLength - x - 1, 0].contester.manpower;
                    defenderReserveSize -= leftFlank[frontLength - x - 1, 0].contester.manpower;
                }
                if (leftFlank[frontLength - 1 - x, attY].contester == null && attRoll.Count > 0)
                {
                    PlaceUnit(leftFlank[frontLength - 1 - x, attY], attRoll.ElementAt(attRoll.Count - 1));
                    attackerReserve.Remove(leftFlank[frontLength - x - 1, attY].contester);
                    attRoll.Remove(leftFlank[frontLength - x - 1, attY].contester);
                    attackerFielded.Add(leftFlank[frontLength - x - 1, attY].contester);
                    attackerFieldedSize += leftFlank[frontLength - x - 1, attY].contester.manpower;
                    attackerReserveSize -= leftFlank[frontLength - x - 1, attY].contester.manpower;
                }
                x++;
            }
            else
            {
                if (rightFlank[i, 0].contester == null && defRoll.Count > 0)
                {
                    PlaceUnit(rightFlank[i, 0], defRoll.ElementAt(defRoll.Count - 1));
                    defenderReserve.Remove(rightFlank[i, 0].contester);
                    defRoll.Remove(rightFlank[i, 0].contester);
                    defenderFielded.Add(rightFlank[i, 0].contester);
                    defenderFieldedSize += rightFlank[i, 0].contester.manpower;
                    defenderReserveSize -= rightFlank[i, 0].contester.manpower;
                }
                if (rightFlank[i, attY].contester == null && attRoll.Count > 0)
                {
                    PlaceUnit(rightFlank[i, attY], attRoll.ElementAt(attRoll.Count - 1));
                    attackerReserve.Remove(rightFlank[i, attY].contester);
                    attRoll.Remove(rightFlank[i, attY].contester);
                    attackerFielded.Add(rightFlank[i, attY].contester);
                    attackerFieldedSize += rightFlank[i, attY].contester.manpower;
                    attackerReserveSize -= rightFlank[i, attY].contester.manpower;
                }
                i++;
            }
        }
    }

    public void UnitsMovement()
    {
        List<Unit> units = new List<Unit>();
        units.AddRange(attackerFielded);
        units.AddRange(defenderFielded);
        foreach (Unit u in units.Where((x) => !x.routing))
        {
            int x = u.position.x;
            int y = u.position.y;
            if (u.type.StartsWith("inf_"))
            {
                Movement_Frontline(u, x, y);
            }
            else if (u.type.StartsWith("cav_"))
            {
                Movement_Flanks(u, x, y);
            }
            else if (u.type.StartsWith("art_"))
            {
                Movement_Backline(u, x, y);
            }
        }
    }
    public void Movement_Frontline(Unit u, int x, int y)
    {
        int dir = 0;
        if (y > frontLine.GetLength(1) / 2)
        {
            dir = -1;
        }
        else if (y < frontLine.GetLength(1) / 2)
        {
            dir = 1;
        }

        if (dir != 0 && frontLine[x, y + dir].contester == null)
        {
            PlaceUnit(frontLine[x, y + dir], u);
        }
        else if (x > frontLine.GetLength(0) / 2 && frontLine[x - 1, y].contester == null)
        {
            PlaceUnit(frontLine[x - 1, y], u);
        }
        else if (x < frontLine.GetLength(0) / 2 && frontLine[x + 1, y].contester == null)
        {
            PlaceUnit(frontLine[x + 1, y], u);
        }
    }
    public void Movement_Backline(Unit u, int x, int y)
    {
        bool att = attackers.Contains(u.owner);

        if (att)
        {
            if (attackerBack[x, System.Math.Max(y - 1, 0)].contester == null)
            {
                PlaceUnit(attackerBack[x, y - 1], u);
            }
            else if (x > attackerBack.GetLength(0) / 2 && attackerBack[x - 1, y].contester == null)
            {
                PlaceUnit(attackerBack[x - 1, y], u);
            }
            else if (x < attackerBack.GetLength(0) / 2 && attackerBack[x + 1, y].contester == null)
            {
                PlaceUnit(attackerBack[x + 1, y], u);
            }
        }
        else
        {
            if (defenderBack[x, System.Math.Min(y + 1, attackerBack.GetLength(1) - 1)].contester == null)
            {
                PlaceUnit(defenderBack[x, y + 1], u);
            }
            else if (x > defenderBack.GetLength(0) / 2 && defenderBack[x - 1, y].contester == null)
            {
                PlaceUnit(defenderBack[x - 1, y], u);
            }
            else if (x < defenderBack.GetLength(0) / 2 && defenderBack[x + 1, y].contester == null)
            {
                PlaceUnit(defenderBack[x + 1, y], u);
            }
        }
    }
    public void Movement_Flanks(Unit u, int x, int y)
    {
        bool left = leftFlank[x, y].contester==u;
        int dir = 0;
        if (y >= frontLine.GetLength(1) / 2)
        {
            dir = -1;
        }
        else if (y < frontLine.GetLength(1) / 2)
        {
            dir = 1;
        }

        if (left)
        {
            if (y+dir<leftFlank.GetLength(1) && y+dir>0 && leftFlank[x, y+dir].contester == null)
            {
                PlaceUnit(leftFlank[x, y + dir], u);
            }
            else if (x + 1 < leftFlank.GetLength(0) && leftFlank[x + 1, y].contester == null)
            {
                PlaceUnit(leftFlank[x + 1, y], u);
            }
        }
        else
        {
            if (y + dir < rightFlank.GetLength(1) && y + dir > 0 && rightFlank[rightFlank.GetLength(0)-1-x, y + dir].contester == null)
            {
                PlaceUnit(rightFlank[rightFlank.GetLength(0) - 1 - x, y + dir], u);
            }
            else if (x-1 > 0 && rightFlank[x - 1, y].contester == null)
            {
                PlaceUnit(rightFlank[x - 1, y], u);
            }
        }
    }

    public void UnitsEngage()
    {
        Engage_Frontline();
        Engage_Backline();
        Engage_Flanks();
    }
    public void Engage_Frontline()
    {

    }
    public void Engage_Backline()
    {

    }
    public void Engage_Flanks()
    {

    }

    public void PlaceUnit(Grid grid, Unit u) {
        grid.contester = u;
        if (u.position != null)
        {
            u.position.contester = null;
        }
        u.position = grid;
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
    public bool IsTerrainValid()
    {

        return true;
    }

    public void CountAllStats()
    {
        CountMorale();
        CountBalance();
    }
    public void CountBalance()
    {
        powerBalance = (float) (attackerReserveSize/2+ attackerFieldedSize) / ((attackerReserveSize/2 + attackerFieldedSize) + (defenderReserveSize/2 + defenderFieldedSize));
    }
    public void CountMorale()
    {
        int man = 1;
        float mor = 1;
        float result = 1;
        foreach (Unit u in defenderFielded)
        {
            mor += u.morale * u.manpower;
            man += u.manpower;
        }
        foreach (Unit u in defenderReserve)
        {
            mor += (u.morale * u.manpower)/2;
            man += u.manpower/2;
        }
        result = mor / man;
        defenderMorale = result;

        man = 1;
        mor = 1;
        result = 1;
        foreach (Unit u in attackerFielded)
        {
            mor += u.morale * u.manpower;
            man += u.manpower;
        }
        foreach (Unit u in attackerReserve)
        {
            mor += (u.morale * u.manpower) / 2;
            man += u.manpower / 2;
        }
        result = mor / man;
        attackerMorale = result;
    }
}
