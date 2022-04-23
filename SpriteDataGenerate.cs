using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpriteDataGenerate
{
    public static int vRamBufferEnd = 0xD49000;
    public static int pixelShadowEnd = 0xD13FD8;
    

    public static void generateSpriteData(List<convertedSprite> allSprites)
    {
        int decompressDataVramPtr = 0xD40000;
        int decompressDataShadowPtr = 0xD031F6;

        string compressedData = "";
        string equates = "";
        string[] decompressCalls = new string[10];
        
        foreach(convertedSprite sprite in allSprites)
        {
            compressedData += sprite.spriteName + ":\n" + sprite.compressedData + "\n\n";

            for (int i = 0; i < 10; i++){
                if (sprite.useageModes[i])
                {
                    convertedSprite.decompressModes mode = (convertedSprite.decompressModes)i;
                    int length = sprite.decompressedSize[i];
                    int addressVram = decompressDataVramPtr;
                    int addressShadow = decompressDataShadowPtr;

                    if  ((mode == convertedSprite.decompressModes.slow) ||
                        (mode == convertedSprite.decompressModes.slowFlip) ||
                        (mode == convertedSprite.decompressModes.slowOff) ||
                        (mode == convertedSprite.decompressModes.slowOffFlip)){

                        //Slow sprite, must be on odd address
                        if (addressVram % 2 == 0)
                        {
                            addressVram++;
                        }
                        if(addressShadow % 2 == 0)
                        {
                            addressShadow++;
                        }
                    }else if ((mode == convertedSprite.decompressModes.fast) || (mode == convertedSprite.decompressModes.fastFlip))
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

                    string equateName = levelDataGenerate.getDecompressedName(mode, sprite.spriteName);

                    equates += equateName + " .equ $" + location.ToString("x") + "\n";

                    if ((mode == convertedSprite.decompressModes.slowOff) ||
                        (mode == convertedSprite.decompressModes.slowOffFlip) ||
                        (mode == convertedSprite.decompressModes.BGOff) ||
                        (mode == convertedSprite.decompressModes.BGOffFlip))
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
            convertedSprite.decompressModes mode = (convertedSprite.decompressModes)i;

            switch (mode)
            {
                case convertedSprite.decompressModes.slow:
                    decompressCallsAll += "\tcall sdcomp_reset_bg_sprite \n\tcall sdcomp_reset_flip\n\tcall sdcomp_reset_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;
                case convertedSprite.decompressModes.slowFlip:
                    decompressCallsAll += "\tcall sdcomp_reset_bg_sprite \n\tcall sdcomp_set_flip\n\tcall sdcomp_reset_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;
                case convertedSprite.decompressModes.slowOff:
                    decompressCallsAll += "\tcall sdcomp_reset_bg_sprite \n\tcall sdcomp_reset_flip\n\tcall sdcomp_reset_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;
                case convertedSprite.decompressModes.slowOffFlip:
                    decompressCallsAll += "\tcall sdcomp_reset_bg_sprite \n\tcall sdcomp_set_flip\n\tcall sdcomp_reset_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;
                case convertedSprite.decompressModes.fast:
                    decompressCallsAll += "\tcall sdcomp_reset_bg_sprite \n\tcall sdcomp_reset_flip\n\tcall sdcomp_set_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;
                case convertedSprite.decompressModes.fastFlip:
                    decompressCallsAll += "\tcall sdcomp_reset_bg_sprite \n\tcall sdcomp_set_flip\n\tcall sdcomp_set_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;
                case convertedSprite.decompressModes.BG:
                    decompressCallsAll += "\tcall sdcomp_set_bg_sprite \n\tcall sdcomp_reset_flip\n\tcall sdcomp_reset_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;
                case convertedSprite.decompressModes.BGFlip:
                    decompressCallsAll += "\tcall sdcomp_set_bg_sprite \n\tcall sdcomp_set_flip\n\tcall sdcomp_reset_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;
                case convertedSprite.decompressModes.BGOff:
                    decompressCallsAll += "\tcall sdcomp_set_bg_sprite \n\tcall sdcomp_reset_flip\n\tcall sdcomp_reset_fast_sprite\n";
                    decompressCallsAll += decompressCalls[i];
                    break;
                case convertedSprite.decompressModes.BGOffFlip:
                    decompressCallsAll += "\tcall sdcomp_set_bg_sprite \n\tcall sdcomp_set_flip\n\tcall sdcomp_reset_fast_sprite\n";
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


}
