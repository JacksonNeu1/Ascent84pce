using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;





public class SpriteTable {
    public const int maxSize = 255;
    public int size = 0;
    public string name;
    public List<string> uniqueDecompressedSprites = new List<string>();
}
public class DataFrame {
    public SpriteTable spriteTable;
    public List<DataFrameElement> elements;

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
//and group these strings into sprite tables and create object data frames 

//Use unique decompressed sprites to precompute memory allocation, and associate memory adresses with decompressed sprite strings



public class LevelDataGenerate : MonoBehaviour
{
    private const int PPU = 10;
    public GameObject FGParent;
    public GameObject MGParent;
    public GameObject BGParent;
    public GameObject AlwaysActiveParent;

    private Dictionary<string, ConvertedSprite> convertedSprites = new Dictionary<string, ConvertedSprite>();
    private Dictionary<string,DecompressedSprite> DecompressedSprites = new Dictionary<string, DecompressedSprite>();

    private List<SpriteTable> spriteTables = new List<SpriteTable>();

    private string FGLevelData;
    private string MGLevelData;
    private string BGLevelData;
    private string spriteTableData;

    void Start()
    {
        spriteTables.Add(new SpriteTable());
        spriteTables[0].name = "Sprite_Table_0";

        DecompressedSprite.MaxUnloadFrame = 0;
        //Spit objects into associated frames
        List<GameObject>[] FGobjectFrames = GetObjectFrames(FGParent);
        List<GameObject>[] MGobjectFrames = GetObjectFrames(MGParent);
        List<GameObject>[] BGobjectFrames = GetObjectFrames(BGParent);

        //Create all converted sprites and decompressed sprites
        //Loop through all objects in game to generate unique decompressed sprites with load indeces
        GenerateSprites(FGobjectFrames, FGParent);
        GenerateSprites(MGobjectFrames, MGParent);
        GenerateSprites(BGobjectFrames, BGParent);

        //convert objects to data frames
        List<DataFrame> FGFrames = GenerateDataFrames(FGobjectFrames,FGParent);
        List<DataFrame> MGFrames = GenerateDataFrames(MGobjectFrames,MGParent);
        List<DataFrame> BGFrames = GenerateDataFrames(BGobjectFrames,BGParent);

        //convert to strings and write
        FGLevelData = DataFramesToString(FGFrames, "FG_Data");
        MGLevelData = DataFramesToString(MGFrames, "MG_Data");
        BGLevelData = DataFramesToString(BGFrames, "BG_Data");

        FileWrite.WriteString(FGLevelData, "FG_Data");
        FileWrite.WriteString(MGLevelData, "MG_Data");
        FileWrite.WriteString(BGLevelData, "BG_Data");

        spriteTableData = "";
        foreach(SpriteTable table in spriteTables)
        {
            spriteTableData += SpriteTableToString(table);
        }
        FileWrite.WriteString(spriteTableData, "Sprite_Tables");
        //SpriteDataGenerate.GenerateSpriteData(new List<ConvertedSprite>(convertedSprites.Values));
    }
    private string SpriteTableToString(SpriteTable sTable)
    {
        string s = "";
        s += sTable.name + ":\n";

        foreach(string spr in sTable.uniqueDecompressedSprites)
        {
            s += "\t.dl " + spr + "\n";
        }
        s += "\n";
        return s;
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
            s += "\t.dl " + frame.spriteTable.name + "\n";
            s += "\t.db " + frame.elements.Count + "\n";
            foreach (DataFrameElement element in frame.elements)
            {
                s += "\t.db ";
                s += element.y + ", ";
                s += element.height + ", ";
                s += element.x + ", ";
                s += element.index + "\n";
            }
            s += "\n";
        }
        s += "\n\n";
        return lookuptbl + "\n\n\n" + s;
    }

