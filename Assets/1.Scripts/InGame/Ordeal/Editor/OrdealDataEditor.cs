#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OrdealData))]
public class OrdealDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        OrdealData data = (OrdealData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("LoadData"))
        {
            data.LoadData();
            AssetDatabase.SaveAssets();
        }
    }

    [MenuItem("Tools/Load All OrdealData")]
    public static void LoadAllOrdealData()
    {
        var all = Resources.LoadAll<OrdealData>("OrdealData");
        foreach (var d in all) d.LoadData();
        AssetDatabase.SaveAssets();
        Debug.Log($"[OrdealData] 전체 LoadData 완료 ({all.Length}개)");
    }
}
#endif
