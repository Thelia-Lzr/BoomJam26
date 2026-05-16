using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DefaultZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private string dragSfxName = "ButtonClick";
    [SerializeField] private string deleteSfxName = "Delete";

    public Vector2 scale = new Vector2(1, 1);
    public int memoryUsed = 0;
    public bool placed = false;

    public GameObject spritePreviewPrefab;
    protected GameObject currentGuideSprite;

    [Header("状态")]
    protected Camera mainCamera;
    private bool dragSoundPlayedThisDrag;
    private const float BottomBanHeight = 150f;

    protected void Start()
    {
        mainCamera = Camera.main;
    }

    public void ZonePosition(Vector3 position)
    {
        Vector3 curpos = Vector3.zero;
        if (scale.x % 2 == 0)
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
        Debug.Log("OnPointerDown");
        if (LevelManager.Instance.currentMode == LevelManager.CurrentMode.PlayMode) return;
        currentGuideSprite = Instantiate(spritePreviewPrefab);
        currentGuideSprite.transform.localScale = transform.localScale;
        currentGuideSprite.transform.position = ScreenToWorldPos(eventData.position);
        dragSoundPlayedThisDrag = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (LevelManager.Instance.currentMode == LevelManager.CurrentMode.PlayMode) return;
        dragSoundPlayedThisDrag = false;
        if (currentGuideSprite != null)
        {
            Destroy(currentGuideSprite);
            currentGuideSprite = null;
            if (eventData.position.y <= BottomBanHeight)
            {
                PlaySfx(deleteSfxName);
                Destroy(gameObject);
                MemoryUsedUI.Instance.ChangeMemoryUsed(-1 * memoryUsed);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (LevelManager.Instance.currentMode == LevelManager.CurrentMode.PlayMode) return;
        if (currentGuideSprite == null) return;
        if (!dragSoundPlayedThisDrag)
        {
            PlaySfx(dragSfxName);
            dragSoundPlayedThisDrag = true;
        }
        Vector3 worldPos = ScreenToWorldPos(eventData.position);
        currentGuideSprite.transform.position = worldPos;
        ZonePosition(worldPos);
    }

    private Vector3 ScreenToWorldPos(Vector2 screenPos)
    {
        Vector3 screenPosWithZ = new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane + 1f);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPosWithZ);
        return worldPos;
    }

    private static void PlaySfx(string soundName)
    {
        if (string.IsNullOrWhiteSpace(soundName)) return;
        if (SoundManager.SoundManager.Instance == null) return;

        SoundManager.SoundManager.Instance.Play(soundName.Trim());
    }
}
