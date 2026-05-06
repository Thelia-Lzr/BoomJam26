using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityZone : MonoBehaviour
{
    //public Color color = new Color(179, 117, 255, 159);
    public Vector2 scale = new Vector2(1, 1);
    public int memoryUsed = 0;
    public bool placed = false;
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        //rend.material.color = color;
        transform.localScale = scale;
    }
    
}
