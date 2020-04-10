using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Province: MonoBehaviour
{
    [Serializable]
    public class ProvinceAsSaveable
    {
        public int id;
        public int pop;
        public int dev;
        public float auto;

        public ProvinceAsSaveable(int id,int pop, int dev, float auto)
        {
            this.id = id;
            this.pop = pop;
            this.dev = dev;
            this.auto = auto;
        }
    }

    //Serializable
    public int id;//
    public Classes.Recruiter recruiter;//
    public Classes.Constructor constructor;//
    public Classes.ModBook mods;//
    public Classes.FinanseBook growthMods;//
    public List<string> traits;//

    public int pop = 1;//
    public int dev = 1;//
    public float auto = 0;//


    //Other
    public Classes.Nation owner;
    public string provName;//
    public List<Province> links;//
    public List<Province> crossings;//
    public Vector2 graphicalCenter;//
    public Vector2 center;//
    public Vector2 graphicalSize;//
    public Classes.ArmyStack armies;//

    public Province Restore(ProvinceAsSaveable province_as, SaveFile saveBase)
    {
        Province basedOn = saveBase.GetProvinces().Find(x => x.id == province_as.id);

        //Base
        armies.GetStack().Clear();

        pop = basedOn.id;
        dev = basedOn.dev;
        auto = basedOn.auto;
        provName = basedOn.provName;
        center = basedOn.center;
        graphicalCenter = basedOn.graphicalCenter;
        graphicalSize = basedOn.graphicalSize;
        links = basedOn.links;
        crossings = basedOn.crossings;

        //placeholder
        recruiter = basedOn.recruiter;
        constructor = basedOn.constructor;
        mods = basedOn.mods;
        traits = basedOn.traits;
        growthMods = basedOn.growthMods;

        //Loaded
        id = province_as.id;

        return this;
    }
    public void Init(Vector2 center, string provName)
    {
        this.center = center;
        transform.position = GameObject.Find("Map/Center").GetComponent<MapHandler>().LocalToScale(center);

        traits = new List<string>();
        links = new List<Province>();
        crossings = new List<Province>();
        mods = new Classes.ModBook();
        growthMods = new Classes.FinanseBook();
        recruiter = new Classes.Recruiter(this);
        constructor = new Classes.Constructor(this);

        this.name = provName;
        armies = new Classes.ArmyStack(this);
        armies.Representate();
    }
    public void RefreshIcon(string mode)
    {
        if (mode.Equals("trade"))
        {
            if (mods.GetMod("trade_hub") == 1)
            {
                GetComponent<MeshRenderer>().material = Resources.Load("Towns/Mat_Hub") as Material;
            }
            else
            {
                GetComponent<MeshRenderer>().material = Resources.Load("Towns/Mat_Town") as Material;
            }
        }
        else
        {
            if (mods.GetMod("capital_town") == 1)
            {
                GetComponent<MeshRenderer>().material = Resources.Load("Towns/Mat_Capital") as Material;
            }
            else if (mods.GetMod("const_fort") > 0)
            {
                GetComponent<MeshRenderer>().material = Resources.Load("Towns/Mat_Fort") as Material;
            }
            else
            {
                GetComponent<MeshRenderer>().material = Resources.Load("Towns/Mat_Town") as Material;
            }
        }
    }
    public bool isAccesibleFor(Classes.Nation nat)
    {
        if (nat == owner)
        {
            return true;
        }
        else if (nat.rel.IsAccesible(owner))
        {
            return true;
        }
        return false;
    }
    public List<Battle> EngageArmies()
    {
        List<Battle> battles = new List<Battle>();
        foreach (Classes.Army attacker in armies.GetStack())
        {
            foreach (Classes.Army defender in armies.GetStack())
            {
                if (attacker.owner != defender.owner)
                {
                    if (!attacker.IsEngaged() && !defender.IsEngaged())
                    {
                        if (!attacker.IsRouting() && !defender.IsRouting())
                        {
                            if (attacker.owner.rel.IsAtWar(defender.owner))
                            {
                                Battle battle = new Battle(attacker, defender, this);
                                battle.StartBattle();
                                if (GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.Contains(attacker) || GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.Contains(defender))
                                {
                                    GameObject.Find("Canvas").GetComponent<InterfaceHandler>().EnableInterface("combat");
                                    GameObject.Find("Canvas").GetComponent<InterfaceHandler>().interface_combat.GetComponent<CombatInterface>().Set(battle: battle);
                                }
                                battles.Add(battle);
                            }
                        }
                    }
                }
            }
        }
        return battles;
    }
    public void TaxIncome()
    {
        float gain = (pop * dev) * (1 - auto);
        owner.ChangeWealth(gain, "Taxation");
    }
    public void ManIncome()
    {
        int gain = (int)(pop * (1 - auto));
        owner.ChangeManpower(gain, "Enlisted");
    }
    public void Grow()
    {
        growthMods.ResetBook();

        //country mod
        growthMods.AddExpense(owner.mods.GetMod("growth_rate"), "Country Base");

        //terrain mods
        if (mods.GetMod("terrain_plains") != -1.234)
        {
            growthMods.AddExpense(0.5f, "Terrain");
        }
        else if (mods.GetMod("terrain_forest") != -1.234)
        {
            growthMods.AddExpense(0.2f, "Terrain");
        }
        else if (mods.GetMod("terrain_hills") != -1.234)
        {
            growthMods.AddExpense(0.2f, "Terrain");
        }
        else if (mods.GetMod("terrain_mountains") != -1.234)
        {
            growthMods.AddExpense(0.5f, "Terrain");
        }

        //constructions mods
        if (mods.GetMod("const_farm") > 0)
        {
            growthMods.AddExpense(0.1f, "Constructions");
        }

        //overcrowding penalty
        float overcrowding = pop / (dev * 10);
        if (overcrowding > 1)
        {
            growthMods.AddExpense(-(overcrowding - 1), "Overcrowding");
            mods.AddMod("overcrowded", 1, 1);
        }
        else

        //capital mod
        if (mods.GetMod("capital_city") == 1)
        {
            growthMods.AddExpense(0.25f, "Capital City");
        }

        //trade
        if (mods.GetMod("trade_routes") > 0)
        {
            growthMods.AddExpense(mods.GetMod("trade_routes") / 10, "Trade routes");
        }
        if (mods.GetMod("trade_hub") == 1)
        {
            growthMods.AddExpense(0.25f, "Trade Hub");
        }

        //apply
        if (UnityEngine.Random.Range(0f, 1f) < growthMods.TotalValue())
        {
            pop++;
        }
    }
    public void Tick(Classes.TimeAndPace time)
    {
        //daily
        if (time.hour == 0)
        {
            recruiter.TickRecruitement();
            constructor.TickConstruction();
        }

        //monthly
        if (time.day == 1 && time.hour == 0)
        {
            UpdateMods();
            TaxIncome();
            ManIncome();
        }
        //yearly
        if (time.month == 0 && time.day == 1 && time.hour == 0)
        {
            Grow();
        }
    }
    public void UpdateMods()
    {
        mods.TickMonth();
    }
    public ProvinceAsSaveable AsSaveable()
    {
        return new ProvinceAsSaveable(id, pop, dev, auto);
    }
    public void DesignateCapital()
    {
        mods.AddMod("capital_town", 1, -1);
        owner.capital = this;
    }
}
