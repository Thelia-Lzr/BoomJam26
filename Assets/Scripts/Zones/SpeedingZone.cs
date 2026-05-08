using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedingZone : DefaultZone
{
    public double speeding = 0.5; //每秒增加的绝对速度
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        //rend.material.color = color;
        transform.localScale = scale;
        base.Start();
    }
}
