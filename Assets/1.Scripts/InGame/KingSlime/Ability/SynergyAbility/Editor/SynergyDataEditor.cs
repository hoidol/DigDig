using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public static class SynergyDataMenuItems
{
    [MenuItem("Tools/Load All SynergyData")]
    static void LoadAllSynergyData()
    {
        var allData = Resources.LoadAll<SynergyData>("SynergyData");
        int count = 0;
        foreach (var data in allData)
        {
            data.LoadData();
            count++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[SynergyData] 전체 LoadData 완료 ({count}개)");
    }
}

[CustomEditor(typeof(SynergyData), true)]
public class SynergyDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SynergyData synergyData = (SynergyData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("LoadData"))
            synergyData.LoadData();
    }
}
#endif
