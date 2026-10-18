using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine;

// Addressables 빌드는 내부에서 예외를 잡아 메시지만 남기므로,
// 같은 빌드를 try/catch 없이 호출해 전체 스택트레이스를 콘솔에 찍는다 (디버그용)
public static class AddressablesBuildDebug
{
    [MenuItem("Tools/Addressables/Build With Full Stack Trace")]
    static void Build()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var builder = settings.ActivePlayerDataBuilder as BuildScriptBase;
        var input = new AddressablesDataBuilderInput(settings);

        MethodInfo method = typeof(BuildScriptPackedMode)
            .GetMethod("BuildDataImplementation", BindingFlags.Instance | BindingFlags.NonPublic)
            .MakeGenericMethod(typeof(AddressablesPlayerBuildResult));
        try
        {
            var result = (AddressablesPlayerBuildResult)method.Invoke(builder, new object[] { input });
            Debug.Log($"[AddressablesBuildDebug] 완료 Error: {result?.Error ?? "(없음)"}");
        }
        catch (TargetInvocationException e)
        {
            Debug.LogError($"[AddressablesBuildDebug] {e.InnerException}");
        }
    }

    // 그룹마다 엔트리를 돌며 null / 경로 없음 / 빈 폴더 같은 이상한 엔트리를 찾는다
    [MenuItem("Tools/Addressables/Validate Entries")]
    static void ValidateEntries()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        int problems = 0;
        foreach (var group in settings.groups)
        {
            if (group == null)
            {
                Debug.LogError("[AddressablesValidate] null 그룹");
                problems++;
                continue;
            }

            int i = 0;
            foreach (var entry in group.entries)
            {
                string tag = $"[AddressablesValidate] {group.Name}[{i++}]";
                if (entry == null)
                {
                    Debug.LogError($"{tag} null 엔트리");
                    problems++;
                    continue;
                }

                string path = AssetDatabase.GUIDToAssetPath(entry.guid);
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogError($"{tag} 경로 없음 guid {entry.guid} address {entry.address}");
                    problems++;
                    continue;
                }

                if (AssetDatabase.IsValidFolder(path))
                {
                    var subs = new System.Collections.Generic.List<UnityEditor.AddressableAssets.Settings.AddressableAssetEntry>();
                    entry.GatherAllAssets(subs, false, true, false);
                    if (subs.Count == 0)
                        Debug.LogWarning($"{tag} 빈 폴더 {path}");
                    else if (subs.Exists(s => s == null))
                    {
                        Debug.LogError($"{tag} 폴더 안에 null 엔트리 {path}");
                        problems++;
                    }
                }
            }
        }
        Debug.Log($"[AddressablesValidate] 완료, 문제 {problems}개");
    }
}
