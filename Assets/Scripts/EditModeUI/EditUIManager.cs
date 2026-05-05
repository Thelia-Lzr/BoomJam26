using System;
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
    List<List<ZoneData>> zoneData;
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
        GenerateZone();
    }
    void GenerateZone()
    {
        List<ZoneData> borders = levelManager.Borders;
        //类别分类
        int enumCount = Enum.GetValues(typeof(ZoneClass)).Length;
        // 创建对应容量的 List
        List<List<ZoneData>> zoneData = new List<List<ZoneData>>(enumCount);
        for (int i = 0; i < enumCount; i++)
        {
            zoneData.Add(new List<ZoneData>());
        }
        for (int i = 0; i < borders.Count; i++)
        {
            zoneData[(int)borders[i].zoneClass].Add(borders[i]);
        }
        int bordernum = 0;
        //统计类别数
        for (int i = 0; i < enumCount; i++)
        {
            if (zoneData[i].Count > 0)
            {
                bordernum++;
            }
        }
        zones = new List<RectTransform>();
        zoneDetails = new List<RectTransform>();
        ZoneContent.sizeDelta = new Vector2(85 * (bordernum + 2), 0);

        for (int i = 0; i < enumCount; i++)
        {
            if (zoneData[i].Count > 0)
            {
                GameObject zone = Instantiate(ZonePrefab, ZoneContent);
                RectTransform rect = zone.GetComponent<RectTransform>();
                switch ((ZoneClass)i)
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
                            for (int j = 0; j < zoneData[i].Count; ++j)
                            {
                                GameObject detail = Instantiate(ZoneDetailPrefab, newDetails.GetComponent<RectTransform>());
                                RectTransform rectDetail = detail.GetComponent<RectTransform>();
                                rectDetail.anchoredPosition = new Vector2(100 + 200 * j, 75);
                                rectDetail.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                                    $"{zoneData[i][j].length}X{zoneData[i][j].height}";
                                rectDetail.GetChild(2).GetComponent<TextMeshProUGUI>().text =
                                    $"{zoneData[i][j].cost}MB";
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
                            for (int j = 0; j < zoneData[i].Count; ++j)
                            {
                                GameObject detail = Instantiate(ZoneDetailPrefab, newDetails.GetComponent<RectTransform>());
                                RectTransform rectDetail = detail.GetComponent<RectTransform>();
                                rectDetail.anchoredPosition = new Vector2(100 + 200 * j, 75);
                                rectDetail.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                                    $"{zoneData[i][j].length}X{zoneData[i][j].height}";
                                rectDetail.GetChild(2).GetComponent<TextMeshProUGUI>().text =
                                    $"{zoneData[i][j].cost}MB";
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
                            for (int j = 0; j < zoneData[i].Count; ++j)
                            {
                                GameObject detail = Instantiate(ZoneDetailPrefab, newDetails.GetComponent<RectTransform>());
                                RectTransform rectDetail = detail.GetComponent<RectTransform>();
                                rectDetail.anchoredPosition = new Vector2(100 + 200 * j, 75);
                                rectDetail.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                                    $"{zoneData[i][j].length}X{zoneData[i][j].height}";
                                rectDetail.GetChild(2).GetComponent<TextMeshProUGUI>().text =
                                    $"{zoneData[i][j].cost}MB";
                            }
                            break;
                        }
                }
                rect.anchoredPosition = new Vector2(125 + i * 85, 40);
                zones.Add(rect);
            }
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
