#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyPatternData))]
public class EnemyPatternDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EnemyPatternData data = (EnemyPatternData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("LoadData"))
        {
            data.LoadData();
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
