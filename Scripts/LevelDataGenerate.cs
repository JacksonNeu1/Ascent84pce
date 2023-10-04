using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Rendering;



public class SpriteTable {
    public const int maxSize = 255;
    public int size = 0;
    public string name;
    public int index; //index in sprite table table
    public List<string> uniqueDecompressedSprites = new List<string>();
}
public class DataFrame {
    public SpriteTable spriteTable;
    public List<DataFrameElement> elements;
    public int spriteGroupIndex; //only used for SGs
    public List<string> GetSpriteNames() //returns all sprites that are used in this frame
    {
        List<string> outp = new List<string>();
        foreach(DataFrameElement e in elements)
        {
            outp.Add(spriteTable.uniqueDecompressedSprites[e.index]);//to prevent duplicate sprites being output more than once
        }
        return outp;
    }

}

public class DataFrameElement : IComparable<DataFrameElement> {
    //also used for sprite group elements
    public int y;
    public int height;
    public int x;
    public int index;
    public int orderInLayer;//use to sort elements in frame
    public string decompressedSpriteName; //Sprite name + useMode + load Index
    public int CompareTo(DataFrameElement other)
    {
        return orderInLayer.CompareTo(other.orderInLayer);
    }

}




//Sprite tables store a list of pointers to sprite data, which is decompressed and has unique sprite, flip, offset, and loaded index (number of times it is loaded during game)

//during game, sprites need to be decompressed into memory locations to match those in the sprite tables 
//each object frame has an associated sprite table 
//each object needs to be associeated with a unique decompressed sprite + loaded index


//Loop through all objects in game to generate unique decompressed sprites with load indeces 
//Loop through all objects again to assign each object its unique decompressed sprite + load index in the form of a string
//and group these strings into sprite tables and create object data frames + sprite groups

//Use unique decompressed sprites to precompute memory allocation, and associate memory adresses with decompressed sprite strings



public class LevelDataGenerate : MonoBehaviour
{
    private const int PPU = 10;
    public GameObject FGParent;
    public GameObject MGParent;
    public GameObject MG2Parent;
    public GameObject BGParent;
    public GameObject AlwaysActiveParent;

    private Dictionary<string, ConvertedSprite> convertedSprites = new Dictionary<string, ConvertedSprite>();
    private Dictionary<string,DecompressedSprite> DecompressedSprites = new Dictionary<string, DecompressedSprite>();

    private List<SpriteTable> spriteTables = new List<SpriteTable>();

    private Dictionary<string, DataFrame> spriteGroups = new Dictionary<string, DataFrame>();

