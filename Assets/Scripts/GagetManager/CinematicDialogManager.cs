using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 基于原 Dialogmanager 的“演出版”对话管理器。
// 原脚本主要负责 CSV 对话、立绘明暗、背景和场景跳转；
// 这个版本额外支持：CSV 驱动音效、立绘动作、目标受击、白/红/黑闪、镜头震动、角色单独缩放。
public class CinematicDialogManager : MonoBehaviour
{
    [Header("1. 剧情仓库")]
    public List<TextAsset> allDialogFiles = new List<TextAsset>();

    // 测试场景使用：不走 SceneController 时，直接播放 allDialogFiles[testFileIndex]。
    public int testFileIndex = 0;

    // 正式流程使用：开启后按 SceneController.currentStoryStep 选择剧情文件。
    public bool useSceneControllerProgress = false;

    // 正式流程使用：开启后剧情结束会进入原来的战斗/主菜单跳转逻辑。
    public bool continueStoryFlowOnFinish = false;

    [Header("2. 立绘配置")]
    public List<string> charNames = new List<string>();
    public List<Sprite> charSprites = new List<Sprite>();

    // 与 charNames / charSprites 一一对应；填 0 或不填则使用 Player Scale。
    // 用来解决不同立绘原图大小不统一的问题。
    public List<float> charScales = new List<float>();
    public Sprite shadowSprite;

