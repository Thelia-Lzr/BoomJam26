using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [Header("Scene Indices")]
    [SerializeField] private int mainMenuSceneIndex = 0;
    [SerializeField] private int levelSelectSceneIndex = 1;
    [SerializeField] private int storySceneIndex = 2;
    [SerializeField] private int[] battleSceneIndices;

    [Header("Story Ids (per level)")]
    [SerializeField] private int[] preBattleStoryIds;
    [SerializeField] private int[] postBattleStoryIds;

    private int currentLevelIndex = -1;
    private int currentStoryId = -1;
    private StoryPhase currentStoryPhase = StoryPhase.None;

    private enum StoryPhase
    {
        None,
        PreBattle,
        PostBattle
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 确保它跨场景存在
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // 重点：如果已经有一个了，就把新出来的删掉
        }
    }


    public int GetCurrentStoryIndex()
    {
        return currentStoryId;
    }

    public void StartLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        currentStoryPhase = StoryPhase.PreBattle;

        if (TryGetStoryId(preBattleStoryIds, levelIndex, out int storyId))
        {
            currentStoryId = storyId;
            SceneManager.LoadScene(storySceneIndex);
            return;
        }

        Debug.LogWarning($"<color=yellow>未配置战前剧情，直接进入战斗：level={levelIndex}</color>");
        currentStoryId = -1;
        GoToBattleForCurrentLevel();
    }

    public void StartPostBattleStory()
    {
        if (currentLevelIndex < 0)
        {
            Debug.LogWarning("<color=yellow>未选择关卡，无法进入战后剧情。</color>");
            GoToLevelSelect();
            return;
        }

        currentStoryPhase = StoryPhase.PostBattle;

        if (TryGetStoryId(postBattleStoryIds, currentLevelIndex, out int storyId))
        {
            currentStoryId = storyId;
            SceneManager.LoadScene(storySceneIndex);
            return;
        }

        Debug.LogWarning($"<color=yellow>未配置战后剧情，返回选关：level={currentLevelIndex}</color>");
        currentStoryId = -1;
        GoToLevelSelect();
    }

    public void OnStoryFinished()
    {
        if (currentStoryPhase == StoryPhase.PreBattle)
        {
            GoToBattleForCurrentLevel();
            return;
        }

        if (currentStoryPhase == StoryPhase.PostBattle)
        {
            GoToLevelSelect();
            return;
        }

        Debug.LogWarning("<color=yellow>剧情结束但未指定阶段，返回选关。</color>");
        GoToLevelSelect();
    }

    public void GoToBattleForCurrentLevel()
    {
        if (TryGetBattleSceneIndex(out int sceneIndex))
        {
            SceneManager.LoadScene(sceneIndex);
            return;
        }

        Debug.LogWarning("<color=yellow>未配置战斗场景，返回选关。</color>");
        GoToLevelSelect();
    }

    public void RestartBattle()
    {
        GoToBattleForCurrentLevel();
    }

    public void GoToLevelSelect()
    {
        SceneManager.LoadScene(levelSelectSceneIndex);
    }

    public void BackToMainMenu()
    {
        ResetState();
        SceneManager.LoadScene(mainMenuSceneIndex);
        Debug.Log("<color=red>【系统】返回主菜单</color>");
    }

    public void StartNewGame()
    {
        ResetState();
        GoToLevelSelect();
    }

    private void ResetState()
    {
        currentLevelIndex = -1;
        currentStoryId = -1;
        currentStoryPhase = StoryPhase.None;
    }

    private bool TryGetBattleSceneIndex(out int sceneIndex)
    {
        sceneIndex = -1;

        if (battleSceneIndices == null || currentLevelIndex < 0 || currentLevelIndex >= battleSceneIndices.Length)
        {
            return false;
        }

        sceneIndex = battleSceneIndices[currentLevelIndex];
        return sceneIndex >= 0;
    }

    private static bool TryGetStoryId(int[] storyIds, int levelIndex, out int storyId)
    {
        storyId = -1;

        if (storyIds == null || levelIndex < 0 || levelIndex >= storyIds.Length)
        {
            return false;
        }

        storyId = storyIds[levelIndex];
        return storyId >= 0;
    }
}