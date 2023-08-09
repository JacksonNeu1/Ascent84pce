using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class MemorySegment : IComparable<MemorySegment>
{
    public int startByte;//First byte of used memeory
    public int length;
    public int endByte;//First free byte after used memory
    public MemorySegment(int start, int len)
    {
        startByte = start;
        length = len;
        endByte = startByte + len;
    }
    public int CompareTo(MemorySegment other)
    {
        return startByte.CompareTo(other.startByte);
    }

}

public class DecompressCallManager : MonoBehaviour
{
    private const int vRamBufferStart = 0xD40000; // 38,400 bytes
    private const int pixelShadowStart = 0xD031F6; //69,090 bytes

    private const int vRamBufferEnd = 0xD49600;
    private const int pixelShadowEnd = 0xD13FD8;

    public GameObject FGParent;
    public GameObject MGParent;
    public GameObject BGParent;
    public GameObject PersistantParent; //for sprites that are used in entire game (player)

    private List<MemorySegment> VirtualMemoryVRam;
    private List<MemorySegment> VirtualMemoryPS;
    private Dictionary<string, int> SpriteEquates;
    void Start()
    {
        /*print("Testing");

        UniqueSprite testS = new UniqueSprite("Test", 4, 7);
        testS.printLodedSegments();
        testS.AddLoadedRegion(10, 13);
        testS.printLodedSegments();
        testS.AddLoadedRegion(30, 40);
        testS.printLodedSegments();

        testS.AddLoadedRegion(3, 6);
        testS.printLodedSegments();

        testS.AddLoadedRegion(3,8);
        testS.printLodedSegments();

        testS.AddLoadedRegion(4,9);
        testS.printLodedSegments();

        testS.AddLoadedRegion(14, 30);
        testS.printLodedSegments();

        testS.AddLoadedRegion(50, 60);
        testS.printLodedSegments();

        testS.AddLoadedRegion(49, 65);
        testS.printLodedSegments();

        testS.AddLoadedRegion(1, 70);
        testS.printLodedSegments();*/

    }


    public void CalculateMemoryLocations(List<DecompressedSprite> allSprites)
    {
        VirtualMemoryVRam = new List<MemorySegment>();
        SpriteEquates = new Dictionary<string, int>();
        string allEquatesString = "";
        string decompressFramesUp = "";
        string decompressFramesDown = "";
        string decompressTableUp = "decompress_calls_table_up:\n";
        string decompressTableDown = "decompress_calls_table_down:\n";
        int maxUnloadFrame = DecompressedSprite.MaxUnloadFrame;

        if(maxUnloadFrame > 255)
        {
            Debug.LogError("Maximum sprite load frame too large: " + maxUnloadFrame);
        }

        for(int currentFrame = 0; currentFrame < maxUnloadFrame; currentFrame++)
        {
            int upCallCount = 0;
            int downCallCount = 0;
            string upDecompressCalls = "";
            string downDecompressCalls = "";
            foreach (DecompressedSprite ds in allSprites)
            {

                int dsLoadIndex = ds.GetLoadIndex(currentFrame);
                if(dsLoadIndex == -1) //Sprite is not used in this frame
                {
                    continue;
                }
                string fullSpriteName = ds.spriteNameAndMode + "_" + dsLoadIndex;

                //Check if loaded this frame (when moving up)
                if (ds.IsLoadedDuringFrame(currentFrame))
                {
                    int equateValue = MemAlloc(ds.DecompressedSize, ds.isFast); // allocate memory and get address
                    allEquatesString += fullSpriteName + " .equ " + equateValue + "\n"; //append to equates string

                    //make upwards decompress instruction for this frame
                    upDecompressCalls += "\t.dl " + ds.compressedName + ", " + fullSpriteName;
                    upDecompressCalls += "\n\t.db %";
                    //append params byte
                    upDecompressCalls += ds.isFast ? "1" : "0"; //is fast
                    upDecompressCalls += (ds.usageMode == DecompressModes.slowOff || ds.usageMode == DecompressModes.slowOffFlip) ? "1" : "0"; // is offset
                    upDecompressCalls += (ds.usageMode == DecompressModes.fastFlip || ds.usageMode == DecompressModes.slowFlip || ds.usageMode == DecompressModes.slowOffFlip) ? "1" : "0";//is flip
                    upDecompressCalls += "00000\n"; //end of byte

                    upCallCount++;
                    SpriteEquates.Add(fullSpriteName, equateValue);
                }

                //check if unloaded this frame (= loaded when moving down)
                if (ds.IsUnloadedDuringFrame(currentFrame))
                {
                    int address;
                    if(SpriteEquates.TryGetValue(fullSpriteName, out address))
                    {
                        MemFree(address);

                        //make downwards decompress instruction for this frame
                        downDecompressCalls += "\t.dl " + ds.compressedName + ", " + fullSpriteName;
                        downDecompressCalls += "\n\t.db %";
                        //append params byte
                        downDecompressCalls += ds.isFast ? "1" : "0"; //is fast
                        downDecompressCalls += (ds.usageMode == DecompressModes.slowOff || ds.usageMode == DecompressModes.slowOffFlip) ? "1" : "0"; // is offset
                        downDecompressCalls += (ds.usageMode == DecompressModes.fastFlip || ds.usageMode == DecompressModes.slowFlip || ds.usageMode == DecompressModes.slowOffFlip) ? "1" : "0";//is flip
                        downDecompressCalls += "00000\n"; //end of byte
                        downCallCount++;
                    }
                    else
                    {
                        Debug.LogError("sprite not found in equates list");
                    }
                }
            }

            decompressFramesUp += "\ndecompress_frame_up_" + currentFrame + ":\n" + "\t.db " + upCallCount + "\n" + upDecompressCalls;
            decompressFramesDown += "\ndecompress_frame_down_" + currentFrame + ":\n" + "\t.db " + downCallCount + "\n" + downDecompressCalls;
            decompressTableUp += "\t.dl decompress_frame_up_" + currentFrame + "\n";
            decompressTableDown += "\t.dl decompress_frame_down_" + currentFrame + "\n";
        }

        //Write files for equates and decompress calls
        FileWrite.WriteString(allEquatesString, "SpriteEquates");
        FileWrite.WriteString(decompressTableUp + "\n\n" + decompressTableDown + "\n\n" + decompressFramesUp + "\n\n" + decompressFramesDown, "DecompressCalls");



    }

