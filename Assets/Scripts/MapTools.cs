using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapTools
{
    public static MapHandler GetMap()
    {
        return GameObject.Find("Map/Center").GetComponent<MapHandler>();
    }
    public static SaveFile GetSave()
    {
        return GetMap().save;
    }
    public static InterfaceHandler GetInterface()
    {
        return GameObject.Find("Canvas").GetComponent<InterfaceHandler>();
    }
    public static InputHandler GetInput()
    {
        return GameObject.Find("Map/Center").GetComponent<InputHandler>();
    }
    public static Toast GetToast()
    {
        return GameObject.Find("UI_Toast").GetComponent<Toast>();
    }
    public static Hint GetHint()
    {
        return GameObject.Find("UI_Hint").GetComponent<Hint>();
    }
    public static Fade GetFade()
    {
        return GameObject.Find("UI_Fade").GetComponent<Fade>();
    }

    public static Province IdToProv(int id)
    {
        return GetSave().GetProvinces().Find((x) => x.id == id);
    }
    public static Vector2 ScaleToLocal(Vector2 scale)
    {
        float xScale = GetMap().transform.localScale.x * 10;
        float yScale = GetMap().transform.localScale.z * 10;
        float xRes = GetMap().map_ingame.width;
        float yRes = GetMap().map_ingame.height;
        float x = ((scale.x / xScale) * xRes) + (xRes / 2);
        float y = ((scale.y / yScale) * yRes) + (yRes / 2);
        return new Vector2(x, y);
    }
    public static Vector2 LocalToScale(Vector2 local)
    {
        float xScale = GetMap().transform.localScale.x * 10;
        float yScale = GetMap().transform.localScale.z * 10;
        float xRes = GetMap().map_ingame.width;
        float yRes = GetMap().map_ingame.height;
        float x = ((local.x - (xRes / 2)) / xRes) * xScale;
        float y = ((local.y - (yRes / 2)) / yRes) * yScale;
        return new Vector2(x, y);
    }
    public static int ColToId(Color col)
    {
        string hex = ColorUtility.ToHtmlStringRGB(col);
        if (!hex.StartsWith("FF"))
        {
            return -1;
        }
        int id = int.Parse(hex.Substring(2));
        if (id > GetSave().GetProvinces().Count)
        {
            return -1;
        }
        return id;
    }
    public static Classes.Nation IdToNat(int id)
    {
        return GetSave().GetNations().Find((x) => x.id == id);
    }
    public static Province ScaleToProv(Vector2 scale)
    {
        Vector2 local = ScaleToLocal(scale);
        return IdToProv(ColToId(GetMap().map_template.GetPixel((int)local.x, (int)local.y)));
    }
    public static Classes.Army IdToArm(int id)
    {
        return GetSave().GetArmies().Find((x) => x.id == id);
    }
    public static int NewId()
    {
        bool isUnique = true;
        int randId = 0;
        do
        {
            randId = UnityEngine.Random.Range(0, 1000000000);
            foreach (Classes.Army a in GetSave().GetArmies())
            {
                if (a.id == randId)
                {
                    isUnique = false;
                    break;
                }
            }
        } while (!isUnique);
        return randId;
    }

    public static bool GameStarted()
    {
        return !GetInterface().GetActiveInterface().Equals("start");
    }
}
