using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CarBehaviour : MonoBehaviour
{
    private bool isAntiGravity;
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
        isAntiGravity = false;
        for (int i = 0; i < count; i++)
        {
            if (zoneResults[i].TryGetComponent<GravityZone>(out GravityZone zone))
            {
                isAntiGravity = true;
            }
        }
        if (isAntiGravity)
        {
            thisrb.gravityScale = -1;
        }
        else
        {
            thisrb.gravityScale = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
