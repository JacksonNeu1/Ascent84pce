using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;





public class SpriteTable {
    public static int maxSize = 255;
    public int size = 0;
    public string name;
    public List<string> sprites = new List<string>();
}
public class DataFrame {
    public SpriteTable spriteTable;
    public List<DataFrameElement> elements;
}
public class DataFrameElement : IComparable<DataFrameElement> {
    public int y;
    public int height;
    public int x;
    public int index;
    public int orderInLayer;//use to sort elements in frame
    public string decompressName;
    public int CompareTo(DataFrameElement other)
    {
        return orderInLayer.CompareTo(other.orderInLayer);
    }

}



public class levelDataGenerate : MonoBehaviour
{
    private static int PPU = 10;
    public GameObject FGParent;
    public GameObject MGParent;
    public GameObject BGParent;

    private Dictionary<string, convertedSprite> convertedSprites = new Dictionary<string, convertedSprite>();

    private List<SpriteTable> spriteTables = new List<SpriteTable>();

    private string FGLevelData;
    private string MGLevelData;
    private string BGLevelData;
    private string spriteTableData;

    void Start()
    {
        spriteTables.Add(new SpriteTable());
        spriteTables[0].name = "Sprite_Table_0";

        List<GameObject>[] FGobjectFrames = getObjectFrames(FGParent);

        //List<GameObject>[] MGobjectFrames = getObjectFrames(MGParent);
       // List<GameObject>[] BGobjectFrames = getObjectFrames(BGParent);

        List<DataFrame> FGFrames = generateDataFrames(FGobjectFrames);

        FGLevelData = dataFramesToString(FGFrames, "FG_Data");

        FileWrite.WriteString(FGLevelData, "FG_Data");


        spriteTableData = "";
        foreach(SpriteTable table in spriteTables)
        {
            spriteTableData += spriteTableToString(table);
        }
        FileWrite.WriteString(spriteTableData, "Sprite_Tables");
        SpriteDataGenerate.generateSpriteData(new List<convertedSprite>(convertedSprites.Values));
    }
    private string spriteTableToString(SpriteTable sTable)
    {
        string s = "";
        s += sTable.name + ":\n";

        foreach(string spr in sTable.sprites)
        {
            s += ".dl " + spr + "\n";
        }
        s += "\n";
        return s;
    }

    private string dataFramesToString(List<DataFrame> dFrames, string baseName)
    {
        string s = "";
        int frameNum = 0;
        foreach (DataFrame frame in dFrames)
        {
            s += baseName + "_" + frameNum + ":\n";
            frameNum++;
            s += ".dl " + frame.spriteTable.name + "\n";
            s += ".db " + frame.elements.Count + "\n";
            foreach (DataFrameElement element in frame.elements)
            {
                s += ".db ";
                s += element.y + ", ";
                s += element.height + ", ";
                s += element.x + ", ";
                s += element.index + "\n";
            }

        }
        s += "\n";
        return s;
    }

