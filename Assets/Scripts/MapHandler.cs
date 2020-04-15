using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System;
using TMPro;
using Pathfinding;


public class MapHandler : MonoBehaviour
{
    public SaveFile save;

    private string path_definitions = "Assets/Map/map_definitions.csv";
    private string path_nations = "Assets/Map/map_nations.csv";
    private string path_units = "Assets/Map/map_units.csv";
    private string path_mods = "Assets/Map/nation_default.csv";
    private string path_provmods = "Assets/Map/prov_mods.csv";

    public GameObject unitPrefab;
    public GameObject arrowPrefab;
    public GameObject townPrefab;

    public Texture2D map_template;
    public Texture2D map_ingame;

    public Province activeProvince;
    public List<Army> activeArmies;

    public bool measurerActive;
    public bool linkerActive;
    public bool refreshActive;

    // Start is called before the first frame update
    void Start()
    {
        save = new SaveFile();

        activeArmies = new List<Army>();

        //load provinces
        StreamReader reader = new StreamReader(path_definitions);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            string[] fields = GetFields(line, ",");
            Province prov = Instantiate(Resources.Load("Prefabs/Town_Prefab") as GameObject, GameObject.Find("Map/Towns").transform).GetComponent<Province>();
            prov.Init(new Vector2(int.Parse(GetFields(fields[7], " ")[0]), int.Parse(GetFields(fields[7], " ")[1])), fields[1]);
            prov.id = Int32.Parse(fields[0]);
            prov.provName = fields[1];
            prov.graphicalCenter = new Vector2(int.Parse(GetFields(fields[2], " ")[0]), int.Parse(GetFields(fields[2], " ")[1]));
            prov.graphicalSize = new Vector2(int.Parse(GetFields(fields[3], " ")[0]), int.Parse(GetFields(fields[3], " ")[1]));
            prov.pop = int.Parse(fields[5]);
            prov.dev = int.Parse(fields[6]);
            foreach(string str in GetFields(fields[9], " "))
            {
                prov.traits.Add(str);
            }
            save.GetProvinces().Add(prov);
        }
        reader.Close();

