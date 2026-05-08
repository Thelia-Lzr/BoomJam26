using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapZone : DefaultZone
{
    //public Color color = new Color(179, 117, 255, 159);
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        //rend.material.color = color;
        transform.localScale = scale;
        base.Start();
    }
}
