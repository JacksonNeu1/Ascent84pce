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
            Texture2D tex = o.GetComponent<SpriteRenderer>().sprite.texture;
            ConvertedSprite convS = SpriteConverter.CreateConvertedSprite(tex);
            compressedSpriteData += convS.spriteName + ":\n" + convS.compressedData + "\n\n";
        }

        FileWrite.WriteString(compressedSpriteData, "TestGeneratedSpriteData");
    }
}
