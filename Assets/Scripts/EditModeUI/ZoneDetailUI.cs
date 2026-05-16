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
        if (LevelManager.Instance.currentMode == LevelManager.CurrentMode.PlayMode) return;
        if (spritePreviewPrefab == null) return;
        currentGuideSprite = Instantiate(spritePreviewPrefab);
        ApplyZoneSize(currentGuideSprite, zoneScale);
        currentGuideSprite.transform.position = ScreenToWorldPos(eventData.position);
        //分类脚本
        switch (zoneClass)
        {
            case ZoneClass.AntiGravity:
                spritePrefab = AntiGravityZonePrefab;
                //spriteScript = AntiGravityZonePrefab.GetComponent<GravityZone>();
                break;
            case ZoneClass.Speeding:
                spritePrefab = SpeedingZonePrefab;
                //spriteScript = SpeedingZonePrefab.GetComponent<SpeedingZone>();
                break;
            case ZoneClass.Swap:
                spritePrefab = SwapZonePrefab;
                //spriteScript = SwapZonePrefab.GetComponent<SwapZone>();
                break;
        }
        currentSprite = Instantiate(spritePrefab);
        ApplyZoneSize(currentSprite, zoneScale);
        currentSprite.transform.position = ScreenToWorldPos(eventData.position);
        spriteScript = currentSprite.GetComponent<DefaultZone>();
        spriteScript.memoryUsed = memoryUsed;
        spriteScript.scale = zoneScale;
        MemoryUsedUI.Instance.ChangeMemoryUsed(memoryUsed);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (LevelManager.Instance.currentMode == LevelManager.CurrentMode.PlayMode) return;
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
        if (LevelManager.Instance.currentMode == LevelManager.CurrentMode.PlayMode) return;
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

    private void ApplyZoneSize(GameObject target, Vector2 size)
    {
        if (target == null)
        {
            return;
        }

        var spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            target.transform.localScale = size;
            return;
        }

        if (spriteRenderer.sprite != null && spriteRenderer.sprite.packingMode == SpritePackingMode.Tight)
        {
            spriteRenderer.sprite = CreateFullRectSprite(spriteRenderer.sprite);
        }

        var boxCollider2D = target.GetComponent<BoxCollider2D>();
        var boxCollider = target.GetComponent<BoxCollider>();
        var targetSize = size;

        if (boxCollider2D != null)
        {
            targetSize = boxCollider2D.size;
        }
        else if (boxCollider != null)
        {
            targetSize = new Vector2(boxCollider.size.x, boxCollider.size.y);
        }

        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        spriteRenderer.size = targetSize;
        target.transform.localScale = Vector3.one;
    }

    private static Sprite CreateFullRectSprite(Sprite source)
    {
        if (source == null)
        {
            return null;
        }

        var rect = source.rect;
        if (rect.width <= 0f || rect.height <= 0f)
        {
            return source;
        }

        var pivot = new Vector2(source.pivot.x / rect.width, source.pivot.y / rect.height);
        return Sprite.Create(source.texture, rect, pivot, source.pixelsPerUnit, 0, SpriteMeshType.FullRect, source.border);
    }
}