    //Loops through all objects in the layer to convert sprites and create unique decompressed sprites (run for all layers)
    private void GenerateSprites(List<GameObject>[] objectFrames, GameObject parentObj)
    {
        for (int i = 0; i < objectFrames.Length; i++)
        {
            //Find all sprites used in this frame
            foreach (GameObject o in objectFrames[i]) //loop through current obj frame
            {
                SpriteRenderer SR = o.GetComponent<SpriteRenderer>();
                Texture2D tex = SR.sprite.texture;
                ConvertedSprite convS;
                //check if sprite has been used before
                if (!convertedSprites.TryGetValue(tex.name, out convS))
                {
                    //This texture has not been converted yet
                    //convert it and add to dictonary
                    convS = SpriteConverter.CreateConvertedSprite(tex);
                    convertedSprites.Add(convS.spriteName, convS);
                }

                bool flipped = SR.flipX;
                int yPos = Mathf.RoundToInt((o.transform.position.y - parentObj.transform.position.y) * PPU - 1) - 256 * i;//ypos in frame
                int xPos = Mathf.RoundToInt((SR.bounds.min.x - parentObj.transform.position.x) * PPU);//xpos relative to parent

                DecompressModes decompressMode = GetDecompressMode(convS, xPos, flipped); //get what mode the sprite is being used in
                convS.useageModes[(int)decompressMode] = true;

                string DecompressedName = DecompressedSprite.GetDecompressedName(decompressMode, convS.spriteName);// appends O/F to string


                //Add or create associated decompressed sprite
                DecompressedSprite ds;
                if(!DecompressedSprites.TryGetValue(DecompressedName,out ds))
                {
                    //DecompressedSprite does not exist yet
                    DecompressedSprites.Add(DecompressedName,new DecompressedSprite(convS,decompressMode, i, parentObj == BGParent, parentObj == MGParent, parentObj == FGParent));
                }
                else
                {
                    //Decompressed sprite already exists, add loaded region
                    ds.AddLoadedRegion(i, parentObj == BGParent, parentObj == MGParent, parentObj == FGParent);
                }

                

            }
        }
    }
    private List<DataFrame> GenerateDataFrames(List<GameObject>[] objectFrames,GameObject parentObj)
    {
        //object frame is all objects within one dataframe (256 height)
        List<DataFrame> output = new List<DataFrame>();
        //loop through each object frame
        for (int i = 0; i < objectFrames.Length; i++)
        {

            DataFrame dFrame = new DataFrame();//data frame for this group
            List<DataFrameElement> frameElements = new List<DataFrameElement>();

            //Find all sprites used in this frame
            foreach (GameObject o in objectFrames[i]) //loop through current obj frame
            {
                
                SpriteRenderer SR = o.GetComponent<SpriteRenderer>();
                Texture2D tex = SR.sprite.texture;
                ConvertedSprite convS;
                //check if sprite has been used before
                if (!convertedSprites.TryGetValue(tex.name, out convS))
                {
                    //This texture has not been converted yet (All should be converted at this point. just an extra check
                    Debug.LogError("Sprite has not been converted yet");
                }

                DataFrameElement frameElement = new DataFrameElement();

                bool flipped = SR.flipX;

                int yPos = Mathf.RoundToInt((o.transform.position.y - parentObj.transform.position.y) * PPU - 1) - 256 * i;//ypos in frame
                int xPos = Mathf.RoundToInt((SR.bounds.min.x - parentObj.transform.position.x) * PPU);//xpos relative to parent

                DecompressModes decompressMode = GetDecompressMode(convS, xPos, flipped); //get what mode the sprite is being used in


                frameElement.y = yPos;
                frameElement.x = xPos/2;
                frameElement.height = convS.height;

                frameElement.orderInLayer = SR.sortingOrder;
               
                string decompressName = DecompressedSprite.GetDecompressedName(decompressMode, convS.spriteName);
                DecompressedSprite ds;
                if(!DecompressedSprites.TryGetValue(decompressName, out ds))
                {
                    Debug.LogError("Missing Decompressed Sprite " + decompressName);
                }

                decompressName += "_" + ds.GetLoadIndex(i, parentObj == BGParent, parentObj == MGParent, parentObj == FGParent);

                frameElement.decompressedSpriteName = decompressName;
                frameElements.Add(frameElement);
            }

            if(frameElements.Count > 255)
            {
                Debug.LogError("Too many elements in data frame " + i);
            }

            frameElements.Sort();//sort elements by order in layer

            //Debug.Log("Frame Index " + i);

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
                for(int j = 0; j < frameElements.Count;j++)//sorted by layer order
                {
                    DataFrameElement frameElement = frameElements[j];

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
                if(table.size + toAdd < SpriteTable.maxSize)
                {   //calc score. Want to efficiently order sprites into tables. More matches is good. If table is getting near full, will reduce score
                   // Debug.Log("Table index " + tableIndex);
                    score = matches / (toAdd + 0.01f) + (SpriteTable.maxSize - table.size) / 200f;
                    //Debug.Log("Score " + score);
                    //Debug.Log("M:" + matches + " A:" + toAdd);
                }
                if(score > bestScore)
                {
                    bestTableIndex = tableIndex;
                    bestScore = score;
                }

            }
           // Debug.Log("best" + bestTableIndex);
            if (bestTableIndex == spriteTables.Count-1)
            {
                //Debug.Log("Add New Sprite Table");
                spriteTables.Add(new SpriteTable());//ensure there is always an empty table at end
                spriteTables[spriteTables.Count-1].name = "Sprite_Table_" + (spriteTables.Count-1);
            }

            //best table found
            for(int j = 0; j < frameElements.Count;j++)
            {
                int index = spriteTables[bestTableIndex].uniqueDecompressedSprites.IndexOf(frameElements[j].decompressedSpriteName);
                if(index == -1)
                {//not found, sprite needs to be added
                    spriteTables[bestTableIndex].uniqueDecompressedSprites.Add(frameElements[j].decompressedSpriteName);
                    spriteTables[bestTableIndex].size++;
                    index = spriteTables[bestTableIndex].uniqueDecompressedSprites.Count - 1;
                }
                frameElements[j].index = index;
            }
            dFrame.elements = frameElements;
            dFrame.spriteTable = spriteTables[bestTableIndex];
            output.Add(dFrame);
        }
        return output;
    }

    

    private List<GameObject>[] GetObjectFrames(GameObject parent)
    {
        List<GameObject>[] objectFrames;

        // find max height
        float maxHeight = 0;
        foreach (SpriteRenderer sr in parent.GetComponentsInChildren<SpriteRenderer>())
        {
            Transform o = sr.transform; 
            if (o.position.y - parent.transform.position.y > maxHeight)
            {
                maxHeight = o.position.y - parent.transform.position.y;
            }
        }
        //Debug.Log(maxHeight);

        int frameCount = (Mathf.RoundToInt(maxHeight*PPU - 1) / 256) + 1;
        //Debug.Log(Mathf.RoundToInt(maxHeight * PPU));
        //Debug.Log("Frame Count " + frameCount);

        objectFrames = new List<GameObject>[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            objectFrames[i] = new List<GameObject>();
        }

        // group objects into frames

        foreach (SpriteRenderer sr in parent.GetComponentsInChildren<SpriteRenderer>())
        {
            Transform o = sr.transform;
            int frameIndex = Mathf.RoundToInt((o.position.y - parent.transform.position.y) * PPU - 1) / 256;
            objectFrames[frameIndex].Add(o.gameObject);
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
            Debug.LogWarning("Fast Sprite on odd x value " + sprite.spriteName);
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
