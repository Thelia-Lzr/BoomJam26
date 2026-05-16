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
    public event Action StartButtonClicked;
    public enum CurrentMode
    {
        TestMode = 0,
        EditMode = 1,
        PlayMode = 2
    }
    public CurrentMode currentMode = CurrentMode.EditMode;
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
    public struct ZoneData
    {
        public string ability;
        //[Header("此关是否启用此区域")]
        //public bool enable;
        [Header("此区域的区域类别")]
        public ZoneClass zoneClass;

        [Header("区域设置")] 
        public float? speedingZoneSpeed;
        public float? gravityZoneGravity;
        [Header("区域宽")]
        public int length;
        [Header("区域高")]
        public int height;
        [Header("区域消耗")]
        public int cost;
    }
    [Header("UI相关")]
    [SerializeField] private Button StartButton;
    [SerializeField] private Button ResetButton;
    [SerializeField] private GameObject EditModeUI;
    [SerializeField] private GameObject PlayModeUI;
    [Header("地图元素")]
    [SerializeField] private GameObject StartPosi;
    [SerializeField] private GameObject EndPosi;
    [Header("预制体")]
    [SerializeField] private GameObject CarPrefab;
    [Header("初始配置")]
    [SerializeField] private List<CarStarts> Starts;
    [SerializeField] public int maxMemory = 64;
    [Header("区域限制")] 
    [SerializeField] public Vector2 min;
    [SerializeField] public Vector2 max;
    [Header("相机限制")]
    [SerializeField] public Vector2 cameraMin;
    [SerializeField] public Vector2 cameraMax;
    [Header("Zone初始配置")]
    [SerializeField] public List<ZoneData> Borders;

    [Header("内存限制星星")] 
    [SerializeField] public bool starEnabled = false;
    [SerializeField] public List<int> memoryLimits;
    
    
    
    private List<GameObject> Cars = new List<GameObject>(); 
    public bool victoryTriggered = false;
    

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
        StartButton.onClick.AddListener(HandleStartButtonClicked);
        ResetButton.onClick.AddListener(Reset);
        victoryTriggered =  false;
    }

    private void HandleStartButtonClicked()
    {
        StartButtonClicked?.Invoke();
        startStimulate();
    }

    void startStimulate()
    {
        for (int i = 0; i < memoryLimits.Count; i++)
        {
            if (MemoryUsedUI.Instance.memoryUsed <= memoryLimits[i])
            {
                StarUI.Instance.currentStar += 1;
            }
        }
        if (currentMode == CurrentMode.EditMode)
        {
            Cars =  new List<GameObject>();
            currentMode =  CurrentMode.PlayMode;
            StartCoroutine(Stimulate());
        }
    }

    IEnumerator Stimulate()
    {
        foreach (CarStarts starts in Starts)
        {
            yield return  new WaitForSeconds((float)starts.Delay);
            GameObject car = Instantiate(CarPrefab, StartPosi.transform.position, StartPosi.transform.rotation);
            Cars.Add(car);
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
    private bool rPressedLastFrame = false;
    private bool ePressedLastFrame = false;
    // Update is called once per frame
    void Update()
    {
        bool rPressedNow = Input.GetKey(KeyCode.R);
        bool ePressedNow = Input.GetKey(KeyCode.E);
        if (rPressedNow && !rPressedLastFrame)
        {
            Reset();
        }
        if (ePressedNow && !ePressedLastFrame)
        {
            startStimulate();
        }
        rPressedLastFrame = rPressedNow;
        ePressedLastFrame = ePressedNow;
        if (currentMode == CurrentMode.EditMode)
        {
            EditModeUI.SetActive(true);
            PlayModeUI.SetActive(false);
        }
        else if (currentMode == CurrentMode.PlayMode)
        {
            EditModeUI.SetActive(false);
            PlayModeUI.SetActive(true);
            
        }
    }

    public void Reset()
    {
        Timer.Instance.Reset();
        if (currentMode == CurrentMode.PlayMode)
        {
            currentMode = CurrentMode.EditMode;
            StarUI.Instance.currentStar = 0;
            foreach (GameObject car in Cars)
            {
                Destroy(car);
            }
        }
    }

    public void Pause()
    {
        foreach (GameObject car in Cars)
        {
            Destroy(car);
        }
    }
}
