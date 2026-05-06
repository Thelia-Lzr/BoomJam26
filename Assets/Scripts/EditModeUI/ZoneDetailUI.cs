using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoneDetailUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("设置")]
    public GameObject spritePrefab;
    public float longPressTime = 0.5f;
    public float spriteZOffset = 0f;
    [Header("状态")]
    public bool isDraggingSprite;
    private GameObject currentSprite;
    private Coroutine longPressCoroutine;
    private Camera mainCamera;
    void Start()
    {
        mainCamera = Camera.main;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (spritePrefab == null) return;
        currentSprite = Instantiate(spritePrefab);
        currentSprite.transform.position = ScreenToWorldPos(eventData.position);
        isDraggingSprite = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
        }
        if (currentSprite != null)
        {
            currentSprite = null;
        }
        isDraggingSprite = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggingSprite || currentSprite == null) return;
        Vector3 worldPos = ScreenToWorldPos(eventData.position);
        currentSprite.transform.position = worldPos;
    }
    //通用方法
    private Vector3 ScreenToWorldPos(Vector2 screenPos)
    {
        Vector3 screenPosWithZ = new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane + 1f);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPosWithZ);
        worldPos.z = spriteZOffset;
        return worldPos;
    }
}
