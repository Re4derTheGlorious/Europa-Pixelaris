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
    public float leftFlankBalance = 0;
    public float rightFlankBalance = 0;
    public float attackerMorale = 1;
    public float defenderMorale = 1;


    //setting
    public List<Army> defenders;
    public List<Army> attackers;
    public Province location;
    public int seed;

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

        SetBattlefield();
        CountAllStats();
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
        }

        DisplaySummary();

        Object.Destroy(representation);
    }
    public void DisplaySummary()
    {

    }
    public void Route(Nation nat)
    {

    }
    public bool Tick(int hour)
    {
        CountAllStats();

        if (stage == 0)
        {
            ResetDamage();
            UnitsMovement();
            UnitsRollout();
            UnitsEngagement();
            ApplyMorale();
            ApplyDamage();

            if (IsBattleOver(hour))
            {
                stage += 2;
            }
            else if (IsSkirmishOver(hour))
            {
                stage++;
            }
        }
        else if(stage == 1)
        {
            ResetDamage();
            UnitsMovement();
            UnitsRollout();
            UnitsEngagement();
            ApplyMorale();
            ApplyDamage();

            if (IsBattleOver(hour))
            {
                stage++;
            }
        }
        else if(stage == 2)
        {
            ResetDamage();
            UnitsMovement();
            UnitsEngagement();
            ApplyDamage();

            if (IsRouteOver(hour))
            {
                stage++;
                EndBattle();
            }
        }

        return false;
    }
    public void EngageUnits(Unit attacker, Unit defender)
    {
        float attackerDamage = attacker.GetCombatStrength(this);
        float defenderDamage = defender.GetCombatStrength(this);
        float ratio = attackerDamage / defenderDamage;
        attackerDamage *= ratio;

        attacker.dmgDealt = attackerDamage;
        defender.dmgDealt = defenderDamage;
        defender.dmgReceived = attackerDamage;
        attacker.dmgReceived = defenderDamage;
    }
    public void SetBattlefield()
    {
        //set seed
        seed = (int) System.DateTime.Now.Ticks;
        Random.seed = seed;

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

    public List<Unit> FindNeighbours(Unit u)
    {
        List<Unit> neighbours = new List<Unit>();

        int[] shifts = { -1, 0, 1 };
        foreach (int x in shifts)
        {
            foreach (int y in shifts)
            {
                Unit contester = null;

                try
                {
                    if (u.type.StartsWith("inf_"))
                    {
                        contester = frontLine[u.position.x + x, u.position.y + y].contester;
                    }
                    else if (u.type.StartsWith("cav_"))
                    {
                        if (leftFlank[u.position.x, u.position.y].contester == u)
                        {
                            contester = leftFlank[u.position.x + x, u.position.y + y].contester;

                        }
                        else
                        {
                            contester = rightFlank[u.position.x + x, u.position.y + y].contester;
                        }
                    }
                    else
                    {
                        if (defenderBack[u.position.x, u.position.y].contester == u)
                        {
                            contester = defenderBack[u.position.x + x, u.position.y + y].contester;

                        }
                        else
                        {
                            contester = attackerBack[u.position.x + x, u.position.y + y].contester;
                        }
                    }
                }
                catch (System.IndexOutOfRangeException e)
                {

                }

                if (contester != null && contester != u)
                {

                    if ((attackers.Contains(u.owner) && attackers.Contains(contester.owner)) || (defenders.Contains(u.owner) && defenders.Contains(contester.owner)))
                    {
                        neighbours.Add(contester);
                    }
                }
            }
        }

        return neighbours;
    }

    //Damage
    public void ApplyMorale()
    {
        List<Unit> units = new List<Unit>();
        units.AddRange(attackerFielded);
        units.AddRange(defenderFielded);
        
        foreach (Unit u in units)
        {
            if (!u.routing) {
                //reduce morale
                u.moraleLoss.ResetBook();
                if (u.massiveCasualities)
                {
                    u.moraleLoss.AddExpense(GlobalValues.morale_loss, "Massive casualities");
                }
                if (u.allyRouting)
                {
                    u.moraleLoss.AddExpense(GlobalValues.ally_routing_morale_loss, "Nearby unit routing");
                }
                if (u.allyWiped)
                {
                    u.moraleLoss.AddExpense(GlobalValues.ally_wiped_morale_loss, "Nearby unit wiped out");
                }
                if (u.flanked)
                {
                    u.moraleLoss.AddExpense(GlobalValues.morale_loss, "Attacked from a flank");
                }
                if (u.underFire && !u.underCover)
                {
                    u.moraleLoss.AddExpense(GlobalValues.morale_loss, "Under fire");
                }
                if (u.dmgReceived > 0)
                {
                    u.moraleLoss.AddExpense(GlobalValues.morale_loss, "Damage sustained");
                }
                if (u.manpower < GlobalValues.insufficient_manpower_treshold * u.MaxManpower())
                {
                    u.moraleLoss.AddExpense(GlobalValues.morale_loss, "Insufficient manpower");
                }
                if (attackers.Contains(u.owner))
                {
                    if (leftFlankBalance > 1 - GlobalValues.flank_dominance_treshold)
                    {
                        u.moraleLoss.AddExpense(GlobalValues.morale_loss, "Enemy cavalry dominating (left flank)");
                    }
                    else if (rightFlankBalance > 1 - GlobalValues.flank_dominance_treshold)
                    {
                        u.moraleLoss.AddExpense(GlobalValues.morale_loss, "Enemy cavalry dominating (right flank)");
                    }

                    if (powerBalance < 0.5f - GlobalValues.balance_losing)
                    {
                        u.moraleLoss.AddExpense(GlobalValues.morale_loss*3, "Fatal odds");
                    }
                    else if (powerBalance < 0.5f - GlobalValues.balance_infavourable)
                    {
                        u.moraleLoss.AddExpense(GlobalValues.morale_loss, "Infavourable odds");
                    }
                }
                else
                {
                    if (leftFlankBalance < GlobalValues.flank_dominance_treshold)
                    {
                        u.moraleLoss.AddExpense(GlobalValues.morale_loss, "Enemy cavalry dominating (left flank)");
                    }
                    else if (rightFlankBalance < GlobalValues.flank_dominance_treshold)
                    {
                        u.moraleLoss.AddExpense(GlobalValues.morale_loss, "Enemy cavalry dominating (right flank)");
                    }

                    if (powerBalance > 0.5f + GlobalValues.balance_losing)
                    {
                        u.moraleLoss.AddExpense(GlobalValues.morale_loss*3, "Fatal odds");
                    }
                    else if(powerBalance > 0.5f + GlobalValues.balance_infavourable)
                    {
                        u.moraleLoss.AddExpense(GlobalValues.morale_loss, "Infavourable odds");
                    }
                }


                if (u.dmgReceived > 0)
                {
                    u.morale -= u.moraleLoss.TotalValue();
                }
                if (u.morale < 0)
                {
                    u.morale = 0;
                }

                //check if routes
                if (u.morale < GlobalValues.route_check_treshold)
                {
                    float rng = Random.Range(0, GlobalValues.route_check_treshold);
                    if (rng>=u.morale && u.dmgReceived>0)
                    {
                        u.routing = true;
                        u.launchFloater = true;

                        foreach(Unit neighbour in FindNeighbours(u))
                        {
                            neighbour.allyRouting = true;
                        }
                    }
                }
            }
        }
    }
    public void ApplyDamage()
    {
        foreach (Unit u in attackerFielded)
        {
            int dmg = (int)u.dmgReceived;
            if (dmg > u.manpower)
            {
                dmg = u.manpower;
                foreach (Unit neighbour in FindNeighbours(u))
                {
                    neighbour.allyWiped = true;
                }
                u.launchFloater = true;
                attackerFielded.Remove(u);
                u.position.contester = null;
                u.position = null;
            }

            attackerCasualities += dmg;
            attackerFieldedSize -= dmg;

            u.manpower -= dmg;
        }
        foreach (Unit u in defenderFielded)
        {
            int dmg = (int)u.dmgReceived;
            if (dmg > u.manpower)
            {
                dmg = u.manpower;
                foreach (Unit neighbour in FindNeighbours(u))
                {
                    neighbour.allyWiped = true;
                }
                u.launchFloater = true;
                attackerFielded.Remove(u);
                u.position.contester = null;
                u.position = null;
            }

            defenderCasualities += dmg;
            defenderFieldedSize -= dmg;

            u.manpower -= dmg;
        }
    }
    public void ResetDamage()
    {
        List<Unit> units = new List<Unit>();
        units.AddRange(attackerFielded);
        units.AddRange(defenderFielded);

        foreach (Unit u in units) {
            u.dmgReceived = 0;
            u.dmgDealt = 0;
            u.flanksSecured = false;
            u.underCover = false;
            u.underFire = false;
            u.flanked = false;
            u.massiveCasualities = false;
            u.allyRouting = false;
        }
    }

    //Rolling out
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

    //Movement
    public void UnitsMovement()
    {
        List<Unit> units = new List<Unit>();
        units.AddRange(attackerFielded);
        units.AddRange(defenderFielded);
        foreach (Unit u in units)
        {
            u.prevPosition = null;
            int x = u.position.x;
            int y = u.position.y;
            if (!u.routing)
            {
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
            else
            {
                if (u.type.StartsWith("inf_"))
                {
                    Routing_Frontline(u, x, y);
                }
                else if (u.type.StartsWith("cav_"))
                {
                    Routing_Flanks(u, x, y);
                }
                else if (u.type.StartsWith("art_"))
                {
                    Routing_Backline(u, x, y);
                }
            }
        }
    }
    public void PlaceUnit(Grid grid, Unit u)
    {
        grid.contester = u;
        if (u.position != null)
        {
            u.position.contester = null;
        }
        u.prevPosition = u.position;
        u.position = grid;
    }
    public void Movement_Frontline(Unit u, int x, int y)
    {
        int dir = 0;
        if (y > frontLine.GetLength(1) / 2)
        {
            dir = -1;
        }
        else if (y < frontLine.GetLength(1) / 2-1)
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
        if (y > leftFlank.GetLength(1) / 2)
        {
            dir = -1;
        }
        else if (y < leftFlank.GetLength(1) / 2-1)
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
            if (y + dir < rightFlank.GetLength(1) && y + dir > 0 && rightFlank[x, y + dir].contester == null)
            {
                PlaceUnit(rightFlank[x, y + dir], u);
            }
            else if (x-1 > 0 && rightFlank[x - 1, y].contester == null)
            {
                PlaceUnit(rightFlank[x - 1, y], u);
            }
        }
        }
    public void Routing_Frontline(Unit u, int x, int y) {
        int dir = 1;
        if (y < frontLine.GetLength(1) / 2)
        {
            dir = -1;
        }

        if(y+dir>=0 && y + dir < frontLine.GetLength(1))
        {
            Unit contester = frontLine[x, y + dir].contester;
            if (contester == null)
            {
                PlaceUnit(frontLine[x, y + dir], u);
            }
            else
            {
                PlaceUnit(frontLine[x, y], frontLine[x, y + dir].contester);
                PlaceUnit(frontLine[x, y + dir], u);
            }
        }
        else
        {
            u.position.contester = null;
            if (attackers.Contains(u.owner))
            {
                attackerRouted += u.manpower;
                attackerFieldedSize -= u.manpower;
                attackerFielded.Remove(u);
            }
            else
            {
                defenderRouted += u.manpower;
                defenderFieldedSize -= u.manpower;
                defenderFielded.Remove(u);
            }
        }
    }
    public void Routing_Flanks(Unit u, int x, int y) { }
    public void Routing_Backline(Unit u, int x, int y) { }

    //Phases
    public bool IsSkirmishOver(int hour)
    {
        if (hour == 23)
        {
            return true;
        }
        return false;
    }
    public bool IsBattleOver(int hour)
    {
        bool isOver = true;

        foreach(Unit u in attackerFielded)
        {
            if (!u.routing)
            {
                isOver = false;
            }
        }
        if (isOver)
        {
            return true;
        }
        isOver = true;
        foreach(Unit u in defenderFielded)
        {
            if (!u.routing)
            {
                isOver = false;
            }
        }
        return isOver;
    }
    public bool IsRouteOver(int hour)
    {
        if(attackerFieldedSize==0 || defenderFieldedSize == 0)
        {
            return true;
        }
        return false;
    }

    //Engaging
    public void UnitsEngagement()
    {
        //frontline
        List<Unit> units = attackerFielded.Where((x) => x.type.StartsWith("inf_")).ToList<Unit>();
        units.AddRange(defenderFielded.Where((x) => x.type.StartsWith("inf_")));
        Engage_Field(frontLine, units);

        //backlines
        units = attackerFielded.Where(x => x.type.StartsWith("art_")).ToList<Unit>();
        units.AddRange(defenderFielded.Where((x) => x.type.StartsWith("art_")));
        Engage_Backline(frontLine, units);

        //flanks
        int left = 0;
        int right = 0;

        if(leftFlankBalance <= GlobalValues.flank_dominance_treshold)
        {
            left = -1;
        }
        else if(leftFlankBalance >= 1 - GlobalValues.flank_dominance_treshold)
        {
            left = 1;
        }
        if (rightFlankBalance <= GlobalValues.flank_dominance_treshold)
        {
            right = -1;
        }
        else if (rightFlankBalance >= 1 - GlobalValues.flank_dominance_treshold)
        {
            right = 1;
        }

        units = attackerFielded.Where((x) => x.type.StartsWith("cav_") && x.position.contester==leftFlank[x.position.x,x.position.y].contester).ToList<Unit>();
        units.AddRange(defenderFielded.Where((x) => x.type.StartsWith("cav_") && x.position.contester == leftFlank[x.position.x, x.position.y].contester));
        if (left!=0 || right!=0)
        {
            if (left == right)
            {
                if (left == 1) {
                    Engage_Flank(defenderBack, units);
                }
                else
                {
                    Engage_Flank(attackerBack, units);
                }
            }
            else {
                Engage_Flank(frontLine, units);
            }
        }
        else
        {
            Engage_Field(leftFlank, units);
        }

        units = attackerFielded.Where((x) => x.type.StartsWith("cav_") && x.position.contester == rightFlank[x.position.x, x.position.y].contester).ToList<Unit>();
        units.AddRange(defenderFielded.Where((x) => x.type.StartsWith("cav_") && x.position.contester == rightFlank[x.position.x, x.position.y].contester));
        if (left != 0 || right != 0)
        {
            if (left == right)
            {
                if (left == 1)
                {
                    Engage_Flank(defenderBack, units);
                }
                else
                {
                    Engage_Flank(attackerBack, units);
                }
            }
            else {
                Engage_Flank(frontLine, units);
            }
        }
        else
        {
            Engage_Field(rightFlank, units);
        }
    }
    public void ResolveEngagement(Unit attacker, Unit defender, int range, int flank)
    {
        float attStr = attacker.GetCombatStrength(this);
        float defStr = defender.GetCombatStrength(this);

        //flanking
        if(range==1 && flank!=0)
        {
            defender.flanked = true;
        }

        //ranged
        if (range > 1)
        {
            defender.underFire = true;
        }

        //damage
        float dmg_mod = attStr / defStr;
        int att_dealt = (int) System.Math.Round(dmg_mod * attacker.strengthBook.mods.GetMod("base"));

        if (att_dealt > defender.manpower || att_dealt<0)
        {
            att_dealt = defender.manpower;
        }

        attacker.targeting = defender;
        attacker.dmgDealt += att_dealt;
        defender.targetedBy.Add(attacker);
        defender.dmgReceived += attacker.dmgDealt;
    }
    public void Engage_Field(Grid[,] field, List<Unit> units)
    {
        //clear stats
        units.ForEach((x) => { x.targeting = null; x.targetedBy.Clear(); });

        foreach (Unit u in units.Where((x) => !x.routing))
        {
            //range
            int range = 1;
            if (u.type.Equals("inf_skirmish") || u.type.Equals("cav_missile"))
            {
                range = 3;
            }

            //direction
            int dir = 1;

            if (u.position.y >= field.GetLength(1) / 2)
            {
                dir = -1;
            }

            //find
            List<int> orderOfSearch = new List<int>() { 0, -1, 1 };
            if (u.position.x < field.GetLength(0) / 2)
            {
                orderOfSearch = new List<int> { 0, 1, -1 };
            }

            u.targeting = null;
            for (int y = u.position.y+dir; y >= 0 && y < field.GetLength(1) && System.Math.Abs(u.position.y-y)<=range && u.targeting==null; y += dir)
            {
                foreach (int offX in orderOfSearch)
                {
                    if(u.position.x + offX >= 0 && u.position.x + offX < field.GetLength(0)) { 
                        Unit contester = field[u.position.x+offX, y].contester;
                        if (contester != null)
                        {
                            Unit inFronter = field[u.position.x + offX, y - dir].contester;
                            if (attackers.Contains(contester.owner) && !attackers.Contains(u.owner))
                            {
                                if (System.Math.Abs(u.position.y - y)<=1 || inFronter==null || (attackers.Contains(contester.owner) && attackers.Contains(inFronter.owner)) ||(defenders.Contains(contester.owner) && defenders.Contains(inFronter.owner)))
                                {
                                    ResolveEngagement(u, contester, System.Math.Abs(u.position.y - y), offX);
                                    break;
                                }
                            }
                            else if (defenders.Contains(contester.owner) && !defenders.Contains(u.owner))
                            {
                                if (System.Math.Abs(u.position.y - y) <= 1 || inFronter == null || (attackers.Contains(contester.owner) && attackers.Contains(inFronter.owner)) || (defenders.Contains(contester.owner) && defenders.Contains(inFronter.owner)))
                                {
                                    ResolveEngagement(u, contester, System.Math.Abs(u.position.y - y), offX);
                                    break;

                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public void Engage_Flank(Grid[,] field, List<Unit> units)
    {
        units.ForEach((x) => { x.targeting = null; x.targetedBy.Clear(); });


        if (field[0, 0]==defenderBack[0, 0])
        {
            if (!defenderFielded.Exists(x => x.type.StartsWith("art_")))
            {
                Engage_Flank(frontLine, units);
                return;
            }
        }
        else if (field[0, 0] == attackerBack[0, 0])
        {
            if (!attackerFielded.Exists(x => x.type.StartsWith("art_")))
            {
                Engage_Flank(frontLine, units);
                return;
            }
        }

        int dirX = -1;
        int dirY = -1;
        int startX = field.GetLength(0) - 1;
        int startY = field.GetLength(1) / 2;
        if (units != null && units.Count > 0) {
            if (units.ElementAt(0).position == leftFlank[units.ElementAt(0).position.x, units.ElementAt(0).position.y]) {
                dirX = 1;
                startX = 0;
            }
            if (attackers.Contains(units.ElementAt(0).owner))
            {
                startY = field.GetLength(1) / 2-1;
                dirY = -1;
            }
        }

        foreach (Unit u in units.Where((x) => !x.routing))
        {
            for(int x = startX; x<field.GetLength(0) && x>=0 && u.targeting==null; x += dirX)
            {
                for(int y = startY; y>=0 && y<field.GetLength(1); y += dirY)
                {
                    Unit contester = field[x, y].contester;
                    if (contester != null)
                    {
                        ResolveEngagement(u, contester, 0, 1);
                        break;
                    }
                }
            }
        }
    }
    public void Engage_Backline(Grid[,] field, List<Unit> units)
    {
        units.ForEach((x) => { x.targeting = null; x.targetedBy.Clear(); });

        foreach (Unit u in units.Where((x) => !x.routing))
        {
            int range = 0;
            if (u.type.Equals("art_field"))
            {
                range = (int)u.owner.owner.mods.GetMod("art_field_range");
            }
            else if (u.type.Equals("art_heavy"))
            {
                range = (int)u.owner.owner.mods.GetMod("art_heavy_range");
            }

            int dir = 1;
            int offX = 1;
            int offY = 0;
            if (attackers.Contains(u.owner))
            {
                dir = -1;
                offY = -1;
            }
            if (u.position.x < field.GetLength(0))
            {
                offX = -1;

            }

            int i = 0;
            for (int y = field.GetLength(1)/2+offY; u.targeting == null && y >= 0 && y < field.GetLength(1); y += dir)
            {
                i = 0;
                for (int x = u.position.x - offX; System.Math.Abs((u.position.x - offX)-x)<=range; x += (int)System.Math.Pow(-1, i) * i)
                {
                    if (x < field.GetLength(0) && x >= 0) {
                        Unit contester = field[x, y].contester;
                        if (contester != null)
                        {
                            ResolveEngagement(u, contester, 999, 0);
                            break;
                        }
                    }
                    i++;
                }
            }
        }
    }

    //Stats
    public void CountAllStats()
    {
        CountMorale();
        CountBalance();
    }
    public void CountBalance()
    {
        powerBalance = (float) (attackerReserveSize/2+ attackerFieldedSize) / ((attackerReserveSize/2 + attackerFieldedSize) + (defenderReserveSize/2 + defenderFieldedSize));

        float leftAttPower = 1;
        float leftDefPower = 1;
        float rightAttPower = 1;
        float rightDefPower = 1;
        for (int x = 0; x < leftFlank.GetLength(0); x++)
        {
            for (int y = 0; y < leftFlank.GetLength(1); y++)
            {
                if (leftFlank[x, y].contester != null)
                {
                    if (attackers.Contains(leftFlank[x, y].contester.owner))
                    {
                        leftAttPower += leftFlank[x, y].contester.manpower;
                    }
                    else
                    {
                        leftDefPower += leftFlank[x, y].contester.manpower;
                    }
                }
                if (rightFlank[x, y].contester != null)
                {
                    if (attackers.Contains(rightFlank[x, y].contester.owner))
                    {
                        rightAttPower += rightFlank[x, y].contester.manpower;
                    }
                    else
                    {
                        rightDefPower += rightFlank[x, y].contester.manpower;
                    }
                }
            }
        }
        leftFlankBalance = leftAttPower / (leftAttPower + leftDefPower);
        rightFlankBalance = rightAttPower / (rightAttPower + rightDefPower);
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