        //load prov mods
        reader = new StreamReader(path_provmods);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            string[] fields = GetFields(line, ",");
            int id = int.Parse(fields[0]);
            for (int i = 1; i<fields.Length; i++)
            {
                string key = GetFields(fields[i], " ")[0];
                
                float value = float.Parse(GetFields(fields[i], " ")[1]);
                MapTools.IdToProv(id).mods.AddMod(key, value, -1);
            }
        }
        reader.Close();

        //load links and crossings
        reader = new StreamReader(path_definitions);
        while (!reader.EndOfStream)
        {
            //links
            string line = reader.ReadLine();
            string[] fields = GetFields(GetFields(line, ",")[4], " ");
            int id = int.Parse(GetFields(line, ",")[0]);
            foreach (string linkId in fields)
            {
                MapTools.IdToProv(id).links.Add(MapTools.IdToProv(int.Parse(linkId)));
            }
            //crossings
            fields = GetFields(GetFields(line, ",")[8], " ");
            foreach (string linkId in fields)
            {
                MapTools.IdToProv(id).crossings.Add(MapTools.IdToProv(int.Parse(linkId)));
            }
            //IdToProv(id).Restore();
        }
        reader.Close();

        //load default nation mods
        Dictionary<string, Classes.ModBook.Pair> dict = LoadNationMods(path_mods);

        //load nations
        reader = new StreamReader(path_nations);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            string[] fields = GetFields(line, ",");
            Nation nation = new Nation();
            nation.id = int.Parse(fields[0]);
            nation.name = fields[1];
            string[] insideProvs = GetFields(fields[2], " ");
            foreach (string id in insideProvs)
            {
                nation.provinces.Add(MapTools.IdToProv(int.Parse(id)));
                MapTools.IdToProv(int.Parse(id)).owner = nation;
            }

            ColorUtility.TryParseHtmlString(fields[3], out nation.color);
            nation.capital = MapTools.IdToProv(int.Parse(fields[4]));
            nation.capital.mods.AddMod("capital_town", 1, -1);
            nation.capital.mods.AddMod("trade_hub", 1, -1);
            nation.mods.PassMods(dict);
            nation.CountTradeHubs();
            save.GetNations().Add(nation);
        }
        reader.Close();

        if (PlayerPrefs.GetString("LoadSave") != null && PlayerPrefs.GetString("LoadSave") != "")
        {
            SwitchSave();
        }
        else
        {
            //load relations
            foreach (Nation nat in save.GetNations())
            {
                nat.rel.FillRelations(save.GetNations());
            }
            MapTools.IdToNat(15).rel.grantsAcces.Add(MapTools.IdToNat(13));
            MapTools.IdToNat(15).rel.grantsAcces.Add(MapTools.IdToNat(18));
            MapTools.IdToNat(15).rel.grantsAcces.Add(MapTools.IdToNat(12));
            save.GetWars().Add(new Classes.War(MapTools.IdToNat(16), MapTools.IdToNat(17)));
            save.GetWars().Add(new Classes.War(MapTools.IdToNat(9), MapTools.IdToNat(4)));


            //load units
            reader = new StreamReader(path_units);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] fields = GetFields(line, ",");
                Army newArmy = new Army(MapTools.IdToProv(int.Parse(fields[0])), MapTools.IdToNat(int.Parse(fields[1])));

                if (newArmy.owner.id == 16) //16
                {
                    for (int i = 0; i < 4; i++)
                    {
                        newArmy.AddUnit(new Unit("inf_skirmish", 100, newArmy, 1f));
                    }
                }
                //else
                //{
                    for (int i = 0; i < 5; i++) { 
                        newArmy.AddUnit(new Unit("inf_skirmish", UnityEngine.Random.Range(10, 100), newArmy, 1f));
                        newArmy.AddUnit(new Unit("inf_light", UnityEngine.Random.Range(10, 100), newArmy, 1f));
                        newArmy.AddUnit(new Unit("inf_heavy", UnityEngine.Random.Range(10, 100), newArmy, 1f));
                    }
                    for (int i = 0; i<3; i++)
                    {
                        newArmy.AddUnit(new Unit("cav_missile", UnityEngine.Random.Range(10, 100), newArmy, 1f));
                        newArmy.AddUnit(new Unit("cav_light", UnityEngine.Random.Range(10, 100), newArmy, 1f));
                        newArmy.AddUnit(new Unit("cav_shock", UnityEngine.Random.Range(10, 100), newArmy, 1f));
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        newArmy.AddUnit(new Unit("art_field", UnityEngine.Random.Range(10, 100), newArmy, 1f));
                        newArmy.AddUnit(new Unit("art_siege", UnityEngine.Random.Range(10, 100), newArmy, 1f));
                        newArmy.AddUnit(new Unit("art_heavy", UnityEngine.Random.Range(10, 100), newArmy, 1f));
                    }
                //}
            }
            reader.Close();
        }

        //fill missing info in provinces
        if (measurerActive)
        {
            ProvinceMeasurer();
            measurerActive = false;
        }
        if (linkerActive)
        {
            ProvinceLinker();
            linkerActive = false;
        }

        //paint provinces
        if (refreshActive)
        {
            Paint(true, true);
            refreshActive = false;
        }

        //pathfinding
        CreateAstarGrid();
    }

    // Update is called once per frame
    void Update()
    {

        if (!MapTools.GetInterface().GetActiveInterface().Equals("menu") && MapTools.GameStarted())
        {
            float increment = 0;
            if (save.GetTime().GetPace() == 0)
            {
                return;
            }
            else if (save.GetTime().GetPace() == 1)
            {
                increment = 8;
            }
            else if (save.GetTime().GetPace() == 2)
            {
                increment = 1f;
            }
            else if (save.GetTime().GetPace() == 3)
            {
                increment = 0.5f;
            }
            else if (save.GetTime().GetPace() == 4)
            {
                increment = 0;
            }

            if (save.GetTime().GetPace() == 0)
            {
                float delta = Time.deltaTime;
                save.GetTime().lastTick += delta;
                save.GetTime().lastHourlyTick += delta;
            }

            //daily ticks
            if (Time.time > save.GetTime().lastTick + increment && save.GetTime().hour >= 24)
            {
                //save time
                save.GetTime().lastTick = Time.time;

                //reset hours
                save.GetTime().hour = 0;

                //increment time
                save.GetTime().TickDay();

            }

            //hourly ticks
            float hIncrement = increment / 24f;
            if (Time.time > save.GetTime().lastHourlyTick + hIncrement && Time.time > save.GetTime().lastTick + hIncrement)
            {

                //save time
                save.GetTime().lastHourlyTick = Time.time;


                //Tick
                foreach (Nation nat in save.GetNations())
                {
                    nat.Tick(save.GetTime());
                }

                //battles
                for (int i = 0; i < save.GetBattles().Count; i++)
                {
                    if (save.GetBattles().ElementAt(i).Tick(save.GetTime().hour))
                    {
                        save.GetBattles().Remove(save.GetBattles().ElementAt(i));
                    }
                }

                //unit engagements
                foreach (Province prov in save.GetProvinces())
                {
                    foreach (Battle bat in prov.EngageArmies())
                    {
                        save.GetBattles().Add(bat);
                    }
                }

                //update interfaces
                MapTools.GetInterface().TickInterface();

                //autosave
                if (!Autosave())
                {
                    MapTools.GetToast().Enable("Cannot autosave");
                }

                //increment time
                save.GetTime().hour += 1;
            }
        }
    }

    private void SwitchSave()
    {
        //saved info
        SaveManager sm = new SaveManager();
        string saveName = PlayerPrefs.GetString("LoadSave");
        PlayerPrefs.DeleteKey("LoadSave");
        sm.LoadGame(System.IO.Path.GetFileName(saveName));

        GameObject.Find("Canvas").transform.Find("UI_Start_Interface").gameObject.GetComponent<StartInterface>().quickStart = true;
    }

    void CreateAstarGrid()
    {
        AstarData data = AstarPath.active.data;
        //AstarPath.active.Scan();
        PointGraph graph = data.AddGraph(typeof(PointGraph)) as PointGraph;
        AstarPath.active.AddWorkItem(new AstarWorkItem(ctx =>
        {
            //add nodes
            foreach (Province prov in save.GetProvinces())
            {
                graph.AddNode((Int3)(Vector3)(MapTools.LocalToScale(prov.center)));
            }

            //connect nodes
            foreach (Province prov in save.GetProvinces())
            {
                foreach(Province link in prov.links)
                {
                    var node1 = AstarPath.active.GetNearest(MapTools.LocalToScale(prov.center)).node;
                    var node2 = AstarPath.active.GetNearest(MapTools.LocalToScale(link.center)).node;
                    var cost = (uint)1;//(uint)(node2.position - node1.position).costMagnitude;
                    node1.AddConnection(node2, cost);
                }
            }
        }));
        AstarPath.active.FlushWorkItems();
    }

    void OnMouseOver()  
    {
        //MapTools.GetInput().MouseInput(this);
    }

    public void Paint(bool paintProv, bool paintBord)
    {
        //provinces
        if (paintProv)
        {
            foreach (Province prov in save.GetProvinces())
            {
                for (float x = prov.graphicalCenter.x - prov.graphicalSize.x / 2; x < 2 + prov.graphicalCenter.x + prov.graphicalSize.x / 2; x++)
                {
                    for (float y = prov.graphicalCenter.y - prov.graphicalSize.y / 2; y < 2 + prov.graphicalCenter.y + prov.graphicalSize.y / 2; y++)
                    {
                        if (map_template.GetPixel((int)x, (int)y) == Color.black)
                        {
                            map_ingame.SetPixel((int)x, (int)y, Color.black);
                        }
                        else if (MapTools.ColToId(map_template.GetPixel((int)x, (int)y)) == prov.id)
                        {
                            map_ingame.SetPixel((int)x, (int)y, prov.owner.color);
                        }
                    }
                }
            }
        }

        //borders
        if (paintBord)
        {
            foreach (Province prov in save.GetProvinces())
            {
                for (float x = prov.graphicalCenter.x - prov.graphicalSize.x / 2; x < 2 + prov.graphicalCenter.x + prov.graphicalSize.x / 2; x++)
                {
                    for (float y = prov.graphicalCenter.y - prov.graphicalSize.y / 2; y < 2 + prov.graphicalCenter.y + prov.graphicalSize.y / 2; y++)
                    {
                        if (map_template.GetPixel((int)x, (int)y) == Color.black)
                        {
                            map_ingame.SetPixel((int)x, (int)y, Color.black);
                        }
                    }
                }
            }
        }

        //apply
        map_ingame.Apply();
    }

    public void HighlightBorder(int id, Color col)
    {
        Paint(false, true);
        Nation nat = new Nation();
        foreach(Nation nation in save.GetNations())
        {
            if (nation.id == id)
            {
                nat = nation;
                break;
            }
        }
        foreach(Province prov in nat.provinces)
        {
            for(int x = (int)(prov.graphicalCenter.x-prov.graphicalSize.x/2); x<(int)(prov.graphicalCenter.x+prov.graphicalSize.x/2)+2; x++)
            {
                for(int y = (int)(prov.graphicalCenter.y-prov.graphicalSize.y/2);y<(int)(prov.graphicalCenter.y+prov.graphicalSize.y/2)+2; y++)
                {
                    if(map_template.GetPixel(x, y)==Color.black)
                    {
                        bool foreign = false;
                        bool own = false;
                        for(int inX = -1; inX<=1; inX++)
                        {
                            for(int inY = -1; inY<=1; inY++)
                            {
                                Color pixCol = map_ingame.GetPixel(x + inX, y + inY);
                                if (pixCol == prov.owner.color)
                                {
                                    own = true;
                                }
                                else if (pixCol != Color.black && pixCol!=col)
                                {
                                    foreign = true;
                                }
                            }
                        }
                        if (foreign && own)
                        {
                            map_ingame.SetPixel(x, y, col);
                        }
                    }
                }
            }
        }
        map_ingame.Apply();
    }

    public string[] GetFields(string line, string separator)
    {
        List<string> fields = new List<string>();
        line += separator;
        do
        {
            fields.Add(line.Substring(0, line.IndexOf(separator)));
            line = line.Remove(0, line.IndexOf(separator) +1);
        } while (!line.Equals(""));
        return fields.ToArray();
    }

    void ProvinceMeasurer()
    {
        //fill data structure
        foreach(Province prov in save.GetProvinces())
        {
            int maxX = 0;
            int minX = map_template.width;
            int maxY = 0;
            int minY = map_template.height;
            for(int x = 0; x<map_template.width; x++)
            {
                for(int y = 0; y<map_template.height; y++)
                {
                    if(MapTools.ColToId(map_template.GetPixel(x, y)) == prov.id)
                    {
                        if (x < minX) {
                            minX = x;
                        }
                        if (x > maxX)
                        {
                            maxX = x;
                        }
                        if (y < minY)
                        {
                            minY = y;
                        }
                        if (y > maxY)
                        {
                            maxY = y;
                        }
                    }
                }
            }
            int xSize = maxX - minX;
            int ySize = maxY - minY;
            prov.graphicalSize = new Vector2(xSize+2, ySize+2);
            prov.graphicalCenter = new Vector2(minX + xSize / 2, minY + ySize / 2);
        }
        ProvinceSaver(",");
    }
    void ProvinceLinker()
    {
        Color backCol = Color.black;
        ColorUtility.TryParseHtmlString("#7860ff", out backCol);
        foreach (Province prov in save.GetProvinces())
        {
            prov.links = new List<Province>();
            for (int x = (int)(prov.graphicalCenter.x - prov.graphicalSize.x / 2); x < (int)(prov.graphicalCenter.x + prov.graphicalSize.x / 2) + 2; x++)
            {
                for (int y = (int)(prov.graphicalCenter.y - prov.graphicalSize.y / 2); y < (int)(prov.graphicalCenter.y + prov.graphicalSize.y / 2) + 2; y++)
                {
                    if(MapTools.ColToId(map_template.GetPixel(x, y)) == prov.id)
                    {
                        for (int inX = -2; inX <= 2; inX++)
                        {
                            for (int inY = -2; inY <= 2; inY++)
                            {
                                Color pixCol = map_template.GetPixel(x + inX, y + inY);
                                if (MapTools.ColToId(pixCol) != prov.id && pixCol!=Color.black && pixCol!=backCol)
                                {
                                    foreach(Province dest in save.GetProvinces())
                                    {
                                        if (dest.id == MapTools.ColToId(pixCol))
                                        {
                                            prov.links.Add(dest);
                                            break;
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
            prov.links = prov.links.Distinct().ToList();
        }
        ProvinceSaver(",");
    }
    public void ProvinceSaver(string separator)
    {
        StreamWriter writer = new StreamWriter(path_definitions);
        foreach (Province prov in save.GetProvinces())
        {
            string line = prov.id + separator + prov.name + separator + prov.graphicalCenter.x + " " + prov.graphicalCenter.y + separator + prov.graphicalSize.x + " " + prov.graphicalSize.y +separator;
            foreach(Province link in prov.links)
            {
                line += link.id + " ";
            }
            line = line.Trim();
            line += separator + prov.pop + separator + prov.dev + separator + prov.center.x + " " + prov.center.y;
            line +="\n";
            writer.Write(line);
        }
        writer.Close();
    }

    private Dictionary<string, Classes.ModBook.Pair> LoadNationMods(string path)
    {
        StreamReader reader = new StreamReader(path);
        string line = "";
        line = reader.ReadToEnd();
        reader.Close();
        string[] fields = GetFields(line, ",");

        Dictionary<string, Classes.ModBook.Pair> dict = new Dictionary<string, Classes.ModBook.Pair>();
        foreach(string field in fields)
        {
            string key = GetFields(field, " ")[0];

            float value = float.Parse((GetFields(field, " ")[1]), CultureInfo.InvariantCulture);
            dict.Add(key, new Classes.ModBook.Pair(value, -1));
        }
        return dict;
    }

    private bool Autosave()
    {
        int freq = PlayerPrefs.GetInt("autosave_frequency");
        if (freq > 0)
        {
            if (save.GetTime().hour == 0)
            {
                if (freq == 1)
                {
                    if (!new SaveManager().SaveGame(autosave: true))
                    {
                        return false;
                    }
                }
                else if (save.GetTime().day == 1)
                {
                    if (freq == 2)
                    {
                        if (!new SaveManager().SaveGame(autosave: true))
                        {
                            return false;
                        }
                    }
                    else if (save.GetTime().month == 0)
                    {
                        if (freq == 3)
                        {
                            if (!new SaveManager().SaveGame(autosave: true))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }
        return true;
    }

}
