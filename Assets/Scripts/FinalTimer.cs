using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinalTimer : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    // Start is called before the first frame update
    void OnEnable()
    {
        tmp = this.GetComponent<TextMeshProUGUI>();
        tmp.text = Timer.Instance.GetTime().ToString("0000");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
