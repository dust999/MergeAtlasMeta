using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public partial class MergeAtlasMeta : EditorWindow
{
    #region WORK WITH FILES

    string GetAtlasMetaPath(Texture2D atlas)
    {
        string path = AssetDatabase.GetAssetPath(atlas);
        return path + ".meta";
    }

    string GetAtlasMetaData(Texture2D atlas)
    {
        string path = GetAtlasMetaPath(atlas);

        string metaData = "";

        using (StreamReader metaReader = new StreamReader(path))
        {
            if (metaReader == null) { Debug.Log("No meta data found for Atlas:" + path); return ""; }
            metaData = metaReader.ReadToEnd();
        }

        return metaData;
    }

    void UpdateAtlasMeta(Texture2D atlas, List<SpriteDifferenceData> sprites, bool needUpdateLeftMeta = false)
    {
        string metaPath = GetAtlasMetaPath(atlas);// + "_new"; //DEBUGING NAME       
        string metaData = GetAtlasMetaData(atlas);

        SpriteDifferenceData.CopyDirection directionOfCopyData = needUpdateLeftMeta ? SpriteDifferenceData.CopyDirection.RightToLeft : SpriteDifferenceData.CopyDirection.LeftToRight;

        foreach (SpriteDifferenceData sprite in sprites)
        { 
            if (sprite.copyDirection != directionOfCopyData) continue;

            Debug.Log(sprite.name1 + " " + directionOfCopyData.ToString());            

            string rawData = sprite.allRawData1;
            string rawdDataReplaced = sprite.allRawData2;

            if (!needUpdateLeftMeta) // NEED UPDATE RIGHT META
            {
                rawData = sprite.allRawData2;
                rawdDataReplaced = sprite.allRawData1;
            }           

            if (rawData == "" || rawdDataReplaced == "")
            {
                Debug.Log("EMPTY DATA" + sprite.name1);
                if (AddSpriteIfCan(ref metaData, sprite))
                {
                    sprite.copyDirection = SpriteDifferenceData.CopyDirection.none; // CLEAR UPDATE FLAG FOR SPRITE 
                    Debug.Log("ADD NEW SPRITE");
                }

                continue;
            }

            if (string.IsNullOrEmpty(rawData)) {
                AddSpriteIfCan(ref metaData, sprite); // UPDATE META IF RAW DATA IS EMPTY
            }
            else {
                if (metaData.Contains("\r"))
                {
                    rawData =           rawData.Replace("\n", "\r\n");
                    rawdDataReplaced =  rawdDataReplaced.Replace("\n", "\r\n");
                }
                metaData = metaData.Replace(rawData, rawdDataReplaced); // UPDATE META IF RAW DATA HAS IT
            }

            sprite.copyDirection = SpriteDifferenceData.CopyDirection.none; // CLEAR UPDATE FLAG FOR SPRITE 
        }

        using (StreamWriter file = new StreamWriter(metaPath, false)) // WRTITE NEW ATLAS META INTO FILE
        {
            file.Write(metaData);            
        }

        // RELOAD UPDATE ATLAS META INTO MEMORY
        string[] atlasPath = metaPath.Split(new string[] { ".meta" }, System.StringSplitOptions.None);
        ReImportAsset(atlasPath[0]);
    }

    void ReImportAsset(string assetPath)
    {
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }

    string LoadAssetAtPath(string assetPath)
    {
        string data;
        using (StreamReader dataReader = new StreamReader(assetPath))
        {
            data = dataReader.ReadToEnd();          
        }
        return data;
    }

    void SaveAssetAtPath(string assetPath, string data)
    {
        using (StreamWriter file = new StreamWriter(assetPath, false)) // WRTITE NEW ATLAS META INTO FILE
        {
            file.Write(data);
        }
    }

    #endregion
}