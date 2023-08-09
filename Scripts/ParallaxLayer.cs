using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public float parallaxScale;
    void Update()
    {
        Vector2 campos = (Vector2)Camera.main.transform.position - Vector2.right * Camera.main.orthographicSize * Camera.main.aspect - Vector2.up * Camera.main.orthographicSize;

        transform.position = new Vector2(campos.x, campos.y * (1 - parallaxScale));

    }
}
