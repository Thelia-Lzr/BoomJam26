using UnityEngine;

public class SceneBgmPlayer : MonoBehaviour
{
    [SerializeField] private string bgmName = "BGM";
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool stopOnDisable = true;

    private bool startedPlayback;

    private void Start()
    {
        if (!playOnStart) return;
        PlayBgm();
    }

    public void PlayBgm()
    {
        if (SoundManager.SoundManager.Instance == null) return;
        if (string.IsNullOrWhiteSpace(bgmName)) return;

        SoundManager.SoundManager.Instance.Play(bgmName.Trim());
        startedPlayback = true;
    }

    public void StopBgm()
    {
        if (!startedPlayback) return;
        if (SoundManager.SoundManager.Instance == null) return;

        SoundManager.SoundManager.Instance.StopAll();
        startedPlayback = false;
    }

    private void OnDisable()
    {
        if (!stopOnDisable) return;
        StopBgm();
    }
}
