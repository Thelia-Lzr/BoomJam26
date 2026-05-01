using System;
using System.Collections;
using System.Collections.Generic;
using SoundManager;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    //特殊结构体定义！
    [System.Serializable]
    private struct CarStarts
    {
        public Vector2 StartVelocity;
        public double Delay;
    }
    [System.Serializable]
    private struct AvailableBorder
    {
        private bool Gravity;
        
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
            car.GetComponent<Rigidbody2D>().velocity = starts.StartVelocity;
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
