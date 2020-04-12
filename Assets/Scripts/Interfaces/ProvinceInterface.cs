using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProvinceInterface : Interface
{
    private Province prov;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI devText;
    private TextMeshProUGUI popText;
    private TextMeshProUGUI autoText;
    private GameObject symbol;
    private RawImage image;
    private RawImage highlights;
    private RawImage terrain;

    public Texture2D castleHighlights;
    public Texture2D capitalHighlights;
    public Texture2D capifortHighlights;

    public Texture2D castleImage;
    public Texture2D capitalImage;
    public Texture2D capifortImage;
    public Texture2D townImage;

    public Texture2D mountainsTerrain;
    public Texture2D forestTerrain;
    public Texture2D grasslandTerrain;

    private GameObject buildings;
    private GameObject mods;

    // Start is called before the first frame update
    void Start()
    {
        image = transform.GetChild(1).GetChild(1).GetComponent<RawImage>();
        highlights = transform.GetChild(1).GetChild(0).GetComponent<RawImage>();
        terrain = transform.GetChild(1).GetChild(2).GetComponent<RawImage>();

        nameText = transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
        popText = transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>();
        devText = transform.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>();
        autoText = transform.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>();
        symbol = transform.GetChild(0).gameObject;

        castleImage = Resources.Load("UI/ProvinceInterface/Castle") as Texture2D;
        capitalImage = Resources.Load("UI/ProvinceInterface/Capital") as Texture2D;
        capifortImage = Resources.Load("UI/ProvinceInterface/Capifort") as Texture2D;
        townImage = Resources.Load("UI/ProvinceInterface/Town") as Texture2D;

        castleHighlights = Resources.Load("UI/ProvinceInterface/Castle_Highlights") as Texture2D;
        capitalHighlights = Resources.Load("UI/ProvinceInterface/Capital_Highlights") as Texture2D;
        capifortHighlights = Resources.Load("UI/ProvinceInterface/Capifort_Highlights") as Texture2D;

        mountainsTerrain = Resources.Load("UI/ProvinceInterface/Mountains") as Texture2D;
        forestTerrain = Resources.Load("UI/ProvinceInterface/Forest") as Texture2D;
        grasslandTerrain = Resources.Load("UI/ProvinceInterface/Grassland") as Texture2D;

        buildings = transform.GetChild(8).gameObject;
        mods = transform.GetChild(7).GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void MouseInput(Province prov)
    {



    }
    public override void KeyboardInput(Province prov)
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Camera.main.GetComponent<CameraHandler>().ZoomTo(prov.transform.position, 7);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {

        }

        MapTools.GetInput().CameraInput();
        MapTools.GetInput().BasicInput(prov);
    }


    public override void Refresh()
    {
        nameText.text = prov.provName;
        devText.text = "" + prov.dev;
        popText.text = "" + prov.pop;
        autoText.text = "" + (int)(prov.auto * 100) + "%";
        SetSymbol(prov.owner.id);
        SetImage();
        SetMods();


        //refresh constructions
        foreach (Transform type in buildings.transform)
        {
            foreach (Transform subtype in type)
            {
                foreach (Transform level in subtype)
                {

                    if (level.gameObject.GetComponent<BuildingFrame>().level <= prov.mods.GetMod(level.gameObject.GetComponent<BuildingFrame>().type))
                    {
                        if (level.gameObject.GetComponent<BuildingFrame>().level == subtype.childCount)
                        {
                            level.gameObject.GetComponent<BuildingFrame>().SetFrameActive(true, true, true);
                        }
                        else
                            level.gameObject.GetComponent<BuildingFrame>().SetFrameActive(true, false, true);
                    }
                    else if (level.gameObject.GetComponent<BuildingFrame>().level <= prov.mods.GetMod(level.gameObject.GetComponent<BuildingFrame>().type) + 1)
                    {
                        level.gameObject.GetComponent<BuildingFrame>().SetFrameActive(true, true, false);
                    }
                    else
                    {
                        level.gameObject.GetComponent<BuildingFrame>().SetFrameActive(false, false, false);
                    }
                }
            }
        }
    }

    public override bool IsSet()
    {
        return prov != null;
    }

    public override void Disable()
    {
        prov = null;
        gameObject.SetActive(false);
        MapTools.GetMap().activeProvince = null;
    }

    public override void Enable()
    {
        gameObject.SetActive(true);
    }

    public override void Set(Classes.Nation nat = null, Province prov = null, Classes.Army arm = null, Classes.TradeRoute route = null, List<Classes.Army> armies = null, List<Classes.Unit> units = null, Battle battle = null)
    {
        this.prov = prov;
        Refresh();
    }

    public void ClearMods()
    {
        foreach(Transform obj in mods.transform)
        {
            Destroy(obj.gameObject);
        }
        mods.transform.DetachChildren();
    }

    public void SetMods()
    {
        ClearMods();

        foreach(KeyValuePair<string, Classes.ModBook.Pair> mod in prov.mods.GetMods())
        {
            if (mod.Value.value != 0 && mod.Value.value != -1.234)
            {
                GameObject newMod = Instantiate(Resources.Load("Prefabs/UI_Mod") as GameObject, mods.transform);
                newMod.GetComponent<ModIcon>().SetMod(mod.Key, mod.Value.value);
            }
        }
    }

    public void SetSymbol(int id)
    {
        symbol.transform.GetChild(0).gameObject.GetComponent<RawImage>().texture = Resources.Load("Symbols/Symb_" + id) as Texture2D;
        symbol.transform.GetChild(2).gameObject.GetComponent<NationSymbolClick>().nat = MapTools.IdToNat(id);
    }

    public void SetImage()
    {
        if (prov.mods.GetMod("const_fort")>0)
        {
            if (prov.owner.capital == prov)
            {
                image.texture = capifortImage;
                highlights.texture = capifortHighlights;
                highlights.color = prov.owner.color;
                highlights.gameObject.SetActive(true);
            }
            else
            {
                image.texture = castleImage;
                highlights.texture = castleHighlights;
                highlights.color = prov.owner.color;
                highlights.gameObject.SetActive(true);
            }
        }
        else if (prov.owner.capital == prov)
        {
            image.texture = capitalImage;
            highlights.texture = capitalHighlights;
            highlights.color = prov.owner.color;
            highlights.gameObject.SetActive(true);
        }
        else
        {
            image.texture = townImage;
            highlights.gameObject.SetActive(false);
        }

        if (prov.mods.GetMod("terrain_mountains")==1)
        {
            terrain.texture = mountainsTerrain;
        }
        else if (prov.mods.GetMod("terrain_forest") == 1)
        {
            terrain.texture = forestTerrain;
        }
        else if (prov.mods.GetMod("terrain_plains") == 1)
        {
            terrain.texture = grasslandTerrain;
        }
    }
}
