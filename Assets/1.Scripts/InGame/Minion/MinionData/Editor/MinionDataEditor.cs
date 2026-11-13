using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MinionData), true)]
public class MinionDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MinionData minionData = (MinionData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("LoadData"))
        {
            minionData.LoadData();
        }
        if (GUILayout.Button("Edit"))
        {
            minionData.Edit();
        }
    }
}
