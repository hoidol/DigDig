#if UNITY_EDITOR
using UnityEditor;

// Play 진입 직전에 Assets/Json 테이블 중 바뀐 것만 데이터 SO에 반영한다.
// 끄기: Tools/Data/Auto Import On Play
[InitializeOnLoad]
public static class PreGameStartEditor
{
    static PreGameStartEditor()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode) return;
        if (!DataTableImporter.AutoImportOnPlay) return;

        DataTableImporter.ImportChanged();
    }
}
#endif