    private int LevelDataSize = 0;
    private int SpriteGroupSize = 0;
    private int SpriteTablesSize = 0;
    private int CompressedSpriteSize = 0;
    void Start()
    {
        spriteTables.Add(new SpriteTable());
        spriteTables[0].name = "Sprite_Table_0";
        spriteTables[0].index = 0;

        DecompressedSprite.MaxUnloadFrame = 50;//TODO Set to 255
        //Spit objects into associated frames and generates sprite data
        //Loop through all objects in game to generate unique decompressed sprites with load indeces
        //Frames only contain solo sprites or sprite group parents
        print("Generating Object Frames");
        List<GameObject>[] FGobjectFrames = GetObjectFrames(FGParent);
        List<GameObject>[] MGobjectFrames = GetObjectFrames(MGParent);
        List<GameObject>[] MG2objectFrames = GetObjectFrames(MG2Parent);
        List<GameObject>[] BGobjectFrames = GetObjectFrames(BGParent);
        GetObjectFrames(AlwaysActiveParent); //Generates sprite data 

        //convert objects to data frames
        print("Generating Data Frames");
        List<DataFrame> FGFrames = GenerateDataFrames(FGobjectFrames,FGParent);
        List<DataFrame> MGFrames = GenerateDataFrames(MGobjectFrames,MGParent);
        List<DataFrame> MG2Frames = GenerateDataFrames(MG2objectFrames, MG2Parent);
        List<DataFrame> BGFrames = GenerateDataFrames(BGobjectFrames,BGParent);
        //Compute memory allocation
        print("Calculating Memory Allocation");
        DecompressCallManager decompManager = new DecompressCallManager();
        decompManager.CalculateMemoryLocations(DecompressedSprites.Values.ToList());

        print("Decompress Calls size " + decompManager.DecompressCallSize + " Bytes");

        //convert to strings and write
        int totalLevelDataSize = 0;
        LevelDataSize = 0;
        string FGLevelData = DataFramesToString(FGFrames, "FG_Data");
        print("FG level data size: " + LevelDataSize + " Bytes");
        totalLevelDataSize += LevelDataSize;
        LevelDataSize = 0;
        string MGLevelData = DataFramesToString(MGFrames, "MG_Data");
        print("MG level data size: " + LevelDataSize + " Bytes");
        totalLevelDataSize += LevelDataSize;
        LevelDataSize = 0;
        string MG2LevelData = DataFramesToString(MG2Frames, "MG2_Data");
        print("MG2 level data size: " + LevelDataSize + " Bytes");
        totalLevelDataSize += LevelDataSize;
        LevelDataSize = 0;
        string BGLevelData = DataFramesToString(BGFrames, "BG_Data");
        print("BG level data size: " + LevelDataSize + " Bytes");
        totalLevelDataSize += LevelDataSize;

        print("Total level data size " + totalLevelDataSize + " Bytes");



        FileWrite.WriteString(FGLevelData, "FG_Data");
        FileWrite.WriteString(MGLevelData, "MG_Data");
        FileWrite.WriteString(MG2LevelData, "MG2_Data");
        FileWrite.WriteString(BGLevelData, "BG_Data");

        string spriteTableData = SpriteTablesToString();
        FileWrite.WriteString(spriteTableData, "Sprite_Tables");
        print("Total sprite table size " + SpriteTablesSize + " Bytes");

        string spriteGroupData = SpriteGroupsToString();
        FileWrite.WriteString(spriteGroupData, "Sprite_Groups");
        print("Total Sprite Group size " + SpriteGroupSize + " Bytes");

        WriteCompressedSpriteData(convertedSprites.Values.ToList());
        print("Total Compressed Sprite Data size " + CompressedSpriteSize + " Bytes");

        print("Total nonprogram file size " + (totalLevelDataSize + SpriteTablesSize + SpriteGroupSize + CompressedSpriteSize + decompManager.DecompressCallSize) + " Bytes");

    }
    private string SpriteTablesToString()
    {
        string spriteTableTable = "Sprite_Table_Table:\n";
        string spriteTableData = "";
        foreach (SpriteTable table in spriteTables)
        {
            spriteTableTable += "\t.dl " + table.name + "\n";
            spriteTableData += table.name + ":\n";
            SpriteTablesSize += 3;
            foreach (string spr in table.uniqueDecompressedSprites)
            {
                spriteTableData += "\t.dl " + spr + "\n";
                SpriteTablesSize += 3;
            }
            spriteTableData += "\n";
        }
        
        return spriteTableTable + "\n\n" + spriteTableData;
    }

    private string DataFramesToString(List<DataFrame> dFrames, string baseName)
    {
        string s = "";
        string lookuptbl = baseName + "_frame_table:\n";
        for(int i = dFrames.Count-1; i>=0; i--)
        {
            DataFrame frame = dFrames[i];
            s += baseName + "_" + i + ":\n";
            lookuptbl += "\t.dl "+ baseName + "_" + (dFrames.Count - 1 - i) + "\n";
            s += "\t.db " + frame.spriteTable.index + "\n";
            s += "\t.db " + frame.elements.Count + "\n";
            LevelDataSize += 5;//table element + table inxed + count
            foreach (DataFrameElement element in frame.elements)
            {
                s += "\t.db ";
                s += element.y + ", ";
                s += element.height + ", ";
                s += element.x + ", ";
                s += element.index +"  ;" + element.decompressedSpriteName + "\n";
                LevelDataSize += 4;
            }
            s += "\n";
        }
        s += "\n\n";
        return lookuptbl + "\n\n\n" + s;
    }

