using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RuneData))]
public class RuneDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RuneData runeData = (RuneData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("LoadData"))
        {
            runeData.LoadData();
        }
        if (GUILayout.Button("Edit"))
        {
            runeData.Edit();
        }
    }
}
