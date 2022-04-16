using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class paletteManager : MonoBehaviour
{
    public Color[] palette1;


    public static paletteManager main;

    private void Awake()
    {
        if (main == null)
        {
            main = this;
        }
    }
    private void Start()
    {
        string paletteData = paletteSetup(palette1);
        Debug.Log(paletteSetup(palette1));
        FileWrite.WriteString(paletteData, "PaletteSetup");

    }
    private string paletteSetup(Color[] palette)
    {
        string s = "ld hl,mpLcdPalette\n";
        foreach (Color color in palette)
        {
            int green = (int)(color.g * 63);
            int red = (int)(color.r * 31);
            int blue = (int)(color.b * 31);

            int byte1 = ((green & 0b001110) << 4) + blue;
            int byte2 = ((green & 0b000001) << 7) + (red << 2) + ((green & 0b110000) >> 4);

            string byte1s = System.Convert.ToString(byte1,2).PadLeft(8,'0');
            string byte2s = System.Convert.ToString(byte2, 2).PadLeft(8,'0');
            s += "ld a,%" + byte1s + "\n";
            s += "ld (hl),a \n";
            s += "inc hl\n";
            s += "ld a,%" + byte2s + "\n";
            s += "ld (hl),a \n";
            s += "inc hl\n";
        }
        return s;
    }
    public int getIndex(Color color)
    {
        //Debug.Log(color);
        //Debug.Log(pallete1[0]);
        for(int i = 0; i < palette1.Length; i++)
        {
            if (color == palette1[i])
            {
                return i;
            }
        }
        return -1;
    }

}
