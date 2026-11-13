using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// Json/Translate.csv → TranslateDataTable 반영 + 코드에서 쓰는 key가 시트에 있는지 검사
// 시트를 다시 내려받았을 때 행이 빠지면 화면에 "*key"로 나오므로 여기서 잡는다
public class TranslateDataTest
{
    TranslateDataTable table;
    HashSet<string> keys;

    [SetUp]
    public void SetUp()
    {
        table = ScriptableObject.CreateInstance<TranslateDataTable>();
        table.LoadData();
        keys = new HashSet<string>(table.rows.Select(r => r.key));
    }

    [TearDown]
    public void TearDown() => UnityEngine.Object.DestroyImmediate(table);

    [Test]
    public void LoadsRows()
    {
        Assert.IsNotEmpty(table.rows, "Translate.csv에서 읽은 행 없음");
        Assert.IsTrue(table.rows.All(r => !string.IsNullOrEmpty(r.kr) || !string.IsNullOrEmpty(r.en)), "Kr/En 모두 빈 행");
    }

    // 코드의 TranslateManager.GetText("key") 리터럴 (주석 줄·이전 아이템 폴더 제외)
    [Test]
    public void CodeLiteralKeys_Exist()
    {
        var regex = new Regex("TranslateManager\\.GetText\\(\"([^\"]+)\"\\)");
        var missing = new SortedSet<string>();
        foreach (string file in Directory.GetFiles(Path.Combine(Application.dataPath, "1.Scripts"), "*.cs", SearchOption.AllDirectories))
        {
            string path = file.Replace('\\', '/');
            if (path.Contains("/Item/Old/") || path.Contains("/Item/Using/") || path.Contains("/LevelUpData/"))
                continue;
            foreach (string line in File.ReadAllLines(file))
            {
                if (line.TrimStart().StartsWith("//"))
                    continue;
                foreach (Match m in regex.Matches(line))
                    if (!keys.Contains(m.Groups[1].Value))
                        missing.Add($"{m.Groups[1].Value} ({Path.GetFileName(file)})");
            }
        }
        Assert.IsEmpty(missing, "Translate.csv에 없는 key:\n" + string.Join("\n", missing));
    }

    // 코드에서 조합하는 key: 난이도·스탯 이름·슬라임 이름
    [Test]
    public void ComposedKeys_Exist()
    {
        var expected = new List<string>();
        expected.AddRange(Enum.GetValues(typeof(DifficultyType)).Cast<DifficultyType>().Select(d => $"Difficulty.{d}"));
        expected.AddRange(Enum.GetValues(typeof(StatType)).Cast<StatType>().Where(s => s != StatType.Count).Select(s => $"{s}_title"));
        expected.AddRange(Enum.GetValues(typeof(GradeType)).Cast<GradeType>().Where(g => Grade.GetGradeText(g) != null).Select(g => $"Grade.{g}"));
        foreach (string guid in AssetDatabase.FindAssets("t:MinionData", new[] { "Assets/AddressableResources/MinionData" }))
        {
            var data = AssetDatabase.LoadAssetAtPath<MinionData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data != null && !string.IsNullOrEmpty(data.key) && data.growth == 1)
                expected.Add($"Minion.{data.key}");
        }

        var missing = expected.Distinct().Where(k => !keys.Contains(k)).ToList();
        Assert.IsEmpty(missing, "Translate.csv에 없는 key:\n" + string.Join("\n", missing));
    }
}
