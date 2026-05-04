using UnityEngine;

public static class SaveManager
{
    private const string UnlockedLevelKey = "Save_UnlockedLevel";
    private const int FirstLevel = 1;

    public static int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt(UnlockedLevelKey, FirstLevel);
    }

    public static bool IsLevelUnlocked(int level)
    {
        return level <= GetUnlockedLevel();
    }

    public static bool IsLevelCleared(int level)
    {
        return level < GetUnlockedLevel();
    }

    public static void CompleteLevel(int clearedLevel)
    {
        if (clearedLevel < FirstLevel)
        {
            Debug.LogWarning($"[SaveManager] Invalid cleared level: {clearedLevel}");
            return;
        }

        UnlockLevel(clearedLevel + 1);
    }

    public static void UnlockLevel(int level)
    {
        if (level < FirstLevel)
        {
            Debug.LogWarning($"[SaveManager] Invalid unlock level: {level}");
            return;
        }

        int currentUnlockedLevel = GetUnlockedLevel();
        if (level <= currentUnlockedLevel)
        {
            Debug.Log($"[SaveManager] Level {level} is already unlocked. Current progress: {currentUnlockedLevel}");
            return;
        }

        PlayerPrefs.SetInt(UnlockedLevelKey, level);
        PlayerPrefs.Save();
        Debug.Log($"[SaveManager] Progress saved. Highest unlocked level: {level}");
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(UnlockedLevelKey);
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Progress reset. Only level 1 is unlocked.");
    }
}
