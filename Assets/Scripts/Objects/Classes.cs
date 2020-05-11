using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Pathfinding;

public class Classes : MonoBehaviour
{
    [System.Serializable]
    public class TradeRoute
    {
        public List<Province> route;
        public List<GameObject> arrows;
        public Nation owner;
        public GameObject rep;

        public TradeRoute(int tradeRange, Nation owner)
        {
            route = new List<Province>();
            arrows = new List<GameObject>();
            this.owner = owner;
        }

        public void MarkProvinces(bool marked)
        {
            int s = -1;
            if (marked)
            {
                s = 1;
            }
            foreach (Province prov in route)
            {
                if (prov != null)
                {
                    prov.mods.AddToMod("trade_route", s, 1);
                }
            }
        }
        public GameObject Representate(GameObject target)
        {
            rep = Instantiate(Resources.Load("Prefabs/UI_Selector") as GameObject, target.transform);
            rep.GetComponent<TradeRouteButton>().owner = this;
            return rep;
        }
        public void Tick(TimeAndPace time)
        {
            //monthly
            if (time.day == 1 && time.hour == 0)
            {
                if (IsValid())
                {
                    MarkProvinces(true);
                    for (int i = route.Count - 1; i >= 0; i--)
                    {
                        float loss = ValueAt(i);
                        if (route.ElementAt(i).owner != owner)
                        {
                            route.ElementAt(i).owner.ChangeWealth(loss, "Trade Duties");
                        }
                    }
                    owner.ChangeWealth(ValueAt(0), "Trade Routes");
                }
            }
        }
        public void SetVisible(bool visible)
        {
            foreach (GameObject arr in arrows)
            {
                if (arr != null)
                {
                    arr.SetActive(visible);
                }
            }
        }
        public void ClearArrows()
        {
            foreach(GameObject arr in arrows)
            {
                Destroy(arr);
            }
            arrows.Clear();
        }
        public void PaintRoute()
        {
            for(int i = route.Count-1; i>0; i--)
            {
                GameObject arr = GameObject.Instantiate(MapTools.GetMap().arrowPrefab, GameObject.Find("Map/Arrows/TradeRoutes").transform);
                arr.GetComponent<ArrowPlacer>().Place(MapTools.LocalToScale(route.ElementAt(i).center), MapTools.LocalToScale(route.ElementAt(i-1).center));
                arrows.Add(arr);
            }
        }
        public float ValueAt(int index)
        {

            float value = 0;

        //    if (RouteSize() > 1)
        //    {
        //        value = route[RouteSize() - 1].pop * route[RouteSize() - 1].dev;
        //    }

        //    if (index < RouteSize() && index >= 0)
        //    {
        //        for (int i = RouteSize() - 1; i >= index; i--)
        //        {
        //            if (route[i].owner != route[0].owner)
        //            {
        //                value -= value * route[i].owner.mods.GetMod("trade_duties");
        //            }
        //            else
        //            {
        //                value -= value * route[0].owner.mods.GetMod("trade_decay");
        //            }
        //        }
        //    }
        //    else if (index >= RouteSize())
        //    {
        //        return value;
        //    }
        //    else if (index >= 0)
        //    {
        //        return 0;
        //    }
            return value;
        }
        public bool IsValid()
        {
            if (route.Count <= 1)
            {
                return false;
            }
            else if (route.ElementAt(route.Count-1).owner == route.ElementAt(0).owner)
            {
                return false;
            }
            else if (route.ElementAt(0).mods.GetMod("trade_hub") != 1)
            {
                return false;
            }
            else if (route.ElementAt(route.Count-1).mods.GetMod("trade_hub") != 1)
            {
                return false;
            }
            return true;
        }
        public void StartRoute(Province prov)
        {
            GameObject arr = GameObject.Instantiate(MapTools.GetMap().arrowPrefab, GameObject.Find("Map/Arrows/TradeRoutes").transform);
            arr.GetComponent<ArrowPlacer>().PlaceTail(MapTools.LocalToScale(prov.center));
            arrows.Add(arr);
            route.Add(prov);
        }
        public void SetRoute(List<Province> path)
        {
            route = path;
            ClearArrows();
            PaintRoute();
        }
        public void ClearRoute()
        {
            if (route.Count <= 1)
            {
                foreach(GameObject arr in arrows)
                {
                    Destroy(arr);
                }
                arrows.Clear();
                route.Clear();
            }
            else
            {
                Province tmp = route.ElementAt(0);
                foreach (GameObject arr in arrows)
                {
                    Destroy(arr);
                }
                arrows.Clear();
                route.Clear();
                StartRoute(tmp);
            }
        }
    }
    [System.Serializable]
    public class TradeGood
    {
        public string name;
        public double value;
        public double bonus;

