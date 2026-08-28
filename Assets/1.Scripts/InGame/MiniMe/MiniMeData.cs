using UnityEngine;


[CreateAssetMenu]
public class MiniMeData : ScriptableObject
{
    public static readonly int MAX_COUNT = 3;
    public string key;
    public string Title => TranslateManager.GetText(key);
    public string desc;
    public int growth;

    public string valueInfo;
    public MiniMe prefab;
    public ConditionData[] unlockConditions; // 추가 효과 해금 조건 (모두 충족해야 효과 활성화)    

    public Sprite thum;
    public Color color;
    public Grade grade;

    public bool CheckUnlock()
    {
        bool unlocked = true;
        if (unlockConditions == null)
            return unlocked;

        for (int i = 0; i < unlockConditions.Length; i++)
        {
            if (!unlockConditions[i].Check())
            {
                unlocked = false;
            }
        }
        return unlocked;
    }

    public string GetDescription()
    {
        return prefab.GetDescription();
    }

    public static MiniMeData GetMiniMeData(string key)
    {
        return MiniMeManager.Instance.GetMiniMeData(key);
    }


#if UNITY_EDITOR
    public void LoadData()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "Json/MiniMeData.csv");
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"[MiniMeData] CSV 파일 없음: {path}");
            return;
        }

        string[] lines = System.IO.File.ReadAllLines(path, System.Text.Encoding.UTF8);
        if (lines.Length < 2) return;

        string[] headers = ParseCsvLine(lines[0]);
        for (int i = 0; i < headers.Length; i++)
            headers[i] = headers[i].Trim();

        int iKey = System.Array.IndexOf(headers, "key");
        int iDesc = System.Array.IndexOf(headers, "desc");
        int iGrowth = System.Array.IndexOf(headers, "growth");
        int iColor = System.Array.IndexOf(headers, "color");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = ParseCsvLine(lines[i]);

            string rowKey = iKey >= 0 && iKey < cols.Length ? cols[iKey].Trim() : "";
            if (rowKey != key) continue;

            if (iDesc >= 0 && iDesc < cols.Length)
                desc = cols[iDesc].Trim();
            if (iGrowth >= 0 && iGrowth < cols.Length && int.TryParse(cols[iGrowth].Trim(), out int lv))
                growth = lv;
            if (iColor >= 0 && iColor < cols.Length && ColorUtility.TryParseHtmlString(cols[iColor].Trim(), out Color parsedColor))
                color = parsedColor;

            string thumPath = $"Assets/2.Sprites/MiniMe/Thum/Growth{growth}.png";
            thum = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(thumPath);
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[MiniMeData] {key} LoadData 완료");
            return;
        }

        //


        Debug.LogWarning($"[MiniMeData] CSV에서 '{key}' 를 찾지 못함");
    }

    // CSV의 childMiniMe 컬럼은 Key가 아닌 표시 이름(Name)으로 적혀 있어 역으로 Key를 찾는다.
    static string ResolveKeyByName(string[] lines, int iKey, int iName, string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = ParseCsvLine(lines[i]);
            if (iName < 0 || iName >= cols.Length) continue;
            if (cols[iName].Trim() != name) continue;
            return iKey >= 0 && iKey < cols.Length ? cols[iKey].Trim() : "";
        }
        Debug.LogWarning($"[MiniMeData] childMiniMe 이름 '{name}' 에 해당하는 Key를 찾지 못함");
        return name;
    }

    static string[] ParseCsvLine(string line)
    {
        var result = new System.Collections.Generic.List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"') { inQuotes = !inQuotes; continue; }
            if (c == ',' && !inQuotes) { result.Add(current.ToString()); current.Clear(); continue; }
            current.Append(c);
        }
        result.Add(current.ToString());
        return result.ToArray();
    }

    public void Edit()
    {
        if (string.IsNullOrEmpty(key) || key != name)
            key = name;

        string prefabRootFolder = $"Assets/3.Prefabs/MiniMe/Growth{growth}";

        // 기존 프리팹 탐색 (grade 폴더 → 루트 폴더 순)
        string[] searchFolders = new[] { prefabRootFolder };
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"{key}MiniMe t:Prefab", searchFolders);
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) != key) continue;

            MiniMe prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<MiniMe>(path);
            if (prefab != null)
            {
                this.prefab = prefab;
                this.prefab.key = key;
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"[MiniMeData] {key}MiniMe prefab 연결 완료: {path}");
                return;
            }
        }

        // 프리팹 없으면 grade 폴더에 새로 생성
        if (!UnityEditor.AssetDatabase.IsValidFolder(prefabRootFolder))
            System.IO.Directory.CreateDirectory(prefabRootFolder);

        var go = new GameObject(key);

        // key+"MiniMe" 이름의 타입이 존재하면 컴포넌트 추가, 없으면 스크립트 파일 생성
        string componentTypeName = key + "MiniMe";
        System.Type miniMeType = System.Type.GetType(componentTypeName);
        if (miniMeType != null && typeof(MiniMe).IsAssignableFrom(miniMeType))
        {
            go.AddComponent(miniMeType);
            Debug.Log($"[MiniMeData] {componentTypeName} 컴포넌트 추가됨");
        }
        else
        {
            string scriptPath = CreateMiniMeScript(componentTypeName);
            Debug.LogWarning($"[MiniMeData] {componentTypeName} 타입 없음 → 스크립트 생성: {scriptPath}\n컴파일 완료 후 Edit을 다시 눌러주세요.");
            DestroyImmediate(go);
            UnityEditor.AssetDatabase.Refresh();
            return;
        }

        string newPath = $"{prefabRootFolder}/{key}.prefab";
        var newPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, newPath);
        DestroyImmediate(go);

        prefab = newPrefab.GetComponent<MiniMe>();
        prefab.key = key;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[MiniMeData] {key} 새 prefab 생성 완료: {prefabRootFolder}");
    }

    string CreateMiniMeScript(string className)
    {
        string scriptFolder = $"Assets/1.Scripts/InGame/Character/MiniMe/Growth{growth}";
        string filePath = $"{scriptFolder}/{className}.cs";

        if (System.IO.File.Exists(filePath))
            return filePath;

        string content =
$@"using UnityEngine;

public class {className} : MiniMe
{{

}}
";
        System.IO.File.WriteAllText(filePath, content);
        return filePath;
    }
#endif
}