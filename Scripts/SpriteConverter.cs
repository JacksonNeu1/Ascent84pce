using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//stores a compressed sprite binary data, and metadata (converted from texture to ti84 sprite)
public class ConvertedSprite {

    
    public string spriteName;
    public string compressedData;
    public int width;
    public int height;
    public bool fast;
    public int compressedSize;
    public int[] decompressedSize;
    public bool[] useageModes;
}

public enum DecompressModes
{
    slow,
    slowFlip,
    slowOff,
    slowOffFlip,
    fast,
    fastFlip,
    // BG,
    // BGOff,
    // BGFlip,
    //BGOffFlip
}

public static class SpriteConverter
{
  
    public static ConvertedSprite CreateConvertedSprite(Texture2D sprite)
    {

        ConvertedSprite convSprite = new ConvertedSprite();

        convSprite.spriteName = sprite.name;

        convSprite.decompressedSize = new int[10];
        convSprite.useageModes = new bool[10];

        int width = sprite.width;
        int height = sprite.height;

        convSprite.width = width;
        convSprite.height = height;

        Color[] cols = sprite.GetPixels();
         
        List<int> alphaData = new List<int>();
        bool hasAlpha = false;
        List<int> uniqueColors = new List<int>();
        List<int> colorData = new List<int>();


        for (int y = height-1; y >=0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                alphaData.Add((int)cols[i].a);
                if (cols[i].a == 0)
                {
                    hasAlpha = true;
                }
                else
                {
                    int palleteIndex = PaletteManager.main.GetIndex(cols[i]); //converts color to LCD index
                    //Debug.Log(palleteIndex);
                    colorData.Add(palleteIndex);
                    if (!uniqueColors.Contains(palleteIndex))
                    {
                        // Debug.Log("unique");
                        uniqueColors.Add(palleteIndex);
                    }
                }
            }
            
        }

        //check if fast sprite 
        convSprite.fast = width%2 == 0;//can be fast if even width
        for(int i = 0; i < alphaData.Count-1; i+=2)
        {
            if(alphaData[i] != alphaData[i + 1])
            {
                convSprite.fast = false;
            }
        }


        int compressedSize = 1;//pallete data size
        int bpc = 0;//1color
        if(uniqueColors.Count > 1)
        {//2color
            bpc = 1;
            compressedSize = 1;
        }
        if (uniqueColors.Count > 2)
        {//4color
            bpc = 2;
            compressedSize = 2;

            for(int i = uniqueColors.Count; i < 4; i++)
            {
                uniqueColors.Add(0);
            }

        }
        if (uniqueColors.Count > 4)
        {//8color
            bpc = 3;
            compressedSize = 4;
            for (int i = uniqueColors.Count; i < 8; i++)
            {
                uniqueColors.Add(0);
            }

        }
        if (uniqueColors.Count > 8)
        {
            bpc = 4;
            compressedSize = 0;
        }


        compressedSize += 3 + //flags and w,h
            (hasAlpha ? (alphaData.Count / 8) : 0) + // alpha
            ((colorData.Count * bpc) / 8);//color

        convSprite.compressedSize = compressedSize;

        //Debug.Log(compressedSize);
        //create flags byte
        string compressedString = "\t.db %";
        compressedString += bpc == 4 ? "1" : "0";
        compressedString += bpc == 3 ? "1" : "0";
        compressedString += bpc == 2 ? "1" : "0";
        compressedString += bpc == 1 ? "1" : "0";
        compressedString += bpc == 0 ? "1" : "0";
        compressedString += !hasAlpha ? "1" : "0";
        compressedString += "00\n";

        //create width/height bytes
        compressedString += "\t.db " + width + ", " + height;


        if (hasAlpha)
        {
            //create alpha data string
            compressedString += Bitstream2db(alphaData, 1);
        }
        if (bpc != 4)
        {
           //create palette data string
            compressedString += Bitstream2db(uniqueColors, 4);

            //convert color data from LCD index to local index
            for(int i = 0; i < colorData.Count; i++)
            {
                colorData[i] = uniqueColors.IndexOf(colorData[i]);
            }

        }
        //create color data string
        compressedString += Bitstream2db(colorData, bpc);
        convSprite.compressedData = compressedString;
        //Debug.Log(compressedString);

        convSprite.decompressedSize[(int)DecompressModes.slow] = GetDecompressedSizeSlow(alphaData, width, height);
        convSprite.decompressedSize[(int)DecompressModes.fast] = GetDecompressedSizeFast(alphaData, width, height);
        //convSprite.decompressedSize[(int)DecompressModes.BG] = getDecompressedSizeBg(width, height);

