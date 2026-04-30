using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using SoundManager;

public class Dialogmanager : MonoBehaviour
{
    [Header("1. 剧情仓库")]
    public List<TextAsset> allDialogFiles = new List<TextAsset>();

    [Header("2. 立绘配置")]
    public List<string> charNames = new List<string>();
    public List<Sprite> charSprites = new List<Sprite>();
    public Sprite shadowSprite;

    [Header("3. UI 引用")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;
    public Button nextButton;

    [Header("4. 场景渲染器")]
    public SpriteRenderer imageLeft;
    public SpriteRenderer imageRight;

    [Header("5. 运行状态")]
    public int dialogIndex = 0;
    public float typeSpeed = 0.03f;
    private bool isFinished = false; // 标记剧情是否结束

    private string[] dialogRows;
    private Coroutine typewriterCoroutine;
    private string currentFullContent;

    [Header("6. 背景切换")]
    public Image backgroundDisplay;
    public List<Sprite> backgroundSprites = new List<Sprite>();
    [Header("7. 缩放补偿设置")]
    [Range(0.1f, 2.0f)] public float playerScale = 0.6f;  // 主角缩放
    [Range(0.1f, 2.0f)] public float shadowScale = 0.8f;  // 杂鱼/兜底缩放 (你新立绘大，就调小这个)

    void Awake()
    {
        if (imageLeft) imageLeft.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        if (imageRight) imageRight.color = new Color(0.3f, 0.3f, 0.3f, 1f);

    }

    void Start()
    {
        if (SceneController.Instance != null)
        {
            // 从场景控制器读取当前剧情编号
            dialogIndex = SceneController.Instance.GetCurrentStoryIndex();
            Debug.Log($"<color=green>【加载】准备播放剧本：{dialogIndex}</color>");
        }


        PlayData(dialogIndex);
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners(); // 先清空，防止 Inspector 里重复绑定
            nextButton.onClick.AddListener(OnClickNext);
        }
    }

    public void PlayData(int fileIndex)
    {
        // 根据剧情编号加载对应的文本资源
        if (fileIndex < 0 || fileIndex >= allDialogFiles.Count) return;

        isFinished = false;
        dialogIndex = 0;
        dialogRows = allDialogFiles[fileIndex].text.Split('\n');
        ShowDiaLogRow();
    }

    public void OnClickNext()
    {

        Debug.Log("按钮被点击了！准备播放音效...");
        if (SoundManager.SoundManager.Instance != null)
        {
            Debug.Log("SoundManager 实例存在，正在发送播放请求");
            SoundManager.SoundManager.Instance.Play("sfx_ui_click_button");
        }
        else
        {
            Debug.LogError("找不到 SoundManager 实例！请检查场景里有没有挂脚本。");
        }
        if (isFinished)
        {
            if (SceneController.Instance != null)
            {
                // 剧情结束后的跳转由场景控制器统一管理
                SceneController.Instance.OnStoryFinished();
            }
            return;
        }

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
            dialogText.text = currentFullContent;
            return;
        }

