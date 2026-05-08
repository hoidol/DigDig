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
        var allData = Resources.LoadAll<ItemData>("ItemData");
        foreach (var data in allData)
            data.LoadData();
        AssetDatabase.SaveAssets();
        Debug.Log($"[ItemData] {allData.Length}개 ItemData LoadData 완료");
    }
}