        public TradeGood()
        {

        }
    }
    [System.Serializable]
    public class TimeAndPace
    {
        public int startYear;
        public int year;
        public int month;
        public int day;
        public int hour;
        private int pace = 0;

        [SerializeField]
        private int daysPerMonth = 30;
        [SerializeField]
        private int monthsPerYear = 12;
        private bool paused = false;

        public float lastTick = 0;
        public float lastHourlyTick = 0;

        public TimeAndPace()
        {
            startYear = 1414;
            year = 0;
            month = 0;
            day = 1;
            hour = 0;
        }

        public void TickDay()
        {
            day++;
            if (day > daysPerMonth)
            {
                day = 1;
                month++;
                if (month >= monthsPerYear)
                {
                    month = 0;
                    year++;
                }
            }
        }
        public string GetMonthName()
        {
            if (month % 12 == 0)
            {
                return "January";
            }
            else if (month % 12 == 1)
            {
                return "February";
            }
            else if (month % 12 == 2)
            {
                return "March";
            }
            else if (month % 12 == 3)
            {
                return "April";
            }
            else if (month % 12 == 4)
            {
                return "May";
            }
            else if (month % 12 == 5)
            {
                return "June";
            }
            else if (month % 12 == 6)
            {
                return "July";
            }
            else if (month % 12 == 7)
            {
                return "August";
            }
            else if (month % 12 == 8)
            {
                return "September";
            }
            else if (month % 12 == 9)
            {
                return "October";
            }
            else if (month % 12 == 10)
            {
                return "November";
            }
            else if (month % 12 == 11)
            {
                return "December";
            }
            return "Wrong month";
        }
        public void Faster()
        {
            if (!paused)
            {
                pace++;
                if (pace > 4)
                {
                    pace = 4;
                }
            }
        }
        public void Slower()
        {
            if (!paused)
            {
                pace--;
                if (pace < 0)
                {
                    pace = 0;
                }
            }
        }
        public void Pause()
        {
            paused = !paused;
        }
        public bool IsPaused()
        {
            return paused;
        }
        public int GetPace()
        {
            if (paused)
            {
                return 0;
            }
            else
            {
                return pace;
            }
        }
        public int GetPaceAbsolute()
        {
            return pace;
        }
        public void SetPace(int pace)
        {
            this.pace = pace;
        }
    }
    [System.Serializable]
    public class Relations
    {
        public Relations(Nation owner) {
            this.owner = owner;
            grantsAcces = new List<Nation>();
            allied = new List<Nation>();
            grantsTrade = new List<Nation>();
            relations = new Dictionary<Nation, ModBook>();
            wars = new List<War>();
        }
        public void TickRelations(TimeAndPace time)
        {
            if(time.day == 1 && time.hour == 0)
            {
                foreach (KeyValuePair<Nation, ModBook> pair in relations)
                {
                    //tick
                    pair.Value.TickMonth();
                    float value = 0;

                    //shared border
                    if (false)
                    {
                        value = -10;
                        pair.Value.AddMod("Border frictions", value, 1);
                    }

                    //War
                    if (IsAtWar(pair.Key))
                    {
                        value = -100;
                        pair.Value.AddMod("Was at war", value, 10 * 12);
                    }

                    //Alliance
                    if (IsAllied(pair.Key))
                    {
                        value = 100;
                        pair.Value.AddMod("Was allied", value, 10 * 12);
                    }

                    //Having trade agreement
                    if (IsTrading(pair.Key))
                    {
                        value = 10;
                        pair.Value.AddMod("Had trade agreement", value, 12);
                    }

                    //Trading
                    value = 10;
                    float amount = 0;
                    foreach (TradeRoute rt in owner.tradeRoutes)
                    {
                        if (rt.route.Count > 1)
                        {
                            if (rt.route.ElementAt(rt.route.Count - 1).owner == pair.Key)
                            {
                                amount++;
                            }
                        }
                    }

                    if (amount > 0)
                    {
                        pair.Value.AddMod("Trading with", value * amount, 1);
                    }

                    //Embargoes

                    //decay due to distance
                }
            }
        }
        public FinanseBook GetRelationToward(Nation nat)
        {
            FinanseBook targetRel = new FinanseBook();
            foreach(KeyValuePair<string, ModBook.Pair> pair in relations[nat].GetMods())
            {
                if (pair.Value.value != 0)
                {
                    targetRel.AddExpense(pair.Value.value, pair.Key);
                }
            }
            return targetRel;
        }
        public string GetTreatiesWith(Nation nat)
        {
            string treaties = "";
            if (IsAtWar(nat))
            {
                treaties += "At war\n";
            }
            if (IsTrading(nat))
            {
                treaties += "Having Trade Agreement\n";
            }
            if (IsAllied(nat))
            {
                treaties += "Allied\n";
            }
            if (IsAccesible(nat))
            {
                treaties += "Having granted acces\n";
            }
            return treaties;
        }
        public void FillRelations(List<Nation> nations)
        {
            foreach(Nation nat in nations)
            {
                if (nat != owner)
                {
                    relations.Add(nat, new ModBook());
                }
            }
        }

