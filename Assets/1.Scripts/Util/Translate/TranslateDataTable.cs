using System.Collections.Generic;
using UnityEngine;

// 번역 데이터. Json/Translate.csv(구글 시트, Key / Kr / En) 한 행 = 문구 하나
// DataTableImporter가 CSV → Resources/TranslateData.asset, TranslateManager가 처음 GetText 할 때 불러옴
public class TranslateDataTable : ScriptableObject
{
    public const string RESOURCE_PATH = "TranslateData";

    [System.Serializable]
    public class Row
    {
        public string key;
        public string kr;
        public string en;
    }

    public List<Row> rows = new List<Row>();

#if UNITY_EDITOR
    const string ASSET_PATH = "Assets/Resources/" + RESOURCE_PATH + ".asset";

    // 에셋이 없으면 만든 뒤 CSV 반영 (DataTableImporter에서 호출)
    public static int ImportAsset()
    {
        var data = UnityEditor.AssetDatabase.LoadAssetAtPath<TranslateDataTable>(ASSET_PATH);
        if (data == null)
        {
            data = CreateInstance<TranslateDataTable>();
            UnityEditor.AssetDatabase.CreateAsset(data, ASSET_PATH);
        }
        data.LoadData();
        return 1;
    }

    public void LoadData()
    {
        TableReader table = TableReader.Load("Translate.csv");
        if (table == null) return;

        var list = new List<Row>();
        var keys = new HashSet<string>();
        foreach (TableReader.Row row in table.rows)
        {
            string key = row.Get("Key");
            if (string.IsNullOrEmpty(key))
                continue;
            if (!keys.Add(key))
            {
                Debug.LogWarning($"[TranslateDataTable] {row.lineNumber}줄 중복 key 무시: {key}");
                continue;
            }
            list.Add(new Row { key = key, kr = row.Get("Kr"), en = row.Get("En") });
        }
        rows = list;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[TranslateDataTable] LoadData 완료 ({list.Count}개)");
    }
#endif
}
