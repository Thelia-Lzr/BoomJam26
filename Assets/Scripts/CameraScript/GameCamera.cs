using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEditorInternal;
using UnityEngine;

public class GameCamera : MonoBehaviour
{

    public static GameCamera Instance;
    private LevelManager levelManager => LevelManager.Instance;
    [Header("摆放框框的摄像机")]
    public Transform Camera1;
    [Header("摄像机移动速度")]
    public float moveSpeed = 5f;

    public float shiftSpeeding = 3f;

    [Header("摄像机地图边界")]
    
    private float minX, maxX;
    private float minY, maxY;
    //---------

    private bool isEditMode;
    private Transform car;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        minX = levelManager.min.x;
        maxX = levelManager.max.x;
        minY = levelManager.min.y;
        maxY = levelManager.max.y;
        levelManager.OnModeChanged += OnModeChanged;
        isEditMode = true;
        Camera1.gameObject.SetActive(true);
    }
    void Update()
    {
        //获取游戏进程，在摆放框框的阶段结束后自动转为绑定在小车上的摄像机
        if (isEditMode)
        {
            HandleEditMode();
        }
        else
        {
            HandlePlayMode();
        }
        //小车摄像机逻辑

    }
    void HandleEditMode()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 pos = Camera1.position;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            pos.x += h * moveSpeed * Time.deltaTime * shiftSpeeding;
            pos.y += v * moveSpeed * Time.deltaTime * shiftSpeeding;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }
        else
        {
            pos.x += h * moveSpeed * Time.deltaTime;
            pos.y += v * moveSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }

        Camera1.position = pos;
    }
    private void OnModeChanged(LevelManager.CurrentMode newMode)
    {
        isEditMode = newMode == LevelManager.CurrentMode.EditMode;

        // 直接开关相机，最干净
        car = levelManager.Cars[0].GetComponent<CarBehaviour>().thisrb.transform;
    }
    private void HandlePlayMode()
    {
        if (car)
        {
            Vector3 targetPos = car.position;
            targetPos.z = Camera1.position.z;
            Camera1.position = Vector3.Lerp(
                Camera1.position,
                targetPos,
                10f * Time.deltaTime
            );
        }
    }
}
