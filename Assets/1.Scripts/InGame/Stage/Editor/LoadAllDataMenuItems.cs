#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class LoadAllDataMenuItems
{
    [MenuItem("Tools/Load All Data")]
    static void LoadAll()
    {
        LoadAllEnemyData();
        LoadAllStageData();
        LoadAllEnemyPatternData();
        AssetDatabase.SaveAssets();
        Debug.Log("[LoadAll] 전체 데이터 로드 완료");
    }

    [MenuItem("Tools/Load All EnemyData")]
    static void LoadAllEnemyData()
    {
        var all = Resources.LoadAll<EnemyData>("EnemyData");
        foreach (var d in all) d.LoadData();
        AssetDatabase.SaveAssets();
        Debug.Log($"[EnemyData] 전체 LoadData 완료 ({all.Length}개)");
    }

    [MenuItem("Tools/Load All StageData")]
    static void LoadAllStageData()
    {
        var all = Resources.LoadAll<StageData>("StageData");
        foreach (var d in all) d.LoadData();
        AssetDatabase.SaveAssets();
        Debug.Log($"[StageData] 전체 LoadData 완료 ({all.Length}개)");
    }

    [MenuItem("Tools/Load All EnemyPatternData")]
    static void LoadAllEnemyPatternData()
    {
        var all = Resources.LoadAll<EnemyPatternData>("EnemyPatternData");
        foreach (var d in all) d.LoadData();
        AssetDatabase.SaveAssets();
        Debug.Log($"[EnemyPatternData] 전체 LoadData 완료 ({all.Length}개)");
    }
}
#endif
