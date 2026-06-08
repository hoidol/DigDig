#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BulletData))]
public class BulletDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BulletData data = (BulletData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("LoadData"))
        {
            data.LoadData();
            AssetDatabase.SaveAssets();
        }
    }
}

public static class BulletDataMenuItems
{
    [MenuItem("Tools/Load All BulletData")]
    static void LoadAll()
    {
        var all = Resources.LoadAll<BulletData>("BulletData");
        foreach (var d in all) d.LoadData();
        AssetDatabase.SaveAssets();
        Debug.Log($"[BulletData] 전체 LoadData 완료 ({all.Length}개)");
    }
}
#endif
