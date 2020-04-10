using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.Cryptography;


public class SaveManager
{
    public string location_path;
    public string save_extension = ".save";

    public SaveManager()
    {
        location_path = Application.persistentDataPath+"/Saves/";
    }

    public SaveFile GetSaveFile(string path)
    {
        StreamReader reader = new StreamReader(path);
        try
        {
            SaveFile save = JsonUtility.FromJson<SaveFile>(reader.ReadToEnd());
            reader.Close();
            return save;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
        finally{
            reader.Close();
        }
    }

    public bool LoadGame(string saveName)
    {
        SaveFile save;
        SaveFile saveBase;
        try
        {
            //Load savefile
            save = GetSaveFile(location_path + saveName);
            saveBase = GameObject.Find("Map/Center").GetComponent<MapHandler>().save;
        }catch(Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }

        //restore
        GameObject.Find("Map/Center").GetComponent<MapHandler>().save = save;
        GameObject.Find("Map/Center").GetComponent<MapHandler>().save.Restore(saveBase);

        return true;
    }

    public bool SaveGame(string name = null, bool autosave = false)
    {
        try
        {
            Directory.CreateDirectory(location_path);
            GameObject.Find("Map/Center").GetComponent<MapHandler>().save.Prepare();
            string saveAsJson = JsonUtility.ToJson(GameObject.Find("Map/Center").GetComponent<MapHandler>().save);
            if (saveAsJson.Length == 0)
            {
                return false;
            }

            Debug.Log(saveAsJson);
            FileStream file;
            if (name!=null)
            {
                 file = File.Create(location_path + name + save_extension);
            }
            else
            {
                 file = File.Create(location_path + NewName(autosave) + save_extension);
            }
            StreamWriter writer = new StreamWriter(file);
            writer.Write(saveAsJson);
            writer.Close();
            file.Close();

            GameObject.Find("Map/Center").GetComponent<MapHandler>().save.Clear();
        }catch(Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }

        return true;
    }

    public string NewName(bool autosave = false)
    {
        string nameBase = "";
        nameBase += GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetActiveNation().name;
        nameBase += "_";
        nameBase += GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetTime().year+GameObject.Find("Map/Center").GetComponent<MapHandler>().save.GetTime().startYear;

        if (autosave)
        {
            nameBase += "_auto";
        }
        else if(File.Exists(location_path+nameBase + save_extension))
        {
            int increment = 0;
            while (File.Exists(location_path + nameBase +"_"+increment + save_extension))
            {
                increment++;
            }
            return nameBase + "_" + increment;
        }

        return nameBase;
    }

    public string CalculateChecksum()
    {
        string checksum = "";
        using (var md5 = MD5.Create())
        {
            string path_SaveManager;
            string path_SaveFile;

            if (Application.isEditor)
            {
                path_SaveManager = Application.dataPath + "/Scripts/SaveManager.cs";
                path_SaveFile = Application.dataPath + "/Scripts/Objects/Savefile.cs";
            }
            else
            {
                path_SaveManager = Path.GetDirectoryName(Application.dataPath) + "/Assets/Scripts/SaveManager.cs";
                path_SaveFile = Path.GetDirectoryName(Application.dataPath) + "/Assets/Scripts/Objects/Savefile.cs";
            }

            using (var stream = File.OpenRead(path_SaveManager))
            {
                var hash = md5.ComputeHash(stream);
                checksum += BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            using (var stream = File.OpenRead(path_SaveFile))
            {
                var hash = md5.ComputeHash(stream);
                checksum += BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        return checksum;
    }

}
