using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnhanceGradeInfo))]
public class EnhanceGradeInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EnhanceGradeInfo data = (EnhanceGradeInfo)target;

        GUILayout.Space(10);
        if (GUILayout.Button("LoadData"))
        {
            data.LoadData();
        }
    }
}
