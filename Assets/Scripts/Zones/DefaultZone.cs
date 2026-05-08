using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DefaultZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Vector2 scale = new Vector2(1, 1);
    public int memoryUsed = 0;
    public bool placed = false;

    //--��ק����
    public GameObject spritePreviewPrefab;
    protected GameObject currentGuideSprite;

    [Header("״̬")]
    protected Camera mainCamera;
    //��Ļ��Ч��Χ
    private const float BottomBanHeight = 150f;
    protected void Start()
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
            curpos.x = Mathf.Round(position.x - 0.5f) + 0.5f;
        }
        if (scale.y % 2 == 0)
        {
            curpos.y = Mathf.Round(position.y);
        }
        else
        {
            curpos.y = Mathf.Round(position.y - 0.5f) + 0.5f;
        }
        transform.position = curpos;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        currentGuideSprite = Instantiate(spritePreviewPrefab);
        currentGuideSprite.transform.localScale = transform.localScale;
        currentGuideSprite.transform.position = ScreenToWorldPos(eventData.position);
    }
    // �ɿ�ʱ��������ק
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
    // ��ק�У�����λ��
    public void OnDrag(PointerEventData eventData)
    {
        if (currentGuideSprite == null) return;
        Vector3 worldPos = ScreenToWorldPos(eventData.position);
        currentGuideSprite.transform.position = worldPos;
        ZonePosition(worldPos);
    }
    //ͨ�÷���
    private Vector3 ScreenToWorldPos(Vector2 screenPos)
    {
        Vector3 screenPosWithZ = new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane + 1f);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPosWithZ);
        return worldPos;
    }
}
