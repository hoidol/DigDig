using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

// SheetDownloadSetting에 등록된 구글 시트 탭을 TSV로 받아 CSV 파일로 저장한다.
// - 같은 탭은 한 번만 받고, range별로 잘라서 저장
// - 내용이 같으면 파일을 건드리지 않음 (불필요한 재반영 방지)
// - 저장 후 바뀐 테이블만 데이터 SO에 반영 (DataTableImporter.ImportChanged)
public static class SheetDownloader
{
    [MenuItem("Tools/Data/Download From Sheet", priority = 60)]
    static void DownloadFromMenu()
    {
        SheetDownloadSetting setting = FindSetting();
        if (setting == null)
        {
            Debug.LogWarning("[SheetDownloader] 설정 없음. Tools/Data/Sheet Download Setting 에서 먼저 설정하세요.");
            return;
        }
        Download(setting);
    }

    [MenuItem("Tools/Data/Sheet Download Setting", priority = 61)]
    static void SelectSetting()
    {
        SheetDownloadSetting setting = FindSetting() ?? CreateSetting();
        Selection.activeObject = setting;
        EditorGUIUtility.PingObject(setting);
    }

    // 다운로드 1건 (일반 항목 / 슬라임 전용 항목 공통)
    class Job
    {
        public string csvPath;
        public string sheetId;
        public string gid;
        public string range;
        public string blockKey; // range가 비어 있으면 1행에서 이 key의 블록을 찾는다
    }

    static List<Job> BuildJobs(SheetDownloadSetting setting, out int skipCount)
    {
        var jobs = new List<Job>();
        skipCount = 0;
        string defaultSheetId = setting.defaultSheetId?.Trim();

        foreach (SheetDownloadSetting.Entry entry in setting.entries)
        {
            string sheetId = string.IsNullOrWhiteSpace(entry.sheetId) ? defaultSheetId : entry.sheetId.Trim();
            if (!entry.enabled || string.IsNullOrWhiteSpace(entry.csvPath) || string.IsNullOrEmpty(sheetId) || string.IsNullOrWhiteSpace(entry.gid))
            {
                skipCount++;
                continue;
            }
            jobs.Add(new Job { csvPath = entry.csvPath.Trim(), sheetId = sheetId, gid = entry.gid.Trim(), range = entry.range });
        }

        if (string.IsNullOrEmpty(defaultSheetId))
            return jobs;

        if (!string.IsNullOrWhiteSpace(setting.slimeGid))
            jobs.Add(new Job { csvPath = SheetDownloadSetting.SLIME_CSV_PATH, sheetId = defaultSheetId, gid = setting.slimeGid.Trim(), range = setting.slimeRange });

        if (!string.IsNullOrWhiteSpace(setting.slimeEnhanceGid))
        {
            foreach (SheetDownloadSetting.SlimeEnhanceEntry entry in setting.slimeEnhanceEntries)
            {
                if (!entry.enabled || string.IsNullOrWhiteSpace(entry.key))
                {
                    skipCount++;
                    continue;
                }
                string key = entry.key.Trim();
                jobs.Add(new Job
                {
                    csvPath = $"{SheetDownloadSetting.SLIME_ENHANCE_CSV_FOLDER}{key}.csv",
                    sheetId = defaultSheetId,
                    gid = setting.slimeEnhanceGid.Trim(),
                    range = entry.range,
                    blockKey = key,
                });
            }
        }
        return jobs;
    }

