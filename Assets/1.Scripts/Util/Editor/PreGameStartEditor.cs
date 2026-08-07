#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PreGameStartEditor
{
    // // Resources/ 기준 개별 경로 — 해당 에셋 하나만 LoadData
    // static readonly string[] scriptableObjectPaths = new string[]
    // {
    //     "StageData/Greed"
    // };

    // // Resources/ 기준 폴더 경로 — 폴더 내 ScriptableObject 전체 LoadData
    // static readonly string[] scriptableObjectFoldPaths = new string[]
    // {
    //     "EnemyData",
    // };

    // static PreGameStartEditor()
    // {
    //     EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    // }

    // static void OnPlayModeStateChanged(PlayModeStateChange state)
    // {
    //     if (state != PlayModeStateChange.ExitingEditMode) return;

    //     int count = 0;

    //     foreach (string path in scriptableObjectPaths)
    //     {
    //         var so = Resources.Load<ScriptableObject>(path);
    //         if (so == null) { Debug.LogWarning($"[PreGameStart] 에셋 없음: Resources/{path}"); continue; }
    //         if (TryCallLoadData(so)) count++;
    //     }

    //     foreach (string folder in scriptableObjectFoldPaths)
    //     {
    //         var all = Resources.LoadAll<ScriptableObject>(folder);
    //         if (all.Length == 0) { Debug.LogWarning($"[PreGameStart] 에셋 없음: Resources/{folder}"); continue; }
    //         foreach (var so in all)
    //             if (TryCallLoadData(so)) count++;
    //     }

    //     if (count > 0)
    //     {
    //         AssetDatabase.SaveAssets();
    //         Debug.Log($"[PreGameStart] LoadData 완료 ({count}개)");
    //     }
    }

    static bool TryCallLoadData(ScriptableObject so)
    {
        var method = so.GetType().GetMethod("LoadData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null)
        {
            Debug.LogWarning($"[PreGameStart] {so.GetType().Name} ({so.name}) — LoadData 메서드 없음");
            return false;
        }
        method.Invoke(so, null);
        return true;
    }
}
#endif
