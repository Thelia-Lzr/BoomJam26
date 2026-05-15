using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    TextMeshProUGUI tmp;
    public static Timer Instance;
    private float currentTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        currentTime = 0f;
        tmp = GetComponent<TextMeshProUGUI>();
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        tmp.text = currentTime.ToString("F2")+"s";
    }

    public float GetTime()
    {
        return currentTime;
    }

    public void Reset()
    {
        currentTime = 0f;
    }
}
