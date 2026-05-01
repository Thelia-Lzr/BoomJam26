using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [Header("摆放框框的摄像机")]
    public Transform Camera1;
    [Header("摄像机移动速度")]
    public float moveSpeed = 3f;

    [Header("摄像机地图边界")]
    public float minX, maxX;
    public float minY, maxY;
    //---------
    [Header("绑定在小车的摄像机")]
    public Transform Camera2;

    void Update()
    {
        //获取游戏进程，在摆放框框的阶段结束后自动转为绑定在小车上的摄像机
        if (true)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 pos = Camera1.position;
            pos.x += h * moveSpeed * Time.deltaTime;
            pos.y += v * moveSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            Camera1.position = pos;
        }
        //小车摄像机逻辑

    }
}
