using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SlimeData))]
public class SlimeDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SlimeData slimeData = (SlimeData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("LoadData"))
        {
            slimeData.LoadData();
        }
        if (GUILayout.Button("Edit"))
        {
            slimeData.Edit();
        }
    }
}
