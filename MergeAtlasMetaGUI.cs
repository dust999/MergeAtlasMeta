using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public partial class MergeAtlasMeta : EditorWindow
{

    Vector2 _scrollSpritesPos = Vector2.zero;
    Vector2 _scrollSpritesPosDifference = Vector2.zero;

    Vector2 _scrollPrefabsPos1 = Vector2.zero;
    Vector2 _scrollPrefabsPos2 = Vector2.zero;

    Vector2 _scrollMainPos = Vector2.zero;

    bool _isShowSprites = false;
    bool _isShowPrefabs = false;

    void OnGUI() // DRAW WINDOW
    {

        GUIStyle styleBold = new GUIStyle();
        styleBold.fontStyle = FontStyle.Bold;
        //styleBold.fixedHeight = 15f;
        styleBold.alignment = TextAnchor.MiddleLeft;

        GUIStyle styleNormal = new GUIStyle();
       // styleNormal.fontStyle = FontStyle.Normal;
        styleNormal.fixedHeight = 15f;
        styleNormal.alignment = TextAnchor.MiddleLeft;

        #region OUTPUT ATLASES

        // ATLASES BLOCK
        GUILayout.BeginHorizontal();
        GUILayout.Space(5f);
        GUILayout.Label("Select atlases");
        _atlas1 = (Texture2D)EditorGUI.ObjectField(new Rect(15f, 25f, 200f, 200f), "", _atlas1, typeof(Texture2D));
        _atlas2 = (Texture2D)EditorGUI.ObjectField(new Rect(320f, 25f, 200f, 200f), "", _atlas2, typeof(Texture2D));
        GUILayout.EndHorizontal();

        #endregion

        GUILayout.Space(220f);

        // LOAD META BLOCK
        _scrollMainPos = GUILayout.BeginScrollView(_scrollMainPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        if (GUILayout.Button("Get Meta Data", GUILayout.Height(30f)))
        {
            UpdateMetaInfo();
        }

        // OUTPUT PREFABS TITLE
        GUILayout.BeginHorizontal();
        GUILayout.Label("GUID: " + _atlasGUID1, GUILayout.ExpandWidth(true));
        GUILayout.Label("GUID: " + _atlasGUID2, GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();

        if (_sprites1 == null || _sprites2 == null) { GUILayout.EndScrollView(); return; }

        #region OUTPUT SPRITES

        GUILayout.BeginHorizontal();
        _isShowSprites = GUILayout.Toggle(_isShowSprites, " [SPRITES USEED IN ATALSES]");
        GUILayout.EndHorizontal();

        if (_isShowSprites)
        {
            // OUTPUT PREFABS LISTS
            GUILayout.BeginHorizontal();

            _scrollSpritesPosDifference = GUILayout.BeginScrollView(_scrollSpritesPosDifference);
            foreach (SpriteDifferenceData sprites in _spriteDifferenceData)
            {
                string title = sprites.name1;
                if (sprites.id1 != sprites.id2)
                {
                    title = "[" + sprites.id1 + "] " + sprites.name1 + "\t\t -> [" + sprites.id2 + "] " + sprites.name2;

                }
                GUIStyle gs = new GUIStyle();
                if (sprites.isDifferentID)
                {
                    gs.fontStyle = FontStyle.Bold;
                    gs.normal.textColor = Color.red;
                    GUI.backgroundColor = Color.red;
                }
                sprites.isExpanded = GUILayout.Toggle(sprites.isExpanded, title);
                if (sprites.isExpanded)
                {
                    GUI.backgroundColor = Color.white;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("ID 1: " + sprites.id1, gs, GUILayout.Width(position.width / 2 - 15f));
                    EditorGUILayout.LabelField("ID 2: " + sprites.id2, gs, GUILayout.Width(position.width / 2 - 15f));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.TextArea(sprites.allRawData1, GUILayout.Width(position.width / 2 - 15f));
                    EditorGUILayout.TextArea(sprites.allRawData2, GUILayout.Width(position.width / 2 - 15f));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Copy ->"))
                    {
                        if (sprites.name1 != "NONE") 
                            sprites.copyDirection = SpriteDifferenceData.CopyDirection.LeftToRight;
                    }
                    if (GUILayout.Button("<- Copy"))
                    {
                        if (sprites.name2 != "NONE")
                            sprites.copyDirection = SpriteDifferenceData.CopyDirection.RightToLeft;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10f);
                }
                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();

        }
        #endregion

        GUILayout.Space(10f);

        #region EDITED META DATA

        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

        GUILayout.BeginVertical( GUILayout.Height(18f));

        bool isNeedUpdateLeftMeta = false;
        bool isNeedUpdateRightMeta = false;
       
        foreach (SpriteDifferenceData sprite in _spriteDifferenceData)
        {
            if (sprite.copyDirection == SpriteDifferenceData.CopyDirection.none) continue;

            string info = "";

            if (sprite.copyDirection == SpriteDifferenceData.CopyDirection.LeftToRight) {

                isNeedUpdateRightMeta = true;

                info = "[1->2]";
                if (sprite.name1 == "NONE") info += "<remove>";
                if (sprite.name2 == "NONE") info += "<add>";
                if (sprite.allRawData1 == null)
                {
                    info += " (+)";
                    continue;
                }
                if (sprite.allRawData2 == null)
                {
                    info += " (-)";
                    continue;
                }
                if (sprite.allRawData1.Length > sprite.allRawData2.Length) info += " (+)";
                if (sprite.allRawData1.Length < sprite.allRawData2.Length) info += " (-)";
            }
            else { 

                isNeedUpdateLeftMeta = true;

                info = "[1<-2]";
                if (sprite.name1 == "NONE") info += "<add>";
                if (sprite.name2 == "NONE") info += "<remove>";
                if (sprite.allRawData1 == null)
                {
                    info += " (+)";
                    continue;
                }
                if (sprite.allRawData2 == null)
                {
                    info += " (-)";
                    continue;
                }
                if (sprite.allRawData2.Length > sprite.allRawData1.Length) info += " (+)";
                if (sprite.allRawData2.Length < sprite.allRawData1.Length) info += " (-)";
            }

            

            GUILayout.BeginHorizontal();
            string name = (sprite.name1 != "NONE") ? sprite.name1 : sprite.name2;
            GUILayout.Label(name , GUILayout.Height(15f));
            GUILayout.Label(info, styleBold, GUILayout.Height(15f));
            if (GUILayout.Button("x", GUILayout.Height(15f)) )
            {
                sprite.copyDirection = SpriteDifferenceData.CopyDirection.none;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();            
        }
        
        GUILayout.EndVertical();

        if (isNeedUpdateLeftMeta || isNeedUpdateRightMeta)
        {
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("NEED UPDATE META", GUILayout.Height(30f)))
            {
                if (isNeedUpdateLeftMeta)
                    UpdateAtlasMeta(_atlas1, _spriteDifferenceData, true);

                if (isNeedUpdateRightMeta)
                    UpdateAtlasMeta(_atlas2, _spriteDifferenceData);

                UpdateMetaInfo();
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
        }

        #endregion

        GUILayout.Space(10f);

        #region OUTPUT PREFABS

        // OUTPUT PREFABS ENABLE
        GUILayout.BeginHorizontal();
        _isShowPrefabs = GUILayout.Toggle(_isShowPrefabs, " [ASSETS USEED IN ATALSES]");
        GUILayout.EndHorizontal();

        if (!_isShowPrefabs)
        {
            GUILayout.EndScrollView();
            return;
        }

        // OUTPUT PREFABS LISTS
        GUILayout.BeginHorizontal();
        // LEFT PART
        _scrollPrefabsPos1 = GUILayout.BeginScrollView(_scrollPrefabsPos1, GUILayout.ExpandWidth(true));
        foreach (PrefabsData prefab in _prefabsData)
        {
            if (GUILayout.Button(prefab.path))
            {
                SelectAsset(prefab.path);               
            }

            foreach (UsedSpriteData sprite in prefab.usedSprites)
            {
                string name1 = "[" + sprite.spriteName + "]";
                string name2 = name1;
                if (sprite.atlasID == _atlasGUID1) name2 = "";
                else name1 = "";

                GUILayout.BeginHorizontal();
                GUILayout.Label(name1, GUILayout.Width(position.width / 2));
                GUILayout.Label(name2, GUILayout.Width(position.width / 2));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("MOVE ->", GUILayout.Width(position.width / 2)))
            {                
                if (UpdatePrefab(prefab, _atlasGUID1, _atlasGUID2))
                {
                    Debug.Log("[UPDATED]: " +prefab.path);
                }
            }
            if(GUILayout.Button("<- MOVE", GUILayout.Width(position.width / 2)))
            {
                if (UpdatePrefab(prefab, _atlasGUID2, _atlasGUID1))
                {
                    Debug.Log("[UPDATED]: " + prefab.path);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(30f);
        }
        GUILayout.EndScrollView();       

        GUILayout.EndHorizontal();

        #endregion

        GUILayout.EndScrollView();
    }

    void UpdateMetaInfo()
    {
        _spriteDifferenceData = new List<SpriteDifferenceData>();
        _prefabsData = new List<PrefabsData>();
        _assetsUsedGUID1 = new List<string>();
        _assetsUsedGUID2 = new List<string>();

        if (_atlas1 == null || _atlas2 == null) { GUILayout.EndScrollView(); return; }
        if (_atlas1 == _atlas2) { GUILayout.EndScrollView(); return; }

        ReadMeta(_atlas1, ref _sprites1, ref _atlasGUID1, ref _assetsUsedGUID1);
        ReadMeta(_atlas2, ref _sprites2, ref _atlasGUID2, ref _assetsUsedGUID2);

        CompareSprites(ref _sprites1, ref _sprites2, ref _spriteDifferenceData);

        GetAllAssetsWithGUID(_atlasGUID1, ref _assetsUsedGUID1, 1);
        GetAllAssetsWithGUID(_atlasGUID2, ref _assetsUsedGUID2, 2);
    }
}