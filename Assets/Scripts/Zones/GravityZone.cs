using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityZone : DefaultZone
{
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        //rend.material.color = color;
        transform.localScale = scale;
    }
    
}
