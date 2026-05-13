using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public static Timer Instance;
    public float curTime;
    private bool timing;
    private void Awake()
    {
        Instance = this;
    }
    void Update()
    {
        if (timing) curTime += Time.deltaTime;
    }
    public void StartTimer()
    {
        curTime = 0;
        timing = true;
        
    }
    public void EndTimer()
    {
        timing = false;
    }
}