    public static void Download(SheetDownloadSetting setting)
    {
        var tabCache = new Dictionary<string, List<string[]>>();
        var changed = new List<string>();
        var failed = new List<string>();
        int sameCount = 0;
        List<Job> jobs = BuildJobs(setting, out int skipCount);

        using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
        {
            try
            {
                for (int i = 0; i < jobs.Count; i++)
                {
                    Job entry = jobs[i];
                    if (EditorUtility.DisplayCancelableProgressBar("Download From Sheet", entry.csvPath, (float)i / jobs.Count))
                        break;

                    try
                    {
                        string tabKey = $"{entry.sheetId}#{entry.gid}";
                        if (!tabCache.TryGetValue(tabKey, out List<string[]> grid))
                            tabCache[tabKey] = grid = ParseTsv(Fetch(client, entry.sheetId, entry.gid));

                        string range = string.IsNullOrWhiteSpace(entry.range) && entry.blockKey != null
                            ? FindBlockRange(grid, entry.blockKey)
                            : entry.range;
                        string content = ToTsv(Crop(grid, range));
                        string fullPath = Path.Combine(Application.dataPath, entry.csvPath);
                        if (File.Exists(fullPath) && File.ReadAllText(fullPath, Encoding.UTF8) == content)
                        {
                            sameCount++;
                            continue;
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                        File.WriteAllText(fullPath, content, new UTF8Encoding(false));
                        changed.Add(entry.csvPath);
                    }
                    catch (Exception e)
                    {
                        failed.Add(entry.csvPath);
                        Debug.LogError($"[SheetDownloader] {entry.csvPath} 실패: {e.Message}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        if (changed.Count > 0)
        {
            AssetDatabase.Refresh();
            DataTableImporter.ImportChanged();
        }

        string changedList = changed.Count > 0 ? $"\n- {string.Join("\n- ", changed)}" : "";
        Debug.Log($"[SheetDownloader] 변경 {changed.Count}개, 동일 {sameCount}개, 실패 {failed.Count}개, 건너뜀(미설정/비활성) {skipCount}개{changedList}");
    }

    static string Fetch(HttpClient client, string sheetId, string gid)
    {
        string url = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=tsv&gid={gid}";
        // 에디터 메인 스레드 SynchronizationContext 데드락 방지를 위해 Task.Run 안에서 대기
        return Task.Run(async () =>
        {
            using (HttpResponseMessage response = await client.GetAsync(url))
            {
                string mediaType = response.Content.Headers.ContentType?.MediaType ?? "";
                // 비공개 시트는 로그인 HTML 페이지가 내려온다
                if (!response.IsSuccessStatusCode || mediaType.Contains("html"))
                    throw new Exception($"다운로드 실패 ({(int)response.StatusCode}). 시트 공유 설정(링크가 있는 모든 사용자: 뷰어)과 sheetId/gid를 확인하세요.");

                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                return Encoding.UTF8.GetString(bytes);
            }
        }).GetAwaiter().GetResult();
    }

    static List<string[]> ParseTsv(string tsv)
    {
        return tsv.Replace("\r\n", "\n").Split('\n').Select(line => line.Split('\t')).ToList();
    }

    static string ToTsv(List<string[]> rows)
    {
        var sb = new StringBuilder();
        foreach (string[] row in rows)
            sb.Append(string.Join("\t", row)).Append('\n');
        return sb.ToString();
    }

    // range(A1 표기) 영역만 잘라낸다. 빈 행(시트에서 구분용으로 띄운 줄 포함)은 모두 제거.
    static List<string[]> Crop(List<string[]> grid, string range)
    {
        var result = new List<string[]>();
        if (string.IsNullOrWhiteSpace(range))
        {
            result.AddRange(grid);
        }
        else
        {
            if (!TryParseRange(range, out int row0, out int col0, out int row1, out int col1))
                throw new Exception($"range 형식 오류: '{range}' (ex. A1:H20, A:H, A3:H)");

            int lastRow = row1 < 0 ? grid.Count - 1 : Math.Min(row1, grid.Count - 1);
            for (int r = row0; r <= lastRow; r++)
            {
                string[] cells = new string[col1 - col0 + 1];
                for (int c = col0; c <= col1; c++)
                    cells[c - col0] = c < grid[r].Length ? grid[r][c] : "";
                result.Add(cells);
            }
        }

        result.RemoveAll(row => row.All(string.IsNullOrWhiteSpace));
        return result;
    }

    // 1행에서 key와 같은 칸을 찾아, 2행(헤더)이 이어지는 열까지를 한 블록으로 본다. → "F2:J" (행은 끝까지, 빈 행은 Crop에서 제거)
    static string FindBlockRange(List<string[]> grid, string key)
    {
        if (grid.Count < 2)
            throw new Exception("블록 탐색 실패: 탭에 1행(key), 2행(헤더)이 필요합니다.");

        int startCol = Array.FindIndex(grid[0], cell => cell.Trim() == key);
        if (startCol < 0)
            throw new Exception($"1행에서 '{key}' 블록을 찾지 못함");

        string[] headerRow = grid[1];
        int endCol = startCol;
        while (endCol + 1 < headerRow.Length && !string.IsNullOrWhiteSpace(headerRow[endCol + 1]))
            endCol++;

        return $"{ToColumnName(startCol)}2:{ToColumnName(endCol)}";
    }

    // 0 → A, 25 → Z, 26 → AA
    static string ToColumnName(int col)
    {
        string name = "";
        for (col++; col > 0; col = (col - 1) / 26)
            name = (char)('A' + (col - 1) % 26) + name;
        return name;
    }

    // "A1:H20" / "A:H" / "A3:H" → 0 기반 행/열. 끝 행이 없으면 row1 = -1 (끝까지)
    static bool TryParseRange(string range, out int row0, out int col0, out int row1, out int col1)
    {
        row0 = col0 = row1 = col1 = 0;
        string[] parts = range.Trim().ToUpperInvariant().Split(':');
        if (parts.Length != 2) return false;
        if (!TryParseCell(parts[0], out row0, out col0) || !TryParseCell(parts[1], out row1, out col1)) return false;
        if (row0 < 0) row0 = 0;
        return col1 >= col0 && (row1 < 0 || row1 >= row0);
    }

    // "H20" → (19, 7), "H" → (-1, 7)
    static bool TryParseCell(string cell, out int row, out int col)
    {
        row = -1;
        col = -1;
        int i = 0;
        while (i < cell.Length && cell[i] >= 'A' && cell[i] <= 'Z')
        {
            col = (col + 1) * 26 + (cell[i] - 'A');
            i++;
        }
        if (i == 0) return false;
        if (i == cell.Length) return true;
        if (!int.TryParse(cell.Substring(i), out int rowNumber) || rowNumber < 1) return false;
        row = rowNumber - 1;
        return true;
    }

    static SheetDownloadSetting FindSetting()
    {
        string guid = AssetDatabase.FindAssets($"t:{nameof(SheetDownloadSetting)}").FirstOrDefault();
        return guid == null ? null : AssetDatabase.LoadAssetAtPath<SheetDownloadSetting>(AssetDatabase.GUIDToAssetPath(guid));
    }

    // Assets/Json, Assets/Resources/Json 아래 CSV 경로를 미리 채워서 생성 (sheetId/gid만 입력하면 되게)
    // 슬라임 CSV는 슬라임 전용 항목으로 등록
    static SheetDownloadSetting CreateSetting()
    {
        var setting = ScriptableObject.CreateInstance<SheetDownloadSetting>();
        string[] csvPaths = new[] { TableReader.ROOT_FOLDER, "Resources/Json" }
            .Select(folder => Path.Combine(Application.dataPath, folder))
            .Where(Directory.Exists)
            .SelectMany(folder => Directory.GetFiles(folder, "*.csv", SearchOption.AllDirectories))
            .Select(f => f.Substring(Application.dataPath.Length + 1).Replace('\\', '/'))
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToArray();

        setting.entries = csvPaths
            .Where(p => p != SheetDownloadSetting.SLIME_CSV_PATH && !p.StartsWith(SheetDownloadSetting.SLIME_ENHANCE_CSV_FOLDER))
            .Select(p => new SheetDownloadSetting.Entry { csvPath = p })
            .ToArray();
        setting.slimeEnhanceEntries = csvPaths
            .Where(p => p.StartsWith(SheetDownloadSetting.SLIME_ENHANCE_CSV_FOLDER))
            .Select(p => Path.GetFileNameWithoutExtension(p))
            .Where(key => key.Length > 0)
            .Select(key => new SheetDownloadSetting.SlimeEnhanceEntry { key = key })
            .ToArray();

        AssetDatabase.CreateAsset(setting, SheetDownloadSetting.DEFAULT_ASSET_PATH);
        AssetDatabase.SaveAssets();
        Debug.Log($"[SheetDownloader] 설정 생성: {SheetDownloadSetting.DEFAULT_ASSET_PATH} ({setting.entries.Length}개 CSV 경로 등록)");
        return setting;
    }
}
