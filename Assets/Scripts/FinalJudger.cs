using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinalJudger : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    // Start is called before the first frame update
    void OnEnable()
    {
        tmp = this.GetComponent<TextMeshProUGUI>();
        tmp.text = MemoryUsedUI.Instance.memoryUsed.ToString("0000");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}