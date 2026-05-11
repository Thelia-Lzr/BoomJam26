using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CinematicDialogManager : MonoBehaviour
{
    [System.Serializable]
    private class CharacterPortraitConfig
    {
        public string characterName = "";
        public Sprite sprite = null;
        public float scale = 0.6f;
    }

    [Header("1. Story Library")]
    public List<TextAsset> allDialogFiles = new List<TextAsset>();
    public int testFileIndex = 0;
    public bool useSceneControllerProgress = false;
    public bool continueStoryFlowOnFinish = false;

    [Header("2. Portraits")]
    [SerializeField] private List<CharacterPortraitConfig> characterPortraits = new List<CharacterPortraitConfig>();

    [Header("2. Portraits Legacy")]
    public List<string> charNames = new List<string>();
    public List<Sprite> charSprites = new List<Sprite>();
    public List<float> charScales = new List<float>();
    public Sprite shadowSprite;

    [Header("3. UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;
    public Button nextButton;

    [Header("4. Scene Renderers")]
    public SpriteRenderer imageLeft;
    public SpriteRenderer imageRight;

    [Header("5. Runtime")]
    public int lineId = 0;
    public float typeSpeed = 0.03f;
    public string finishText = "\uFF08\u5267\u60C5\u7ED3\u675F\uFF0C\u70B9\u51FB\u7EE7\u7EED\uFF09";

    [Header("6. Backgrounds")]
    public Image backgroundDisplay;
    public List<Sprite> backgroundSprites = new List<Sprite>();

    [Header("7. Scale Settings")]
    [Range(0.1f, 2.0f)] public float playerScale = 0.6f;
    [Range(0.1f, 2.0f)] public float shadowScale = 0.8f;

    [Header("8. Effects")]
    public Image flashImage;
    public Transform cameraShakeTarget;

    [Header("9. Finish")]
    public FinishAction finishAction = FinishAction.Auto;

    private const int ColTag = 0;
    private const int ColId = 1;
    private const int ColName = 2;
    private const int ColPos = 3;
    private const int ColContent = 4;
    private const int ColNext = 5;
    private const int ColBgIndex = 6;
    private const int ColSoundName = 7;
    private const int ColActorAction = 8;
    private const int ColTargetPos = 9;
    private const int ColTargetAction = 10;
    private const int ColScreenEffect = 11;
    private const int ColBgmName = 12;

    private enum PortraitActionType
    {
        None,
        Shake,
        Hit,
        Punch,
        Impact,
        Jump,
        Enter,
        Exit,
        FadeIn,
        FadeOut,
        AttackLight,
        AttackHeavy,
        DamageHeavy,
        BossEntry
    }

    private enum FlashKind
    {
        None,
        White,
        Red,
        Black
    }

    public enum FinishAction
    {
        Auto,
        Stay,
        LevelSelect,
        Level1Battle,
        Level2Battle,
        Level3Battle,
        Level4Battle,
        Level5Battle,
        MainMenu
    }

    private struct RowCues
    {
        public string SoundName;
        public string BgmName;
        public string ActorAction;
        public string ActorPos;
        public string TargetAction;
        public string TargetPos;
        public string ScreenEffect;
    }

    private struct DialogRow
    {
        public int RowNumber;
        public int Id;
        public string RawRow;
        public string Name;
        public string Pos;
        public string Content;
        public string Next;
        public string BgIndex;
        public RowCues Cues;
    }

    private struct PortraitLookup
    {
        public Sprite Sprite;
        public float Scale;
        public bool HasCustomScale;
    }

    private readonly Dictionary<int, DialogRow> dialogRowsById = new Dictionary<int, DialogRow>();
    private Coroutine typewriterCoroutine;
    private string currentFullContent = "";
    private bool isFinished;
    private bool isTransitioning;
    private Vector3 leftPortraitAnchor;
    private Vector3 rightPortraitAnchor;

    private void Awake()
    {
        if (imageLeft != null) leftPortraitAnchor = imageLeft.transform.localPosition;
        if (imageRight != null) rightPortraitAnchor = imageRight.transform.localPosition;

        SetPortraitDim(imageLeft);
        SetPortraitDim(imageRight);

        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(true);
            flashImage.enabled = true;
            flashImage.raycastTarget = false;
            flashImage.transform.SetAsLastSibling();
            flashImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    private void Start()
    {
        int fileIndex = testFileIndex;

        if (useSceneControllerProgress && SceneController.Instance != null)
        {
            int storyIndex = SceneController.Instance.GetCurrentStoryIndex();
            if (storyIndex >= 0 && storyIndex < allDialogFiles.Count)
            {
                fileIndex = storyIndex;
            }
            else
            {
                Debug.LogWarning(
                    $"[CinematicDialogManager] SceneController story index {storyIndex} is out of range. Falling back to test file index {testFileIndex}.");
            }
        }

        PlayData(fileIndex);

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnClickNext);
        }
    }

    public void PlayData(int fileIndex)
    {
        if (fileIndex < 0 || fileIndex >= allDialogFiles.Count)
        {
            Debug.LogWarning($"[CinematicDialogManager] Dialog file index out of range: {fileIndex}");
            return;
        }

        isFinished = false;
        isTransitioning = false;
        lineId = 0;
        BuildDialogCache(allDialogFiles[fileIndex]);
        ShowDialogRow();
    }

    public void OnClickNext()
    {
        if (isTransitioning) return;

        PlaySound("sfx_ui_click_button");

        if (typewriterCoroutine != null)
        {
            StopTypewriter();
            if (dialogText != null) dialogText.text = currentFullContent;
            return;
        }

        if (isFinished)
        {
            ContinueAfterFinish();
            return;
        }

        ShowDialogRow();
    }

    public void OnClickSkip()
    {
        if (isTransitioning) return;

        PlaySound("sfx_ui_click_button");
        StopTypewriter();
        isFinished = true;
        ContinueAfterFinish();
    }

    public void ShowDialogRow()
    {
        if (!dialogRowsById.TryGetValue(lineId, out DialogRow row))
        {
            Debug.LogWarning($"[CinematicDialogManager] Could not find dialog id {lineId}. Ending dialog.");
            EndDialog();
            return;
        }

        TryChangeBackground(row.BgIndex);
        UpdateUI(row.Name, row.Content, row.Pos);
        PlayRowCues(row.Cues);
        UpdateNextLine(row.Next, row.RawRow, row.RowNumber);
    }

    private void BuildDialogCache(TextAsset dialogFile)
    {
        dialogRowsById.Clear();

        string[] rows = dialogFile.text.Split('\n');
        for (int i = 0; i < rows.Length; i++)
        {
            string rawRow = rows[i].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(rawRow)) continue;

            List<string> cells = ParseCsvLine(rawRow);
            if (cells.Count < 6) continue;
            if (Cell(cells, ColTag).Trim() != "#") continue;

            string rawId = Cell(cells, ColId).Trim();
            if (!int.TryParse(rawId, out int rowId))
            {
                Debug.LogWarning($"[CinematicDialogManager] Invalid dialog id '{rawId}' in {dialogFile.name}, row {i + 1}.");
                continue;
            }

            string pos = Cell(cells, ColPos).Trim();
            DialogRow row = new DialogRow
            {
                RowNumber = i + 1,
                Id = rowId,
                RawRow = rawRow,
                Name = Cell(cells, ColName),
                Pos = pos,
                Content = Cell(cells, ColContent),
                Next = Cell(cells, ColNext).Trim(),
                BgIndex = Cell(cells, ColBgIndex),
                Cues = ReadRowCues(cells, pos)
            };

            if (dialogRowsById.ContainsKey(row.Id))
            {
                Debug.LogWarning($"[CinematicDialogManager] Duplicate dialog id {row.Id} in {dialogFile.name}, row {row.RowNumber}. Keeping the first row.");
                continue;
            }

            dialogRowsById[row.Id] = row;
        }
    }

    private void UpdateUI(string rawName, string content, string pos)
    {
        string pureName = PureName(rawName);
        currentFullContent = content.Trim();

        PortraitLookup portrait = FindPortrait(pureName);
        Sprite targetSprite = portrait.Sprite;

        bool isNarrator = IsNarrator(pureName, pos, targetSprite);
        if (targetSprite == null && !isNarrator) targetSprite = shadowSprite;

        if (nameText != null) nameText.text = rawName.Trim();

        StopTypewriter();
        typewriterCoroutine = StartCoroutine(TypeText(currentFullContent));

        ApplyRender(targetSprite, pos, isNarrator, portrait);
    }

    private void ApplyRender(Sprite sprite, string pos, bool isNarrator, PortraitLookup portrait)
    {
        if (imageLeft == null || imageRight == null) return;

        imageLeft.DOKill();
        imageRight.DOKill();

        if (isNarrator)
        {
            SetPortraitDim(imageLeft);
            SetPortraitDim(imageRight);
            return;
        }

        if (IsLeft(pos))
        {
            if (sprite != null) imageLeft.sprite = sprite;
            SetPortraitActive(imageLeft);
            SetPortraitDim(imageRight);
            imageLeft.transform.localScale = GetScaleForSprite(sprite, portrait);
        }
        else if (IsRight(pos))
        {
            if (sprite != null) imageRight.sprite = sprite;
            SetPortraitActive(imageRight);
            SetPortraitDim(imageLeft);
            imageRight.transform.localScale = GetScaleForSprite(sprite, portrait);
        }
        else
        {
            SetPortraitDim(imageLeft);
            SetPortraitDim(imageRight);
        }
    }

    private RowCues ReadRowCues(List<string> cells, string actorPos)
    {
        return new RowCues
        {
            SoundName = Cell(cells, ColSoundName).Trim(),
            BgmName = Cell(cells, ColBgmName).Trim(),
            ActorAction = Cell(cells, ColActorAction).Trim(),
            ActorPos = actorPos,
            TargetAction = Cell(cells, ColTargetAction).Trim(),
            TargetPos = Cell(cells, ColTargetPos).Trim(),
            ScreenEffect = Cell(cells, ColScreenEffect).Trim()
        };
    }

    private void PlayRowCues(RowCues cues)
    {
        PlaySound(cues.SoundName);
        PlaySound(cues.BgmName);
        PlayPortraitAction(cues.ActorAction, cues.ActorPos);
        PlayPortraitAction(cues.TargetAction, cues.TargetPos);
        PlayScreenEffect(cues.ScreenEffect);
    }

    private void PlayPortraitAction(string action, string pos)
    {
        SpriteRenderer target = GetPortraitByPos(pos);
        if (target == null || string.IsNullOrWhiteSpace(action)) return;

        PortraitActionType actionType = ParsePortraitAction(action);
        if (actionType == PortraitActionType.None) return;

        switch (actionType)
        {
            case PortraitActionType.Shake:
                PlayShake(target);
                break;
            case PortraitActionType.Hit:
                PlayHit(target);
                break;
            case PortraitActionType.Punch:
                PlayPunch(target, pos);
                break;
            case PortraitActionType.Impact:
                PlayImpact(target, pos);
                break;
            case PortraitActionType.Jump:
                PlayJump(target);
                break;
            case PortraitActionType.Enter:
                PlayEnter(target, pos);
                break;
            case PortraitActionType.Exit:
                PlayExit(target, pos);
                break;
            case PortraitActionType.FadeIn:
                PlayFadeIn(target);
                break;
            case PortraitActionType.FadeOut:
                PlayFadeOut(target);
                break;
            case PortraitActionType.AttackLight:
                PlayAttackLight(target, pos);
                break;
            case PortraitActionType.AttackHeavy:
                PlayAttackHeavy(target, pos);
                break;
            case PortraitActionType.DamageHeavy:
                PlayDamageHeavy(target);
                break;
            case PortraitActionType.BossEntry:
                PlayBossEntry(target, pos);
                break;
        }
    }

    private void PlayPunch(SpriteRenderer target, string pos)
    {
        Transform t = target.transform;
        t.DOKill();

        Vector3 origin = GetPortraitAnchor(pos);
        float dir = IsLeft(pos) ? 1f : -1f;
        t.localPosition = origin;

        DOTween.Sequence()
            .Append(t.DOLocalMoveX(origin.x + dir * 1.0f, 0.16f).SetEase(Ease.OutQuad))
            .Append(t.DOLocalMoveX(origin.x, 0.26f).SetEase(Ease.OutBack));
    }

    private void PlayImpact(SpriteRenderer target, string pos)
    {
        Transform t = target.transform;
        t.DOKill();

        Vector3 originPos = GetPortraitAnchor(pos);
        Vector3 originScale = t.localScale;
        float dir = IsLeft(pos) ? 1f : -1f;
        t.localPosition = originPos;

        DOTween.Sequence()
            .Append(t.DOLocalMoveX(originPos.x + dir * 1.4f, 0.18f).SetEase(Ease.OutCubic))
            .Join(t.DOScale(originScale * 1.08f, 0.18f))
            .Append(t.DOLocalMoveX(originPos.x, 0.3f).SetEase(Ease.OutBack))
            .Join(t.DOScale(originScale, 0.3f));
    }

    private void PlayShake(SpriteRenderer target)
    {
        Transform t = target.transform;
        t.DOKill();
        t.DOShakePosition(0.35f, new Vector3(0.16f, 0.06f, 0f), 20, 90f, false, true);
    }

    private void PlayHit(SpriteRenderer target)
    {
        Transform t = target.transform;
        t.DOKill();
        Vector3 originScale = t.localScale;

        DOTween.Sequence()
            .Append(t.DOShakePosition(0.26f, new Vector3(0.22f, 0.08f, 0f), 24, 90f, false, true))
            .Join(t.DOPunchScale(originScale * 0.06f, 0.26f, 8, 0.5f));
    }

    private void PlayDamageHeavy(SpriteRenderer target)
    {
        Transform t = target.transform;
        t.DOKill();
        Vector3 originScale = t.localScale;

        DOTween.Sequence()
            .Append(t.DOShakePosition(0.45f, new Vector3(0.35f, 0.12f, 0f), 28, 90f, false, true))
            .Join(t.DOPunchScale(originScale * 0.1f, 0.35f, 10, 0.6f));
    }

    private void PlayJump(SpriteRenderer target)
    {
        Transform t = target.transform;
        t.DOKill();

        Vector3 origin = GetPortraitAnchorForRenderer(target);
        t.localPosition = origin;
        DOTween.Sequence()
            .Append(t.DOLocalMoveY(origin.y + 0.25f, 0.08f).SetEase(Ease.OutQuad))
            .Append(t.DOLocalMoveY(origin.y, 0.12f).SetEase(Ease.InQuad));
    }

    private void PlayEnter(SpriteRenderer target, string pos)
    {
        Transform t = target.transform;
        t.DOKill();

        Vector3 origin = GetPortraitAnchor(pos);
        float dir = IsLeft(pos) ? -1f : 1f;

        target.color = new Color(target.color.r, target.color.g, target.color.b, 0f);
        t.localPosition = origin + new Vector3(dir * 1.2f, 0f, 0f);

        DOTween.Sequence()
            .Append(t.DOLocalMove(origin, 0.28f).SetEase(Ease.OutCubic))
            .Join(target.DOFade(1f, 0.2f));
    }

    private void PlayExit(SpriteRenderer target, string pos)
    {
        Transform t = target.transform;
        t.DOKill();

        Vector3 origin = GetPortraitAnchor(pos);
        float dir = IsLeft(pos) ? -1f : 1f;
        t.localPosition = origin;

        DOTween.Sequence()
            .Append(t.DOLocalMove(origin + new Vector3(dir * 1.2f, 0f, 0f), 0.25f).SetEase(Ease.InCubic))
            .Join(target.DOFade(0f, 0.2f));
    }

    private void PlayFadeIn(SpriteRenderer target)
    {
        target.DOKill();
        target.color = new Color(target.color.r, target.color.g, target.color.b, 0f);
        target.DOFade(1f, 0.25f);
    }

    private void PlayFadeOut(SpriteRenderer target)
    {
        target.DOKill();
        target.DOFade(0f, 0.25f);
    }

    private void PlayAttackLight(SpriteRenderer target, string pos)
    {
        PlayPunch(target, pos);
    }

    private void PlayAttackHeavy(SpriteRenderer target, string pos)
    {
        PlayImpact(target, pos);
        PlayWhiteFlash(Color.white, 0.9f);
        PlayCameraShake(0.3f, new Vector3(0.18f, 0.1f, 0f));
    }

    private void PlayBossEntry(SpriteRenderer target, string pos)
    {
        Transform t = target.transform;
        Vector3 originScale = t.localScale;

        PlayEnter(target, pos);
        DOTween.Sequence()
            .AppendInterval(0.05f)
            .Append(t.DOScale(originScale * 1.12f, 0.2f).SetEase(Ease.OutBack))
            .Append(t.DOScale(originScale, 0.16f).SetEase(Ease.OutQuad));
        PlayScreenEffect("blackflashshake");
    }

    private void PlayScreenEffect(string effect)
    {
        if (string.IsNullOrWhiteSpace(effect)) return;

        string key = NormalizeKey(effect);
        if (IsNoneKey(key)) return;

        switch (ParseFlashKind(key))
        {
            case FlashKind.Red:
                PlayWhiteFlash(new Color(1f, 0.08f, 0.04f), 0.75f);
                break;
            case FlashKind.Black:
                PlayWhiteFlash(Color.black, 0.65f);
                break;
            case FlashKind.White:
                PlayWhiteFlash(Color.white, 0.85f);
                break;
        }

        if (HasScreenShake(key)) PlayCameraShake(0.25f, new Vector3(0.12f, 0.08f, 0f));
    }

    private void PlayWhiteFlash()
    {
        PlayWhiteFlash(Color.white, 0.85f);
    }

    private void PlayWhiteFlash(Color color, float alpha)
    {
        if (flashImage == null)
        {
            Debug.LogWarning("[CinematicDialogManager] Flash effect requested, but Flash Image is not assigned.");
            return;
        }

        flashImage.gameObject.SetActive(true);
        flashImage.enabled = true;
        flashImage.raycastTarget = false;
        flashImage.transform.SetAsLastSibling();
        flashImage.DOKill();
        flashImage.color = new Color(color.r, color.g, color.b, 0f);

        DOTween.Sequence()
            .Append(flashImage.DOFade(alpha, 0.05f))
            .Append(flashImage.DOFade(0f, 0.22f));
    }

    private void PlayCameraShake(float duration, Vector3 strength)
    {
        Transform target = cameraShakeTarget;
        if (target == null && Camera.main != null) target = Camera.main.transform;
        if (target == null) return;

        target.DOKill();
        target.DOShakePosition(duration, strength, 18, 90f, false, true);
    }

    private PortraitActionType ParsePortraitAction(string action)
    {
        string key = NormalizeKey(action);
        if (IsNoneKey(key)) return PortraitActionType.None;

        if (Matches(key, "shake", "\u6296\u52A8")) return PortraitActionType.Shake;
        if (Matches(key, "hit", "\u53D7\u51FB")) return PortraitActionType.Hit;
        if (Matches(key, "punch", "\u7A81\u523A")) return PortraitActionType.Punch;
        if (Matches(key, "impact", "\u649E\u51FB")) return PortraitActionType.Impact;
        if (Matches(key, "jump", "\u8DF3\u52A8")) return PortraitActionType.Jump;
        if (Matches(key, "enter", "\u5165\u573A")) return PortraitActionType.Enter;
        if (Matches(key, "exit", "\u9000\u573A")) return PortraitActionType.Exit;
        if (Matches(key, "fadein", "\u6DE1\u5165")) return PortraitActionType.FadeIn;
        if (Matches(key, "fadeout", "\u6DE1\u51FA")) return PortraitActionType.FadeOut;
        if (Matches(key, "attack_light", "\u8F7B\u653B\u51FB")) return PortraitActionType.AttackLight;
        if (Matches(key, "attack_heavy", "\u91CD\u653B\u51FB")) return PortraitActionType.AttackHeavy;
        if (Matches(key, "damage_heavy", "\u91CD\u4F24")) return PortraitActionType.DamageHeavy;
        if (Matches(key, "boss_entry", "\u5F3A\u654C\u767B\u573A")) return PortraitActionType.BossEntry;

        Debug.LogWarning($"[CinematicDialogManager] Unknown portrait action: {action}");
        return PortraitActionType.None;
    }

    private FlashKind ParseFlashKind(string key)
    {
        if (ContainsAny(key, "redflash", "\u7EA2\u95EA")) return FlashKind.Red;
        if (ContainsAny(key, "blackflash", "\u9ED1\u95EA")) return FlashKind.Black;
        if (ContainsAny(key, "flash", "\u767D\u95EA")) return FlashKind.White;
        return FlashKind.None;
    }

    private bool HasScreenShake(string key)
    {
        return ContainsAny(key, "shake", "\u9707\u52A8");
    }

    private void TryChangeBackground(string value)
    {
        if (backgroundDisplay == null || string.IsNullOrWhiteSpace(value)) return;
        if (!int.TryParse(value.Trim(), out int index)) return;
        if (index < 0 || index >= backgroundSprites.Count) return;
        if (backgroundDisplay.sprite == backgroundSprites[index]) return;

        backgroundDisplay.DOKill();
        backgroundDisplay.DOColor(Color.black, 0.2f).OnComplete(() =>
        {
            backgroundDisplay.sprite = backgroundSprites[index];
            backgroundDisplay.DOColor(Color.white, 0.35f);
        });
    }

    private void UpdateNextLine(string next, string row, int rowNumber)
    {
        if (next.ToLowerInvariant() == "end")
        {
            isFinished = true;
            return;
        }

        if (int.TryParse(next, out int nextLineId))
        {
            lineId = nextLineId;
            return;
        }

        Debug.LogWarning($"[CinematicDialogManager] Invalid next id '{next}' in row {rowNumber}: {row}");
        EndDialog();
    }

    private void ContinueAfterFinish()
    {
        if (isTransitioning) return;

        if (!continueStoryFlowOnFinish)
        {
            EndDialog();
            return;
        }

        isTransitioning = true;
        if (SoundManager.SoundManager.Instance != null)
            SoundManager.SoundManager.Instance.StopAll();

        if (finishAction == FinishAction.Auto && useSceneControllerProgress && SceneController.Instance != null)
        {
            SceneController.Instance.FinishCurrentStory();
            return;
        }

        if (TryRunFinishAction()) return;

        Debug.LogWarning("[CinematicDialogManager] No finish action was run. Use Auto during normal flow, or choose a Finish Action for standalone testing.");
        EndDialog();
    }

    private bool TryRunFinishAction()
    {
        if (finishAction == FinishAction.Stay) return false;

        if (SceneController.Instance == null)
        {
            Debug.LogWarning("[CinematicDialogManager] Finish Action needs a SceneController in the scene.");
            return false;
        }

        switch (finishAction)
        {
            case FinishAction.LevelSelect:
                SceneController.Instance.GoToLevelSelect();
                return true;
            case FinishAction.Level1Battle:
                SceneController.Instance.GoToLevel1Battle();
                return true;
            case FinishAction.Level2Battle:
                SceneController.Instance.GoToLevel2Battle();
                return true;
            case FinishAction.Level3Battle:
                SceneController.Instance.GoToLevel3Battle();
                return true;
            case FinishAction.Level4Battle:
                SceneController.Instance.GoToLevel4Battle();
                return true;
            case FinishAction.Level5Battle:
                SceneController.Instance.GoToLevel5Battle();
                return true;
            case FinishAction.MainMenu:
                SceneController.Instance.BackToMainMenu();
                return true;
            case FinishAction.Auto:
            case FinishAction.Stay:
            default:
                return false;
        }
    }

    public void EndDialog()
    {
        isFinished = true;
        isTransitioning = false;
        StopTypewriter();
        if (dialogText != null) dialogText.text = finishText;
    }

    private void StopTypewriter()
    {
        if (typewriterCoroutine == null) return;

        StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = null;
    }

    private IEnumerator TypeText(string text)
    {
        if (dialogText == null) yield break;

        dialogText.text = "";
        StringBuilder builder = new StringBuilder(text.Length);
        foreach (char c in text)
        {
            builder.Append(c);
            dialogText.text = builder.ToString();
            yield return new WaitForSeconds(typeSpeed);
        }

        typewriterCoroutine = null;
    }

    private PortraitLookup FindPortrait(string pureName)
    {
        for (int i = 0; i < characterPortraits.Count; i++)
        {
            CharacterPortraitConfig config = characterPortraits[i];
            if (config == null) continue;

            string configName = PureName(config.characterName);
            if (!IsPortraitNameExactMatch(pureName, configName)) continue;

            return new PortraitLookup
            {
                Sprite = config.sprite,
                Scale = config.scale,
                HasCustomScale = config.scale > 0f
            };
        }

        for (int i = 0; i < characterPortraits.Count; i++)
        {
            CharacterPortraitConfig config = characterPortraits[i];
            if (config == null) continue;

            string configName = PureName(config.characterName);
            if (!IsPortraitNameMatch(pureName, configName)) continue;

            return new PortraitLookup
            {
                Sprite = config.sprite,
                Scale = config.scale,
                HasCustomScale = config.scale > 0f
            };
        }

        int legacyIndex = FindLegacyPortraitIndex(pureName);
        return new PortraitLookup
        {
            Sprite = GetLegacyPortraitSprite(legacyIndex),
            Scale = GetLegacyPortraitScale(legacyIndex),
            HasCustomScale = legacyIndex >= 0 && legacyIndex < charScales.Count && charScales[legacyIndex] > 0f
        };
    }

    private int FindLegacyPortraitIndex(string pureName)
    {
        for (int i = 0; i < charNames.Count; i++)
        {
            string configName = PureName(charNames[i]);
            if (!IsPortraitNameExactMatch(pureName, configName)) continue;

            if (i < charSprites.Count) return i;
        }

        for (int i = 0; i < charNames.Count; i++)
        {
            string configName = PureName(charNames[i]);
            if (!IsPortraitNameMatch(pureName, configName)) continue;

            if (i < charSprites.Count) return i;
        }

        return -1;
    }

    private bool IsPortraitNameMatch(string pureName, string configName)
    {
        if (string.IsNullOrEmpty(pureName) || string.IsNullOrEmpty(configName)) return false;
        return pureName.Contains(configName) || configName.Contains(pureName);
    }

    private bool IsPortraitNameExactMatch(string pureName, string configName)
    {
        if (string.IsNullOrEmpty(pureName) || string.IsNullOrEmpty(configName)) return false;
        return pureName == configName;
    }

    private Sprite GetLegacyPortraitSprite(int charIndex)
    {
        if (charIndex >= 0 && charIndex < charSprites.Count) return charSprites[charIndex];
        return null;
    }

    private bool IsNarrator(string pureName, string pos, Sprite sprite)
    {
        return (IsCenter(pos) && sprite == null)
               || pureName.Contains("\u65C1\u767D")
               || pureName.Contains("\u4F20\u9882\u8005");
    }

    private float GetLegacyPortraitScale(int charIndex)
    {
        if (charIndex >= 0 && charIndex < charScales.Count && charScales[charIndex] > 0f)
        {
            return charScales[charIndex];
        }

        return playerScale;
    }

    private Vector3 GetScaleForSprite(Sprite sprite, PortraitLookup portrait)
    {
        bool isShadow = sprite != null && shadowSprite != null && sprite.name == shadowSprite.name;
        float scale = isShadow ? shadowScale : playerScale;

        if (!isShadow && portrait.HasCustomScale)
        {
            scale = portrait.Scale;
        }

        return new Vector3(scale, scale, 1f);
    }

    private SpriteRenderer GetPortraitByPos(string pos)
    {
        if (IsLeft(pos)) return imageLeft;
        if (IsRight(pos)) return imageRight;
        return null;
    }

    private Vector3 GetPortraitAnchor(string pos)
    {
        if (IsLeft(pos)) return leftPortraitAnchor;
        if (IsRight(pos)) return rightPortraitAnchor;
        return Vector3.zero;
    }

    private Vector3 GetPortraitAnchorForRenderer(SpriteRenderer target)
    {
        if (target == imageLeft) return leftPortraitAnchor;
        if (target == imageRight) return rightPortraitAnchor;
        return target != null ? target.transform.localPosition : Vector3.zero;
    }

    private void SetPortraitActive(SpriteRenderer renderer)
    {
        if (renderer != null) renderer.DOColor(Color.white, 0.16f);
    }

    private void SetPortraitDim(SpriteRenderer renderer)
    {
        if (renderer == null) return;

        float alpha = renderer.color.a;
        renderer.DOColor(new Color(0.3f, 0.3f, 0.3f, alpha), 0.16f);
    }

    private void PlaySound(string soundName)
    {
        if (string.IsNullOrWhiteSpace(soundName)) return;
        if (SoundManager.SoundManager.Instance != null)
            SoundManager.SoundManager.Instance.Play(soundName.Trim());
    }

    private string Cell(List<string> cells, int index)
    {
        return index >= 0 && index < cells.Count ? cells[index] : "";
    }

    private string NormalizeKey(string raw)
    {
        return string.IsNullOrWhiteSpace(raw) ? "" : raw.Trim().ToLowerInvariant();
    }

    private bool IsNoneKey(string key)
    {
        return string.IsNullOrEmpty(key) || key == "none" || key == "-" || key == "\u65E0";
    }

    private bool Matches(string key, params string[] aliases)
    {
        for (int i = 0; i < aliases.Length; i++)
        {
            if (key == aliases[i]) return true;
        }

        return false;
    }

    private bool ContainsAny(string key, params string[] aliases)
    {
        for (int i = 0; i < aliases.Length; i++)
        {
            if (key.Contains(aliases[i])) return true;
        }

        return false;
    }

    private List<string> ParseCsvLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string current = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                bool escapedQuote = inQuotes && i + 1 < line.Length && line[i + 1] == '"';
                if (escapedQuote)
                {
                    current += '"';
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current);
        return result;
    }

    private string PureName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        return Regex.Replace(raw, @"[^\u4e00-\u9fa5a-zA-Z0-9]", "").Trim();
    }

    private bool IsLeft(string pos)
    {
        string value = pos.Trim().ToLowerInvariant();
        return value == "\u5DE6" || value == "left" || value == "l";
    }

    private bool IsRight(string pos)
    {
        string value = pos.Trim().ToLowerInvariant();
        return value == "\u53F3" || value == "right" || value == "r";
    }

    private bool IsCenter(string pos)
    {
        string value = pos.Trim().ToLowerInvariant();
        return value == "\u4E2D" || value == "center" || value == "c";
    }
}
