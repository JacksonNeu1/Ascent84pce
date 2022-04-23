using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestStruct {
   public int x;
   public int y;
}


public class test : MonoBehaviour
{
    private List<TestStruct> structs = new List<TestStruct>();
    // Start is called before the first frame update
    void Start()
    {

        TestStruct s1 = new TestStruct();
        s1.x = 1;
        s1.y = 2;
        structs.Add(s1);

        Debug.Log(s1.x);
        TestStruct s2 = structs[0];
        s2.x = 5;
        Debug.Log(s2.x);
        Debug.Log(structs[0].x);
        testFunc(s2);
        Debug.Log(structs[0].x);

    }

    private void testFunc(TestStruct s)
    {
        s.x = 10;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
