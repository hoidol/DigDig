#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

// EnhanceGradeInfo.csv / EnhanceLevelInfo.csv 로 등급별 EnhanceGradeInfo, EnhanceExpInfo 에셋을
// 만들고(이미 있으면 재사용) CSV 값을 채운 뒤, "SlimeData" Addressable 그룹에 라벨과 함께 등록한다.
public static class EnhanceDataMenuItems
{
    const string GRADE_INFO_FOLDER = "Assets/AddressableResources/SlimeData/EnhanceGradeInfo";
    const string EXP_INFO_FOLDER = "Assets/AddressableResources/SlimeData/EnhanceExpInfo";
    const string GROUP_NAME = "SlimeData";

    [MenuItem("Tools/Slime/Create And Load Enhance Data")]
    static void CreateAndLoadEnhanceData()
    {
        foreach (GradeType grade in (GradeType[])Enum.GetValues(typeof(GradeType)))
        {
            EnhanceGradeInfo gradeInfo = GetOrCreateAsset<EnhanceGradeInfo>(GRADE_INFO_FOLDER, grade.ToString(), asset => asset.grade = grade);
            gradeInfo.LoadData();
            AddToAddressables(gradeInfo, "EnhanceGradeInfo");

            EnhanceExpInfo expInfo = GetOrCreateAsset<EnhanceExpInfo>(EXP_INFO_FOLDER, grade.ToString(), asset => asset.grade = grade);
            expInfo.LoadData();
            AddToAddressables(expInfo, "EnhanceExpInfo");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[EnhanceDataMenuItems] EnhanceGradeInfo / EnhanceExpInfo 생성 및 Addressable 등록 완료");
    }

    static T GetOrCreateAsset<T>(string folder, string assetName, Action<T> init) where T : ScriptableObject
    {
        EnsureFolder(folder);
        string path = $"{folder}/{assetName}.asset";
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            init(asset);
            AssetDatabase.CreateAsset(asset, path);
        }
        return asset;
    }

    static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;

        string parent = System.IO.Path.GetDirectoryName(folder).Replace("\\", "/");
        string newFolderName = System.IO.Path.GetFileName(folder);
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, newFolderName);
    }

    static void AddToAddressables(UnityEngine.Object asset, string label)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogWarning("[EnhanceDataMenuItems] AddressableAssetSettings 없음");
            return;
        }

        AddressableAssetGroup group = settings.FindGroup(GROUP_NAME) ?? settings.DefaultGroup;
        string assetPath = AssetDatabase.GetAssetPath(asset);
        string guid = AssetDatabase.AssetPathToGUID(assetPath);

        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
        entry.address = asset.name;
        entry.SetLabel(label, true, true);
    }
}
#endif
