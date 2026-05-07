using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoneDetailUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("设置")]
    public GameObject spritePreviewPrefab;

    public ZoneClass zoneClass;
    public Vector2 zoneScale;
    public int memoryUsed;
    [Header("区域预制体")]
    public GameObject spritePrefab;
    public GameObject AntiGravityZonePrefab;
    public GameObject SpeedingZonePrefab;
    public GameObject SwapZonePrefab;
    [Header("状态")]
    private GameObject currentGuideSprite;
    private GameObject currentSprite;
    private DefaultZone spriteScript;
    private Camera mainCamera;
    //屏幕有效范围
    private const float BottomBanHeight = 150f;
    void Start()
    {
        mainCamera = Camera.main;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (spritePreviewPrefab == null) return;
        currentGuideSprite = Instantiate(spritePreviewPrefab);
        currentGuideSprite.transform.localScale = zoneScale;
        currentGuideSprite.transform.position = ScreenToWorldPos(eventData.position);
        //标记，之后要分类
        currentSprite = Instantiate(spritePrefab);
        currentSprite.transform.localScale = zoneScale;
        currentSprite.transform.position = ScreenToWorldPos(eventData.position);
        spriteScript = currentSprite.GetComponent<DefaultZone>();
        spriteScript.memoryUsed = memoryUsed;
        spriteScript.scale = zoneScale;
        MemoryUsedUI.Instance.ChangeMemoryUsed(memoryUsed);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (currentGuideSprite != null)
        {
            Destroy(currentGuideSprite);
            currentGuideSprite = null;
            if (eventData.position.y <= BottomBanHeight)
            {
                Destroy(currentSprite);
                MemoryUsedUI.Instance.ChangeMemoryUsed(-1 * memoryUsed);
            }
            currentSprite = null;
            spriteScript = null;
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (currentGuideSprite == null) return;
        Vector3 worldPos = ScreenToWorldPos(eventData.position);
        currentGuideSprite.transform.position = worldPos;
        spriteScript.ZonePosition(worldPos);
    }
    //通用方法
    private Vector3 ScreenToWorldPos(Vector2 screenPos)
    {
        Vector3 screenPosWithZ = new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane + 1f);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPosWithZ);
        worldPos.z = 0;
        return worldPos;
    }
}
