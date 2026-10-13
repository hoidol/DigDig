#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

// Assets/Json 아래 CSV/TSV 테이블을 읽는다. (에디터 전용 - 데이터 SO의 LoadData에서 사용)
// 데이터 클래스(런타임 어셈블리)에서 참조해야 하므로 Editor 폴더가 아닌 곳에 두고 #if UNITY_EDITOR로 감싼다.
// - 헤더 줄에 탭이 있으면 탭, 없으면 쉼표로 구분한다. "..." 로 감싼 칸 안의 구분자는 무시한다.
// - 같은 파일은 수정 시각이 바뀌기 전까지 캐시해서, Import All 때 에셋마다 다시 읽지 않는다.
public class TableReader
{
    public const string ROOT_FOLDER = "Json";

    public readonly string path;
    public readonly string[] headers;
    public readonly List<Row> rows = new List<Row>();
    // 컬럼명은 대소문자 구분 안 함 (시트의 "Key"와 코드의 "key"를 같은 컬럼으로 봄)
    readonly Dictionary<string, int> headerIndexDic = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    static readonly Dictionary<string, (DateTime writeTime, TableReader table)> cacheDic = new Dictionary<string, (DateTime, TableReader)>();

    // relativePath: Assets/Json 기준 경로 (ex. "EnemyData.csv", "SlimeEnhance/Flame.csv")
    public static string GetFullPath(string relativePath) => Path.Combine(Application.dataPath, ROOT_FOLDER, relativePath);

    // 파일이 없으면 경고 후 null 반환
    public static TableReader Load(string relativePath)
    {
        string fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"[TableReader] 파일 없음: {fullPath}");
            return null;
        }

        DateTime writeTime = File.GetLastWriteTimeUtc(fullPath);
        if (cacheDic.TryGetValue(fullPath, out var cached) && cached.writeTime == writeTime)
            return cached.table;

        var table = new TableReader(relativePath, File.ReadAllLines(fullPath, Encoding.UTF8));
        cacheDic[fullPath] = (writeTime, table);
        return table;
    }

    TableReader(string path, string[] lines)
    {
        this.path = path;

        // 시트에서 범위를 넉넉히 받아 앞쪽에 빈 행이 있을 수 있으므로, 첫 번째로 내용이 있는 행을 헤더로 쓴다
        int headerLine = Array.FindIndex(lines, line => !string.IsNullOrWhiteSpace(line));
        if (headerLine < 0)
        {
            headers = Array.Empty<string>();
            return;
        }

        char delimiter = lines[headerLine].IndexOf('\t') >= 0 ? '\t' : ',';
        headers = Split(lines[headerLine], delimiter);
        for (int i = 0; i < headers.Length; i++)
        {
            headers[i] = headers[i].Trim();
            if (!headerIndexDic.ContainsKey(headers[i]))
                headerIndexDic[headers[i]] = i;
        }

        for (int i = headerLine + 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            rows.Add(new Row(this, Split(lines[i], delimiter), i + 1));
        }
    }

    public bool HasColumn(string column) => headerIndexDic.ContainsKey(column);

    public int GetColumnIndex(string column) => headerIndexDic.TryGetValue(column, out int index) ? index : -1;

    // column 값이 value와 같은 첫 행 (없으면 null)
    public Row Find(string column, string value)
    {
        int index = GetColumnIndex(column);
        foreach (Row row in rows)
        {
            if (row.Get(index) == value)
                return row;
        }
        return null;
    }

    static string[] Split(string line, char delimiter)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new StringBuilder();
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"') { inQuotes = !inQuotes; continue; }
            if (c == delimiter && !inQuotes) { result.Add(current.ToString()); current.Clear(); continue; }
            current.Append(c);
        }
        result.Add(current.ToString());
        return result.ToArray();
    }

    public class Row
    {
        readonly TableReader table;
        readonly string[] cols;
        public readonly int lineNumber; // 파일 기준 줄 번호 (로그용)

        public Row(TableReader table, string[] cols, int lineNumber)
        {
            this.table = table;
            this.cols = cols;
            this.lineNumber = lineNumber;
        }

        // 컬럼이 없거나 칸이 비어 있으면 "" 반환
        public string Get(string column) => Get(table.GetColumnIndex(column));
        public string Get(int index) => index >= 0 && index < cols.Length ? cols[index].Trim() : "";

        public bool TryGetInt(string column, out int value)
            => int.TryParse(Get(column), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        public bool TryGetFloat(string column, out float value)
            => float.TryParse(Get(column), NumberStyles.Float, CultureInfo.InvariantCulture, out value);

        public bool TryGetEnum<T>(string column, out T value) where T : struct, Enum
            => Enum.TryParse(Get(column), out value);
    }
}
#endif
