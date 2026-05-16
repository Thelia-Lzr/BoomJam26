using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static LevelManager;
//using static UnityEditor.Progress;

/// <summary>
/// ?可以和playerUImanager合并成一个UIManager(?)
/// </summary>
public class UIManager : MonoBehaviour
{

    //输出单例
    public static UIManager Instance;
    //UI数据
    private const float ITEM_SPACING = 85f;
    private const float ITEM_START_X = 125f;
    private const float ITEM_Y = 13f;
    private const float DETAIL_ITEM_START_X = 60f;
    private const float DETAIL_ITEM_SPACING = 130f;
    private const float DETAIL_ITEM_Y = 70f;
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
    /// <summary>
    /// 区域预制体
    /// </summary>
    [field: SerializeField]
    public GameObject ZoneDetailPrefab { get; private set; }
    //--设置--
    public float longPressTime = 0.5f;    // 长按判定时间（秒）
    public float spriteZOffset = 0f;      // Sprite在世界坐标中的Z轴位置（2D游戏设为0即可）
                                          //image
    [field: SerializeField]
    public Sprite AntiGravityZoneSprite { get; private set; }
    [field: SerializeField]
    public Sprite SpeedingZoneSprite { get; private set; }
    [field: SerializeField]
    public Sprite SwapZoneSprite { get; private set; }
    [field: SerializeField]
    public Sprite AntiGravityZoneDetailSprite { get; private set; }
    [field: SerializeField]
    public Sprite SpeedingZoneDetailSprite { get; private set; }
    [field: SerializeField]
    public Sprite SwapZoneDetailSprite { get; private set; }

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
        ZoneContent.sizeDelta = new Vector2(85 * (bordernum + 1), 0);

        for (int i = 0; i < enumCount; i++)
        {
            if (zoneData[i].Count > 0)
            {
                GameObject zone = Instantiate(ZonePrefab, ZoneContent);
                RectTransform rect = zone.GetComponent<RectTransform>();
                GameObject newDetails = null;
                switch ((ZoneClass)i)
                {
                    case ZoneClass.Swap:
                        {
                            rect.GetChild(0).GetComponent<Image>().sprite = SwapZoneSprite;
                            rect.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Swap";
                            newDetails = new GameObject("SwapZoneDetail", typeof(RectTransform));
                            break;
                        }
                    case ZoneClass.AntiGravity:
                        {
                            rect.GetChild(0).GetComponent<Image>().sprite = AntiGravityZoneSprite;
                            rect.GetChild(1).GetComponent<TextMeshProUGUI>().text = "AntiGravity";
                            newDetails = new GameObject("AntiGravityZoneDetail", typeof(RectTransform));
                            break;
                        }
                    case ZoneClass.Speeding:
                        {
                            rect.GetChild(0).GetComponent<Image>().sprite = SpeedingZoneSprite;
                            rect.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Speeding";
                            newDetails = new GameObject("SpeedingZoneDetail", typeof(RectTransform));
                            break;
                        }
                }
                newDetails.SetActive(false);
                RectTransform detailRect = newDetails.GetComponent<RectTransform>();
                detailRect.SetParent(ZoneDetailContent, false);
                detailRect.sizeDelta = Vector2.zero;
                zoneDetails.Add(detailRect);
                for (int j = 0; j < zoneData[i].Count; ++j)
                {
                    GameObject detail = Instantiate(ZoneDetailPrefab, newDetails.GetComponent<RectTransform>());
                    ZoneDetailUI detailUI = detail.GetComponent<ZoneDetailUI>();
                    detailUI.zoneClass = (ZoneClass)i;
                    detailUI.zoneScale = new Vector3(zoneData[i][j].length, zoneData[i][j].height, 1f);
                    detailUI.memoryUsed = zoneData[i][j].cost;
                    switch ((ZoneClass)i)
                    {
                        case ZoneClass.Swap:
                            {
                                detail.GetComponent<Image>().sprite = SwapZoneDetailSprite;
                                break;
                            }
                        case ZoneClass.AntiGravity:
                            {
                                detail.GetComponent<Image>().sprite = AntiGravityZoneDetailSprite;
                                break;
                            }
                        case ZoneClass.Speeding:
                            {
                                detail.GetComponent<Image>().sprite = SpeedingZoneDetailSprite;
                                break;
                            }
                    }
                    RectTransform rectDetail = detail.GetComponent<RectTransform>();
                    rectDetail.anchoredPosition =
                        new Vector2(DETAIL_ITEM_START_X + DETAIL_ITEM_SPACING * j, DETAIL_ITEM_Y);
                    rectDetail.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                        $"{zoneData[i][j].length}X{zoneData[i][j].height}";
                    rectDetail.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                        $"{zoneData[i][j].cost}MB";
                }
                rect.anchoredPosition = new Vector2(ITEM_START_X + i * ITEM_SPACING, ITEM_Y);
                zones.Add(rect);
            }
        }
        ZoneIndex = 0;
        zones[ZoneIndex].localScale = new Vector3(1.1f, 1.1f, 1.1f);
        zoneDetails[ZoneIndex].gameObject.SetActive(true);
    } //生成所有选项

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
