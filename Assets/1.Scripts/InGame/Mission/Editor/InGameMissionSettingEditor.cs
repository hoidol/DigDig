using System;
using UnityEditor;
using UnityEngine;

// 미션 목록([SerializeReference])에 종류를 골라 추가하는 버튼 + 에셋 만들기 메뉴
[CustomEditor(typeof(InGameMissionSetting))]
public class InGameMissionSettingEditor : Editor
{
    const string ASSET_PATH = "Assets/Resources/InGameMissionSetting.asset";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();

        var setting = (InGameMissionSetting)target;
        if (GUILayout.Button("미션 추가"))
        {
            var menu = new GenericMenu();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<Mission>())
            {
                if (type.IsAbstract)
                    continue;
                Type t = type;
                menu.AddItem(new GUIContent(t.Name), false, () =>
                {
                    Undo.RecordObject(setting, "Add Mission");
                    setting.missions.Add((Mission)Activator.CreateInstance(t));
                    EditorUtility.SetDirty(setting);
                });
            }
            menu.ShowAsContext();
        }
        if (GUILayout.Button("기본 미션으로 되돌리기"))
        {
            Undo.RecordObject(setting, "Reset Missions");
            setting.missions = InGameMissionSetting.CreateDefaults();
            EditorUtility.SetDirty(setting);
        }
    }

    [MenuItem("Tools/Mission/Create InGameMissionSetting")]
    static void CreateAsset()
    {
        var existing = AssetDatabase.LoadAssetAtPath<InGameMissionSetting>(ASSET_PATH);
        if (existing == null)
        {
            existing = CreateInstance<InGameMissionSetting>();
            AssetDatabase.CreateAsset(existing, ASSET_PATH);
            AssetDatabase.SaveAssets();
        }
        Selection.activeObject = existing;
    }
}
