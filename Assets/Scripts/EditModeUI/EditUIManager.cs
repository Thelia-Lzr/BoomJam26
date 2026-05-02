using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static LevelManager;
using static UnityEditor.Progress;

/// <summary>
/// ?可以和playerUImanager合并成一个UIManager(?)
/// </summary>
public class EditUIManager : MonoBehaviour
{
    //获取单例
    private LevelManager levelManager;
    private List<ZoneData> borders;
    //记录当前选择的区域编号
    public int ZoneIndex;
    //保存
    private List<RectTransform> zones;
    //保存
    private List<RectTransform> zoneDetails;
    [field: SerializeField]
    public RectTransform ZoneContent { get; private set; }
    [field: SerializeField]
    public GameObject ZonePrefab { get; private set; }
    //
    [field: SerializeField]
    public RectTransform ZoneDetailContent { get; private set; }
    [field: SerializeField]
    public GameObject ZoneDetailPrefab { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        levelManager = LevelManager.Instance;
        ////配置
        borders = levelManager.Borders;
        GenerateZone();
    }
    void GenerateZone()
    {
        zones = new List<RectTransform>();
        zoneDetails = new List<RectTransform>();
        ZoneContent.sizeDelta = new Vector2(85 * (borders.Count + 2), 0);
        for (int i = 0; i < borders.Count; ++i)
        {
            GameObject zone = Instantiate(ZonePrefab, ZoneContent);
            RectTransform rect = zone.GetComponent<RectTransform>();
            switch (borders[i].zoneClass)
            {
                case ZoneClass.Swap:
                    {
                        rect.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Swap";
                        GameObject newDetails = new GameObject("SwapZoneDetail", typeof(RectTransform));
                        newDetails.SetActive(false);
                        RectTransform detailRect = newDetails.GetComponent<RectTransform>();
                        detailRect.SetParent(ZoneDetailContent, false);
                        detailRect.sizeDelta = Vector2.zero;
                        zoneDetails.Add(detailRect);
                        for (int j = 0; j < borders[i].Details.Count; ++j)
                        {
                            GameObject detail = Instantiate(ZoneDetailPrefab, newDetails.GetComponent<RectTransform>());
                            RectTransform rectDetail = detail.GetComponent<RectTransform>();
                            rectDetail.anchoredPosition = new Vector2(100 + 200 * j, 75);
                            rectDetail.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                                $"{borders[i].Details[j].length}X{borders[i].Details[j].height}";
                            rectDetail.GetChild(2).GetComponent<TextMeshProUGUI>().text =
                                $"{borders[i].Details[j].cost}MB";
                        }
                        break;
                    }
                case ZoneClass.AntiGravity:
                    {
                        rect.GetChild(1).GetComponent<TextMeshProUGUI>().text = "AntiGravity";
                        GameObject newDetails = new GameObject("AntiGravityZoneDetail", typeof(RectTransform));
                        newDetails.SetActive(false);
                        RectTransform detailRect = newDetails.GetComponent<RectTransform>();
                        detailRect.SetParent(ZoneDetailContent, false);
                        detailRect.sizeDelta = Vector2.zero;
                        zoneDetails.Add(detailRect);
                        for (int j = 0; j < borders[i].Details.Count; ++j)
                        {
                            GameObject detail = Instantiate(ZoneDetailPrefab, newDetails.GetComponent<RectTransform>());
                            RectTransform rectDetail = detail.GetComponent<RectTransform>();
                            rectDetail.anchoredPosition = new Vector2(100 + 200 * j, 75);
                            rectDetail.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                                $"{borders[i].Details[j].length}X{borders[i].Details[j].height}";
                            rectDetail.GetChild(2).GetComponent<TextMeshProUGUI>().text =
                                $"{borders[i].Details[j].cost}MB";
                        }
                        break;
                    }
                case ZoneClass.Speeding:
                    {
                        rect.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Speeding";
                        GameObject newDetails = new GameObject("SpeedingZoneDetail", typeof(RectTransform));
                        newDetails.SetActive(false);
                        RectTransform detailRect = newDetails.GetComponent<RectTransform>();
                        detailRect.SetParent(ZoneDetailContent, false);
                        detailRect.sizeDelta = Vector2.zero;
                        zoneDetails.Add(detailRect);
                        for (int j = 0; j < borders[i].Details.Count; ++j)
                        {
                            GameObject detail = Instantiate(ZoneDetailPrefab, newDetails.GetComponent<RectTransform>());
                            RectTransform rectDetail = detail.GetComponent<RectTransform>();
                            rectDetail.anchoredPosition = new Vector2(100 + 200 * j, 75);
                            rectDetail.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                                $"{borders[i].Details[j].length}X{borders[i].Details[j].height}";
                            rectDetail.GetChild(2).GetComponent<TextMeshProUGUI>().text =
                                $"{borders[i].Details[j].cost}MB";
                        }
                        break;
                    }
            }
            // 初始化位置 (先按顺序排开)
            rect.anchoredPosition = new Vector2(125 + i * 85, 40);
            zones.Add(rect);
        }
        ZoneIndex = 0;
        zones[ZoneIndex].localScale = new Vector3(1.1f, 1.1f, 1.1f);
        zoneDetails[ZoneIndex].gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void HandleInput()
    {
        int cur = (int)(ZoneContent.anchoredPosition.x * -1 + 42.5f) / 85;
        if(cur != ZoneIndex)
        {
            zones[ZoneIndex].localScale = Vector3.one;
            zones[cur].localScale = new Vector3(1.1f, 1.1f, 1.1f);
            zoneDetails[ZoneIndex].gameObject.SetActive(false);
            zoneDetails[cur].gameObject.SetActive(true);
            ZoneIndex = cur;
        }
    }

    public void Drag(int idx)
    {

    }
}