        ShowDiaLogRow();
    }

    public void ShowDiaLogRow()
    {
        // 逐行查找当前对白编号对应的数据
        if (dialogRows == null) return;
        foreach (string row in dialogRows)
        {
            if (string.IsNullOrWhiteSpace(row)) continue;
            string[] cells = row.Split(',');
            if (cells.Length < 6) continue;

            if (cells[1].Trim() == dialogIndex.ToString())
            {
                if (cells[0].Trim() == "#")
                {
                    // 背景切换逻辑
                    if (cells.Length >= 7)
                    {
                        if (int.TryParse(cells[6].Trim(), out int bgIndex))
                        {
                            Debug.Log($"[DialogManager] CSV requests background change -> index: {bgIndex} (row: '{row}')");
                            ChangeBackground(bgIndex);
                        }
                        else
                        {
                            Debug.LogWarning($"[DialogManager] Failed to parse background index from CSV column 7: '{cells[6]}' (row: '{row}')");
                        }
                    }

                    UpdateUI(cells[2], cells[4], cells[3]);

                    // 安全解析下一句 ID，防止 Parse "end" 崩溃
                    string nextTarget = cells[5].Trim();
                    if (nextTarget.ToLower() == "end")
                    {
                        isFinished = true;
                    }
                    else
                    {
                        int.TryParse(nextTarget, out dialogIndex);
                    }
                    return;
                }
            }
            if (cells[0].Trim().ToLower() == "end") { EndDialog(); return; }
        }

        if (dialogIndex == -1) EndDialog();
    }

    private void UpdateUI(string _name, string _content, string _pos)
    {
        string csvPureName = PureName(_name);
        string pos = _pos.Trim();
        currentFullContent = _content.Trim();

        // 1. 匹配立绘
        Sprite targetSprite = null;
        for (int i = 0; i < charNames.Count; i++)
        {
            string configName = PureName(charNames[i]);
            if (!string.IsNullOrEmpty(csvPureName) && !string.IsNullOrEmpty(configName))
            {
                if (csvPureName.Contains(configName) || configName.Contains(csvPureName))
                {
                    if (i < charSprites.Count) { targetSprite = charSprites[i]; break; }
                }
            }
        }

        // 2. 判定旁白：只有在没找到立绘且位置是“中”时，才视为旁白
        bool isNarrator = (pos == "中" && targetSprite == null) || csvPureName.Contains("旁白") || csvPureName.Contains("传颂者");

        // 3. 兜底立绘
        if (targetSprite == null && !isNarrator) targetSprite = shadowSprite;

        // 4. 【统一处理名字】
        // 无论什么情况，名字只显示在 nameText 组件上
        nameText.text = _name.Trim();

        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);

        // 5. 【统一处理内容】
        // 彻底去掉内容框里的名字拼接，内容框只显示纯台词
        // 这样就不可能出现双重括号或重复名字了
        string displayText = currentFullContent;

        typewriterCoroutine = StartCoroutine(TypeText(displayText));

        // 6. 渲染立绘
        ApplyRender(targetSprite, pos, isNarrator);

        if (dialogIndex == 0 && SoundManager.SoundManager.Instance != null)
        {
            SoundManager.SoundManager.Instance.Play("Plot");
        }
    }

    private void ApplyRender(Sprite sprite, string pos, bool isNarrator)
    {
        if (imageLeft == null || imageRight == null) return;
        imageLeft.DOKill(); imageRight.DOKill();
        Color active = Color.white;
        Color inactive = new Color(0.3f, 0.3f, 0.3f, 1f);

        // 使用面板上的变量，方便随时调整
        Vector3 pScale = new Vector3(playerScale, playerScale, 1f);
        Vector3 sScale = new Vector3(shadowScale, shadowScale, 1f);

        bool isShadow = (sprite != null && shadowSprite != null && sprite.name == shadowSprite.name);

        if (isNarrator)
        {
            imageLeft.DOColor(inactive, 0.2f); imageRight.DOColor(inactive, 0.2f);
            // 旁白时，建议把立绘缩得更小或者保持原样
        }
        else if (pos == "左")
        {
            if (sprite != null) imageLeft.sprite = sprite;
            imageLeft.DOColor(active, 0.2f);
            imageRight.DOColor(inactive, 0.2f);
            imageLeft.transform.localScale = isShadow ? sScale : pScale;
        }
        else if (pos == "右")
        {
            if (sprite != null) imageRight.sprite = sprite;
            imageRight.DOColor(active, 0.2f);
            imageLeft.DOColor(inactive, 0.2f);
            imageRight.transform.localScale = isShadow ? sScale : pScale;
        }
    }
    private void ChangeBackground(int index)
    {
        if (backgroundDisplay == null)
        {
            Debug.LogWarning($"[DialogManager] Cannot change background: backgroundDisplay is not assigned (requested index {index}).");
            return;
        }
        if (index < 0 || index >= backgroundSprites.Count)
        {
            Debug.LogWarning($"[DialogManager] Cannot change background: index {index} is out of range (0 - {backgroundSprites.Count - 1}).");
            return;
        }
        if (backgroundDisplay.sprite == backgroundSprites[index])
        {
            Debug.Log($"[DialogManager] Background already set to index {index}, no change needed.");
            return;
        }

        backgroundDisplay.DOKill();
        backgroundDisplay.DOColor(Color.black, 0.3f).OnComplete(() => {
            backgroundDisplay.sprite = backgroundSprites[index];
            backgroundDisplay.DOColor(Color.white, 0.5f);
        });
    }

    IEnumerator TypeText(string t)
    {
        dialogText.text = "";
        foreach (char c in t) { dialogText.text += c; yield return new WaitForSeconds(typeSpeed); }
        typewriterCoroutine = null;
    }

    public void EndDialog()
    {
        isFinished = true;
        dialogText.text = "（剧情结束，点击继续）";
    }

    private string PureName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        return Regex.Replace(raw, @"[^\u4e00-\u9fa5a-zA-Z0-9]", "").Trim();
    }
    // 剧情场景“退出”按钮专用
    public void OnExitButtonClicked()
    {
        Debug.Log("<color=cyan>【UI操作】退出按钮被点击了！</color>");

        if (SceneController.Instance != null)
        {
            Debug.Log("<color=green>【系统】检测到 SceneController 实例，准备重置进度并返回主菜单...</color>");

            // 调用控制器的返回方法
            SceneController.Instance.BackToMainMenu();
        }
        else
        {
            Debug.LogWarning("<color=red>【警告】找不到 SceneController 实例！正在执行 SceneManager 强制跳转...</color>");

            // 兜底方案：如果单例丢失，尝试强制跳转到索引 1
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
}
