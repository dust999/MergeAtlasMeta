using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public partial class MergeAtlasMeta : EditorWindow
{
    Texture2D _atlas1;
    string _atlasGUID1;
    SpriteData[] _sprites1;
    List<string> _assetsUsedGUID1;    

    Texture2D _atlas2;
    string _atlasGUID2;
    SpriteData[] _sprites2;
    List<string> _assetsUsedGUID2;

    List <SpriteDifferenceData> _spriteDifferenceData;
    int differenceCount = 0;

    List<PrefabsData> _prefabsData;

    [MenuItem("Tools/Merge Atlases Meta")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MergeAtlasMeta));
    } 
}

public class SpriteData
{
    public bool isDifferent = true;
    public bool isDifferentID = true;
    public bool isExpanded = false;
    public string id;
    public string name;
    /*public Rect sizes;
    public Vector2 pivot;           // TODO: NEED TO ADD NORMAL YAML PARSER FROM GITHUB TO MAKE IT EASY
    public string bonesData;*/
    public string allRawData;

    public SpriteData(string id, string name)
    {
        this.id = id;
        this.name = name;
    }
}

public class SpriteDifferenceData
{
    public enum CopyDirection {none, LeftToRight, RightToLeft };
    public CopyDirection copyDirection = CopyDirection.none;

    public bool isDifferentID = false;
    public bool isExpanded = false;

    public string id1 = "";
    public string id2 = "";
    public string name1 = "";
    public string name2 = "";
    /*public Rect sizes;
    public Vector2 pivot;           // TODO: NEED TO ADD NORMAL YAML PARSER FROM GITHUB TO MAKE IT EASY
    public string bonesData;*/
    public string allRawData1 = "";
    public string allRawData2 = "";    
}

public class PrefabsData
{     
    public string path;

    public List<UsedSpriteData> usedSprites;

    public bool isUsedAtlas1 = false;
    public bool isUsedAtlas2 = false;
}

public class UsedSpriteData
{
    public string atlasID;
    public string spriteID;    
    public string spriteName;
}