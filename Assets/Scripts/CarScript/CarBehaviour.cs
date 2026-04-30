using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CarBehaviour : MonoBehaviour
{
    //框影响
    private bool isAntiGravity;
    private double speeding;
    private Rigidbody2D thisrb;
    private Collider2D checkcol;
    private Collider2D[] zoneResults = new Collider2D[10];
    
    private void Awake()
    {
        isAntiGravity = false;
        thisrb = GetComponent<Rigidbody2D>();
        checkcol =  GetComponentInChildren<Collider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        int count = checkcol.GetContacts(zoneResults);
        //初始化
        isAntiGravity = false;
        speeding = 0;
        //收集框效果
        for (int i = 0; i < count; i++)
        {
            if (zoneResults[i].TryGetComponent<GravityZone>(out GravityZone gravityZone))
            {
                isAntiGravity = true;
            }

            if (zoneResults[i].TryGetComponent<SpeedingZone>(out SpeedingZone speedingZone))
            {
                speeding += speedingZone.speeding;
            }
        }
        //处理框效果
        if (isAntiGravity)
        {
            thisrb.gravityScale = -1;
        }
        else
        {
            thisrb.gravityScale = 1;
        }

        if (speeding > 0)
        {
            
            thisrb.velocity += thisrb.velocity.normalized * (float)(speeding *  Time.deltaTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
