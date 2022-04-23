using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LayerParentGuidelines : MonoBehaviour
{
    public int maxFrames = 100;
    private float width = 320f;
    private float height = 240f;
    private float PPU = 10f;


    private void OnDrawGizmos()
    {
        for(int i = 0; i < maxFrames; i++)
        {
            Vector3 BL = transform.position + Vector3.up * i * height / PPU;
            Vector3 BR = transform.position + (Vector3.up * i * height / PPU) + (Vector3.right * width/PPU);
            Vector3 TL = transform.position + Vector3.up * (i+1) * height / PPU;
            Vector3 TR = transform.position + Vector3.up * (i + 1) * height / PPU + Vector3.right * width/PPU;
            Gizmos.DrawLine(BL, BR);
            Gizmos.DrawLine(BR, TR);
            Gizmos.DrawLine(BL, TL);
            Gizmos.DrawLine(TL, TR);
        }
    }
}
