using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RelicData))]
public class RelicDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RelicData relicData = (RelicData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Edit"))
        {
            relicData.Edit();
        }
    }
}

public static class RelicDataMenuItems
{
    [MenuItem("Tools/Load All RelicData")]
    static void LoadAllRelicData()
    {
        string[] guids = AssetDatabase.FindAssets("t:RelicData", new[] { "Assets/AddressableResources/RelicData" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            RelicData data = AssetDatabase.LoadAssetAtPath<RelicData>(path);
            data.Edit();
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[RelicData] {guids.Length}개 RelicData Edit 완료");
    }
}
