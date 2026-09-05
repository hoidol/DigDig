using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnhanceExpInfo))]
public class EnhanceExpInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EnhanceExpInfo data = (EnhanceExpInfo)target;

        GUILayout.Space(10);
        if (GUILayout.Button("LoadData"))
        {
            data.LoadData();
        }
    }
}
