using System.Collections;
using System.Collections.Generic;

using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;

public class ArmyInterface : Interface, IScrollHandler
{
    private GameObject topFrame;
    private GameObject topMore;
    private GameObject frames;
    private GameObject bottomFrame;
    private GameObject bottomMore;
    public ArmyInterface anotherInterface;
    public bool isAdditional;

    public List<Classes.Army> armies;

    public int size = 2;
    private int shift = 0;
    private int totalFrames = 0;

    //Unity
    void Start()
    {
        topFrame = transform.GetChild(0).gameObject;
        topMore = transform.GetChild(1).gameObject;
        topMore.SetActive(false);
        frames = transform.GetChild(2).gameObject;
        bottomMore = transform.GetChild(3).gameObject;
        bottomMore.SetActive(false);
        bottomFrame = transform.GetChild(4).gameObject;
        ClearFrames();
    }
    void Update()
    {
        if (frames.transform.childCount < 1)
        {
            ClearFrames();
            gameObject.SetActive(false);
        }
        //RefreshSize();
    }

    //Interface
    public override void Refresh()
    {
        if (armies.Count > 1)
        {
            //set text
            string text = "Various Armies";
            topFrame.GetComponent<Unitframe>().ClearOwners();
            bottomFrame.GetComponent<Unitframe>().ClearOwners();
            SetTop(text);
            bottomFrame.GetComponent<Unitframe>().SetArmy(armies);
            bottomFrame.GetComponent<Unitframe>().actAll = true;
            ClearFrames();

            int i = 0;
            totalFrames = armies.Count;
            bottomMore.SetActive(false);
            topMore.SetActive(false);
            foreach (Classes.Army a in armies)
            {
                if (i >= shift)
                {
                    if (i >= size + shift)
                    {
                        if (i < totalFrames)
                        {
                            SetBottomMore(i);
                        }
                        break;
                    }
                    GameObject newFrame = Instantiate(Resources.Load("Prefabs/UI_Unitframe") as GameObject, frames.transform);
                    List<Classes.Army> newArmy = new List<Classes.Army>();
                    newArmy.Add(a);
                    newFrame.GetComponent<Unitframe>().SetArmy(newArmy);
                }
                else
                {
                    SetTopMore(i);
                }
                i++;
            }
        }
        else if (armies.Count == 1)
        {
            topFrame.GetComponent<Unitframe>().ClearOwners();
            SetTop(armies.ElementAt(0).name);
            bottomFrame.GetComponent<Unitframe>().SetArmy(armies);

            ClearFrames();
            //add units
            int i = 0;
            bottomMore.SetActive(false);
            topMore.SetActive(false);
            foreach (Classes.Unit u in armies.ElementAt(0).units)
            {
                if (i >= shift)
                {
                    if (i >= size + shift)
                    {
                        if (i < totalFrames)
                        {
                            SetBottomMore(i);
                        }
                        break;
                    }
                    GameObject newFrame = Instantiate(Resources.Load("Prefabs/UI_Unitframe") as GameObject, frames.transform);
                    newFrame.GetComponent<Unitframe>().SetUnit(u);
                }
                else
                {
                    SetTopMore(i);
                }
                i++;
            }

            //add recruited units
            foreach (Classes.Unit u in armies.ElementAt(0).recruiter.recruitementQueue)
            {
                if (i >= shift)
                {
                    if (i >= size + shift)
                    {
                        if (i < totalFrames)
                        {
                            SetBottomMore(i);
                        }
                        break;
                    }
                    GameObject newFrame = Instantiate(Resources.Load("Prefabs/UI_Unitframe") as GameObject, frames.transform);
                    newFrame.GetComponent<Unitframe>().SetUnit(u, armies.ElementAt(0).recruiter);
                }
                else
                {
                    SetTopMore(i);
                }   
            i++;
            }

            //update total frames for scrolling purposes
            totalFrames = armies.ElementAt(0).units.Count + armies.ElementAt(0).recruiter.recruitementQueue.Count;
        }
        else
        {
            Disable();
        }

        RefreshSize();
    }
    public override bool IsSet()
    {
        return true;
    }
    public override void Enable()
    {
        gameObject.SetActive(true);
    }
    public override void Disable()
    {
        if (!isAdditional)
        {
            if (anotherInterface)
            {
                if (transform.parent.Find("ArmyFade").GetComponent<RawImage>().raycastTarget)
                {
                    ReorgAction();
                }
            }
            MapTools.GetMap().activeArmies.Clear();
        }
        ClearFrames();
        gameObject.SetActive(false);
    }
    public override void Set(Classes.Nation nat = null, Province prov = null, Classes.Army arm = null, Classes.TradeRoute route = null, List<Classes.Army> armies = null, List<Classes.Unit> units = null, Battle battle = null)
    {
        

        ClearFrames();
        if (armies != null)
        {
            //copy list
            List<Classes.Army> newList = new List<Classes.Army>();
            foreach(Classes.Army a in armies)
            {
                newList.Add(a);
            }
            this.armies = newList;
            Refresh();
        }
        else if(arm != null)
        {
            List<Classes.Army> newList = new List<Classes.Army>();
            newList.Add(arm);
            this.armies = newList;
            Refresh();
        }

        shift = 0;
        RefreshSize();
    }
    public override void KeyboardInput(Province prov)
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            MapTools.GetInterface().interface_army.GetComponent<ArmyInterface>().NewArmyAction();
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            MapTools.GetInterface().interface_army.GetComponent<ArmyInterface>().HaltAction();
        }
        
        else if (Input.GetKeyDown(KeyCode.G))
        {
            MapTools.GetInterface().interface_army.GetComponent<ArmyInterface>().MergeAction();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            MapTools.GetInterface().interface_army.GetComponent<ArmyInterface>().SplitAction();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            MapTools.GetInterface().EnableInterface("none");
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            MapTools.GetInterface().interface_army.GetComponent<ArmyInterface>().ReorgAction();
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            if (armies.Count == 1)
            {
                Camera.main.GetComponent<CameraHandler>().ZoomTo(armies.ElementAt(0).rep.transform.position, 7);
            }
        }
    }
    public override void MouseInput(Province prov)
    {
        MapTools.GetInput().SelectionInput();


        if (!EventSystem.current.IsPointerOverGameObject())
        {

            //movement
            if (Input.GetMouseButtonUp(1))
            {
                if (!transform.parent.Find("ArmyFade").GetComponent<RawImage>().raycastTarget)
                {
                    if (MapTools.GetMap().activeArmies.Count > 0)
                    {
                        foreach (Classes.Army a in MapTools.GetMap().activeArmies)
                        {
                            if (a.owner == MapTools.GetSave().GetActiveNation())
                            {
                                if (!a.Move(prov))
                                {
                                    GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("No acces to province!");
                                }
                            }
                        }
                    }
                }
                else
                {
                    MapTools.GetToast().Enable("Cannot move while reorganizing");
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000))
                {
                    if (hit.collider.gameObject.GetComponent<UnitHandler>() == null)
                    {
                        MapTools.GetInterface().EnableInterface("none");
                        return;
                    }
                }
            }
        }

        //MapTools.GetInput().BasicInput(prov);
        MapTools.GetInput().CameraInput();
    }

    //Frames
    public void ClearFrames()
    {
        foreach(Transform obj in frames.transform)
        {
            Destroy(obj.gameObject);
        }
        frames.transform.DetachChildren();
    }
    public void SetTop(string newText)
    {
        topFrame.GetComponent<Unitframe>().SetFade(0);
        topFrame.GetComponent<Unitframe>().SetUnit(null);
        topFrame.GetComponent<Unitframe>().icon.color = Color.clear;
        topFrame.GetComponent<Unitframe>().text.text = newText;
    }
    public void SetTopMore(int i)
    {
        topMore.SetActive(true);
        topMore.GetComponent<Unitframe>().SetFade(0.5f);
        topMore.GetComponent<Unitframe>().ClearOwners();
        topMore.GetComponent<Unitframe>().goTop = true;
        topMore.GetComponent<Unitframe>().icon.color = Color.clear;
        topMore.GetComponent<Unitframe>().text.text = "..." + (i + 1) + " more...";
    }
    public void SetBottom(string newText, bool actAll)
    {
        bottomFrame.GetComponent<Unitframe>().SetFade(0);
        bottomFrame.GetComponent<Unitframe>().SetUnit(null);
        bottomFrame.GetComponent<Unitframe>().icon.color = Color.clear;
        bottomFrame.GetComponent<Unitframe>().text.text = newText;
        bottomFrame.GetComponent<Unitframe>().actAll = actAll;
    }
    public void SetBottomMore(int i)
    {
        bottomMore.SetActive(true);
        bottomMore.GetComponent<Unitframe>().SetFade(0.5f);
        bottomMore.GetComponent<Unitframe>().ClearOwners();
        bottomMore.GetComponent<Unitframe>().goBottom = true;
        bottomMore.GetComponent<Unitframe>().icon.color = Color.clear;
        bottomMore.GetComponent<Unitframe>().text.text = "..." + (totalFrames - i) + " more...";
    }

    //Scrolling
    public void OnScroll(PointerEventData eventData)
    {
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (shift < totalFrames-size)
            {
                shift++;
                GameObject.Find("Map/Center").GetComponent<InputHandler>().BlockInput();
                Refresh();
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (shift - 1 >= 0)
            {
                shift--;
                GameObject.Find("Map/Center").GetComponent<InputHandler>().BlockInput();
                Refresh();
            }
        }
    }
    public void GoToTop()
    {
        shift = 0;
        Refresh();
    }
    public void GoToBottom()
    {
        shift = totalFrames - size;
        Refresh();
    }

    //Actions
    public void MergeAction()
    {
        if (!anotherInterface.gameObject.activeSelf)
        {
            Classes.Army target = null;
            foreach (Classes.Army a in GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies)
            {
                if (a.destProvince == null)
                {
                    if (a.owner == GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetActiveNation())
                    {
                        target = a;
                    }
                }
            }
            List<Classes.Army> toDispose = new List<Classes.Army>();

            foreach (Classes.Army a in GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies)
            {
                if (a != target)
                {
                    if (a.owner == target.owner)
                    {
                        if (a.location == target.location)
                        {
                            if (!a.IsOccupied())
                            {
                                if (target.recruiter.recruitementQueue != null)
                                {
                                    foreach (Classes.Unit u in a.units)
                                    {
                                        target.AddUnit(u);
                                    }
                                    toDispose.Add(a);
                                }
                                else
                                {
                                    GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cannot merge while armies are recruiting");
                                }
                            }
                            else
                            {
                                GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Merged armies cannot be occupied");
                            }
                        }
                        else
                        {
                            GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Merged armies need to be in the same location");
                        }
                    }
                    else
                    {
                        GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Merged armies need to be of the same owner");
                    }
                }
            }

            foreach (Classes.Army a in toDispose)
            {
                armies.Remove(a);
                a.Dispose();
            }

            Refresh();
        }
        else
        {
            GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cant merge while reorganizing");
        }
    }
    public void SplitAction()
    {
        if (!anotherInterface.gameObject.activeSelf)
        {
            List<Classes.Army> newArmies = new List<Classes.Army>();
            foreach (Classes.Army a in GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies)
            {
                if (a.units.Count > 1)
                {
                    if (!a.IsOccupied())
                    {
                        Classes.Army newArmy = new Classes.Army(a.location, a.owner);

                        for (int i = 0; i < a.units.Count; i += 2)
                        {
                            newArmy.AddUnit(a.units.ElementAt(i));
                        }
                        foreach (Classes.Unit u in newArmy.units)
                        {
                            if (a.units.Contains(u))
                            {
                                a.units.Remove(u);
                            }
                        }

                        newArmies.Add(newArmy);
                    }
                    else
                    {
                        GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Split armies cannot be occupied");
                    }
                }
                else
                {
                    GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cant further split army");
                }
            }

            foreach (Classes.Army a in newArmies)
            {
                GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.Add(a);
                this.armies.Add(a);
            }
            Refresh();
        }
        else
        {
            GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cant split while reorganizing");
        }
    }
    public void ReorgAction()
    {
        //end reorganizing
        if (anotherInterface.gameObject.activeSelf)
        {
            transform.parent.Find("ArmyFade").GetComponent<Fade>().FadeOut(0);
            transform.parent.Find("ArmyFade").GetComponent<RawImage>().raycastTarget = false;
            List<Classes.Army> newList = new List<Classes.Army>();
            newList.Add(armies.ElementAt(0));
            newList.Add(anotherInterface.armies.ElementAt(0));
            Set(armies: newList);
            anotherInterface.Disable();
        }
        //start reorganizing
        else if (GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.Count == 2)
        {
            if (GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.ElementAt(0).location == GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.ElementAt(1).location)
            {
                if (!GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.ElementAt(0).IsOccupied() && GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.ElementAt(1).destProvince == null)
                {
                    Reorganize(GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.ElementAt(0), GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.ElementAt(1));
                }
                else
                {
                    GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Reorganized armies cannot be occupied");
                }
            }
            else
            {
                GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cant reorganize armies in different provinces");
            }
        }
        else
        {
            GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cant reorganize more or less than two armies at once");
        }
    }
    public void Reorganize(Classes.Army armyA, Classes.Army armyB)
    {
        transform.parent.Find("ArmyFade").GetComponent<Fade>().FadeIn(0.5f);
        transform.parent.Find("ArmyFade").GetComponent<RawImage>().raycastTarget = true;
        anotherInterface.Enable();
        anotherInterface.Set(arm: armyB);
        this.Set(arm: armyA);
    }
    public void HaltAction()
    {
        if (!anotherInterface.gameObject.activeSelf)
        {
            foreach (Classes.Army a in GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies)
            {
                if (!a.IsStationed())
                {
                    if (!a.IsRouting())
                    {
                        a.Move(a.location);
                    }
                    else
                    {
                        GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cannot halt routing army");
                    }
                }
            }
        }
        else
        {
            GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cant halt while reorganizing");
        }
    }
    public void EraseAction()
    {
        if (!anotherInterface.gameObject.activeSelf)
        {
            List<Classes.Army> toDispose = new List<Classes.Army>();
            foreach (Classes.Army a in GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies)
            {
                if (a.location.owner == a.owner)
                {
                    if (!a.IsOccupied())
                    {
                        toDispose.Add(a);
                    }
                    else
                    {
                        GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cannot erase occupied armies");
                    }
                }
                else
                {
                    GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cannot erase on foreign province");
                }
            }

            foreach (Classes.Army a in toDispose)
            {
                int manRegained = (int)(a.CurrentManpower() * a.owner.mods.GetMod("manpower_getback"));
                a.owner.ChangeManpower(manRegained, "Disbanded units");
                a.Dispose();
            }
            Refresh();
        }
        else
        {
            GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cant erase while reorganizing");
        }
    }
    public void NewArmyAction()
    {
        if (!anotherInterface.gameObject.activeSelf) { 
        Classes.Army newArmy = null;
        if (GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.Count == 1)
        {
            if (GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.ElementAt(0).units.Count > 1)
            {
                if (!GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.ElementAt(0).IsOccupied())
                {
                    newArmy = new Classes.Army(GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.ElementAt(0).location, GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.ElementAt(0).owner);
                    newArmy.AddUnit(GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.ElementAt(0).units.ElementAt(0));
                    GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.ElementAt(0).units.Remove(newArmy.units.ElementAt(0));
                    GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.Add(newArmy);
                }
                else
                {
                    GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cannot make new army out of army that is occupied");
                }
            }
            else
            {
                GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Need at least two units to create new army from existing one");
            }
        }
        else
        {

        }
        Reorganize(GameObject.Find("Map/Center").GetComponent<MapHandler>().activeArmies.ElementAt(0), newArmy);
        Refresh();
        }
        else
        {
            GameObject.Find("UI_Toast").GetComponent<Toast>().Enable("Cant make new army while reorganizing");
        }
    }

    //Other
    public void RefreshSize()
    {
        if (frames.transform.childCount <= 1)
        {
            frames.GetComponent<RectTransform>().sizeDelta = new Vector2(frames.GetComponent<RectTransform>().sizeDelta.x, 35);
        }
        else
        {
            frames.GetComponent<RectTransform>().sizeDelta = new Vector2(frames.GetComponent<RectTransform>().sizeDelta.x, frames.transform.childCount * 29 + 6);
        }
    }
}