        //create flipped alpha data
        List<int> alphaFlipped = new List<int>();
        for (int y = 0; y < height; y++)
        {
            for (int x = width-1;x >=0; x--)
            {
                alphaFlipped.Add(alphaData[x + y * width]);
            }
        }

        convSprite.decompressedSize[(int)DecompressModes.slowFlip] = GetDecompressedSizeSlow(alphaFlipped, width, height);
        convSprite.decompressedSize[(int)DecompressModes.fastFlip] = GetDecompressedSizeFast(alphaFlipped, width, height);
       // convSprite.decompressedSize[(int)DecompressModes.BGFlip] = convSprite.decompressedSize[(int)DecompressModes.BG];

        //create offset data
        for (int x = 0; x < height; x++)
        {
            alphaData.Insert(x * (width+1), 0);
            alphaFlipped.Insert(x * (width + 1), 0);
        }
        convSprite.decompressedSize[(int)DecompressModes.slowOff] = GetDecompressedSizeSlow(alphaData, width+1, height);

        //convSprite.decompressedSize[(int)DecompressModes.BGOff] = getDecompressedSizeBg(width+1, height);
        //convSprite.decompressedSize[(int)DecompressModes.BGOffFlip] = convSprite.decompressedSize[(int)DecompressModes.BGOff];

        convSprite.decompressedSize[(int)DecompressModes.slowOffFlip] = GetDecompressedSizeSlow(alphaFlipped, width+1, height);

        return convSprite;
    }

    //converts list of ints to bitstream
    private static string Bitstream2db(List<int> vals,int bitsPerNum) //converts a bitstream into a series of .db bytes formatted as a string
    {
        string s= "";
        int c = 0;

        foreach(int i in vals)
        {
            for(int bitIndex = bitsPerNum-1; bitIndex > -1; bitIndex--)
            {
                bool bitValue = ((i >> bitIndex) & 1) == 1;
                if (c % 64 == 0)
                {
                    s += "\n\t.db %";
                } else if (c % 8 == 0)
                {
                    s += ", %";
                }
                s += bitValue ? "1" : "0";
                c++;
            }
        }
        while(c % 8 != 0)
        {
            s += "0";
            c++;
        }
        return s + "\n";
    }

    private static int GetDecompressedSizeSlow(List<int> alphaData,int width,int height)
    {
        int size = 1 + height;
        for (int row = 0; row < height; row++)
        {

            int i = 0;//index in row
            do
            {
                while ((i < width ? alphaData[(row * width) + i] : 0) == 0 && (i + 1 < width ? alphaData[(row * width) + i + 1] : 0) == 0 && i < width)
                {
                    i += 2;
                }
                if(i >= width)
                {
                    break;
                }
                //leading alpha pair found
                size += 4;//gap length, leading pix mask, leading pix data,ldir length
                i += 2;//move to next pair
                bool hasLdir = false;
                while ((i < width ? alphaData[(row * width) + i] : 0) == 1 && (i + 1 < width ? alphaData[(row * width) + i + 1] : 0) == 1)
                {
                    i += 2;
                    size++;
                    hasLdir = true;
                }
                if ((i < width ? alphaData[(row * width) + i] : 0) == 0 && (i + 1 < width ? alphaData[(row * width) + i + 1] : 0) == 0 && hasLdir)
                {
                    size--;//subtract 1, final 11 pair is ending pixels
                }

                size += 2; //ending mask and pixels 
                i += 2;
            }while (i < width);
        }

        return size;
    }

    private static int GetDecompressedSizeFast(List<int> alphaData, int width, int height)
    {
        int size = 1 + height;
        for (int row = 0; row < height; row++)
        {

            int i = 0;//index in row
            do
            {
                //skip through leading 0s
                while ((i < width ? alphaData[(row * width) + i] : 0) == 0 && (i + 1 < width ? alphaData[(row * width) + i + 1] : 0) == 0 && i < width)
                {
                    i += 2;
                }
                if (i >= width)
                {
                    break;
                }
                //leading alpha pair found
                size += 2;//gap length,ldir length
                while ((i < width ? alphaData[(row * width) + i] : 0) == 1 && (i + 1 < width ? alphaData[(row * width) + i + 1] : 0) == 1)
                {
                    i += 2;
                    size++;
                }
                i += 2;
            } while (i < width);
        }

        return size;
    }

    /*
    private static int getDecompressedSizeBg(int width, int height)
    {
        return 1 + ((width % 2 == 0 ? width : width+1) * height);
    }*/

}
