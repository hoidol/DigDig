using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(StageData), true)]
public class StageDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        StageData stageData = (StageData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("LoadData"))
        {
            stageData.LoadData();
            AssetDatabase.SaveAssets();
        }
        if (GUILayout.Button("Edit"))
        {
            stageData.Edit();
            AssetDatabase.SaveAssets();
        }
    }
}
#endif

