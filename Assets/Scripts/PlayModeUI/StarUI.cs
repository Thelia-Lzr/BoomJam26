using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StarUI : MonoBehaviour
{
    public static StarUI Instance;
    public int currentStar;
    
    private TextMeshProUGUI tmp;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        tmp = GetComponent<TextMeshProUGUI>();
        tmp.text = "CurrentStar:" + currentStar.ToString();
        currentStar = 0;
    }

    // Update is called once per frame
    void Update()
    {
        tmp.text = "CurrentStar:" + currentStar.ToString();
    }
}
