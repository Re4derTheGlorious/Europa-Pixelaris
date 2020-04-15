using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit
{
    [System.Serializable]
    public class UnitAsSaveable
    {
        public string type;
        public int manpower;
        public float morale;
        public int owner;
        public string name;

        public UnitAsSaveable(string type, int manpower, float morale, int owner, string name)
        {
            this.type = type;
            this.manpower = manpower;
            this.morale = morale;
            this.owner = owner;
            this.name = name;
        }
    }

    //Serializable
    public string type = "";//
    public int manpower;//
    public float morale;//
    public Army owner;//
    public string unitName = "";//


    //Other
    public bool ranged = false;
    public bool longRanged = false;
    public Battle.Grid position;
    public Battle.Grid prevPosition;
    public float dmgDealt;
    public float dmgReceived;
    public bool routing = false;
    public List<Unit> targetedBy = new List<Unit>();
    public Unit targeting;
    public BattleGrid rep;


    public Unit Restore(UnitAsSaveable unit_as, SaveFile saveBase)
    {
        type = unit_as.type;
        manpower = unit_as.manpower;
        morale = unit_as.morale;
        owner = GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetArmies().Find(x => x.id == unit_as.owner);
        owner.AddUnit(this);
        unitName = unit_as.name;
        return this;
    }
    public int MaxManpower()
    {
        int regimentSize = 0;
        if (type.StartsWith("inf_"))
        {
            regimentSize = (int)owner.owner.mods.GetMod("inf_regiment_size");
        }
        else if (type.StartsWith("cav_"))
        {
            regimentSize = (int)owner.owner.mods.GetMod("cav_regiment_size");
        }
        else if (type.StartsWith("art_"))
        {
            regimentSize = (int)owner.owner.mods.GetMod("art_regiment_size");
        }
        return regimentSize;
    }
    public float GetCombatStrength(int stage)
    {
        float strength = 1;
        float strengthMod = 1;

        Classes.FinanseBook strengthBook = new Classes.FinanseBook();

        //stage mods
        if (stage == 0)
        {
            if (type.Equals("inf_light") || type.Equals("cav_light"))
            {
                strengthBook.AddExpense(-0.5f, "Non skirmish unit on skirmish stage");
            }
            else if (type.Equals("inf_heavy") || type.Equals("cav_shock"))
            {
                strengthBook.AddExpense(-0.75f, "Heavy unit on skirmish stage");
            }
            else if (type.StartsWith("art_"))
            {
                strengthBook.AddExpense(-1f, "Artillery unit on skirmish stage");
            }
        }
        else if (stage == 1)
        {
            if (type.Equals("inf_skirmish") || type.Equals("cav_missile"))
            {
                strengthBook.AddExpense(-0.5f, "Skirmish unit on battle stage");
            }
        }
        else if (stage == 2)
        {
            if (type.Equals("inf_heavy") || type.Equals("cav_shock"))
            {
                strengthBook.AddExpense(-0.75f, "Heavy unit on disengagement stage");
            }
            else if (type.StartsWith("art_"))
            {
                strengthBook.AddExpense(-1f, "Artillery unit on disengagement stage");
            }
        }

        if (1 + strengthBook.TotalValue() <= 0.1f)
        {
            return 0.1f;
        }
        strengthMod = (1 + strengthBook.TotalValue());
        strength = strengthMod * manpower;
        return strength;
    }
    public void ResetCombatInfo()
    {

    }
    public UnitAsSaveable AsSaveable()
    {
        return new UnitAsSaveable(type, manpower, morale, owner.id, unitName);
    }
    public void GenerateName(Province prov, Army arm)
    {
        unitName = "";
        if (prov != null)
        {
            unitName += prov.name;
        }
        else if (arm != null)
        {
            if (arm.location.owner == arm.owner)
            {
                unitName += arm.location.name;
            }
            else
            {
                unitName += "Auxilary";
            }
        }

        if (type.StartsWith("inf_"))
        {
            unitName += " Infantry Regiment";
        }
        else if (type.StartsWith("cav_"))
        {
            unitName += " Cavalry Squad";
        }
        else if (type.StartsWith("art_"))
        {
            unitName += " Siege Team";
        }

        if (unitName.Equals(""))
        {
            unitName = "Unit";
        }
    }

    public Unit(string type, int size, Army owner, float morale = 0)
    {
        this.manpower = size;
        this.type = type;
        this.owner = owner;
        this.morale = morale;

        GenerateName(null, owner);
    }
}
