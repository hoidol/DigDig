using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AuthorityData))]
public class AuthorityDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AuthorityData authorityData = (AuthorityData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Edit"))
        {
            authorityData.Edit();
        }
    }
}

public static class AuthorityDataMenuItems
{
    [MenuItem("Tools/Load All AuthorityData")]
    static void LoadAllAuthorityData()
    {
        string[] guids = AssetDatabase.FindAssets("t:AuthorityData", new[] { "Assets/AddressableResources/AuthorityData" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AuthorityData data = AssetDatabase.LoadAssetAtPath<AuthorityData>(path);
            data.Edit();
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[AuthorityData] {guids.Length}개 AuthorityData Edit 완료");
    }
}
