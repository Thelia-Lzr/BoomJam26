using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IZoneDeleter
{
    // 删除场景内所有区域
    void DeleteAllZones();
}

public class DeletAllZones : MonoBehaviour, IZoneDeleter
{
    // 供按钮调用的删除入口
    public void DeleteAllZones()
    {
        // 仅在编辑模式下允许删除
        if (LevelManager.Instance != null && LevelManager.Instance.currentMode == LevelManager.CurrentMode.PlayMode)
        {
            return;
        }

        // 查找场景内所有区域实例
        DefaultZone[] zones = FindObjectsOfType<DefaultZone>();
        if (zones.Length == 0)
        {
            return;
        }

        int memoryToFree = 0;
        foreach (DefaultZone zone in zones)
        {
            // 累计内存并销毁对象
            memoryToFree += zone.memoryUsed;
            Destroy(zone.gameObject);
        }

        // 更新内存占用
        if (MemoryUsedUI.Instance != null && memoryToFree != 0)
        {
            MemoryUsedUI.Instance.ChangeMemoryUsed(-memoryToFree);
        }
    }
}
