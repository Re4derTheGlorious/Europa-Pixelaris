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
                foreach(Province prov in provinces)
                {
                    this.provinces.Add(prov.id);
                }
            }
        }

        //Serializable
        public int id;//

        public List<Province> provinces;//
        public TradeRoute[] tradeRoutes;
        public ModBook mods;
        public Relations rel;
        public FinanseBook expBook;
        public FinanseBook manBook;

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
            rel = new Relations(this);
            expBook = new FinanseBook();
            manBook = new FinanseBook();
            mods = new ModBook();
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

            foreach(int id in nation_as.provinces)
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
                tradeRoutes = new TradeRoute[centers];
                for (int i = 0; i < tradeRoutes.Length; i++)
                {
                    tradeRoutes[i] = new TradeRoute((int)mods.GetMod("trade_range"), this);
                }
            }
            return centers;
        }
        public void Tick(TimeAndPace time)
        {
            //reset books
            if (time.day == 1 && time.hour == 0)
            {
                expBook.ResetBook();
            }

            //mods
            if (time.day == 1 && time.hour == 0) {
                mods.TickMonth();
            }

            //trade routes
            foreach (TradeRoute tr in tradeRoutes)
            {
                tr.Tick(time);
            }

            //provinces
            foreach (Province prov in provinces)
            {
                prov.Tick(time);
            }

            //armies
            foreach(Army army in armies)
            {
                army.Tick(time);
            }

            //relations
            rel.TickRelations(time);
        }
        public float BribeSize()
        {
            return expBook.TotalIn()*12; //years worth of income
        }
        public void AddProvince(Province prov)
        {
            if (!provinces.Contains(prov))
            {
                provinces.Add(prov);
            }
        }
        public NationAsSaveable AsSaveable()
        {
            return new NationAsSaveable(id, power, man, wealth, provinces, capital.id);
        }
    }
    public class Army
    {
        [System.Serializable]
        public class ArmyAsSaveable
        {
            public List<int> path;
            public string name;
            public int id;
            public int owner;
            public int location;
            public int destLocation;
            public float moveProgress;
            public float movementStage;
            public bool engaged;
            public bool routing;
            public bool waiting;

            public ArmyAsSaveable(List<Province> path, string name, int id, Nation owner, Province location, Province destLocation, float moveProgress, float movementStage, bool engaged, bool routing, bool waiting)
            {
                this.name = name;
                this.id = id;
                this.owner = owner.id;
                this.location = location.id;
                this.engaged = engaged;
                this.waiting = waiting;
                this.routing = routing;
                if (destLocation != null)
                {
                    this.destLocation = destLocation.id;
                    this.moveProgress = moveProgress;
                    this.movementStage = movementStage;
                }
                else
                {
                    this.destLocation = -1;
                }

                this.path = new List<int>();
                foreach (Province prov in path)
                {
                    this.path.Add(prov.id);
                }
            }
        }

        //Serializable
        [SerializeField]
        public string name = "Default Name";//
        public int id;//
        public Nation owner;//
        public Province location;//
        public Recruiter recruiter;

        public List<Province> path;//
        public float moveProgress = 0;//
        public Province destProvince;//
        public float movementStage = 0;//

        private bool engaged = false;//
        private bool routing = false;//
        public bool waitingToMove = false;//

        public List<Unit> units;//


        //Other
        public GameObject rep;

        public Army(Province loc, Nation own)
        {
            id = MapTools.NewId();
            location = loc;
            owner = own;
            Representate();

            units = new List<Unit>();
            recruiter = new Recruiter(this);
            path = new List<Province>();

            owner.armies.Add(this);
            MapTools.GetSave().GetArmies().Add(this);

            GenerateName();
        }
        public void Representate()
        {
            if (rep == null)
            {
                rep = Instantiate(Resources.Load("Prefabs/Army") as GameObject, GameObject.Find("Map").transform.Find("Armies"));
                rep.name = name;

                Vector3 newPos = MapTools.LocalToScale(location.center);
                newPos.z = rep.transform.position.z;
                rep.transform.position = newPos;
                rep.GetComponent<UnitHandler>().owners.Add(this);

                if (destProvince == null && moveProgress == 0)
                {
                    rep.SetActive(false);
                    location.armies.Add(this);
                    location.armies.RefreshStack();
                }
                else
                {
                    rep.SetActive(true);
                }
            }
        }
        public Army Restore(ArmyAsSaveable army_as, SaveFile saveBase)
        {
            //Base
            recruiter = new Recruiter(this);

            //Loaded
            name = army_as.name;
            id = army_as.id;
            owner = MapTools.IdToNat(army_as.owner);
            location = MapTools.IdToProv(army_as.location);
            location.armies.Add(this);
            if (army_as.destLocation >=0)
            {
                destProvince = MapTools.IdToProv(army_as.destLocation);
                movementStage = army_as.movementStage;
                moveProgress = army_as.moveProgress;
                path = new List<Province>();
                foreach(int id in army_as.path)
                {
                    path.Add(MapTools.IdToProv(id));
                }
            }
            else
            {
                destProvince = null;
            }
            engaged = army_as.engaged;
            routing = army_as.routing;
            waitingToMove = army_as.waiting;

            if (destProvince != null)
            {
                rep.SetActive(true);

                Vector3 newPos = MapTools.LocalToScale(TickMovement(MapTools.GetSave().GetTime().hour, true));
                newPos.z = rep.transform.position.z;
                rep.transform.position = newPos;
            }
            location.armies.RefreshStack();

            return this;
        }
        public bool IsActive()
        {
            return MapTools.GetMap().activeArmies.Contains(this);
        }
        public void SetActive(bool a)
        {
            if (a)
            {
                if (!MapTools.GetMap().activeArmies.Contains(this))
                {
                    MapTools.GetMap().activeArmies.Add(this);
                }
            }
            else
            {
                if (MapTools.GetMap().activeArmies.Contains(this))
                {
                    MapTools.GetMap().activeArmies.Remove(this);
                }
            }
        }
        public bool Move(Province prov)
        {
            if (prov != null)
            {
                if (prov.isAccesibleFor(owner))
                {
                    if (prov != location)
                    {
                        //move
                        if (prov != destProvince && movementStage == 0 && moveProgress == 0)
                        {
                            ClearArrows();
                            SetPathTo(prov);
                        }
                        else if (destProvince != null)
                        {
                            //change at second stage
                            if (movementStage == 1)
                            {
                                float remaining = 1 - moveProgress;
                                Province cacheDest = destProvince;
                                ClearArrows();
                                SetPathTo(prov);
                                if (path.ElementAt(1) == cacheDest)
                                {
                                    moveProgress = remaining;
                                    waitingToMove = false;
                                }
                                else
                                {
                                    path.Insert(0, cacheDest);
                                    destProvince = cacheDest;
                                    movementStage = 1;
                                    moveProgress = 1 - remaining;
                                    waitingToMove = false;
                                }
                                PaintPath();
                            }
                            //change at first stage
                            else
                            {
                                float remaining = moveProgress;
                                Province cacheDest = destProvince;
                                ClearArrows();
                                SetPathTo(prov);
                                if (path.ElementAt(1) == cacheDest)
                                {
                                    moveProgress = remaining;
                                    waitingToMove = false;
                                }
                                else
                                {
                                    path.Insert(0, cacheDest);
                                    destProvince = cacheDest;
                                    movementStage = 1;
                                    moveProgress = 1 - remaining;
                                    waitingToMove = false;
                                }
                                PaintPath();
                            }
                        }
                    }
                    else if (prov == location && destProvince != null)
                    {
                        if (movementStage == 0)
                        {
                            //cancel if no progress yet
                            if (moveProgress == 0)
                            {
                                movementStage = 1;
                                moveProgress = 1;
                                destProvince = prov;
                                TickMovement(0);

                            }
                            //cancel if progress
                            else
                            {
                                float cacheProgress = moveProgress;
                                Province cacheDest = destProvince;
                                location = destProvince;
                                SetPathTo(prov);
                                waitingToMove = false;
                                movementStage = 1;
                                moveProgress = 1 - cacheProgress;
                                location = prov;
                                destProvince = cacheDest;
                                ClearArrows();
                                PaintPath();
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        public void SetPathTo(Province prov)
        {
            Vector2 start = MapTools.LocalToScale(location.center);
            Vector2 end = MapTools.LocalToScale(prov.center);

            path.Clear();
            var p = rep.GetComponent<Seeker>().StartPath(start, end, OnPathComplete);

            p.BlockUntilCalculated();
        }
        public void OnPathComplete(Path p)
        {
            foreach (Vector3 vec in p.vectorPath)
            {
                path.Add(MapTools.ScaleToProv(vec));
            }
            if (path.Count > 1)
            {
                if (MapTools.GetSave().GetTime().hour != 0)
                {
                    waitingToMove = true;
                }
                movementStage = 0;
                moveProgress = 0;
                destProvince = path.ElementAt(1);
                location.armies.RefreshStack();

                //create arrows
                PaintPath();

                rep.SetActive(true);
            }
        }
        public void PaintPath()
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                rep.GetComponent<UnitHandler>().AddArrow(path.ElementAt(i).center, path.ElementAt(i + 1).center).SelfDestructAt(path.ElementAt(i+1), this);
            }
        }
        public float GetMovementSpeed()
        {
            //base
            float baseSpeed = owner.mods.GetMod("movement_speed");

            //terrain impact
            float penalty = 0;
            float cap = 0.9f;
            if (location.mods.GetMod("terrain_mountains") == 1)
            {
                penalty += 0.5f;
            }
            else if (location.mods.GetMod("terrain_hills") == 1)
            {
                penalty += 0.25f;
            }
            else if (location.mods.GetMod("terrain_forest") == 1)
            {
                penalty += 0.1f;
            }

            //crossing impact
            if (location.crossings.Contains(destProvince) && movementStage == 1)
            {
                penalty += 0.35f;
            }

            //cap penalty
            if (penalty > cap)
            {
                penalty = cap;
            }
            return baseSpeed * (1 - penalty);
        }
        public void Replenish()
        {
            if (!engaged && !routing)
            {
                foreach (Unit u in units)
                {
                    //replenish morale
                    u.morale += owner.mods.GetMod("morale_recovery");
                    if (u.morale > 1)
                    {
                        u.morale = 1;
                    }

                    //get regiment size
                    int regimentSize = 0;
                    if (u.type.StartsWith("inf_"))
                    {
                        regimentSize = (int)owner.mods.GetMod("inf_regiment_size");
                    }
                    else if (u.type.StartsWith("cav_"))
                    {
                        regimentSize = (int)owner.mods.GetMod("cav_regiment_size");
                    }
                    else if (u.type.StartsWith("art_"))
                    {
                        regimentSize = (int)owner.mods.GetMod("art_regiment_size");
                    }

                    //count needed man
                    int neededManpower = (int)(owner.mods.GetMod("replenishement_rate") * regimentSize);
                    if(neededManpower+u.manpower > regimentSize)
                    {
                        neededManpower = regimentSize - u.manpower;
                    }

                    //count available man
                    int availableMan = neededManpower;
                    if (neededManpower > owner.man)
                    {
                        availableMan = owner.man;
                    }

                    //pay for replenished man
                    float cost = (availableMan / regimentSize);
                    if (u.type.Equals("inf_light"))
                    {
                        cost *= (int)owner.mods.GetMod("inf_light_cost");
                    }
                    else if (u.type.Equals("inf_heavy"))
                    {
                        cost *= (int)owner.mods.GetMod("inf_heavy_cost");
                    }
                    else if (u.type.Equals("inf_skirmish"))
                    {
                        cost *= (int)owner.mods.GetMod("inf_skirmish_cost");
                    }
                    else if (u.type.Equals("cav_missile"))
                    {
                        cost *= (int)owner.mods.GetMod("cav_missile_cost");
                    }
                    else if (u.type.Equals("cav_light"))
                    {
                        cost *= (int)owner.mods.GetMod("cav_light_cost");
                    }
                    else if (u.type.Equals("cav_shock"))
                    {
                        cost *= (int)owner.mods.GetMod("cav_shock_cost");
                    }
                    else if (u.type.Equals("art_field"))
                    {
                        cost *= (int)owner.mods.GetMod("art_field_cost");
                    }
                    else if (u.type.Equals("art_heavy"))
                    {
                        cost *= (int)owner.mods.GetMod("art_heavy_cost");
                    }
                    else if (u.type.Equals("art_siege"))
                    {
                        cost *= (int)owner.mods.GetMod("art_siege_cost");
                    }
                    owner.ChangeWealth(-cost, "Replenishments");
                    owner.ChangeManpower(-availableMan, "Replenishments");

                    //replenish
                    u.manpower += availableMan;
                }
            }
        }
        public void CountUpkeep()
        {
            float cost = 0;
            foreach (Unit u in units)
            {
                float unitCost = 0;
                if (u.type == "inf_skirmish")
                {
                    cost += owner.mods.GetMod("inf_skirmish_cost");
                }
                else if (u.type == "inf_light")
                {
                    cost += owner.mods.GetMod("inf_light_cost");
                }
                else if (u.type == "inf_heavy")
                {
                    cost += owner.mods.GetMod("inf_heavy_cost");
                }
                unitCost *= owner.mods.GetMod("upkeep_modifier") * (int)owner.mods.GetMod("regiment_size");
                cost += unitCost;
            }

            owner.ChangeWealth(-cost, "Upkeep");
        }
        public void Route()
        {
            int link = Random.Range(0, location.links.Count);
            Move(location.links.ElementAt(link));
            routing = true;
        }
        public float ArmyMorale()
        {
            float mor = 0;
            float man = 0;
            foreach (Unit u in units)
            {
                mor += u.morale*u.manpower;
                man += u.manpower;
            }
            return mor / man;
        }
        public int TotalManpower()
        {
            int total = 0;
            foreach (Unit u in units)
            {
                total += u.manpower;
            }
            return total;
        }
        public int CurrentManpower()
        {
            int current = 0;
            foreach (Unit u in units)
            {
                current += u.manpower;
            }
            return current;
        }
        public void AddUnit(Unit u)
        {
            units.Add(u);
            units.Sort(delegate (Unit a, Unit b)
            {
                return (b.type).CompareTo(a.type);
            });
        }
        public Vector3 TickMovement(float hour, bool dontAdvance = false)
        {
            if (destProvince != null && !engaged)
            {
                Vector3 newPos = location.center;
                if (!waitingToMove)
                {
                    if (hour == 0)
                    {
                        moveProgress += GetMovementSpeed();
                        if (dontAdvance)
                        {
                            moveProgress = 0;
                        }

                        //end movement
                        if (moveProgress >= 0.99 && movementStage == 1)
                        {
                            moveProgress = 0;
                            movementStage = 0;
                            path.RemoveAt(0);

                            if (path.Count > 1)
                            {
                                //continue movement along the path
                                destProvince = path.ElementAt(1);
                            }
                            else
                            {
                                //finish movement
                                destProvince = null;
                                path.RemoveAt(0);
                                location.armies.RefreshStack();
                                rep.SetActive(false);
                            }

                            return location.center;
                        }
                        //end stage
                        else if (moveProgress >= 0.99 && movementStage == 0)
                        {
                            moveProgress = 0;
                            movementStage = 1;

                            destProvince.armies.Add(this);
                            location.armies.Remove(this);

                            Province cache = location;
                            location = destProvince;
                            destProvince = cache;
                        }
                    }

                    float hourlyGain = (hour / 23f) * GetMovementSpeed();
                    float graphicalProgress = moveProgress + hourlyGain;

                    if (movementStage == 0)
                    {
                        newPos = Vector3.Lerp(location.center, Vector3.Lerp(location.center, destProvince.center, 0.5f), graphicalProgress);
                    }
                    else if (movementStage == 1)
                    {
                        newPos = Vector3.Lerp(Vector3.Lerp(location.center, destProvince.center, 0.5f), location.center, graphicalProgress);
                    }
                }
                if (hour == 0)
                {
                    waitingToMove = false;
                }
                return newPos;
            }
            else
            {
                return location.center;
            }
        }
        public void Dispose()
        {
            GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetArmies().Remove(this);
            owner.armies.Remove(this);
            if (GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.Contains(this))
            {
                GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.Remove(this);
            }
            location.armies.Remove(this);

            rep.SetActive(false);
            rep.transform.parent = null;
            Destroy(rep.gameObject);
        }
        public void Tick(TimeAndPace time)
        {
            //hourly
            Vector3 newPos = MapTools.LocalToScale(TickMovement(time.hour));
            newPos.z = rep.transform.position.z;
            rep.transform.position = newPos;

            //daily
            if (time.hour == 0)
            {
                recruiter.TickRecruitement();
            }

            //monthly
            if (time.day == 1 && time.hour == 0)
            {
                //replenishement
                Replenish();

                //upkeep
                CountUpkeep();
            }
        }
        public void ClearArrows()
        {
            foreach(Transform arr in GameObject.Find("Map/Arrows/Movement").transform)
            {
                if (arr.GetComponent<ArrowPlacer>().GetOwner() == this)
                {
                    Destroy(arr.gameObject);
                }
            }
        }
        public void GenerateName()
        {
            name = owner.name + " Army";
        }

        //setget
        public bool IsOccupied()
        {
            if (destProvince != null)
            {
                return true;
            }
            else if (engaged)
            {
                return true;
            }
            return false;
        }
        public bool IsRouting()
        {
            if (routing)
            {
                return true;
            }
            return false;
        }
        public bool IsStationed()
        {
            if (destProvince != null)
            {
                return false;
            }
            if (engaged != false)
            {
                return false;
            }
            return true;
        }
        public bool IsEngaged()
        {
            if (engaged)
            {
                return true;
            }
            return false;
        }
        public void SetEngaged(bool e)
        {
            engaged = e;
        }

        public ArmyAsSaveable AsSaveable()
        {
            return new ArmyAsSaveable(path, name, id, owner, location, destProvince, moveProgress, movementStage, engaged, routing, waitingToMove);
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
    }
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
        public bool attacking;
        public Unit lastTarget;
        public Unit lastAttacker;
        public int dmgDealt;
        public int dmgReceived;
        public bool routed = false;
        public bool routing = false;
        public bool moved = false;


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

            FinanseBook strengthBook = new FinanseBook();

            //stage mods
            if (stage == 1)
            {
                if(type.Equals("inf_light") || type.Equals("cav_light"))
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
            else if (stage == 2)
            {
                if (type.Equals("inf_skirmish") || type.Equals("cav_missile"))
                {
                    strengthBook.AddExpense(-0.5f, "Skirmish unit on battle stage");
                }
            }
            else if (stage == 3)
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

            if (1+strengthBook.TotalValue() <= 0.1f)
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
                if(arm.location.owner == arm.owner)
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
            else if (type.StartsWith("art_")){
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
 
            if(type.Equals("inf_skirmish") || type.Equals("cav_missile") || type.StartsWith("art_"))
            {
                ranged = true;
            }
            if (type.StartsWith("art_"))
            {
                longRanged = true;
            }

            GenerateName(null, owner);
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
        public FinanseBook()
        {
            exp = new Dictionary<string, float>();
        }
        public void ResetBook()
        {
            exp = new Dictionary<string, float>();
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
            return total;
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
