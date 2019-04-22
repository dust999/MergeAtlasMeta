using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public partial class MergeAtlasMeta : EditorWindow
{
    #region PREFABS AND SCENES USED IN ATLASES

    void GetAllAssetsWithGUID(string guid, ref List<string> AssetsUsedGUID, int atlasID, string path = "")
    {
        // SEARCH IN FILES
        path = (path == "") ? "Assets/" : path;

        string[] fileEntries = Directory.GetFiles(path);
        foreach (string fileName in fileEntries)
        {
            if (!IsSceneOrPrefab(fileName)) continue;

            string prefabDataFromFile = "";
            if (!IsContainGUID(guid, fileName, ref prefabDataFromFile)) continue;

            if (AssetsUsedGUID.Contains(fileName)) continue;

            AssetsUsedGUID.Add(fileName);
            GetDataFromPrefab(guid, prefabDataFromFile, fileName, atlasID);
        }

        // SEARCH IN DIRS RECURSIVETLY
        string[] dirsEntries = Directory.GetDirectories(path);

        foreach (string dir in dirsEntries)
        {
            GetAllAssetsWithGUID(guid, ref AssetsUsedGUID, atlasID, dir);
        }
    }

    void GetDataFromPrefab(string guid, string prefabDataFromFile, string fileName, int altasID)
    {
        if (prefabDataFromFile == "") return;

        PrefabsData prefabData = GetStoredPrefabByPath(fileName); // GET OLD OR CREATE NEW

        prefabData.path = fileName;

        if (altasID == 1)
            prefabData.isUsedAtlas1 = true;

        if (altasID == 2)
            prefabData.isUsedAtlas2 = true;

        if (prefabData.usedSprites == null)
            prefabData.usedSprites = new List<UsedSpriteData>();

        // GET ALL SPRITES FORM CURRNET ATLAS IN PREFAB
        string[] spritesData = prefabDataFromFile.Split(new string[] { "guid: " + guid }, System.StringSplitOptions.None);
        for (int i = 0; i < spritesData.Length - 1; i++)
        {
            UsedSpriteData usedSprite = new UsedSpriteData();

            string[] spriteID = spritesData[i].Split(new string[] { "fileID: " }, System.StringSplitOptions.None);
            string id = spriteID[spriteID.Length - 1];
            id = id.Substring(0, id.Length - 2);

            if (IsInUsedSprites(prefabData.usedSprites, guid, id)) continue;

            usedSprite.atlasID = guid;
            usedSprite.spriteID = id;
            usedSprite.spriteName = GetSpriteNameById(id, altasID);

            prefabData.usedSprites.Add(usedSprite);
        }

        _prefabsData.Remove(prefabData);
        _prefabsData.Add(prefabData); // SAVE PREFAB DATA
    }

    PrefabsData GetStoredPrefabByPath(string path)
    {
        foreach (PrefabsData p in _prefabsData)
        {
            if (p.path == path) return p;
        }
        return new PrefabsData();
    }

    bool IsInUsedSprites(List<UsedSpriteData> usedSpriteData, string guid, string id)
    {
        foreach (UsedSpriteData usedSprite in usedSpriteData)
        {
            if (usedSprite.spriteName == id && usedSprite.atlasID == guid) return true;
        }
        return false;
    }

    bool IsSceneOrPrefab(string fileName)
    {
        if (fileName.Contains(".meta")) return false;
        if (!fileName.Contains(".unity") && !fileName.Contains(".prefab")) return false;
        return true;
    }

    bool IsContainGUID(string guid, string assetPath, ref string prefabData)
    {
        bool isContainsGUID = false;
        string data = LoadAssetAtPath(assetPath);
        if (data.Contains(guid))
        {
            prefabData = data;
            isContainsGUID = true;
        }
        return isContainsGUID;
    }

    void SelectAsset(string path)
    {
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
    }

    bool UpdatePrefab(PrefabsData prefab, string AtlasFromGUID, string AtlasToGUID)
    {
        string prefabData = LoadAssetAtPath(prefab.path);

        //fileID: 21300154, guid: 42387af249ba57d42b8083c8cbaee62e
        foreach (UsedSpriteData sprite in prefab.usedSprites)
        {
            if (sprite.atlasID != AtlasFromGUID) continue;

            SpriteData[] sprites = AtlasToGUID.Equals(_atlasGUID1) ? _sprites1 : _sprites2; // GET SPRITES LIST

            int id = GetSpriteID(sprite.spriteName, sprites);
            if (id < 0) continue;

            string newID = sprites[id].id; // GET NEW SPRITE ID

            string oldData = "fileID: " + sprite.spriteID + ", guid: " + sprite.atlasID;
            string newData = "fileID: " + newID + ", guid: " + AtlasToGUID;

            prefabData = prefabData.Replace(oldData, newData);
        }
        
        SaveAssetAtPath(prefab.path, prefabData);

        ReImportAsset(prefab.path);

        return true;
    }
    #endregion
}