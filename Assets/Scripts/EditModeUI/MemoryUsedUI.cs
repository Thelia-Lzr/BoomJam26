using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemoryUsedUI : MonoBehaviour
{
    public static MemoryUsedUI Instance;
    public int memoryUsed;
    public Image memoryTile;

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
        memoryTile.fillAmount = (float)memoryUsed / (float)LevelManager.Instance.maxMemory;
    }
    public void ChangeMemoryUsed(int i)
    {
        memoryUsed += i;
        tmp.text = (memoryUsed- LevelManager.Instance.maxMemory).ToString() + "MB可用，共" + LevelManager.Instance.maxMemory.ToString() + "MB";

    }
}
