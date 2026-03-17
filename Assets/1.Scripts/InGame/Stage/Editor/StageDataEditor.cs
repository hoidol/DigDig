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
        if (GUILayout.Button("Edit"))
        {
            var method = stageData.GetType().GetMethod("Edit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (method != null)
            {
                method.Invoke(stageData, null);
            }
            else
            {
                Debug.LogWarning("Edit() method does not exist on AbilityData.");
            }
        }
    }
}
#endif

