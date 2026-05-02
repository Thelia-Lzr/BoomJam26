using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ?可以和playerUImanager合并成一个UIManager(?)
/// </summary>
public class EditUIManager : MonoBehaviour
{
    private LevelManager levelManager;
    private LevelManager.AvailableBorder border;
    [field: SerializeField]
    public GameObject SwapZone {  get; private set; }
    [field: SerializeField]
    public GameObject AntiGravityZone { get; private set; }
    [field: SerializeField]
    public GameObject SpeedingZone { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        levelManager = LevelManager.Instance;
        //配置
        border = levelManager.Borders[0];
        int pos = 0;
        if (border.SwapZone.enable == true)
        {
            SwapZone.GetComponent<RectTransform>().anchoredPosition = new Vector2(25 + 175 * pos, 25);
            SwapZone.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text =
               $"{border.SwapZone.length}x{border.SwapZone.height}";
            ++pos;
        }
        else
        {
            SwapZone.SetActive(false);
        }
        if (border.AntiGravityZone.enable == true)
        {
            AntiGravityZone.GetComponent<RectTransform>().anchoredPosition = new Vector2(25 + 175 * pos, 25);
            AntiGravityZone.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text =
               $"{border.AntiGravityZone.length}x{border.AntiGravityZone.height}";
            ++pos;
        }
        else
        {
            AntiGravityZone.SetActive(false);
        }
        if (border.SpeedingZone.enable == true)
        {
            SpeedingZone.GetComponent<RectTransform>().anchoredPosition = new Vector2(25 + 175 * pos, 25);
            SpeedingZone.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text =
                $"{border.SpeedingZone.length}x{border.SpeedingZone.height}";
            ++pos;
        }
        else
        {
            SpeedingZone.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
