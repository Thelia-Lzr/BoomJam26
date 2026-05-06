using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MemoryUsedUI : MonoBehaviour
{
    public static MemoryUsedUI Instance;
    public float memoryUsed;

    private TextMeshProUGUI tmp;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        memoryUsed = 0;
        tmp = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        tmp.text 
            = "Current Memory Used: " + memoryUsed.ToString();
    }
}