    private string SpriteGroupsToString()
    {
        string s = "";
        string sgtable = "Sprite_Groups:\n";

        List<DataFrame> allSG = spriteGroups.Values.ToList();
        List<string> sgNames = spriteGroups.Keys.ToList();
        for(int i = 0; i < allSG.Count; i++)
        {
            DataFrame SG = allSG[i];
            s += sgNames[i] + ":\n";
            sgtable += "\t.dl " + sgNames[i] + "\n";
            s += "\t.db " + SG.spriteTable.index + "\n";
            s += "\t.db " + SG.elements.Count + "\n";
            SpriteGroupSize += 5;
            foreach (DataFrameElement element in SG.elements)
            {
                s += "\t.db ";
                s += element.y + ", ";
                s += element.height + ", ";
                s += element.x + ", ";
                s += element.index + "  ;" + element.decompressedSpriteName + "\n";
                SpriteGroupSize += 4;
            }
            s += "\n";
        }
        s += "\n\n";
        return sgtable + "\n\n\n" + s;

    }

    private List<DataFrame> GenerateDataFrames(List<GameObject>[] objectFrames,GameObject parentObj ,bool MG2 = false)
    {
        //object frame is all objects within one dataframe (256 height)
        //Includes parent of sprite groups in this frame (not children)
        List<DataFrame> output = new List<DataFrame>();
        //loop through each object frame
        for (int i = 0; i < objectFrames.Length; i++)
        {

            DataFrame dFrame = new DataFrame();//data frame for this group
            List<DataFrameElement> frameElements = new List<DataFrameElement>();

            //Find all sprites used in this frame
            foreach (GameObject o in objectFrames[i]) //loop through current obj frame
            {
                DataFrameElement frameElement = new DataFrameElement();

                if (o.GetComponent<SpriteGroupParent>() != null)
                {
                    //This is a sprite group parent
                    string groupName = o.GetComponent<SpriteGroupParent>().GroupName;
                    DataFrame spriteGroup;
                    if (!spriteGroups.TryGetValue(groupName, out spriteGroup))
                    {
                        //Run generate sprite group data 
                        spriteGroup = GenerateSpriteGroup(o, parentObj, groupName);
                    }

                    int yPos = Mathf.RoundToInt((o.transform.position.y - parentObj.transform.position.y) * PPU - 1) - 256 * i;//ypos in frame
                    int xPos = Mathf.RoundToInt((o.transform.position.x - parentObj.transform.position.x) * PPU);//xpos relative to parent

                    frameElement.y = yPos;
                    frameElement.x = xPos / 2;
                    frameElement.height = 255; //indicates sprite group
                    frameElement.index = spriteGroup.spriteGroupIndex;
                    frameElement.orderInLayer = o.GetComponent<SortingGroup>().sortingOrder;
                    frameElement.decompressedSpriteName = "SG_" + groupName;
                }
                else //This is a solo sprite
                {
                    SpriteRenderer SR = o.GetComponent<SpriteRenderer>();
                    Sprite spr = SR.sprite;
                    ConvertedSprite convS;
                    //check if sprite has been used before
                    if (!convertedSprites.TryGetValue(spr.name, out convS))
                    {
                        //This texture has not been converted yet (All should be converted at this point. just an extra check
                        Debug.LogError("Sprite has not been converted yet" + spr.name);
                    }
                    bool flipped = SR.flipX;

                    int yPos = Mathf.RoundToInt((o.transform.position.y - parentObj.transform.position.y) * PPU - 1) - 256 * i;//ypos in frame
                    int xPos = Mathf.RoundToInt((SR.bounds.min.x - parentObj.transform.position.x) * PPU);//xpos relative to parent

                    DecompressModes decompressMode = GetDecompressMode(convS, xPos, flipped); //get what mode the sprite is being used in


                    frameElement.y = yPos;
                    frameElement.x = xPos / 2;
                    frameElement.height = convS.height;

                    frameElement.orderInLayer = SR.sortingOrder;

                    string decompressName = DecompressedSprite.GetDecompressedName(decompressMode, convS.spriteName);
                    DecompressedSprite ds;
                    if (!DecompressedSprites.TryGetValue(decompressName, out ds))
                    {
                        Debug.LogError("Missing Decompressed Sprite " + decompressName);
                    }

                    decompressName += "_" + ds.GetLoadIndex(i, parentObj == BGParent, (parentObj == MGParent) || (parentObj == MG2Parent), parentObj == FGParent);

                    frameElement.decompressedSpriteName = decompressName;
                }

                frameElements.Add(frameElement);
            }

            if(frameElements.Count > 255)
            {
                Debug.LogError("Too many elements in data frame " + i);
            }

            frameElements.Sort();//sort elements by order in layer
            int bestTableIndex = GetBestSpriteTable(frameElements);

            dFrame.elements = frameElements;
            dFrame.spriteTable = spriteTables[bestTableIndex];
            output.Add(dFrame);
        }
        return output;
    }