    private int MemAlloc(int size, bool fastSprite) //Fast sprite = even# mem address 
    {
        //Begin with vram search
        int address = vRamBufferStart;
        if( (fastSprite && address%2 != 0) || (!fastSprite && address %2 == 0))
        {
            address++; //inc to get correct final bit of addr
        }
        //Loop through each memory segment and check if it overlaps with segment to be added
        foreach(MemorySegment memSegment in VirtualMemoryVRam)
        {
            bool currentSegmentOverlaps = (address < memSegment.endByte) && (memSegment.startByte < (address + size) );
            currentSegmentOverlaps |= address + size >= vRamBufferEnd;
            if (currentSegmentOverlaps)
            {
                address = memSegment.endByte;
                if ((fastSprite && address % 2 != 0) || (!fastSprite && address % 2 == 0))
                {
                    address++; //inc to get correct final bit of addr
                }
            }
            else
            {
                VirtualMemoryVRam.Add(new MemorySegment(address, size));
                VirtualMemoryVRam.Sort();
                return address;
            }
        }

        //No space in vram, search pixel shadow buffer
        address = pixelShadowStart;
        if ((fastSprite && address % 2 != 0) || (!fastSprite && address % 2 == 0))
        {
            address++; //inc to get correct final bit of addr
        }
        //Loop through each memory segment and check if it overlaps with segment to be added
        foreach (MemorySegment memSegment in VirtualMemoryPS)
        {
            bool currentSegmentOverlaps = (address < memSegment.endByte) && (memSegment.startByte < (address + size));
            currentSegmentOverlaps |= address + size >= pixelShadowEnd;
            if (currentSegmentOverlaps)
            {
                address = memSegment.endByte;
                if ((fastSprite && address % 2 != 0) || (!fastSprite && address % 2 == 0))
                {
                    address++; //inc to get correct final bit of addr
                }
            }
            else
            {
                VirtualMemoryVRam.Add(new MemorySegment(address, size));
                VirtualMemoryVRam.Sort();
                return address;
            }
        }

        return -1;
    }
    private void MemFree(int address)
    {
        for(int i = 0; i < VirtualMemoryVRam.Count;i++)
        {
            MemorySegment memSegment = VirtualMemoryVRam[i];
            if(memSegment.startByte == address)
            {
                VirtualMemoryVRam.RemoveAt(i);
                return;
            }
        }
        for (int i = 0; i < VirtualMemoryPS.Count; i++)
        {
            MemorySegment memSegment = VirtualMemoryPS[i];
            if (memSegment.startByte == address)
            {
                VirtualMemoryPS.RemoveAt(i);
                return;
            }
        }
    }



   
}
