using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoneDetailUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private string dragSfxName = "ButtonClick";
    [SerializeField] private string deleteSfxName = "Delete";

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
    private bool dragSoundPlayedThisDrag;
    private const float BottomBanHeight = 150f;

    void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!InGameDialogue.IsDialogActive) return;
        if (currentGuideSprite == null && currentSprite == null) return;

        CancelDrag();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (InGameDialogue.IsDialogActive) return;
        if (LevelManager.Instance.currentMode == LevelManager.CurrentMode.PlayMode) return;
        if (spritePreviewPrefab == null) return;
        dragSoundPlayedThisDrag = false;
        currentGuideSprite = Instantiate(spritePreviewPrefab);
        currentGuideSprite.transform.localScale = zoneScale;
        currentGuideSprite.transform.position = ScreenToWorldPos(eventData.position);
        switch (zoneClass)
        {
            case ZoneClass.AntiGravity:
                spritePrefab = AntiGravityZonePrefab;
                break;
            case ZoneClass.Speeding:
                spritePrefab = SpeedingZonePrefab;
                break;
            case ZoneClass.Swap:
                spritePrefab = SwapZonePrefab;
                break;
        }
        currentSprite = Instantiate(spritePrefab);
        Debug.Log("inited");
        currentGuideSprite.transform.localScale = zoneScale;
        currentSprite.transform.position = ScreenToWorldPos(eventData.position);
        spriteScript = currentSprite.GetComponent<DefaultZone>();
        spriteScript.memoryUsed = memoryUsed;
        spriteScript.scale = zoneScale;
        MemoryUsedUI.Instance.ChangeMemoryUsed(memoryUsed);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (InGameDialogue.IsDialogActive)
        {
            CancelDrag();
            return;
        }
        if (LevelManager.Instance.currentMode == LevelManager.CurrentMode.PlayMode) return;
        dragSoundPlayedThisDrag = false;
        if (currentGuideSprite != null)
        {
            Destroy(currentGuideSprite);
            currentGuideSprite = null;
            if (eventData.position.y <= BottomBanHeight)
            {
                PlaySfx(deleteSfxName);
                Destroy(currentSprite);
                MemoryUsedUI.Instance.ChangeMemoryUsed(-1 * memoryUsed);
            }
            currentSprite = null;
            spriteScript = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (InGameDialogue.IsDialogActive)
        {
            CancelDrag();
            return;
        }
        if (LevelManager.Instance.currentMode == LevelManager.CurrentMode.PlayMode) return;
        if (currentGuideSprite == null) return;
        if (!dragSoundPlayedThisDrag)
        {
            PlaySfx(dragSfxName);
            dragSoundPlayedThisDrag = true;
        }
        Vector3 worldPos = ScreenToWorldPos(eventData.position);
        currentGuideSprite.transform.position = worldPos;
        spriteScript.ZonePosition(worldPos);
    }

    private Vector3 ScreenToWorldPos(Vector2 screenPos)
    {
        Vector3 screenPosWithZ = new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane + 1f);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPosWithZ);
        worldPos.z = 0;
        return worldPos;
    }

    private static void PlaySfx(string soundName)
    {
        if (string.IsNullOrWhiteSpace(soundName)) return;
        if (SoundManager.SoundManager.Instance == null) return;

        SoundManager.SoundManager.Instance.Play(soundName.Trim());
    }

    private void CancelDrag()
    {
        if (currentGuideSprite != null)
        {
            Destroy(currentGuideSprite);
            currentGuideSprite = null;
        }

        if (currentSprite != null)
        {
            Destroy(currentSprite);
            currentSprite = null;
            MemoryUsedUI.Instance.ChangeMemoryUsed(-1 * memoryUsed);
        }

        spriteScript = null;
        dragSoundPlayedThisDrag = false;
    }
}