    private int GetBestSpriteTable(List<DataFrameElement> frameElements)
    {
        //Find best sprite table to use
        int bestTableIndex = 0;
        float bestScore = 0;
        for (int tableIndex = 0; tableIndex < spriteTables.Count; tableIndex++)
        {
            SpriteTable table = spriteTables[tableIndex];
            int matches = 0;
            int toAdd = 0;
            List<string> spritesToAdd = new List<string>(); //avoid counting duplicate sprites as needing to be added
                                                            //loop through all sprites in this frame
            for (int j = 0; j < frameElements.Count; j++)//sorted by layer order
            {
                DataFrameElement frameElement = frameElements[j];

                if (frameElement.height == 255)
                {
                    continue; //skips elements that are SG parents
                }

                if (table.uniqueDecompressedSprites.Contains(frameElement.decompressedSpriteName))
                {
                    matches++;//if table already contains this
                }
                else if (!spritesToAdd.Contains(frameElement.decompressedSpriteName))//add to list if not already
                {
                    toAdd++;
                    spritesToAdd.Add(frameElement.decompressedSpriteName);
                }

            }

            float score = 0;
            //check if best table
            if (table.size + toAdd < SpriteTable.maxSize)
            {   //calc score. Want to efficiently order sprites into tables. More matches is good. If table is getting near full, will reduce score
                //Debug.Log("Table index " + tableIndex);
                score = matches / (toAdd + 0.01f) + (SpriteTable.maxSize - Mathf.Max(0, table.size - 150)) / 50f;
                //Debug.Log("Score " + score);
                // Debug.Log("M:" + matches + " A:" + toAdd);
            }
            if (score > bestScore)
            {
                bestTableIndex = tableIndex;
                bestScore = score;
            }

        }
        // Debug.Log("best" + bestTableIndex);
        if (bestTableIndex == spriteTables.Count - 1)
        {
            //Debug.Log("Add New Sprite Table");
            spriteTables.Add(new SpriteTable());//ensure there is always an empty table at end
            spriteTables[spriteTables.Count - 1].name = "Sprite_Table_" + (spriteTables.Count - 1);
            spriteTables[spriteTables.Count - 1].index = spriteTables.Count;
        }

        //best table found
        for (int j = 0; j < frameElements.Count; j++)
        {
            if (frameElements[j].height == 255)
            {
                continue; //skips elements that are SG parents
            }

            int index = spriteTables[bestTableIndex].uniqueDecompressedSprites.IndexOf(frameElements[j].decompressedSpriteName);
            if (index == -1)
            {//not found, sprite needs to be added
                spriteTables[bestTableIndex].uniqueDecompressedSprites.Add(frameElements[j].decompressedSpriteName);
                spriteTables[bestTableIndex].size++;
                index = spriteTables[bestTableIndex].uniqueDecompressedSprites.Count - 1;
            }
            frameElements[j].index = index;
        }

        return bestTableIndex;
    }


