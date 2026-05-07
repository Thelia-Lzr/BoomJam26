using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DefaultZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Vector2 scale = new Vector2(1, 1);
    public int memoryUsed = 0;
    public bool placed = false;

    //--拖拽设置
    public GameObject spritePreviewPrefab;
    private GameObject currentGuideSprite;

    [Header("状态")]
    private Camera mainCamera;
    //屏幕有效范围
    private const float BottomBanHeight = 150f;
    void Start()
    {
        mainCamera = Camera.main;
    }
    public void ZonePosition(Vector3 position)
    {
        Vector3 curpos = Vector3.zero;
        if(scale.x % 2 == 0)
        {
            curpos.x = Mathf.Round(position.x);
        }
        else
        {
            curpos.x = Mathf.Round(position.x) + 0.5f;
        }
        if (scale.y % 2 == 0)
        {
            curpos.y = Mathf.Round(position.y);
        }
        else
        {
            curpos.y = Mathf.Round(position.y) + 0.5f;
        }
        transform.position = curpos;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        currentGuideSprite = Instantiate(spritePreviewPrefab);
        currentGuideSprite.transform.localScale = transform.localScale;
        currentGuideSprite.transform.position = ScreenToWorldPos(eventData.position);
    }
    // 松开时：结束拖拽
    public void OnPointerUp(PointerEventData eventData)
    {
        if (currentGuideSprite != null)
        {
            Destroy(currentGuideSprite);
            currentGuideSprite = null;
            if (eventData.position.y <= BottomBanHeight)
            {
                Destroy(gameObject);
                MemoryUsedUI.Instance.ChangeMemoryUsed(-1 * memoryUsed);
            }
        }
    }
    // 拖拽中：更新位置
    public void OnDrag(PointerEventData eventData)
    {
        if (currentGuideSprite == null) return;
        Vector3 worldPos = ScreenToWorldPos(eventData.position);
        currentGuideSprite.transform.position = worldPos;
        ZonePosition(worldPos);
    }
    //通用方法
    private Vector3 ScreenToWorldPos(Vector2 screenPos)
    {
        Vector3 screenPosWithZ = new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane + 1f);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPosWithZ);
        return worldPos;
    }
}
