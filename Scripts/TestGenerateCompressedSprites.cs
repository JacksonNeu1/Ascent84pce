using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGenerateCompressedSprites : MonoBehaviour
{
    public GameObject[] spritesToConvert;

    private void Start()
    {
        string compressedSpriteData = "";
        foreach(GameObject o in spritesToConvert)
        {

            ConvertedSprite convS = SpriteConverter.CreateConvertedSprite(o.GetComponent<SpriteRenderer>().sprite);
            compressedSpriteData += convS.spriteName + ":\n" + convS.compressedData + "\n\n";
        }

        FileWrite.WriteString(compressedSpriteData, "TestGeneratedSpriteData");
    }
}
