using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

// 슬라임 유형 태그 (SlimeData.csv Tags → SlimeData.tags, CLAUDE.md "유물")
public class SlimeTagTest
{
    const int MIN_SLIMES_PER_TAG = 3; // 유물이 반응할 만한 최소 종 수

    static Dictionary<string, string> CsvTags()
    {
        TableReader table = TableReader.Load("SlimeData.csv");
        Assert.IsNotNull(table, "SlimeData.csv 없음");
        Assert.IsTrue(table.HasColumn("Tags"), "SlimeData.csv에 Tags 열 없음 (시트 L열 + 다운로드 범위 A:L)");
        return table.rows.Where(r => r.Get("Key") != "").ToDictionary(r => r.Get("Key"), r => r.Get("Tags"));
    }

    [Test]
    public void Parse_SplitsByBar_AndReportsUnknown()
    {
        Assert.AreEqual(SlimeTag.Shooter | SlimeTag.Status, SlimeTags.Parse(" Shooter | Status ", out string error));
        Assert.IsNull(error);
        Assert.AreEqual(SlimeTag.None, SlimeTags.Parse("", out error));
        Assert.AreEqual(SlimeTag.Zone, SlimeTags.Parse("Zone|Typo", out error));
        Assert.AreEqual("Typo", error);
        Assert.AreEqual("Shooter|Status", SlimeTags.ToCsv(SlimeTag.Shooter | SlimeTag.Status));
    }

    [Test]
    public void CountMatching_CountsSlimesHavingTag()
    {
        var equipped = new[] { SlimeTag.Shooter | SlimeTag.Status, SlimeTag.Zone | SlimeTag.Status, SlimeTag.Support };
        Assert.AreEqual(2, SlimeTags.CountMatching(equipped, SlimeTag.Status));
        Assert.AreEqual(1, SlimeTags.CountMatching(equipped, SlimeTag.Shooter));
        Assert.AreEqual(0, SlimeTags.CountMatching(equipped, SlimeTag.Heavy));
        Assert.AreEqual(0, SlimeTags.CountMatching(equipped, SlimeTag.None));
    }

    // 유물 matchTags: 태그 중 하나라도 가진 슬라임 수 (유물 정보 창 "해당 슬라임 N/5")
    [Test]
    public void Relic_CountMatchingSlimes_AnyTag()
    {
        var equipped = new[] { SlimeTag.Zone | SlimeTag.Heavy, SlimeTag.Shooter | SlimeTag.Heavy, SlimeTag.Zone, SlimeTag.Support, SlimeTag.Orbit | SlimeTag.Status };
        Assert.AreEqual(4, RelicData.CountMatchingSlimes(SlimeTag.Zone | SlimeTag.Heavy | SlimeTag.Status, equipped));
        Assert.AreEqual(2, RelicData.CountMatchingSlimes(SlimeTag.Heavy, equipped));
        Assert.AreEqual(0, RelicData.CountMatchingSlimes(SlimeTag.None, equipped), "matchTags 없으면 0");
    }

    // 모든 슬라임: 아는 태그만, 형태는 정확히 하나
    [Test]
    public void Csv_EverySlimeHasOneForm()
    {
        var problems = new List<string>();
        foreach (var (key, raw) in CsvTags())
        {
            SlimeTag tags = SlimeTags.Parse(raw, out string error);
            if (error != null)
                problems.Add($"{key}: 모르는 태그 {error}");
            if (!SlimeTags.HasSingleForm(tags))
                problems.Add($"{key}: 형태가 하나가 아님 ({raw})");
        }
        Assert.IsEmpty(problems, string.Join("\n", problems));
    }

    // 형태·특성마다 해당 슬라임이 최소 3종 (유물이 쓸모 있으려면)
    [Test]
    public void Csv_EveryTagHasEnoughSlimes()
    {
        var all = CsvTags().Values.Select(raw => SlimeTags.Parse(raw, out _)).ToList();
        var problems = SlimeTags.Forms.Concat(SlimeTags.Traits)
            .Select(tag => (tag, count: SlimeTags.CountMatching(all, tag)))
            .Where(x => x.count < MIN_SLIMES_PER_TAG)
            .Select(x => $"{x.tag}: {x.count}종")
            .ToList();
        Assert.IsEmpty(problems, $"{MIN_SLIMES_PER_TAG}종 미만인 태그\n{string.Join("\n", problems)}");
    }

    // 에셋에 CSV 태그가 반영됐는지 (Tools/Data/Import SlimeData, Play 시 자동)
    [Test]
    public void Assets_MatchCsv()
    {
        Dictionary<string, string> csv = CsvTags();
        var problems = new List<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:SlimeData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<SlimeData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data == null || !csv.TryGetValue(data.key, out string raw))
                continue;
            SlimeTag expected = SlimeTags.Parse(raw, out _);
            if (data.tags != expected)
                problems.Add($"{data.key}: 에셋 {SlimeTags.ToCsv(data.tags)} / CSV {raw}");
        }
        Assert.IsEmpty(problems, "에셋 태그가 CSV와 다름 (Import SlimeData 필요)\n" + string.Join("\n", problems));
    }
}
