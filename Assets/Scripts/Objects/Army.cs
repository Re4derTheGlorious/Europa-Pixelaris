using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pathfinding;

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
    public Classes.Recruiter recruiter;

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
        recruiter = new Classes.Recruiter(this);
        path = new List<Province>();

        owner.armies.Add(this);
        MapTools.GetSave().GetArmies().Add(this);

        GenerateName();
    }
    public void Representate()
    {
        if (rep == null)
        {
            rep = MonoBehaviour.Instantiate(Resources.Load("Prefabs/Army") as GameObject, GameObject.Find("Map").transform.Find("Armies"));
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
        recruiter = new Classes.Recruiter(this);

        //Loaded
        name = army_as.name;
        id = army_as.id;
        owner = MapTools.IdToNat(army_as.owner);
        location = MapTools.IdToProv(army_as.location);
        location.armies.Add(this);
        if (army_as.destLocation >= 0)
        {
            destProvince = MapTools.IdToProv(army_as.destLocation);
            movementStage = army_as.movementStage;
            moveProgress = army_as.moveProgress;
            path = new List<Province>();
            foreach (int id in army_as.path)
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
            rep.GetComponent<UnitHandler>().AddArrow(path.ElementAt(i).center, path.ElementAt(i + 1).center).SelfDestructAt(path.ElementAt(i + 1), this);
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
                if (neededManpower + u.manpower > regimentSize)
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
            mor += u.morale * u.manpower;
            man += u.manpower;
        }
        return mor / man;
    }
    public int TotalManpower()
    {
        int total = 0;
        foreach (Unit u in units)
        {
            total += u.manpower*u.MaxManpower();
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
        MonoBehaviour.Destroy(rep.gameObject);
    }
    public void Tick(Classes.TimeAndPace time)
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
        foreach (Transform arr in GameObject.Find("Map/Arrows/Movement").transform)
        {
            if (arr.GetComponent<ArrowPlacer>().GetOwner() == this)
            {
                MonoBehaviour.Destroy(arr.gameObject);
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
