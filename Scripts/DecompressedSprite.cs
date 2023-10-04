using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecompressedSprite //Decompressed sprite with certain usage mode
{
    public static int MaxUnloadFrame; //Greatest unload frame out of all decompress sprites. Used to know when to stop calculating memory allocation
    public ConvertedSprite associatedCompressedSprite;
    public DecompressModes usageMode;
    public string spriteNameAndMode;
    public string compressedName;
    public int DecompressedSize; //Size of data in bytes when decompressed;
    public bool isFast; //true if fast sprite 
    public List<Vector2> LoadedSegments = new List<Vector2>(); //Lower and upper frames where this sprite should be loaded. Sorted by lower frame number 
    public DecompressedSprite(ConvertedSprite associatedSprite, DecompressModes _usageMode, int frameNum, bool bg, bool mg, bool fg)
    {
        associatedCompressedSprite = associatedSprite;
        spriteNameAndMode = GetDecompressedName(_usageMode,associatedSprite.spriteName);
        compressedName = associatedSprite.spriteName;
        usageMode = _usageMode;
        DecompressedSize = associatedSprite.decompressedSize[(int)usageMode];
        isFast = (usageMode == DecompressModes.fast || usageMode == DecompressModes.fastFlip);
        AddLoadedRegion(frameNum, bg, mg, fg);
    }

    public void PrintLodedSegments()
    {
        string s = "";
        foreach (Vector2 v in LoadedSegments)
        {
            s += " (" + v.x + "," + v.y + ") ";
        }
        Debug.Log(s);
    }

    //Adds a new loaded region for this sprite. If the start or end of this region overlaps with an existing region they will be combined.
    //Will also avoid creating gaps of minGapSize or less.
    public void AddLoadedRegion(int frameNum, bool bg, bool mg, bool fg)
    {

        Vector2 AddedRegion = GetLowerAndUpperLoadFrames(frameNum, bg, mg, fg);

        int lowerLoadFrame = Mathf.RoundToInt(AddedRegion.x);
        int upperLoadFrame = Mathf.RoundToInt(AddedRegion.y);

        int newLowerBound = -1;
        int newUpperBound = -1;

        int minGapSize = 1; //Minimum gap size to avoid between regions. Prevents unnecessary loading/unloading

        List<Vector2> newLoadedSegments = new List<Vector2>();
        bool newSegmentAdded = false;

        //loop through existing loading segments
        for (int i = 0; i < LoadedSegments.Count; i++)
        {
            Vector2 LoadSegment = LoadedSegments[i];

            //if we have added the new segment, continue for all existing segments to finish loop
            if (newSegmentAdded)
            {
                newLoadedSegments.Add(LoadSegment);
                continue;
            }

            if (LoadSegment.y < lowerLoadFrame - minGapSize)
            {
                //current segment is entirely before new segment
                newLoadedSegments.Add(LoadSegment);
                continue;
            }

            //current segment is not entirely before new segment

            if (newLowerBound == -1) //Have not yet found new lower bound
            {
                if (lowerLoadFrame >= LoadSegment.x && lowerLoadFrame <= LoadSegment.y + minGapSize)
                {
                    //Current segment is not entirely before new segment
                    //Current segemnt Contains lower bound
                    newLowerBound = (int)LoadSegment.x;
                }
                else
                {
                    //Current segment is not entirely before new segment
                    //Does not contain lower bound
                    newLowerBound = lowerLoadFrame;
                }
            }


            if (LoadSegment.x > upperLoadFrame + minGapSize)
            {   //current segment is entirely after new segment
                if (!newSegmentAdded)
                {
                    newUpperBound = upperLoadFrame;
                    newLoadedSegments.Add(new Vector2(newLowerBound, newUpperBound));
                    newSegmentAdded = true;
                }

                newLoadedSegments.Add(LoadSegment);
                continue;
            }

            //current segment not after new segment

            if (upperLoadFrame >= LoadSegment.x - minGapSize && upperLoadFrame <= LoadSegment.y)
            {
                //Current segment contains upper bound
                newUpperBound = (int)LoadSegment.y;
                newLoadedSegments.Add(new Vector2(newLowerBound, newUpperBound));
                newSegmentAdded = true;
                continue;
            }

        }
        if (!newSegmentAdded)
        {
            if (newLowerBound == -1)
            {
                newLowerBound = lowerLoadFrame;
            }
            if (newUpperBound == -1)
            {
                newUpperBound = upperLoadFrame;
            }
            newLoadedSegments.Add(new Vector2(newLowerBound, newUpperBound));

        }
        LoadedSegments = newLoadedSegments;
    }
    private Vector2 GetLowerAndUpperLoadFrames(int frameNum, bool bg, bool mg, bool fg)
    {
        //Computes the frames where this sprite should begin decompressing when cam is moving up (lower) or down (upper)


        //Number of frames between when sprite is first used and when to begin decompressing. Done to ensure sprite is finished decompressing by the time it is shown on screen 
        //Larger numbers will begin decompressing sprites earlier, but would increase memory useage as sprites will be in ram for longer
        const int FGLowerBufferDistance = 1;
        const int MGLowerBufferDistance = 1;
        const int BGLowerBufferDistance = 1;

        int lowerLoadFrame = frameNum;
        int upperLoadFrame = frameNum + FGLowerBufferDistance;
        if (fg)
        {
            lowerLoadFrame = Mathf.Max(0, frameNum - 2 - FGLowerBufferDistance);
            upperLoadFrame = frameNum + 2;
        }
        else if (mg)
        {
            lowerLoadFrame = Mathf.Max(0, (frameNum - 2) * 2 + 1 - MGLowerBufferDistance);
            upperLoadFrame = (frameNum + 2) * 2;
        }
        else if (bg)
        {
            lowerLoadFrame = Mathf.Max(0, (frameNum - 2) * 4 + 1 - BGLowerBufferDistance);
            upperLoadFrame = (frameNum + 2) * 4;
        }
        else
        { //Sprite is in AlwaysActive group
            lowerLoadFrame = 0;
            upperLoadFrame = MaxUnloadFrame;
        }
        return new Vector2(lowerLoadFrame, upperLoadFrame);

    }
    public int GetLoadIndex(int frameNum , bool bg, bool mg, bool fg)
    {
        Vector2 minLoadedRange = GetLowerAndUpperLoadFrames(frameNum, bg, mg, fg);
        for(int i = 0; i < LoadedSegments.Count; i++)
        {
            if (LoadedSegments[i].x <= minLoadedRange.x && LoadedSegments[i].y >= minLoadedRange.y)
            {
                return i;
            }
        }
        Debug.LogError("Error in get load index");
        return -1;
    }
    public int GetLoadIndex(int loadFrame) //Returns the load index containing the given loadFrame
    {
        for (int i = 0; i < LoadedSegments.Count; i++)
        {
            if (LoadedSegments[i].x <= loadFrame && LoadedSegments[i].y >= loadFrame-1)
            {
                return i;
            }
        }
        return -1;
    }
    public bool IsLoadedDuringFrame(int loadFrame)
    {
        for (int i = 0; i < LoadedSegments.Count; i++)
        {
            if (LoadedSegments[i].x == loadFrame)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsUnloadedDuringFrame(int loadFrame)
    {
        for (int i = 0; i < LoadedSegments.Count; i++)
        {
            if (LoadedSegments[i].y == loadFrame)
            {
                return true;
            }
        }
        return false;
    }


    public static string GetDecompressedName(DecompressModes mode, string name)
    {
        string outS = name;

        switch (mode)
        {
            case DecompressModes.slow:
                outS += "_Slow";
                break;
            case DecompressModes.slowFlip:
                outS += "_Slow_F";
                break;
            case DecompressModes.slowOff:
                outS += "_Slow_O";
                break;
            case DecompressModes.slowOffFlip:
                outS += "_Slow_O_F";
                break;
            case DecompressModes.fast:
                outS += "_Fast";
                break;
            case DecompressModes.fastFlip:
                outS += "_Fast_F";
                break;

        }
        return outS;
    }

}