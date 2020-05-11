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
    public Battle.Grid position;
    public Battle.Grid prevPosition;
    public float dmgDealt;
    public float dmgReceived;
    public bool routing = false;
    public bool flanked = false;
    public bool flanksSecured = false;
    public bool allyRouting = false;
    public bool allyWiped = false;
    public bool underFire = false;
    public bool underCover = false;
    public bool launchFloater = false;
    public bool massiveCasualities = false;
    public List<Unit> targetedBy = new List<Unit>();
    public Unit targeting;
    public BattleGrid rep;
    public Classes.FinanseBook strengthBook;
    public Classes.FinanseBook moraleLoss = new Classes.FinanseBook();


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
    public float GetCombatStrength(Battle battle)
    {
        float strength = 1;
        float strengthMod = 1;

        if (strengthBook == null)
        {
            strengthBook = new Classes.FinanseBook();
        }
        else
        {
            strengthBook.ResetBook();
        }

        //base
        strengthBook.AddExpense(manpower, "Manpower");

        

        //stage mods
        if (battle.stage == 0)
        {
            if (type.Equals("inf_skirmish") || type.Equals("cav_missile"))
            {
                strengthBook.mods.AddMod("base", GlobalValues.skirmish_unit_mod);
                strengthBook.mods.AddMod("Skirmish unit on skirmish stage", GlobalValues.skirmish_on_skirmish_mod);
            }
            else if (type.Equals("inf_light") || type.Equals("cav_light"))
            {
                strengthBook.mods.AddMod("base", GlobalValues.light_unit_mod);
                strengthBook.mods.AddMod("Light unit on skirmish stage", GlobalValues.light_on_skirmish_mod);
            }
            else if (type.Equals("inf_heavy") || type.Equals("cav_shock"))
            {
                strengthBook.mods.AddMod("base", GlobalValues.heavy_unit_mod);
                strengthBook.mods.AddMod("Heavy unit on skirmish stage", GlobalValues.heavy_on_skirmish_mod);
            }
            else if (type.StartsWith("art_"))
            {
                strengthBook.AddExpense(GlobalValues.art_on_skirmish, "Artillery unit on skirmish stage");
            }
        }
        else if (battle.stage == 1)
        {
            if (type.Equals("inf_skirmish") || type.Equals("cav_missile"))
            {
                strengthBook.mods.AddMod("base", GlobalValues.skirmish_unit_mod);
                strengthBook.mods.AddMod("Skirmish unit on battle stage", GlobalValues.skirmish_on_battle_mod);
            }
            else if (type.Equals("inf_light") || type.Equals("cav_light"))
            {
                strengthBook.mods.AddMod("base", GlobalValues.light_unit_mod);
                strengthBook.mods.AddMod("Light unit on battle stage", GlobalValues.light_on_battle_mod);
            }
            else if (type.Equals("inf_heavy") || type.Equals("cav_shock"))
            {
                strengthBook.mods.AddMod("base", GlobalValues.heavy_unit_mod);
                strengthBook.mods.AddMod("Heavy unit on battle stage", GlobalValues.heavy_on_battle_mod);
            }
            else if (type.StartsWith("art_"))
            {
                strengthBook.AddExpense(GlobalValues.art_on_battle, "Artillery unit on battle stage");
            }
        }
        else if (battle.stage == 2)
        {
            if (type.Equals("inf_skirmish") || type.Equals("cav_missile"))
            {
                strengthBook.mods.AddMod("base", GlobalValues.skirmish_unit_mod);
                strengthBook.mods.AddMod("Skirmish unit on chase stage", GlobalValues.skirmish_on_chase_mod);
            }
            else if (type.Equals("inf_light") || type.Equals("cav_light"))
            {
                strengthBook.mods.AddMod("base", GlobalValues.light_unit_mod);
                strengthBook.mods.AddMod("Light unit on chase stage", GlobalValues.light_on_chase_mod);
            }
            else if (type.Equals("inf_heavy") || type.Equals("cav_shock"))
            {
                strengthBook.mods.AddMod("base", GlobalValues.heavy_unit_mod);
                strengthBook.mods.AddMod("Heavy unit on chase stage", GlobalValues.heavy_on_chase_mod);
            }
            else if (type.StartsWith("art_"))
            {
                strengthBook.AddExpense(GlobalValues.art_on_chase, "Artillery unit on chase stage");

            }
        }

        if (routing)
        {
            strengthBook.mods.AddMod("Unit routing", 0);
        }
        if (flanksSecured) {
            strengthBook.mods.AddMod("Flanks secured", GlobalValues.flanks_secured_boost);
        }

        return strengthBook.TotalValue();
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
