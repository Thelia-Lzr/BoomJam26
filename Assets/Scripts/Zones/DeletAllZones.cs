using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IZoneDeleter
{
    void DeleteAllZones();
}

public class DeletAllZones : MonoBehaviour, IZoneDeleter
{
    [SerializeField] private string deleteSfxName = "Delete";

    public void DeleteAllZones()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.currentMode == LevelManager.CurrentMode.PlayMode)
        {
            return;
        }

        DefaultZone[] zones = FindObjectsOfType<DefaultZone>();
        if (zones.Length == 0)
        {
            return;
        }

        PlayDeleteSfx();

        int memoryToFree = 0;
        foreach (DefaultZone zone in zones)
        {
            memoryToFree += zone.memoryUsed;
            Destroy(zone.gameObject);
        }

        if (MemoryUsedUI.Instance != null && memoryToFree != 0)
        {
            MemoryUsedUI.Instance.ChangeMemoryUsed(-memoryToFree);
        }
    }

    private void PlayDeleteSfx()
    {
        if (string.IsNullOrWhiteSpace(deleteSfxName)) return;
        if (SoundManager.SoundManager.Instance == null) return;

        SoundManager.SoundManager.Instance.Play(deleteSfxName.Trim());
    }
}
