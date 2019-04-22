using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public partial class MergeAtlasMeta : EditorWindow
{
    #region Atlases META and Sprites

    void ReadMeta (Texture2D atlas,  ref SpriteData[] sprites, ref string GUID, ref List<string> AssetsUsedGUID)
    {
        string metaData = GetAtlasMetaData(atlas);

        string guid = GetAtlasGUID(metaData);
        GUID = guid;
        GetAtlasSprites(metaData, ref sprites);       

        GetAllRawData(metaData, ref sprites);
    }    

    string GetAtlasGUID(string metaData)
    {
        string[] lines = metaData.Split('\n');

        string guid = null;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("guid:")) {
                guid = lines[i].Split(' ')[1]; // GET VALUE FORM ROW
                break;
            }
        }
        return guid.Trim();
    }

    void GetAtlasSprites(string metaData, ref SpriteData[] sprites)
    {
        string[] lines = metaData.Split('\n');

        int startSpritesRow = -1;
        int endSpritesRow = -1;

        for (int i=0; i< lines.Length; i++)
        {
            if (lines[i].Contains("fileIDToRecycleName:"))
            {
                startSpritesRow = i + 1; continue;
            }
            if( startSpritesRow>0 && lines[i].Contains("externalObjects:"))
            {
                endSpritesRow = i; break;
            }         
        }

        int spritesCount = endSpritesRow - startSpritesRow;
        sprites = new SpriteData[spritesCount];

        for (int i = startSpritesRow; i < startSpritesRow + spritesCount; i++)
        {
            string[] idNameData = lines[i].Split(':');
            string id = idNameData[0].Replace(" ","").Trim();
            string name = idNameData[1].Replace(" ", "").Trim();

            int currentSprite = i - startSpritesRow;
            sprites[currentSprite] = new SpriteData(id, name);
        }       
    }

    void CompareSprites(ref SpriteData[] sprites1, ref SpriteData[] sprites2, ref List<SpriteDifferenceData> spriteDifferenceData)
    {
        for (int i=0; i< sprites1.Length; i++)// SpriteData sprite in sprites1
        {
            bool isDifferent = false;
            bool isDifferentID = false;

            int id = GetSpriteID(sprites1[i].name, sprites2);
            if (id < 0)
            {
                SpriteDifferenceData item = new SpriteDifferenceData();

                item.isDifferentID = true;
                item.name1 = sprites1[i].name;
                item.name2 = "NONE";
                item.id1 = sprites1[i].id;
                item.id2 = "-------";
                item.allRawData1 = sprites1[i].allRawData;
                item.allRawData2 = "";

                spriteDifferenceData.Add(item);                
                continue;
            }           

            if (sprites1[i].allRawData == sprites2[id].allRawData)
            {                
                sprites1[i].isDifferent = false;
                sprites2[id].isDifferent = false;
            }
            else
            {
                isDifferent = true;
            }

            if (sprites1[i].id == sprites2[id].id)
            {
                sprites1[i].isDifferentID = false;
                sprites2[id].isDifferentID = false;                
            }
            else
            {
                isDifferent = true;
                isDifferentID = true;
            }

            if (isDifferent)
            {
                SpriteDifferenceData item = new SpriteDifferenceData();

                if (isDifferentID) item.isDifferentID = true;

                item.name1 = sprites1[i].name;
                item.name2 = sprites2[id].name;
                item.id1 = sprites1[i].id;
                item.id2 = sprites2[id].id;
                item.allRawData1 = sprites1[i].allRawData;
                item.allRawData2 = sprites2[id].allRawData;

                spriteDifferenceData.Add(item);
            }
        }

        for (int i = 0; i < sprites2.Length; i++)// SpriteData sprite in sprites2
        {
            int id = GetSpriteID(sprites2[i].name, sprites1);
            if (id < 0)
            {
                SpriteDifferenceData item = new SpriteDifferenceData();

                item.isDifferentID = true;
                item.name1 = "NONE";
                item.name2 = sprites2[i].name;
                item.id1 = "-------";
                item.id2 = sprites2[i].id;
                item.allRawData1 = "";
                item.allRawData2 = sprites2[i].allRawData;    
                
                spriteDifferenceData.Add(item);                               
                continue;
            }
        }
    }

    string SpriteAdditionalDetails(SpriteData sprite)
    {
        string details = "";

        if (sprite.isDifferent)
            details +="[D]";

        if (sprite.isDifferentID)
            details += "[ID]";

        return details;
    }
    
    void GetAllRawData(string meta, ref SpriteData [] sprites)
    {
        string[] spritesPart = meta.Split(new string[] { "sprites:" }, System.StringSplitOptions.None); // CUT SPRITES FROM TOP
        string[] spritesLines = spritesPart[1].Split('\n'); // CUT BY LINE

        int currentSpriteID = -1;        
        string currentRaw = "";
        bool isStartOfSprite = false;
        bool isEndOfSprite = false;
        bool isEndOfSprites = false;

        foreach (string row in spritesLines)
        {
            if (row.Contains("outline:") && isEndOfSprite) // LAST LINE OF SPRITES
            {                
                currentRaw = currentRaw.Substring(0, currentRaw.Length - 1); // current item               

                if (currentSpriteID > -1)
                {
                    //Debug.Log(currentRaw);
                    sprites[currentSpriteID].allRawData = currentRaw; // SAVE RAW DATA                        
                }                
                break; // EXIT FORM LOOP
            }

            if (row.Contains("- serializedVersion:")) { // FIRST LINE OF EACH SPRITE

                isStartOfSprite = true; 
                
                if (isEndOfSprite) { // IF SPRITE DATA ENDED STORE IT
                    currentRaw = currentRaw.Substring(0, currentRaw.Length - 1); // current item

                    if (currentSpriteID > -1) // IF HAS ID
                    {
                        sprites[currentSpriteID].allRawData = currentRaw; // SAVE RAW DATA                        
                    }

                    currentSpriteID = -1;
                    currentRaw = "";
                    isEndOfSprite = false;
                }                           
            }

            // GET NAME
            if (isStartOfSprite && row.Contains("name:") && currentSpriteID < 0) {
                string[] name = row.Split(':');
                currentSpriteID = GetSpriteID(name[1].Trim(), sprites);
            }

            if (row.Contains("weights:"))
                isEndOfSprite = true;

            if (!isStartOfSprite)
                continue;

            currentRaw += row.Replace("\r","") + "\n"; 
        }

        if (currentRaw.Length > 2)
            currentRaw = currentRaw.Substring(0, currentRaw.Length - 1);
    }

    int GetSpriteID(string name, SpriteData[] sprites)
    {
        for (int i = 0; i < sprites.Length; i++)
        {            
            if (sprites[i].name.Equals(name)) {
                return i;
            }
        }
        return -1;
    }

    string GetSpriteNameById(string id, int i)
    {
        SpriteData [] sprites;

        if (i == 1)
            sprites = _sprites1;
        else
            sprites = _sprites2;

        foreach (SpriteData sprite in sprites)
            if (sprite.id == id) return sprite.name;

        return "";
    }

    bool AddSpriteIfCan(ref string metaData, SpriteDifferenceData spriteToAdd)
    {
        bool isLeftAtlas = true;
        bool isNeedAddSprtieToSpritesList = true;

        SpriteData[] sprites;

        if (spriteToAdd.copyDirection == SpriteDifferenceData.CopyDirection.LeftToRight)     isLeftAtlas = false;

        sprites = (isLeftAtlas) ? _sprites1 : _sprites2;

        string AddingSpriteID = (isLeftAtlas) ? spriteToAdd.id2 : spriteToAdd.id1; // INVERT IDs (COPY FORM -> TO)

        SpriteData[] spritesNew = new SpriteData[sprites.Length + 1];       

        for (int i = 0; i < sprites.Length; i++)
        {
            if(sprites[i].id == AddingSpriteID)
            {
                Debug.LogError("ATLAS CONTAINS ID:" + AddingSpriteID);
                //return false;
                isNeedAddSprtieToSpritesList = false;
            }

            spritesNew[i] = sprites[i];
        }
        
        string addingName = (isLeftAtlas) ? spriteToAdd.name2 : spriteToAdd.name1;
        string addingRawData = (isLeftAtlas) ? spriteToAdd.allRawData2 : spriteToAdd.allRawData1;

        spritesNew[spritesNew.Length - 1] = new SpriteData(AddingSpriteID, addingName);
        spritesNew[spritesNew.Length - 1].allRawData = addingRawData;
        spritesNew[spritesNew.Length - 1].isExpanded = false;
        spritesNew[spritesNew.Length - 1].isDifferent = false;
        spritesNew[spritesNew.Length - 1].isDifferentID = false;

        metaData = (isNeedAddSprtieToSpritesList) ? AddSptiteIntoSpriteList(metaData, spritesNew) : metaData; // ADD TO SPRITES LIST AT THE TOP OF META
        metaData = AddSpriteRawDataInAtlasMeta(metaData, spritesNew); // ADD RAW DATA TO THE BOTTOM OF META

        return true;
    }

    string AddSptiteIntoSpriteList(string metaData, SpriteData[] sprites)
    {
        string endOfRow = metaData.Contains("\r") ? "\r" : "";

        // START OF SPRITES
        string[] metaDataPart = metaData.Split(new string[] { "fileIDToRecycleName:" }, System.StringSplitOptions.None);
        string startMeta = metaDataPart[0] + "fileIDToRecycleName:" + endOfRow + "\n"; // PREFIX SPRITES LIST

        // END OF SPRIRES
        metaDataPart = metaDataPart[1].Split(new string[] { "externalObjects:" }, System.StringSplitOptions.None);

        int offsetChars = (endOfRow == "") ? 1 : 2;
        string spritesPart = metaDataPart[0].Substring(offsetChars, metaDataPart[0].Length - offsetChars); // REMOVE FIRST \N symbol (may be problrems with \r symbol)
        string endMeta = "externalObjects:" + metaDataPart[1]; // ALL PART OF ATLAS EXCEPT SPRITES AND HEADER

        metaDataPart = spritesPart.Split('\n'); // SPLIT BY LINES
        string lastItem = metaDataPart[metaDataPart.Length - 2]; // GET LAST ITEM
        //"    21300182: OtherObjects_Whale_Smile"        

        string whiteSpace = ""; // NEED TO ADD TO ADDING'S ITEM
        for (int i = 0; i < lastItem.Length; i++)
        {
            if (lastItem[i] != ' ') break;
            whiteSpace += " ";
        }

        string addItem = whiteSpace + sprites[sprites.Length - 1].id + ": " + sprites[sprites.Length - 1].name + endOfRow + "\n"; // COMBINE NEW SYMBOL WITH WHITE SPACE PREFIX

        metaDataPart = spritesPart.Split(new string[] { lastItem }, System.StringSplitOptions.None);

        string oldSprites = metaDataPart[0] + lastItem + "\n"; // ALL SPRITES PLUS LAST ITEM

        metaDataPart = metaDataPart[1].Split('\n');        

        spritesPart = oldSprites + addItem + metaDataPart[1]; // ALL SPRITES LIST PLUS NEW ITEM PLUS END OF PART SPRITES

        metaData = startMeta + spritesPart + endMeta; // ADD NEW SPRITES LIST (ID:NAME) IN ATLAS META

        return metaData;
    }

    string AddSpriteRawDataInAtlasMeta(string metaData, SpriteData [] sprites)
    {
        string endOfRow = metaData.Contains("\r") ? "\r" : "";
        // ADD RAW DATA (UPDATE LAST ITEM  +  NEW)
        for(int i = sprites.Length - 2; i >= 0; i--) { //SKIP ALL EMPTY RAW DATAS IN ATLAS

            if (string.IsNullOrEmpty(sprites[i].allRawData)) continue;     

            if(endOfRow != "")
            {
                sprites[i].allRawData = sprites[i].allRawData.Replace("\n", "\r\n");
                sprites[sprites.Length - 1].allRawData= sprites[sprites.Length - 1].allRawData.Replace("\n", "\r\n");
            }

            string addToTheEndNewSprite = sprites[i].allRawData + endOfRow + "\n" + sprites[sprites.Length - 1].allRawData; // COMBINE LAST ITEM WITH NEW ONE

            metaData = metaData.Replace(sprites[i].allRawData, addToTheEndNewSprite); // ADD NEW SPRITE TO THE END OF ATLAS META
            break;
        }

        return metaData;
    }
    
    #endregion
}