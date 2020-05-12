using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Nation
{
    [System.Serializable]
    public class NationAsSaveable
    {
        public List<int> provinces;
        public int power;
        public int man;
        public float wealth;
        public int id;
        public int capital;

        public NationAsSaveable(int id, int power, int man, float wealth, List<Province> provinces, int capital)
        {
            this.power = power;
            this.man = man;
            this.wealth = wealth;
            this.id = id;
            this.capital = capital;

            this.provinces = new List<int>();
            foreach (Province prov in provinces)
            {
                this.provinces.Add(prov.id);
            }
        }
    }

    //Serializable
    public int id;//

    public List<Province> provinces;//
    public Classes.TradeRoute[] tradeRoutes;
    public Classes.ModBook mods;
    public Classes.Relations rel;
    public Classes.FinanseBook expBook;
    public Classes.FinanseBook manBook;

    public int power = 0;//
    public int man = 10000;//
    public float wealth = 100;//


    //Other
    public string name;//
    public Province capital = null;
    public List<Army> armies;
    public Color color;//


    public Nation()
    {
        provinces = new List<Province>();
        rel = new Classes.Relations(this);
        expBook = new Classes.FinanseBook();
        manBook = new Classes.FinanseBook();
        mods = new Classes.ModBook();
        armies = new List<Army>();
    }


    public Nation Restore(NationAsSaveable nation_as, SaveFile saveBase)
    {
        //set base
        name = saveBase.GetNations().Find(x => x.id == nation_as.id).name;
        color = saveBase.GetNations().Find(x => x.id == nation_as.id).color;

        //placeholder
        tradeRoutes = saveBase.GetNations().Find(x => x.id == nation_as.id).tradeRoutes;
        mods = saveBase.GetNations().Find(x => x.id == nation_as.id).mods;
        rel = saveBase.GetNations().Find(x => x.id == nation_as.id).rel;
        expBook = saveBase.GetNations().Find(x => x.id == nation_as.id).expBook;
        manBook = saveBase.GetNations().Find(x => x.id == nation_as.id).manBook;

        //set loaded
        power = nation_as.power;
        man = nation_as.man;
        wealth = nation_as.wealth;
        id = nation_as.id;

        foreach (int id in nation_as.provinces)
        {
            provinces.Add(MapTools.IdToProv(id));
            provinces.ElementAt(provinces.Count - 1).owner = this;
        }

        //set capital
        MapTools.IdToProv(nation_as.capital).DesignateCapital();


        return this;
    }
    public void ChangeWealth(float amount, string name)
    {
        wealth += amount;
        expBook.AddExpense(amount, name);
    }
    public void ChangeManpower(float amount, string name)
    {
        man += (int)amount;
        manBook.AddExpense(amount, name);
    }
    public int CountTradeHubs()
    {
        int centers = 2;
        foreach (Province prov in provinces)
        {
            if (prov == capital)
            {
                centers++;
                prov.mods.AddMod("trade_hub", 1, -1);
            }
            else
            {
                prov.mods.AddMod("trade_hub", 0, -1);
            }
        }
        if (tradeRoutes == null)
        {
            tradeRoutes = new Classes.TradeRoute[centers];
            for (int i = 0; i < tradeRoutes.Length; i++)
            {
                tradeRoutes[i] = new Classes.TradeRoute((int)mods.GetMod("trade_range"), this);
            }
        }
        return centers;
    }
    public void Tick(Classes.TimeAndPace time)
    {
        //reset books
        if (time.day == 1 && time.hour == 0)
        {
            expBook.ResetBook();
        }

        //mods
        if (time.day == 1 && time.hour == 0)
        {
            mods.TickMonth();
        }

        //trade routes
        foreach (Classes.TradeRoute tr in tradeRoutes)
        {
            tr.Tick(time);
        }

        //provinces
        foreach (Province prov in provinces)
        {
            prov.Tick(time);
        }

        //armies
        foreach (Army army in armies)
        {
            army.Tick(time);
        }

        //relations
        rel.TickRelations(time);
    }
    public float BribeSize()
    {
        return expBook.TotalIn() * 12; //years worth of income
    }
    public void AddProvince(Province prov)
    {
        if (!provinces.Contains(prov))
        {
            provinces.Add(prov);
        }
    }
    public float GetProduction(string goodName)
    {
        float prod = 0;
        foreach(Province prov in provinces)
        {
            if (prov.tradeGood.name == goodName)
            {
                prod+=prov.dev*prov.pop*prov.tradeGood.value;
            }
        }
        return prod;
    }
    public NationAsSaveable AsSaveable()
    {
        return new NationAsSaveable(id, power, man, wealth, provinces, capital.id);
    }
}
