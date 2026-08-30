using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MiniMeData))]
public class MiniMeDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MiniMeData miniMeData = (MiniMeData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("LoadData"))
        {
            miniMeData.LoadData();
        }
        if (GUILayout.Button("Edit"))
        {
            miniMeData.Edit();
        }
    }
}
