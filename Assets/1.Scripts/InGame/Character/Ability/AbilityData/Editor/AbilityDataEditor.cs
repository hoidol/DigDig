using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public static class AbilityDataMenuItems
{
    [MenuItem("Tools/Load All AbilityData")]
    static void LoadAllAbilityData()
    {
        var allData = Resources.LoadAll<AbilityData>("AbilityData");
        int count = 0;
        foreach (var data in allData)
        {
            data.LoadData();
            count++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[AbilityData] 전체 LoadData 완료 ({count}개)");
    }
}

[CustomEditor(typeof(AbilityData), true)]
public class AbilityDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AbilityData abilityData = (AbilityData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("LoadData"))
        {
            abilityData.LoadData();
            AssetDatabase.SaveAssets();
        }
        if (GUILayout.Button("Edit"))
        {
            abilityData.Edit();
            AssetDatabase.SaveAssets();
        }
    }
}
#endif

