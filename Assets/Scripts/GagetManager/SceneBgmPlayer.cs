using UnityEngine;

public class SceneBgmPlayer : MonoBehaviour
{
    [SerializeField] private string bgmName = "BGM";
    [SerializeField] private string playModeBgmName = "";
    [SerializeField] private string victorySoundName = "Victory";
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool stopOnDisable = true;

    private bool startedPlayback;
    private string currentSoundName;
    private LevelManager levelManager;
    private bool trackingLevelState;
    private LevelManager.CurrentMode lastMode;
    private bool lastVictoryTriggered;

    private void Start()
    {
        if (TryGetLevelManager())
        {
            SyncLevelAudio();
            return;
        }

        if (!playOnStart) return;
        PlaySound(ResolveEditModeBgmName());
    }

    private void Update()
    {
        if (!TryGetLevelManager()) return;
        SyncLevelAudio();
    }

    public void PlayBgm()
    {
        if (TryGetLevelManager())
        {
            PlayAudioForLevelState(levelManager.currentMode, levelManager.victoryTriggered);
            lastMode = levelManager.currentMode;
            lastVictoryTriggered = levelManager.victoryTriggered;
            trackingLevelState = true;
            return;
        }

        PlaySound(ResolveEditModeBgmName());
    }

    public void StopBgm()
    {
        if (!startedPlayback)
        {
            currentSoundName = string.Empty;
            return;
        }

        if (SoundManager.SoundManager.Instance != null)
        {
            SoundManager.SoundManager.Instance.StopAll();
        }

        startedPlayback = false;
        currentSoundName = string.Empty;
    }

    private void OnDisable()
    {
        trackingLevelState = false;
        levelManager = null;

        if (!stopOnDisable) return;
        StopBgm();
    }

    private bool TryGetLevelManager()
    {
        if (levelManager != null) return true;

        levelManager = LevelManager.Instance;
        return levelManager != null;
    }

    private void SyncLevelAudio()
    {
        LevelManager.CurrentMode currentMode = levelManager.currentMode;
        bool victoryTriggered = levelManager.victoryTriggered;

        if (!trackingLevelState)
        {
            lastMode = currentMode;
            lastVictoryTriggered = victoryTriggered;
            trackingLevelState = true;

            if (playOnStart)
            {
                PlayAudioForLevelState(currentMode, victoryTriggered);
            }

            return;
        }

        if (!lastVictoryTriggered && victoryTriggered)
        {
            PlayVictorySound();
        }
        else if (lastMode != currentMode)
        {
            SwitchModeBgm(currentMode);
        }

        lastMode = currentMode;
        lastVictoryTriggered = victoryTriggered;
    }

    private void PlayAudioForLevelState(LevelManager.CurrentMode currentMode, bool victoryTriggered)
    {
        if (victoryTriggered)
        {
            PlayVictorySound();
            return;
        }

        SwitchModeBgm(currentMode);
    }

    private void SwitchModeBgm(LevelManager.CurrentMode currentMode)
    {
        if (currentMode == LevelManager.CurrentMode.EditMode)
        {
            PlaySound(ResolveEditModeBgmName());
            return;
        }

        PlaySound(ResolvePlayModeBgmName());
    }

    private void PlayVictorySound()
    {
        string soundName = victorySoundName == null ? string.Empty : victorySoundName.Trim();
        if (string.IsNullOrWhiteSpace(soundName)) return;

        StopBgm();
        PlaySound(soundName);
    }

    private void PlaySound(string soundName)
    {
        if (SoundManager.SoundManager.Instance == null) return;

        soundName = soundName == null ? string.Empty : soundName.Trim();
        if (string.IsNullOrWhiteSpace(soundName)) return;

        if (startedPlayback && currentSoundName == soundName) return;

        SoundManager.SoundManager.Instance.Play(soundName);
        startedPlayback = true;
        currentSoundName = soundName;
    }

    private string ResolveEditModeBgmName()
    {
        return bgmName == null ? string.Empty : bgmName.Trim();
    }

    private string ResolvePlayModeBgmName()
    {
        if (!string.IsNullOrWhiteSpace(playModeBgmName))
        {
            return playModeBgmName.Trim();
        }

        return ResolveEditModeBgmName();
    }
}
