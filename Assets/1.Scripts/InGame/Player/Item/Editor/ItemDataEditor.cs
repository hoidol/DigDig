using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ItemData itemData = (ItemData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("LoadData"))
        {
            itemData.LoadData();
        }
        if (GUILayout.Button("Edit"))
        {
            itemData.Edit();
        }
    }
}

public static class ItemDataMenuItems
{
    [MenuItem("Tools/Load All ItemData")]
    static void LoadAllItemData()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/AddressableResources/ItemData" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData data = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            data.LoadData();
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[ItemData] {guids.Length}개 ItemData LoadData 완료");
    }
}
