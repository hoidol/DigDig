using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EquipmentData))]
public class EquipmentDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EquipmentData equipmentData = (EquipmentData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Edit"))
        {
            equipmentData.Edit();
        }
    }
}

public static class EquipmentDataMenuItems
{
    [MenuItem("Tools/Load All EquipmentData")]
    static void LoadAllEquipmentData()
    {
        string[] guids = AssetDatabase.FindAssets("t:EquipmentData", new[] { "Assets/AddressableResources/EquipmentData" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EquipmentData data = AssetDatabase.LoadAssetAtPath<EquipmentData>(path);
            data.Edit();
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[EquipmentData] {guids.Length}개 EquipmentData Edit 완료");
    }
}