    [Header("3. UI 引用")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;
    public Button nextButton;

    [Header("4. 场景渲染器")]
    public SpriteRenderer imageLeft;
    public SpriteRenderer imageRight;

    [Header("5. 运行状态")]
    public int lineId = 0;
    public float typeSpeed = 0.03f;
    public string finishText = "（剧情结束，点击继续）";

    [Header("6. 背景切换")]
    public Image backgroundDisplay;
    public List<Sprite> backgroundSprites = new List<Sprite>();

    [Header("7. 缩放补偿设置")]
    [Range(0.1f, 2.0f)] public float playerScale = 0.6f;
    [Range(0.1f, 2.0f)] public float shadowScale = 0.8f;

    [Header("8. 演出特效")]
    public Image flashImage;
    public Transform cameraShakeTarget;

    // CSV 基础列：标记,id,角色,位置,台词,下一句,背景
    // CSV 可选演出列：音效,自身动作,目标位置,目标动作,屏幕效果
    // 普通台词可以只写前 7 列，后面的演出列为空即可。
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

    private string[] dialogRows;
    private Coroutine typewriterCoroutine;
    private string currentFullContent = "";
    private bool isFinished;

    private void Awake()
    {
        SetPortraitDim(imageLeft);
        SetPortraitDim(imageRight);

        // 闪光遮罩必须在 Canvas 最上层、透明、不拦截点击。
        // 播放白闪/红闪/黑闪时脚本会临时改变它的颜色和 alpha。
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

        // 与原 Dialogmanager 不同：这里保留测试入口。
        // 测试时关闭 useSceneControllerProgress，正式接入时再打开。
        if (useSceneControllerProgress && SceneController.Instance != null)
        {
            int storyIndex = SceneController.Instance.GetCurrentStoryIndex();
            if (storyIndex >= 0 && storyIndex < allDialogFiles.Count)
            {
                fileIndex = storyIndex;
            }
            else
            {
                Debug.LogWarning($"[CinematicDialogManager] SceneController story index {storyIndex} is out of range. Falling back to test file index {testFileIndex}.");
            }
        }

        PlayData(fileIndex);

        if (nextButton != null)
        {
            // 运行时绑定按钮，避免 Inspector 里重复绑定导致一次点击触发多次。
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
        lineId = 0;
        dialogRows = allDialogFiles[fileIndex].text.Split('\n');
        ShowDialogRow();
    }

    public void OnClickNext()
    {
        PlaySound("sfx_ui_click_button");

        // 点击时如果打字机还没结束，本次点击只补全文字，不进入下一句。
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
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

    public void ShowDialogRow()
    {
        if (dialogRows == null) return;

        foreach (string rawRow in dialogRows)
        {
            if (string.IsNullOrWhiteSpace(rawRow)) continue;

            List<string> cells = ParseCsvLine(rawRow.TrimEnd('\r'));
            if (cells.Count < 6) continue;
            if (Cell(cells, ColTag).Trim() != "#") continue;
            if (Cell(cells, ColId).Trim() != lineId.ToString()) continue;

            string name = Cell(cells, ColName);
            string pos = Cell(cells, ColPos).Trim();
            string content = Cell(cells, ColContent);
            string next = Cell(cells, ColNext).Trim();

            TryChangeBackground(Cell(cells, ColBgIndex));
            UpdateUI(name, content, pos);

            // 原 Dialogmanager 到这里基本只更新 UI；
            // 演出版会继续读取后面的可选演出列，驱动音效、动作和屏幕效果。
            PlayRowCues(ReadRowCues(cells, pos));
            UpdateNextLine(next, rawRow);
            return;
        }

        EndDialog();
    }

    private void UpdateUI(string rawName, string content, string pos)
    {
        string pureName = PureName(rawName);
        currentFullContent = content.Trim();

        int charIndex = FindPortraitIndex(pureName);
        Sprite targetSprite = GetPortraitSprite(charIndex);

        // 旁白不会替换左右立绘，只让当前在场角色变暗。
        bool isNarrator = IsNarrator(pureName, pos, targetSprite);
        if (targetSprite == null && !isNarrator) targetSprite = shadowSprite;

        if (nameText != null) nameText.text = rawName.Trim();

        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypeText(currentFullContent));

        ApplyRender(targetSprite, pos, isNarrator, charIndex);
    }

    private void ApplyRender(Sprite sprite, string pos, bool isNarrator, int charIndex)
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
            imageLeft.transform.localScale = GetScaleForSprite(sprite, charIndex);
        }
        else if (IsRight(pos))
        {
            if (sprite != null) imageRight.sprite = sprite;
            SetPortraitActive(imageRight);
            SetPortraitDim(imageLeft);
            imageRight.transform.localScale = GetScaleForSprite(sprite, charIndex);
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

        // 自身动作作用于本句说话人的 pos；目标动作作用于 targetPos。
        // 例如：自身动作=撞击，目标位置=右，目标动作=受击，屏幕效果=白闪震动。
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

        Vector3 origin = t.localPosition;
        float dir = IsLeft(pos) ? 1f : -1f;

        DOTween.Sequence()
            .Append(t.DOLocalMoveX(origin.x + dir * 1.0f, 0.16f).SetEase(Ease.OutQuad))
            .Append(t.DOLocalMoveX(origin.x, 0.26f).SetEase(Ease.OutBack));
    }

    private void PlayImpact(SpriteRenderer target, string pos)
    {
        Transform t = target.transform;
        t.DOKill();

        Vector3 originPos = t.localPosition;
        Vector3 originScale = t.localScale;
        float dir = IsLeft(pos) ? 1f : -1f;

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

        Vector3 origin = t.localPosition;
        DOTween.Sequence()
            .Append(t.DOLocalMoveY(origin.y + 0.25f, 0.08f).SetEase(Ease.OutQuad))
            .Append(t.DOLocalMoveY(origin.y, 0.12f).SetEase(Ease.InQuad));
    }

    private void PlayEnter(SpriteRenderer target, string pos)
    {
        Transform t = target.transform;
        t.DOKill();

        Vector3 origin = t.localPosition;
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

        Vector3 origin = t.localPosition;
        float dir = IsLeft(pos) ? -1f : 1f;

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
        // 组合预设：重攻击 = 强撞击 + 白闪 + 镜头震动。
        PlayImpact(target, pos);
        PlayWhiteFlash(Color.white, 0.9f);
        PlayCameraShake(0.3f, new Vector3(0.18f, 0.1f, 0f));
    }

    private void PlayBossEntry(SpriteRenderer target, string pos)
    {
        Transform t = target.transform;
        Vector3 originScale = t.localScale;

        // 组合预设：强敌登场 = 入场 + 轻微放大回弹 + 黑闪震动。
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

        // 只改 alpha，颜色由 white/red/black 参数决定。
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

        // 关键词规则集中在这里；动画怎么播由 PlayPortraitAction 负责。
        if (Matches(key, "shake", "抖动")) return PortraitActionType.Shake;
        if (Matches(key, "hit", "受击")) return PortraitActionType.Hit;
        if (Matches(key, "punch", "突刺")) return PortraitActionType.Punch;
        if (Matches(key, "impact", "撞击")) return PortraitActionType.Impact;
        if (Matches(key, "jump", "跳动")) return PortraitActionType.Jump;
        if (Matches(key, "enter", "入场")) return PortraitActionType.Enter;
        if (Matches(key, "exit", "退场")) return PortraitActionType.Exit;
        if (Matches(key, "fadein", "淡入")) return PortraitActionType.FadeIn;
        if (Matches(key, "fadeout", "淡出")) return PortraitActionType.FadeOut;
        if (Matches(key, "attack_light", "轻攻击")) return PortraitActionType.AttackLight;
        if (Matches(key, "attack_heavy", "重攻击")) return PortraitActionType.AttackHeavy;
        if (Matches(key, "damage_heavy", "重伤")) return PortraitActionType.DamageHeavy;
        if (Matches(key, "boss_entry", "强敌登场")) return PortraitActionType.BossEntry;

        Debug.LogWarning($"[CinematicDialogManager] Unknown portrait action: {action}");
        return PortraitActionType.None;
    }

    private FlashKind ParseFlashKind(string key)
    {
        if (ContainsAny(key, "redflash", "红闪")) return FlashKind.Red;
        if (ContainsAny(key, "blackflash", "黑闪")) return FlashKind.Black;
        if (ContainsAny(key, "flash", "白闪")) return FlashKind.White;
        return FlashKind.None;
    }

    private bool HasScreenShake(string key)
    {
        return ContainsAny(key, "shake", "震动");
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

    private void UpdateNextLine(string next, string row)
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

        Debug.LogWarning($"[CinematicDialogManager] Invalid next id '{next}' in row: {row}");
        EndDialog();
    }

    private void ContinueAfterFinish()
    {
        if (!continueStoryFlowOnFinish || SceneController.Instance == null)
        {
            EndDialog();
            return;
        }

        SceneController.Instance.AdvanceStoryIndex();
        int finishedStep = SceneController.Instance.GetCurrentStoryIndex() - 1;

        // 保留原 Dialogmanager 的硬编码流程：剧情 0/1/2 分别接战斗 1/2/3。
        if (finishedStep == 0) SceneController.Instance.GoToBattle1();
        else if (finishedStep == 1) SceneController.Instance.GoToBattle2();
        else if (finishedStep == 2) SceneController.Instance.GoToBattle3();
        else SceneController.Instance.BackToMainMenu();
    }

    public void EndDialog()
    {
        isFinished = true;
        if (dialogText != null) dialogText.text = finishText;
    }

    private IEnumerator TypeText(string text)
    {
        if (dialogText == null) yield break;

        dialogText.text = "";
        foreach (char c in text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        typewriterCoroutine = null;
    }

    private int FindPortraitIndex(string pureName)
    {
        for (int i = 0; i < charNames.Count; i++)
        {
            string configName = PureName(charNames[i]);
            if (string.IsNullOrEmpty(pureName) || string.IsNullOrEmpty(configName)) continue;

            if (pureName.Contains(configName) || configName.Contains(pureName))
            {
                if (i < charSprites.Count) return i;
            }
        }

        return -1;
    }

    private Sprite GetPortraitSprite(int charIndex)
    {
        if (charIndex >= 0 && charIndex < charSprites.Count) return charSprites[charIndex];
        return null;
    }

    private bool IsNarrator(string pureName, string pos, Sprite sprite)
    {
        return (IsCenter(pos) && sprite == null) || pureName.Contains("旁白") || pureName.Contains("传颂者");
    }

    private Vector3 GetScaleForSprite(Sprite sprite, int charIndex)
    {
        bool isShadow = sprite != null && shadowSprite != null && sprite.name == shadowSprite.name;
        float scale = isShadow ? shadowScale : playerScale;

        // 优先使用角色单独缩放；没配置时才使用 Player Scale。
        if (!isShadow && charIndex >= 0 && charIndex < charScales.Count && charScales[charIndex] > 0f)
        {
            scale = charScales[charIndex];
        }

        return new Vector3(scale, scale, 1f);
    }

    private SpriteRenderer GetPortraitByPos(string pos)
    {
        if (IsLeft(pos)) return imageLeft;
        if (IsRight(pos)) return imageRight;
        return null;
    }

    private void SetPortraitActive(SpriteRenderer renderer)
    {
        if (renderer != null) renderer.DOColor(Color.white, 0.16f);
    }

    private void SetPortraitDim(SpriteRenderer renderer)
    {
        if (renderer != null)
        {
            // 退场会把 alpha 变成 0；变暗时保留 alpha，避免退场角色被旁白重新显示出来。
            float alpha = renderer.color.a;
            renderer.DOColor(new Color(0.3f, 0.3f, 0.3f, alpha), 0.16f);
        }
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
        return string.IsNullOrEmpty(key) || key == "none" || key == "-" || key == "无";
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
        // 比 string.Split(',') 稍安全：支持引号里的逗号，例如 "你好,世界"。
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
        return value == "左" || value == "left" || value == "l";
    }

    private bool IsRight(string pos)
    {
        string value = pos.Trim().ToLowerInvariant();
        return value == "右" || value == "right" || value == "r";
    }

    private bool IsCenter(string pos)
    {
        string value = pos.Trim().ToLowerInvariant();
        return value == "中" || value == "center" || value == "c";
    }
}
