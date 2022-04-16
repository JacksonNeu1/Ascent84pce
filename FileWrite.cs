using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class FileWrite : MonoBehaviour
{
    // Start is called before the first frame update
    public static void WriteString(string data,string fileName)
    {
        string basePath = "C:/Users/jax9h/OneDrive/Desktop/ti84/ascent/";
        string fullPath = basePath + fileName + ".txt";
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(fullPath, false);
        writer.Write(data);
        writer.Close();
    }
}