        public List<Nation> grantsAcces;
        public List<Nation> allied;
        public List<Nation> grantsTrade;
        public List<War> wars;
        public Dictionary<Nation, ModBook> relations;
        public Nation owner;

        public bool IsTrading(Nation nat)
        {
            if (grantsTrade.Contains(nat))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool IsAccesible(Nation nat)
        {
            if (grantsAcces.Contains(nat) || IsAtWar(nat))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool IsAllied(Nation nat)
        {
            if (allied.Contains(nat))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool IsAtWar(Nation nat)
        {
            foreach (War war in wars)
            {
                if (war.GetEnemiesOf(owner).Contains(nat))
                {
                    return true;
                }
            }
            return false;
        }

        public void ChangeAcces(Nation nat, bool b)
        {
            if (b)
            {
                if (!grantsAcces.Contains(nat))
                {
                    grantsAcces.Add(nat);
                }
            }
            else
            {
                if (grantsAcces.Contains(nat))
                {
                    grantsAcces.Remove(nat);
                }
            }
        }
        public void ChangeTrade(Nation nat, bool b)
        {
            if (b)
            {
                if (!grantsAcces.Contains(nat))
                {
                    grantsTrade.Add(nat);
                }
            }
            else
            {
                if (grantsAcces.Contains(nat))
                {
                    grantsTrade.Remove(nat);
                }
            }
        }
    }
    [System.Serializable]
    public class FinanseBook
    {
        public Dictionary<string, float> exp;
        public ModBook mods;
        public FinanseBook()
        {
            exp = new Dictionary<string, float>();
            mods = new ModBook();
            mods.AddMod("base", 1);
        }
        public void ResetBook()
        {
            exp = new Dictionary<string, float>();
            mods = new ModBook();
            mods.AddMod("base", 1);
        }
        public void AddExpense(float value, string name)
        {
            if (exp.ContainsKey(name))
            {
                exp[name] += value;
            }
            else
            {
                exp.Add(name, value);
            }
        }
        public string GetFinanses()
        {
            Sort();
            bool separated = false;

            string expStr = "In:\n";
            foreach (string key in exp.Keys)
            {
                if(!separated && exp[key] < 0)
                {
                    expStr += "\nOut:\n";
                    separated = true;
                }
                if (exp[key] != 0)
                {
                    expStr += key + " " + exp[key] + "\n";
                }
            }

            if (!separated)
            {
                expStr += "\nOut:\n";
            }

            if (mods.GetMods().Count > 1)
            {
                expStr += "\nModified by:\n";
                foreach(KeyValuePair<string, ModBook.Pair> pair in mods.GetMods())
                {
                    expStr += pair.Key + ": " + pair.Value.value + "\n";
                }
            }

            expStr += "\nTotal:\n";
            expStr += TotalValue();
            return expStr;
        }
        public void Sort()
        {
            exp = exp.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }
        public float TotalValue()
        {
            float total = 0;
            foreach(KeyValuePair<string, float> pair in exp)
            {
                total+=pair.Value;
            }

             float mod = mods.GetMod("base");
            foreach(KeyValuePair<string, ModBook.Pair> pair in mods.GetMods())
            {
                mod *= pair.Value.value;
            }

            return total*mod;
        }
        public float TotalIn()
        {
            float total = 0;
            foreach (KeyValuePair<string, float> pair in exp)
            {
                if (pair.Value > 0)
                {
                    total += pair.Value;
                }
            }
            return total;
        }
        public float TotalOut()
        {
            float total = 0;
            foreach (KeyValuePair<string, float> pair in exp)
            {
                if (pair.Value < 0)
                {
                    total += pair.Value;
                }
            }
            return total;
        }
    }
    [System.Serializable]
    public class ModBook
    {
        [System.Serializable]
        public class ModAsSaveable
        {
            public string name;
            public float value;
            public int remaining;

            public ModAsSaveable(string name, float value, int remaining)
            {
                this.name = name;
                this.value = value;
                this.remaining = remaining;
            }
        }

        public class Pair{
            public float value;
            public int monthsRemaining;
            public Pair(float value, int monthsRemaining)
            {
                this.value = value;
                this.monthsRemaining = monthsRemaining;
            }
        }

        [SerializeField]
        private Dictionary<string, Pair> mods;

        public ModBook()
        {
            mods = new Dictionary<string, Pair>();
        }

        public void AddMod(string name, float value, int monthsRemaining = -1)
        {
            if (mods.ContainsKey(name))
            {
                mods.Remove(name);
                mods.Add(name, new Pair(value, monthsRemaining));
            }
            else
            {
                mods.Add(name, new Pair(value, monthsRemaining));
            }
        }

        public void PassMods(Dictionary<string, Pair> newMods)
        {
            mods = newMods;
        }

        public float GetMod(string name)
        {
            if (mods.ContainsKey(name))
            {
                return mods[name].value;
            }
            else
            {
                //Debug.Log("***No such mod key as " + name + "***");
                return -1.234f;
            }
        }

        public void AddToMod(string type, float value, int monthsRemaining)
        {
            if (mods.ContainsKey(type))
            {
                mods[type].value = mods[type].value + value;
                mods[type].monthsRemaining = monthsRemaining;
            }
            else
            {
                mods.Add(type, new Pair(value, monthsRemaining));
            }
        }

        public void RemoveMod(string type)
        {
            if (mods.ContainsKey(type))
            {
                mods.Remove(type);
            }
        }

        public void TickMonth()
        {
            foreach(KeyValuePair<string, Pair> pair in mods)
            {
                if (pair.Value.monthsRemaining - 1 > 0)
                {
                    pair.Value.monthsRemaining--;
                }
                
                if(pair.Value.monthsRemaining == 0)
                {
                    mods.Remove(pair.Key);
                }
            }
        }

        public void Restore(ModAsSaveable[] modsAS)
        {
            mods.Clear();
            foreach(ModAsSaveable mod in modsAS)
            {
                AddMod(mod.name, mod.value, mod.remaining);
            }
        }
        public List<ModAsSaveable> AsSaveable()
        {
            List <ModAsSaveable> modsAS = new List<ModAsSaveable>();
            foreach(KeyValuePair<string, Pair> pair in mods)
            {
                modsAS.Add(new ModAsSaveable(pair.Key, pair.Value.value, pair.Value.monthsRemaining));
            }
            return modsAS;
        }

        public Dictionary<string, Pair> GetMods()
        {
            return mods;
        }
    }
    [System.Serializable]
    public class Recruiter
    {
        public Recruiter(Army army)
        {
            this.holderArmy = army;
            recruitementQueue = new List<Unit>();
        }
        public Recruiter(Province prov)
        {
            this.holderProv = prov;
            recruitementQueue = new List<Unit>();
        }

        public Nation GetOwnerNation()
        {
            Nation owner = null;
            if (holderProv != null)
            {
                owner = holderProv.owner;
            }
            else if (holderArmy != null)
            {
                owner = holderArmy.owner;
            }
            return owner;
        }
        public void Recruit(string type)
        {
            if ((int)GetOwnerNation().mods.GetMod("regiment_size") <= GetOwnerNation().man){
                int regimentSize = 0;

                if (type.Equals("inf_skirmish"))
                {
                    GetOwnerNation().ChangeWealth(-GetOwnerNation().mods.GetMod("inf_skirmish_cost"), "Recruitement");
                    regimentSize = (int)GetOwnerNation().mods.GetMod("inf_regiment_size");
                }
                else if (type.Equals("inf_light"))
                {
                    GetOwnerNation().ChangeWealth(-GetOwnerNation().mods.GetMod("inf_light_cost"), "Recruitement");
                    regimentSize = (int)GetOwnerNation().mods.GetMod("inf_regiment_size");
                }
                else if (type.Equals("inf_heavy"))
                {
                    GetOwnerNation().ChangeWealth(-GetOwnerNation().mods.GetMod("inf_heavy_cost"), "Recruitement");
                    regimentSize = (int)GetOwnerNation().mods.GetMod("inf_regiment_size");
                }

                else if (type.Equals("cav_missile"))
                {
                    GetOwnerNation().ChangeWealth(-GetOwnerNation().mods.GetMod("cav_missile_cost"), "Recruitement");
                    regimentSize = (int)GetOwnerNation().mods.GetMod("cav_regiment_size");
                }
                else if (type.Equals("cav_light"))
                {
                    GetOwnerNation().ChangeWealth(-GetOwnerNation().mods.GetMod("cav_light_cost"), "Recruitement");
                    regimentSize = (int)GetOwnerNation().mods.GetMod("cav_regiment_size");
                }
                else if (type.Equals("cav_shock"))
                {
                    GetOwnerNation().ChangeWealth(-GetOwnerNation().mods.GetMod("cav_shock_cost"), "Recruitement");
                    regimentSize = (int)GetOwnerNation().mods.GetMod("cav_regiment_size");
                }

                else if (type.Equals("art_field"))
                {
                    GetOwnerNation().ChangeWealth(-GetOwnerNation().mods.GetMod("art_field_cost"), "Recruitement");
                    regimentSize = (int)GetOwnerNation().mods.GetMod("art_regiment_size");
                }
                else if (type.Equals("art_heavy"))
                {
                    GetOwnerNation().ChangeWealth(-GetOwnerNation().mods.GetMod("art_heavy_cost"), "Recruitement");
                    regimentSize = (int)GetOwnerNation().mods.GetMod("art_regiment_size");
                }
                else if (type.Equals("art_siege"))
                {
                    GetOwnerNation().ChangeWealth(-GetOwnerNation().mods.GetMod("art_siege_cost"), "Recruitement");
                    regimentSize = (int)GetOwnerNation().mods.GetMod("art_regiment_size");
                }

                GetOwnerNation().ChangeManpower(-regimentSize, "Recruitement");

                Unit newUnit = new Unit(type, regimentSize, null);
                newUnit.GenerateName(holderProv, holderArmy);

                recruitementQueue.Add(newUnit);
            }
        }
        public void TickRecruitement()
        {
            if (recruitementQueue.Count > 0)
            {
                recruitementProgress += GetOwnerNation().mods.GetMod("recruitement_speed");
                if (recruitementProgress >= 1)
                {
                    if (holderProv != null)
                    {
                        foreach (Army army in holderProv.armies.GetStack())
                        {
                            if (army.owner == GetOwnerNation())
                            {
                                army.AddUnit(recruitementQueue.ElementAt(0));
                                recruitementQueue.RemoveAt(0);
                                recruitementProgress = 0f;
                                if (GameObject.Find("Canvas").GetComponent<InterfaceHandler>().GetActiveInterface().Equals("army"))
                                {
                                    GameObject.Find("Canvas").GetComponent<InterfaceHandler>().RefreshInterface();
                                }
                                return;
                            }
                        }
                        Army newArmy = new Army(holderProv, GetOwnerNation());
                        newArmy.AddUnit(recruitementQueue.ElementAt(0));
                        recruitementQueue.RemoveAt(0);
                        recruitementProgress = 0f;
                    }
                    else if (holderArmy != null)
                    {
                        holderArmy.AddUnit(recruitementQueue.ElementAt(0));
                        recruitementQueue.RemoveAt(0);
                        recruitementProgress = 0f;
                    }
                    if (GameObject.Find("Canvas").GetComponent<InterfaceHandler>().GetActiveInterface().Equals("army"))
                    {
                        GameObject.Find("Canvas").GetComponent<InterfaceHandler>().RefreshInterface();
                    }
                }
            }
        }
        public Army holderArmy;
        public Province holderProv;
        public List<Unit> recruitementQueue;
        public float recruitementProgress = 0;

    }
    [System.Serializable]
    public class Constructor {
        public Constructor(Province prov)
        {
            holder = prov;
            constructionQueue = new List<string>();
        }
        public void Construct(string construction)
        {
            if (holder.mods.GetMod(construction) == -1.234f)
            {
                constructionQueue.Add(construction);
                holder.owner.ChangeWealth(-100, "Construction");
            }
        }
        public void TickConstruction()
        {
            if (constructionQueue.Count > 0)
            {
                constructionProgress += holder.owner.mods.GetMod("construction_speed");
                if (constructionProgress >= 1)
                {
                    holder.mods.AddMod(constructionQueue.ElementAt(0), 1, -1);
                    constructionQueue.RemoveAt(0);
                    holder.RefreshIcon("political");
                    GameObject.Find("Canvas").GetComponent<InterfaceHandler>().RefreshMapMode();
                    constructionProgress = 0;
                }
            }
        }

        [SerializeField]
        private Province holder;
        [SerializeField]
        private List<string> constructionQueue;

        private float constructionProgress = 0;
    }
    [System.Serializable]
    public class ArmyStack{
        public ArmyStack(Province owner)
        {
            this.owner = owner;
            stack = new List<Army>();
        }
        public void Representate()
        {

            rep = Instantiate(Resources.Load("Prefabs/Army") as GameObject, GameObject.Find("Map").transform.Find("Armies"));
            rep.name = owner.name + " STACK";
            rep.GetComponent<UnitHandler>().RelocateTo(owner);
            rep.SetActive(false);
        }
        public void Add(Army army)
        {
            if (!stack.Contains(army))
            {
                stack.Insert(0, army);
            }
        }
        public void Remove(Army army)
        {
            stack.Remove(army);
            if (stack.Count < 1)
            {
                rep.SetActive(false);
            }
        }
        public void RefreshStack()
        {
            //look for eligible units
            rep.SetActive(false);
            rep.GetComponent<UnitHandler>().ChangeOwner(null);
            List<Army> newOwners = new List<Army>();
            foreach (Army a in stack)
            {

                if (a.destProvince == null)
                {
                    rep.SetActive(true);

                    //change representated army
                    newOwners.Add(a);
                }
            }
            if (newOwners.Count > 0)
            {
                rep.GetComponent<UnitHandler>().ChangeOwner(newOwners);
            }
        }
        public Army GetArmy()
        {
            foreach(Army a in stack)
            {
                if (a.destProvince == null)
                {
                    return a;
                }
            }
            return null;
        }
        public List<Army> GetStack()
        {
            return stack;
        }

        public List<Army> stack;
        public GameObject rep;
        public Province owner;
    }
    [System.Serializable]
    public class Negotiations
    {
        public Nation proposer;
        public Nation receiver;
        public Offer topic;
        public List<Offer> offer;
        [SerializeField]
        private FinanseBook value;

        public Negotiations(Nation prop, Nation rec, Offer topic)
        {
            this.topic = topic;
            proposer = prop;
            receiver = rec;
            offer = new List<Offer>();
            value = new FinanseBook();
        }
        public void Offer(Offer demand)
        {
            offer.Add(demand);
        }
        public float NegValue()
        {
            value.ResetBook();

            //base
            if (topic.baseValue > 0) {
                value.AddExpense(topic.baseValue, "Base reluctance");
            }
            else if (topic.baseValue < 0) {
                value.AddExpense(topic.baseValue, "Base reluctance");
            }

            //relations
            int v = (int)(receiver.rel.GetRelationToward(proposer).TotalValue() / 10f);
            value.AddExpense(v, "Relations impact");

            //offers
            foreach(Offer off in offer)
            {
                if (off.offerName.Equals("give_bribe"))
                {
                    value.AddExpense(10, "Bribe");
                }
            }

            //distance malus

            //relative power

            //strategic reasons

            //economic reasons

            return value.TotalValue();
        }
        public string NegValueBreakdown()
        {
            string breakdown = "";
            breakdown = value.GetFinanses();
            return breakdown;
        }
        public void NegFinalize()
        {
            //execute topic
            if (topic.offerName.Equals("declare_war"))
            {
                GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetWars().Add(new War(proposer, receiver));
            }
            else if (topic.offerName.Equals("enter_alliance"))
            {
                proposer.rel.allied.Add(receiver);
                receiver.rel.allied.Add(proposer);
            }
            else if (topic.offerName.Equals("give_acces"))
            {
                proposer.rel.grantsAcces.Add(receiver);
            }
            else if (topic.offerName.Equals("get_acces"))
            {
                receiver.rel.grantsAcces.Add(proposer);
            }
            else if (topic.offerName.Equals("give_trade"))
            {
                proposer.rel.grantsTrade.Add(receiver);
            }
            else if (topic.offerName.Equals("get_trade"))
            {
                receiver.rel.grantsTrade.Add(proposer);
            }

            //execute offerings
            foreach(Offer off in offer)
            {
                if (off.offerName.Equals("give_bribe"))
                {
                    float bribeValue = receiver.BribeSize();
                    proposer.ChangeWealth(-bribeValue, "Bribe");
                    receiver.ChangeWealth(bribeValue, "Bribe");
                }
            }
        }
        public string NegSummary()
        {
            //string summary = "";
            //foreach(string off in influences)
            //{
            //    summary += off + "\n";
            //}
            //summary += "\n";
            //return summary;

            return "Some shit";
        }
    }
    [System.Serializable]
    public class War
    {
        public List<Nation> attackers;
        public List<Nation> defenders;

        [SerializeField]
        private TimeAndPace startDate;

        public War(Nation attacker, Nation defender)
        {
            attackers = new List<Nation>();
            defenders = new List<Nation>();
            AddParticipant(attacker, true);
            AddParticipant(defender, false);

            startDate = new TimeAndPace();
            startDate.day = GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetTime().day;
            startDate.month = GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetTime().month;
            startDate.year = GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetTime().year;
        }

        public List<Nation> GetEnemiesOf(Nation nat)
        {
            if (attackers.Contains(nat))
            {
                return defenders;
            }
            return attackers;
        }
        public void AddParticipant(Nation nat, bool attacker)
        {
            if (attacker)
            {
                if (!attackers.Contains(nat))
                {
                    attackers.Add(nat);
                }
            }
            else
            {
                if (!defenders.Contains(nat))
                {
                    defenders.Add(nat);
                }
            }
            nat.rel.wars.Add(this);
        }
    }
}