    private List<DataFrame> generateDataFrames(List<GameObject>[] objectFrames)
    {
        List<DataFrame> output = new List<DataFrame>();
        //loop through each object frame
        for (int i = 0; i < objectFrames.Length; i++)
        {

            DataFrame dFrame = new DataFrame();//data frame for this group
            List<DataFrameElement> frameElements = new List<DataFrameElement>();

            

            //Find all sprites used in this frame
            foreach (GameObject o in objectFrames[i])
            {
                
                SpriteRenderer SR = o.GetComponent<SpriteRenderer>();
                Texture2D tex = SR.sprite.texture;
                convertedSprite convS;
                //check if sprite has been used before
                if (!convertedSprites.TryGetValue(tex.name, out convS))
                {
                    //This texture has not been converted
                    convS = spriteConverter.createConvertedSprite(tex);
                    convertedSprites.Add(convS.spriteName, convS);
                }

                DataFrameElement frameElement = new DataFrameElement();
                bool flipped = SR.flipX;

                int yPos = Mathf.RoundToInt(o.transform.localPosition.y * PPU - 1) - 256 * i;//ypos in frame
                int xPos = Mathf.RoundToInt((SR.bounds.min.x - o.transform.parent.position.x) * PPU);//xpos relative to parent

                convertedSprite.decompressModes decompressMode = getDecompressMode(convS, xPos, flipped, false);
                convS.useageModes[(int)decompressMode] = true;

                frameElement.y = yPos;
                frameElement.x = xPos/2;
                frameElement.height = convS.height - 1;

                frameElement.orderInLayer = SR.sortingOrder;
               
                string decompressName = getDecompressedName(decompressMode, convS.spriteName);

                //Debug.Log(decompressName + " x:" + xPos/2 + " y" + yPos);


                frameElement.decompressName = decompressName;
                frameElements.Add(frameElement);
            }

            if(frameElements.Count > 255)
            {
                Debug.LogError("Too many elements in data frame ");
            }

            frameElements.Sort();//sort elements by order in layer

           // Debug.Log("Frame Index " + i);
            //Find best sprite table to use
            int bestTableIndex = 0;
            float bestScore = 0;
            for (int tableIndex = 0; tableIndex < spriteTables.Count;tableIndex++)
            {
                SpriteTable table = spriteTables[tableIndex];
                int matches = 0;
                int toAdd = 0;
                List<string> spritesToAdd = new List<string>();
                //loop through all sprites in this frame
                for(int j = 0; j < frameElements.Count;j++)//sorted by layer order
                {
                    DataFrameElement frameElement = frameElements[j];

                    if (table.sprites.Contains(frameElement.decompressName))
                    {
                        matches++;//if table already contains this
                    }
                    else if (!spritesToAdd.Contains(frameElement.decompressName))//add to list if not already
                    {
                        toAdd++;
                        spritesToAdd.Add(frameElement.decompressName);
                    }
                    
                }

                float score = 0;
                //check if best table
                if(table.size + toAdd < SpriteTable.maxSize)
                {//calc score
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
                int index = spriteTables[bestTableIndex].sprites.IndexOf(frameElements[j].decompressName);
                if(index == -1)
                {//not found, sprite needs to be added
                    spriteTables[bestTableIndex].sprites.Add(frameElements[j].decompressName);
                    spriteTables[bestTableIndex].size++;
                    index = spriteTables[bestTableIndex].sprites.Count - 1;
                }
                frameElements[j].index = index;
            }
            dFrame.elements = frameElements;
            dFrame.spriteTable = spriteTables[bestTableIndex];
            output.Add(dFrame);
        }
        return output;
    }


    private List<GameObject>[] getObjectFrames(GameObject parent)
    {
        List<GameObject>[] objectFrames;

        // find max height
        float maxHeight = 0;
        foreach (Transform o in parent.transform)
        {
            if (o.position.y > maxHeight)
            {
                maxHeight = o.localPosition.y;
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

        foreach (Transform o in parent.transform)
        {
            int frameIndex = Mathf.RoundToInt(o.localPosition.y * PPU - 1) / 256;
            objectFrames[frameIndex].Add(o.gameObject);
            //Debug.Log("frame index: " + frameIndex + "  " + o.gameObject.name);
        }

        return objectFrames;
    }


    private convertedSprite.decompressModes getDecompressMode(convertedSprite sprite, int x,bool flipped,bool bg)
    {
        if (bg)
        {
            if (x % 2 == 0)
            {
                //BG no offset
                return flipped ? convertedSprite.decompressModes.BGFlip : convertedSprite.decompressModes.BG;
            }
            //offset
            return flipped ? convertedSprite.decompressModes.BGOffFlip : convertedSprite.decompressModes.BGOff;
        }
        //Not BG
        if (sprite.fast)
        {
            if(x%2 == 0)
            {   //Fast sprite
                return flipped ? convertedSprite.decompressModes.fastFlip : convertedSprite.decompressModes.fast;
            }
            Debug.LogWarning("Fast Sprite on odd x value");
        }
        //Slow sprite
        if(x%2 == 0)
        {
            //no offset
            return flipped ? convertedSprite.decompressModes.slowFlip : convertedSprite.decompressModes.slow;
        }
        //Slow offset
        return flipped ? convertedSprite.decompressModes.slowOffFlip : convertedSprite.decompressModes.slowOff;
    }

    public static string getDecompressedName(convertedSprite.decompressModes mode, string name)
    {
        string outS = name;

        switch (mode){
            case convertedSprite.decompressModes.slow:
                outS += "_Slow";
                break;
            case convertedSprite.decompressModes.slowFlip:
                outS += "_Slow_F";
                break;
            case convertedSprite.decompressModes.slowOff:
                outS += "_Slow_O";
                break;
            case convertedSprite.decompressModes.slowOffFlip:
                outS += "_Slow_O_F";
                break;
            case convertedSprite.decompressModes.fast:
                outS += "_Fast";
                break;
            case convertedSprite.decompressModes.fastFlip:
                outS += "_Fast_F";
                break;
            case convertedSprite.decompressModes.BG:
                outS += "_BG";
                break;
            case convertedSprite.decompressModes.BGFlip:
                outS += "_BG_F";
                break;
            case convertedSprite.decompressModes.BGOff:
                outS += "_BG_O";
                break;
            case convertedSprite.decompressModes.BGOffFlip:
                outS += "_BG_O_F";
                break;

        }
        return outS;
    }
}
