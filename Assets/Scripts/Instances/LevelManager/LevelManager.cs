using System;
using System.Collections;
using System.Collections.Generic;
using SoundManager;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
/// <summary>
/// 临时枚举类，应该放在其他地方的
/// </summary>
public enum ZoneClass
{
    Swap = 0,
    AntiGravity = 1,
    Speeding = 2,
}
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    //特殊结构体定义！
    [System.Serializable]
    private struct CarStarts
    {
        public float motorSpeed;
        //public Vector2 StartVelocity;
        public float Mass;
        public double Delay;
       
    }
    /// <summary>
    /// 由于UIManager要获取这个值，所以我改成public了
    /// </summary>
    [System.Serializable]
    public struct AvailableBorder
    {
        //这个grivaity没用过
        public bool Gravity;
    }
    [System.Serializable]
    public struct ZoneData
    {
        public string ability;
        //[Header("此关是否启用此区域")]
        //public bool enable;
        [Header("此区域的区域细节")]
        public ZoneClass zoneClass;
        [Header("此区域的区域细节")]
        public List<ZoneDetail> Details;
    }
    [System.Serializable]
    public struct ZoneDetail
    {
        [Header("区域宽")]
        public int length;
        [Header("区域高")]
        public int height;
        [Header("区域消耗")]
        public int cost;
    }
    [Header("UI相关")]
    [SerializeField] private Button StartButton;
    [Header("地图元素")]
    [SerializeField] private GameObject StartPosi;
    [SerializeField] private GameObject EndPosi;
    [Header("预制体")]
    [SerializeField] private GameObject CarPrefab;
    [Header("初始配置")]
    [SerializeField] private List<CarStarts> Starts;
    [Header("区域初始配置")]
    [SerializeField] public List<ZoneData> Borders;
    
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        StartButton.onClick.AddListener(startStimulate);
    }

    void startStimulate()
    {
        StartCoroutine(Stimulate());
    }

    IEnumerator Stimulate()
    {
        foreach (CarStarts starts in Starts)
        {
            yield return  new WaitForSeconds((float)starts.Delay);
            GameObject car = Instantiate(CarPrefab, StartPosi.transform.position, StartPosi.transform.rotation);
            CarBehaviour thiscar = car.GetComponent<CarBehaviour>();
            //thiscar.thisrb.velocity = starts.StartVelocity;
            JointMotor2D motor = thiscar.motorWheel.motor;
            motor.motorSpeed = starts.motorSpeed;
            thiscar.motorWheel.motor = motor;
            thiscar.thisrb.mass = starts.Mass;
            // Rigidbody2D rb = car.GetComponent<Rigidbody2D>();
            // rb.velocity = starts.StartVelocity;
            // rb.mass = starts.Mass;
        }
    }
    public Button StartButton1
    {
        get => StartButton;
        set => StartButton = value;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
