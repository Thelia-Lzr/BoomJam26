using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverWindow : MonoBehaviour
{
    [field: SerializeField]
    public TextMeshProUGUI UsingTime { get; private set; }
    [field: SerializeField]
    public TextMeshProUGUI UsingMemory { get; private set; }
    [field: SerializeField]
    public RectTransform MemoryBar { get; private set; }
    public void Initial(float time,int memory, int maxMemory)
    {
        UsingTime.text = $"用时：{time:F2}";
        UsingTime.text = $"使用容量：{memory}";
        MemoryBar.anchoredPosition = new Vector2(-400 * (1 - memory/maxMemory), 0);
    }
}
