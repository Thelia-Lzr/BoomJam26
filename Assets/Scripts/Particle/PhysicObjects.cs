using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicObjects : MonoBehaviour
{
    //这个脚本只有一个作用：使得所有需要物理模拟的物体在Start被点击后才开始模拟,场景重置时被拉回
    // Start is called before the first frame update
    private Rigidbody2D rigidBody2D;
    private Vector3 startPosition;
    void Start()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        rigidBody2D.bodyType = RigidbodyType2D.Kinematic;
    }

    // Update is called once per frame
    void Update()
    {
        if (rigidBody2D.bodyType == RigidbodyType2D.Kinematic &&
            LevelManager.Instance.currentMode == LevelManager.CurrentMode.PlayMode)
        {
            rigidBody2D.bodyType = RigidbodyType2D.Dynamic;
        }
        
        if (rigidBody2D.bodyType == RigidbodyType2D.Dynamic &&
            LevelManager.Instance.currentMode == LevelManager.CurrentMode.EditMode)
        {
            rigidBody2D.bodyType = RigidbodyType2D.Kinematic;
            transform.position = startPosition;
        }

    }
}
