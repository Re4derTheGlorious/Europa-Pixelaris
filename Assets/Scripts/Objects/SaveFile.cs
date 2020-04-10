using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveFile
{
    //save info
    [SerializeField]
    private string checksum;
    [SerializeField]
    private string version;

    //game info
    [SerializeField]
    private bool IronMan;

    //enviroment
    private List<Province> provinces;
    [SerializeField]
    public List<Province.ProvinceAsSaveable> provinces_as;

    private List<Classes.Nation> nations;
    [SerializeField]
    public List<Classes.Nation.NationAsSaveable> nations_as;

    private List<Battle> battles;
    private List<Classes.War> wars;

    private List<Classes.Army> armies;
    [SerializeField]
    public List<Classes.Army.ArmyAsSaveable> armies_as;
    [SerializeField]
    public List<Classes.Unit.UnitAsSaveable> units_as;

    //time
    [SerializeField]
    private Classes.TimeAndPace time;

    //active entities
    private Classes.Nation activeNation;
    [SerializeField]
    public int activeNation_as;

    public SaveFile()
    {
        time = new Classes.TimeAndPace();
        wars = new List<Classes.War>();
        battles = new List<Battle>();
        provinces = new List<Province>();
        nations = new List<Classes.Nation>();
        armies = new List<Classes.Army>();
    }

    public void Restore(SaveFile saveBase)
    {
        //Provinces
        foreach(Province.ProvinceAsSaveable prov in provinces_as)
        {
            Province newProvince = saveBase.GetProvinces().Find(x => x.id == prov.id).Restore(prov, saveBase);
            newProvince.Restore(prov, saveBase);
            provinces.Add(newProvince);
        }

        //Nations
        foreach(Classes.Nation.NationAsSaveable nat in nations_as)
        {
            Classes.Nation newNation = new Classes.Nation().Restore(nat, saveBase);
            nations.Add(newNation);
        }

        //Armies
        foreach (Classes.Army.ArmyAsSaveable a in armies_as)
        {
            Classes.Army newArmy = new Classes.Army(GameObject.Find("Map/Center").GetComponent<MapHandler>().IdToProv(a.location), GameObject.Find("Map/Center").GetComponent<MapHandler>().IdToNat(a.owner)).Restore(a, saveBase);
            armies.Add(newArmy);
        }

        //Units
        foreach(Classes.Unit.UnitAsSaveable u in units_as)
        {
            new Classes.Unit("", 0, null).Restore(u, saveBase);
        }

        activeNation = GameObject.Find("Map/Center").GetComponent<MapHandler>().IdToNat(activeNation_as);

        Clear();
    }
    public void Prepare()
    {
        provinces_as = new List<Province.ProvinceAsSaveable>();
        nations_as = new List<Classes.Nation.NationAsSaveable>();
        armies_as = new List<Classes.Army.ArmyAsSaveable>();
        units_as = new List<Classes.Unit.UnitAsSaveable>();
        foreach(Province prov in provinces)
        {
            provinces_as.Add(prov.AsSaveable());
        }
        foreach(Classes.Nation nat in nations)
        {
            nations_as.Add(nat.AsSaveable());
        }
        foreach (Classes.Army a in armies)
        {
            armies_as.Add(a.AsSaveable());
            foreach(Classes.Unit u in a.units)
            {
                units_as.Add(u.AsSaveable());
            }
        }
        activeNation_as = activeNation.id;
        version = Application.version;

        //checksum
        checksum = new SaveManager().CalculateChecksum();
    }
    public void Clear()
    {
        provinces_as.Clear();
        nations_as.Clear();
        armies_as.Clear();
        units_as.Clear();
        activeNation_as = 0;
    }

    public bool IsIronMan()
    {
        return IronMan;
    }
    public string GetChecksum()
    {
        return checksum;
    }
    public string GetVersion()
    {
        return version;
    }
    public bool IsValid()
    {
        if (version != Application.version)
        {
            return false;
        }
        else if(!checksum.Equals(new SaveManager().CalculateChecksum()))
        {
            return false;
        }
        return true;
    }

    public List<Province> GetProvinces()
    {
        return provinces;
    }
    public List<Classes.Nation> GetNations()
    {
        return nations;
    }
    public List<Battle> GetBattles()
    {
        return battles;
    }
    public List<Classes.War> GetWars()
    {
        return wars;
    }

    public ref Classes.TimeAndPace GetTime()
    {
        return ref time;
    }

    public ref Classes.Nation GetActiveNation()
    {
        return ref activeNation;
    }

    public List<Classes.Army> GetArmies()
    {
        return armies;
    }
}
