using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpriteDataGenerate
{
    public static int vRamBufferEnd = 0xD49000;
    public static int pixelShadowEnd = 0xD13FD8;
    
    /*
    public static void GenerateSpriteData(List<ConvertedSprite> allSprites) //Computes locations for sprites to be decompressed to
    {
        int decompressDataVramPtr = 0xD40000;
        int decompressDataShadowPtr = 0xD031F6;

        string compressedData = "";
        string equates = "";
        string[] decompressCalls = new string[10];
        
        foreach(ConvertedSprite sprite in allSprites)
        {
            compressedData += sprite.spriteName + ":\n" + sprite.compressedData + "\n\n";

            for (int i = 0; i < 10; i++){
                if (sprite.useageModes[i])
                {
                    
                    mode = (DecompressModes)i;
                    int length = sprite.decompressedSize[i];
                    int addressVram = decompressDataVramPtr;
                    int addressShadow = decompressDataShadowPtr;

                    if  ((mode == DecompressModes.slow) ||
                        (mode == DecompressModes.slowFlip) ||
                        (mode == DecompressModes.slowOff) ||
                        (mode == DecompressModes.slowOffFlip)){

                        //Slow sprite, must be on odd address
                        if (addressVram % 2 == 0)
                        {
                            addressVram++;
                        }
                        if(addressShadow % 2 == 0)
                        {
                            addressShadow++;
                        }
                    }else if ((mode == DecompressModes.fast) || (mode == DecompressModes.fastFlip))
                    {
                        //Fast sprite, must be on even address
                        if (addressVram % 2 == 1)
                        {
                            addressVram++;
                        }
                        if (addressShadow % 2 == 1)
                        {
                            addressShadow++;
                        }
                    }

                    int location = 0;
                    if (addressVram + length < vRamBufferEnd)
                    {
                        location = addressVram;
                        decompressDataVramPtr = addressVram + length;
                    }else if (addressShadow + length < pixelShadowEnd)
                    {
                        location = addressShadow;
                        decompressDataVramPtr = addressShadow + length;
                    }
                    else
                    {
                        Debug.LogError("No more room for decompressed sprite data :(");
                    }

                    string equateName = LevelDataGenerate.GetDecompressedName(mode, sprite.spriteName);

                    equates += equateName + " .equ $" + location.ToString("x") + "\n";

                    if ((mode == DecompressModes.slowOff) || (mode == DecompressModes.slowOffFlip))
                    {
                        decompressCalls[i] += "\tcall sdcomp_set_offset \n";
                    }

                    decompressCalls[i] += "\tld hl, " + sprite.spriteName + "\n\tld de, " + equateName + "\n\tcall sprite_decompress\n";

                }

            }
            


        }

        //Combine decompress calls
        string decompressCallsAll = "decompress_calls:\n\n";
        for(int i = 0; i < 10; i++)
        {
            DecompressModes mode = (DecompressModes)i;

            switch (mode)
            {
                case DecompressModes.slow:
                    decompressCallsAll += "\tcall sdcomp_reset_bg_sprite \n\tcall sdcomp_reset_flip\n\tcall sdcomp_reset_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;
                case DecompressModes.slowFlip:
                    decompressCallsAll += "\tcall sdcomp_reset_bg_sprite \n\tcall sdcomp_set_flip\n\tcall sdcomp_reset_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;
                case DecompressModes.slowOff:
                    decompressCallsAll += "\tcall sdcomp_reset_bg_sprite \n\tcall sdcomp_reset_flip\n\tcall sdcomp_reset_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;
                case DecompressModes.slowOffFlip:
                    decompressCallsAll += "\tcall sdcomp_reset_bg_sprite \n\tcall sdcomp_set_flip\n\tcall sdcomp_reset_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;
                case DecompressModes.fast:
                    decompressCallsAll += "\tcall sdcomp_reset_bg_sprite \n\tcall sdcomp_reset_flip\n\tcall sdcomp_set_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;
                case DecompressModes.fastFlip:
                    decompressCallsAll += "\tcall sdcomp_reset_bg_sprite \n\tcall sdcomp_set_flip\n\tcall sdcomp_set_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;

            }



        }
        decompressCallsAll += "\tret\n";

        //Debug.Log(decompressCallsAll);
       // Debug.Log(compressedData);
       // Debug.Log(equates);

        FileWrite.WriteString(decompressCallsAll, "Decompress_Calls");
        FileWrite.WriteString(compressedData, "Sprite_Data");
        FileWrite.WriteString(equates, "Equates");
    }

    */
}
