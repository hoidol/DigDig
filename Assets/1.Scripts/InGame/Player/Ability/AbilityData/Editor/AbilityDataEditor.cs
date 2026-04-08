using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(AbilityData), true)]
public class AbilityDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AbilityData abilityData = (AbilityData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Edit"))
        {
            var method = abilityData.GetType().GetMethod("Edit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (method != null)
            {
                method.Invoke(abilityData, null);
            }
            else
            {
                Debug.LogWarning("Edit() method does not exist on AbilityData.");
            }
        }
    }
}
#endif

