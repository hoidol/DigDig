using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PlayerData), true)]

public class PlayerDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PlayerData playerData = (PlayerData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Edit"))
        {
            var method = playerData.GetType().GetMethod("Edit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (method != null)
            {
                method.Invoke(playerData, null);
            }
            else
            {
                Debug.LogWarning("Edit() method does not exist on PlayerData.");
            }
        }
    }
}
#endif