    private DataFrame GenerateSpriteGroup(GameObject parent,GameObject frameParent,string groupName)
    {
        DataFrame spriteGroup = new DataFrame();
        List<DataFrameElement> groupElements = new List<DataFrameElement>();
        int parentXpos = (int)(parent.transform.position.x - frameParent.transform.position.x);
        int parentYpos = (int)(parent.transform.position.y - frameParent.transform.position.y);

        foreach (SpriteRenderer SR in parent.GetComponentsInChildren<SpriteRenderer>())
        {
            Sprite spr = SR.sprite;
            ConvertedSprite convS;
            //check if sprite has been used before
            if (!convertedSprites.TryGetValue(spr.name, out convS))
            {
                //This texture has not been converted yet (All should be converted at this point. just an extra check
                Debug.LogError("Sprite has not been converted yet" + spr.name);
            }

            DataFrameElement frameElement = new DataFrameElement();

            bool flipped = SR.flipX;

            int yPos = Mathf.RoundToInt((parentYpos - SR.gameObject.transform.position.y ) * PPU);//ypos below parent
            int xPos = Mathf.RoundToInt((SR.bounds.min.x - parent.transform.position.x) * PPU);//xpos relative to parent

            DecompressModes decompressMode = GetDecompressMode(convS, xPos + parentXpos, flipped); //get what mode the sprite is being used in

            frameElement.y = yPos;
            frameElement.x = xPos / 2;
            frameElement.height = convS.height;

            frameElement.orderInLayer = SR.sortingOrder;

            string decompressName = DecompressedSprite.GetDecompressedName(decompressMode, convS.spriteName);
            DecompressedSprite ds;
            if (!DecompressedSprites.TryGetValue(decompressName, out ds))
            {
                Debug.LogError("Missing Decompressed Sprite " + decompressName);
            }

            decompressName += "_" + ds.GetLoadIndex(parentYpos / 256, frameParent == BGParent, (frameParent == MGParent) || (frameParent == MG2Parent), frameParent == FGParent);

            frameElement.decompressedSpriteName = decompressName;
            groupElements.Add(frameElement);
        }

        if (groupElements.Count > 255)
        {
            Debug.LogError("Too many elements in sprite group "+ groupName);
        }

        groupElements.Sort();//sort elements by order in layer

        int tableIndex = GetBestSpriteTable(groupElements);
        spriteGroup.elements = groupElements;
        spriteGroup.spriteTable = spriteTables[tableIndex];
        spriteGroup.spriteGroupIndex = spriteGroup.elements.Count;
        spriteGroups.Add(groupName, spriteGroup);
        return spriteGroup;
    }


    public void WriteCompressedSpriteData(List<ConvertedSprite> allSprites) //Computes locations for sprites to be decompressed to
    {

        string compressedData = "";

        foreach (ConvertedSprite sprite in allSprites)
        {
            compressedData += sprite.spriteName + ":\n" + sprite.compressedData + "\n\n";
            CompressedSpriteSize += sprite.compressedSize;
        }
        FileWrite.WriteString(compressedData, "Sprite_Data");
    }


