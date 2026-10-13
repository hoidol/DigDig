using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// Assets/Json 테이블을 데이터 SO에 일괄 반영한다. (각 SO의 LoadData 호출)
// - Tools/Data/Import All     : 전부 다시 반영
// - Tools/Data/Import Changed : 원본 테이블이 바뀐 데이터만 반영 (Play 진입 시 PreGameStartEditor가 자동 호출)
public static class DataTableImporter
{
    static readonly string[] ASSET_ROOTS = { "Assets/AddressableResources", "Assets/Resources/CharacterData" };
    const string MENU_ROOT = "Tools/Data/";
    const string AUTO_IMPORT_MENU = MENU_ROOT + "Auto Import On Play";

    class Entry
    {
        public readonly string name;
        public readonly string[] sources; // Assets/Json 기준 파일 또는 폴더(하위 *.csv 전체)
        public readonly Func<int> import; // 반영한 에셋 개수 반환

        public Entry(string name, string[] sources, Func<int> import)
        {
            this.name = name;
            this.sources = sources;
            this.import = import;
        }
    }

    // 새 데이터 타입은 여기에 한 줄 추가
    static readonly Entry[] entries =
    {
        new Entry("CharacterData", new[] { "CharacterData.csv" }, () => LoadAll<CharacterData>(d => d.LoadData())),
        new Entry("EnemyData", new[] { "EnemyData.csv" }, () => LoadAll<EnemyData>(d => d.LoadData())),
        new Entry("StageData", new[] { "StageData.csv", "PhaseData.csv", "EventData.csv", "EnemyPatternData" }, () => LoadAll<StageData>(d => d.LoadData())),
        new Entry("SlimeData", new[] { "SlimeData.csv", "SlimeEnhance" }, () => LoadAll<SlimeData>(d => d.LoadData())),
        new Entry("ItemData", new[] { "ItemData.csv" }, () => LoadAll<ItemData>(d => d.LoadData())),
        new Entry("EquipmentData", new[] { "EquipmentData.csv" }, () => LoadAll<EquipmentData>(d => d.Edit())),
        new Entry("EnhanceExpInfo", new[] { "EnhanceLevelInfo.csv" }, () => LoadAll<EnhanceExpInfo>(d => d.LoadData())),
    };

    static string PrefKey(string name) => $"DataTableImporter:{Application.dataPath}:{name}";

    public static bool AutoImportOnPlay
    {
        get => EditorPrefs.GetBool(PrefKey("AutoImportOnPlay"), true);
        set => EditorPrefs.SetBool(PrefKey("AutoImportOnPlay"), value);
    }

    [MenuItem(MENU_ROOT + "Import All", priority = 0)]
    public static void ImportAll() => Import(entries, true);

    [MenuItem(MENU_ROOT + "Import Changed", priority = 1)]
    public static void ImportChanged() => Import(entries, false);

    [MenuItem(AUTO_IMPORT_MENU, priority = 20)]
    static void ToggleAutoImportOnPlay()
    {
        AutoImportOnPlay = !AutoImportOnPlay;
        Debug.Log($"[DataTableImporter] Play 시 자동 반영: {(AutoImportOnPlay ? "ON" : "OFF")}");
    }

    [MenuItem(AUTO_IMPORT_MENU, true)]
    static bool ToggleAutoImportOnPlayValidate()
    {
        Menu.SetChecked(AUTO_IMPORT_MENU, AutoImportOnPlay);
        return true;
    }

    [MenuItem(MENU_ROOT + "Import CharacterData", priority = 39)]
    static void ImportCharacterData() => ImportByName("CharacterData");

    [MenuItem(MENU_ROOT + "Import EnemyData", priority = 40)]
    static void ImportEnemyData() => ImportByName("EnemyData");

    [MenuItem(MENU_ROOT + "Import StageData", priority = 41)]
    static void ImportStageData() => ImportByName("StageData");

    [MenuItem(MENU_ROOT + "Import SlimeData", priority = 42)]
    static void ImportSlimeData() => ImportByName("SlimeData");

    [MenuItem(MENU_ROOT + "Import ItemData", priority = 43)]
    static void ImportItemData() => ImportByName("ItemData");

    static void ImportByName(string name) => Import(entries.Where(e => e.name == name), true);

    static void Import(IEnumerable<Entry> targets, bool force)
    {
        var results = new List<string>();
        foreach (Entry entry in targets)
        {
            if (!force && EditorPrefs.GetString(PrefKey(entry.name)) == GetStamp(entry))
                continue;

            try
            {
                int count = entry.import();
                // LoadData 중 임시 CSV가 생성될 수 있어서 반영 후에 다시 계산해서 저장
                EditorPrefs.SetString(PrefKey(entry.name), GetStamp(entry));
                results.Add($"{entry.name} {count}개");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError($"[DataTableImporter] {entry.name} 반영 실패 (다음 Import 때 다시 시도)");
            }
        }

        if (results.Count == 0) return;

        AssetDatabase.SaveAssets();
        Debug.Log($"[DataTableImporter] 데이터 반영 완료: {string.Join(", ", results)}");
    }

    // ASSET_ROOTS 아래 T 타입(자식 클래스 포함) SO 전부 LoadData
    static int LoadAll<T>(Action<T> load) where T : ScriptableObject
    {
        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", ASSET_ROOTS))
        {
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)) is T data)
            {
                load(data);
                count++;
            }
        }
        return count;
    }

    // 원본 테이블들의 수정 시각/크기를 이어 붙인 값. 이전 반영 때와 다르면 변경된 것으로 본다.
    static string GetStamp(Entry entry)
    {
        var sb = new StringBuilder();
        foreach (string source in entry.sources)
        {
            string fullPath = TableReader.GetFullPath(source);
            IEnumerable<string> files = Directory.Exists(fullPath)
                ? Directory.GetFiles(fullPath, "*.csv").OrderBy(f => f, StringComparer.Ordinal)
                : new[] { fullPath };

            foreach (string file in files)
            {
                var info = new FileInfo(file);
                sb.Append(source).Append('/').Append(info.Name).Append(':');
                sb.Append(info.Exists ? $"{info.LastWriteTimeUtc.Ticks}_{info.Length}" : "none").Append('|');
            }
        }
        return sb.ToString();
    }
}
