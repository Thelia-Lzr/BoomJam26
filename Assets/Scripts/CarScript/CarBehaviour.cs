using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class CarBehaviour : MonoBehaviour
{
    //框影响
    private bool isAntiGravity;
    private double speeding;
    
    public Rigidbody2D thisrb;
    public Collider2D checkcol;
    public HingeJoint2D motorWheel;
    private Collider2D[] zoneResults = new Collider2D[10];
    
    private void Awake()
    {
        isAntiGravity = false;
        foreach (Transform child in transform)
        {
            if (child.CompareTag("CarItself"))
            {
                thisrb = child.GetComponent<Rigidbody2D>();
                //Debug.Log(thisrb);
            }
        }

        foreach (Transform child in thisrb.transform)
        {
            if (child.CompareTag("Car"))
            {
                checkcol =  child.GetComponent<Collider2D>();
                //Debug.Log(checkcol);
            }
        }

        foreach (Transform child in transform)
        {
            if (child.CompareTag("CarMotorWheel"))
            {
                motorWheel = child.GetComponent<HingeJoint2D>();
            }
        }
        //thisrb = GetComponent<Rigidbody2D>();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if (LevelManager.Instance == null) return;
        Vector2 pos = checkcol.transform.position;
        Vector2 min = LevelManager.Instance.min;
        Vector2 max = LevelManager.Instance.max;
        if (!LevelManager.Instance.victoryTriggered &&  (pos.x < min.x || pos.x > max.x || pos.y < min.y || pos.y > max.y))
        {
            LevelManager.Instance.Reset();
        }
        int count = checkcol.GetContacts(zoneResults);
        //初始化
        isAntiGravity = false;
        speeding = 0;
        Debug.Log(count);
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
            Debug.Log(thisrb.gravityScale);
        }
        else
        {
            thisrb.gravityScale = 1;
        }

        if (speeding > 0)
        {
            //
            thisrb.velocity += thisrb.velocity.normalized * (float)(speeding *  Time.deltaTime);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