    private List<GameObject>[] GetObjectFrames(GameObject parent)
    {
        List<GameObject>[] objectFrames;

        // find max height
        float maxHeight = 0;
        foreach (Transform t in parent.GetComponentsInChildren<Transform>())
        {
            
            if (t.gameObject.tag == "SpriteGroupElement")
            {
                //Children of SG arent included directly in object frames
                continue;
            }
            if(t.gameObject.GetComponent<SpriteRenderer>() == null && t.gameObject.GetComponent<SpriteGroupParent>() == null)
            {
                //if object is not a sprite or a parent, ignore it
                continue;
            }
 
            
            if (t.position.y - parent.transform.position.y > maxHeight)
            {
                maxHeight = t.position.y - parent.transform.position.y;
            }
        }
        //Debug.Log(maxHeight);

        int frameCount = (Mathf.RoundToInt(maxHeight*PPU - 1) / 256) + 1;

        frameCount = 30; //For testing 

        //Debug.Log(Mathf.RoundToInt(maxHeight * PPU));
        //Debug.Log("Frame Count " + frameCount);

        objectFrames = new List<GameObject>[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            objectFrames[i] = new List<GameObject>();
        }

        // group objects into frames and generate sprites

        foreach (Transform t in parent.GetComponentsInChildren<Transform>())
        {
            int frameIndex = Mathf.RoundToInt((t.position.y - parent.transform.position.y) * PPU - 1) / 256;
            SpriteRenderer SR = t.gameObject.GetComponent<SpriteRenderer>();

            if (SR != null) //If object has a sprite, generate sprite data 
            {
                Sprite sprite = SR.sprite;
                ConvertedSprite convS;
                //check if sprite has been used before
                if (!convertedSprites.TryGetValue(sprite.name, out convS))
                {
                    //This texture has not been converted yet
                    //convert it and add to dictonary
                    convS = SpriteConverter.CreateConvertedSprite(sprite);
                    convertedSprites.Add(convS.spriteName, convS);
                }

                bool flipped = SR.flipX;
                int xPos = Mathf.RoundToInt((SR.bounds.min.x - parent.transform.position.x) * PPU);//xpos relative to parent

                if (xPos < 0 || (xPos + convS.width > 320))
                {
                    Debug.LogError("Sprite out of bounds " + convS.spriteName + " " + parent.name + " X:" + xPos + " Y:" + t.position.y);
                }
                if (convS.fast && xPos % 2 != 0)
                {
                    Debug.LogWarning("Fast Sprite on odd x value " + convS.spriteName + " " + parent.name + "X:" + xPos + " Y:" + t.position.y);
                }

                DecompressModes decompressMode = GetDecompressMode(convS, xPos, flipped); //get what mode the sprite is being used in
                convS.useageModes[(int)decompressMode] = true;

                string DecompressedName = DecompressedSprite.GetDecompressedName(decompressMode, convS.spriteName);// appends O/F to string


                //Add or create associated decompressed sprite
                DecompressedSprite ds;
                if (!DecompressedSprites.TryGetValue(DecompressedName, out ds))
                {
                    //DecompressedSprite does not exist yet
                    DecompressedSprites.Add(DecompressedName, new DecompressedSprite(convS, decompressMode, frameIndex, parent == BGParent, parent == MGParent, parent == FGParent));
                }
                else
                {
                    //Decompressed sprite already exists, add loaded region
                    ds.AddLoadedRegion(frameIndex, parent == BGParent, (parent == MGParent) || (parent == MG2Parent), parent == FGParent);
                }



            }

            if (t.gameObject.tag == "SpriteGroupElement")
            {
                //Children of SG arent included directly in object frames
                continue;
            }
            if (t.gameObject.GetComponent<SpriteRenderer>() == null && t.gameObject.GetComponent<SpriteGroupParent>() == null)
            {
                //if object is not a sprite or a parent, ignore it
                continue;
            }
            
            objectFrames[frameIndex].Add(t.gameObject);
            //Debug.Log("frame index: " + frameIndex + "  " + o.gameObject.name);
        }

        return objectFrames;
    }


    private DecompressModes GetDecompressMode(ConvertedSprite sprite, int x,bool flipped)
    {
        if (sprite.fast)
        {
            if(x%2 == 0)
            {   //Fast sprite
                return flipped ? DecompressModes.fastFlip : DecompressModes.fast;
            }
        }
        //Slow sprite
        if(x%2 == 0)
        {
            //no offset
            return flipped ? DecompressModes.slowFlip : DecompressModes.slow;
        }
        //Slow offset
        return flipped ? DecompressModes.slowOffFlip : DecompressModes.slowOff;
    }

    
}
