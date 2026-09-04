using UnityEngine;


[CreateAssetMenu]
public class SlimeData : ScriptableObject
{
    public static readonly int MAX_COUNT = 3;
    public string key;
    public string Title => TranslateManager.GetText(key);
    public string desc;
    public int growth;

    public string valueInfo;
    public Slime prefab;
    public ConditionData[] unlockConditions; // 추가 효과 해금 조건 (모두 충족해야 효과 활성화)    

    public Sprite thum;
    public Color color;
    public GradeType grade;

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

    public static SlimeData GetSlimeData(string key)
    {
        return SlimeManager.Instance.GetSlimeData(key);
    }


#if UNITY_EDITOR
    public void LoadData()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "Json/SlimeData.csv");
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"[SlimeData] CSV 파일 없음: {path}");
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
        int iGrade = System.Array.IndexOf(headers, "grade");

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
            if (iGrade >= 0 && iGrade < cols.Length && System.Enum.TryParse(cols[iGrade].Trim(), out GradeType parsedGrade))
                grade = parsedGrade;

            prefab = FindAssetByName<Slime>($"Assets/3.Prefabs/Slime/Growth{growth}", $"{key}Slime");
            thum = FindAssetByName<Sprite>($"Assets/2.Sprites/Slime/Growth{growth}", key);

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[SlimeData] {key} LoadData 완료");
            return;
        }

        Debug.LogWarning($"[SlimeData] CSV에서 '{key}' 를 찾지 못함");
    }

    // folder 안에서 파일명이 fileName과 일치하는 T 타입 에셋을 찾는다.
    T FindAssetByName<T>(string folder, string fileName) where T : UnityEngine.Object
    {
        if (!UnityEditor.AssetDatabase.IsValidFolder(folder))
        {
            Debug.LogWarning($"[SlimeData] 폴더 없음: {folder}");
            return null;
        }

        string typeFilter = typeof(T) == typeof(Slime) ? "t:Prefab" : "t:Sprite";
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"{fileName} {typeFilter}", new[] { folder });
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) != fileName) continue;

            T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
        }

        Debug.LogWarning($"[SlimeData] {folder} 에서 '{fileName}' 를 찾지 못함");
        return null;
    }

    // CSV의 childSlime 컬럼은 Key가 아닌 표시 이름(Name)으로 적혀 있어 역으로 Key를 찾는다.
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
        Debug.LogWarning($"[SlimeData] childSlime 이름 '{name}' 에 해당하는 Key를 찾지 못함");
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

        string prefabRootFolder = $"Assets/3.Prefabs/Slime/Growth{growth}";
        string prefabPath = $"{prefabRootFolder}/{key}Slime.prefab";

        // 이미 해당 경로에 프리팹이 있으면 그대로 연결만 하고 덮어쓰지 않는다.
        Slime existingPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<Slime>(prefabPath);
        if (existingPrefab != null)
        {
            prefab = existingPrefab;
            prefab.key = key;
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[SlimeData] {key} 기존 prefab 연결 완료: {prefabPath}");
            return;
        }

        // 프리팹 없으면 grade 폴더에 새로 생성
        if (!UnityEditor.AssetDatabase.IsValidFolder(prefabRootFolder))
            System.IO.Directory.CreateDirectory(prefabRootFolder);

        var go = new GameObject(key + "Slime");

        // key+"Slime" 이름의 타입이 존재하면 컴포넌트 추가, 없으면 스크립트 파일 생성
        string componentTypeName = key + "Slime";
        System.Type slimeType = System.Type.GetType(componentTypeName);
        if (slimeType != null && typeof(Slime).IsAssignableFrom(slimeType))
        {
            go.AddComponent(slimeType);
            Debug.Log($"[SlimeData] {componentTypeName} 컴포넌트 추가됨");
        }
        else
        {
            string scriptPath = CreateSlimeScript(componentTypeName);
            Debug.LogWarning($"[SlimeData] {componentTypeName} 타입 없음 → 스크립트 생성: {scriptPath}\n컴파일 완료 후 Edit을 다시 눌러주세요.");
            DestroyImmediate(go);
            UnityEditor.AssetDatabase.Refresh();
            return;
        }

        var newPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        DestroyImmediate(go);

        prefab = newPrefab.GetComponent<Slime>();
        prefab.key = key;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[SlimeData] {key} 새 prefab 생성 완료: {prefabRootFolder}");
    }

    string CreateSlimeScript(string className)
    {
        string scriptFolder = $"Assets/1.Scripts/InGame/Character/Slime/Growth{growth}";
        string filePath = $"{scriptFolder}/{className}.cs";

        if (System.IO.File.Exists(filePath))
            return filePath;

        string content =
$@"using UnityEngine;

public class {className} : Slime
{{

}}
";
        System.IO.File.WriteAllText(filePath, content);
        return filePath;
    }
#endif
}