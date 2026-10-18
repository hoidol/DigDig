using System.IO;
using UnityEditor;
using UnityEngine;

// 유저 데이터 초기화 (에디터 전용)
// SaveManager는 에디터에서 Assets/, 빌드에서 persistentDataPath에 User*.json으로 저장 → 둘 다 지우고 PlayerPrefs도 전부 삭제
public static class UserDataResetMenu
{
    const string SAVE_FILE_PATTERN = "User*.json";

    [MenuItem("Tools/Data/Reset All UserData", priority = 100)]
    static void ResetAll()
    {
        if (!EditorUtility.DisplayDialog("유저 데이터 초기화",
                "UserData JSON 파일과 PlayerPrefs를 모두 삭제합니다.\n되돌릴 수 없습니다.", "초기화", "취소"))
            return;

        int count = DeleteSaveFiles();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        AssetDatabase.Refresh();

        Debug.Log($"[UserDataReset] 세이브 파일 {count}개 + PlayerPrefs 삭제 완료");
    }

    [MenuItem("Tools/Data/Reset All UserData", true)]
    static bool ResetAllValidate()
    {
        // 플레이 중에는 매니저가 메모리 데이터를 다시 저장해버리므로 막는다
        return !EditorApplication.isPlaying;
    }

    static int DeleteSaveFiles()
    {
        int count = 0;
        foreach (string dir in new[] { Application.dataPath, Application.persistentDataPath })
        {
            if (!Directory.Exists(dir))
                continue;

            foreach (string path in Directory.GetFiles(dir, SAVE_FILE_PATTERN, SearchOption.TopDirectoryOnly))
            {
                File.Delete(path);
                if (File.Exists(path + ".meta"))
                    File.Delete(path + ".meta");
                Debug.Log($"[UserDataReset] 삭제 {path}");
                count++;
            }
        }
        return count;
    }
}
