using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

// 슬라임 유형 태그 (MinionData.csv Tags → MinionData.tags, CLAUDE.md "권능")
public class MinionTagTest
{
    const int MIN_MINIONS_PER_TAG = 3; // 권능이 반응할 만한 최소 종 수

    static Dictionary<string, string> CsvTags()
    {
        TableReader table = TableReader.Load("MinionData.csv");
        Assert.IsNotNull(table, "MinionData.csv 없음");
        Assert.IsTrue(table.HasColumn("Tags"), "MinionData.csv에 Tags 열 없음 (시트 L열 + 다운로드 범위 A:L)");
        return table.rows.Where(r => r.Get("Key") != "").ToDictionary(r => r.Get("Key"), r => r.Get("Tags"));
    }

    [Test]
    public void Parse_SplitsByBar_AndReportsUnknown()
    {
        Assert.AreEqual(MinionTag.Shooter | MinionTag.Status, MinionTags.Parse(" Shooter | Status ", out string error));
        Assert.IsNull(error);
        Assert.AreEqual(MinionTag.None, MinionTags.Parse("", out error));
        Assert.AreEqual(MinionTag.Zone, MinionTags.Parse("Zone|Typo", out error));
        Assert.AreEqual("Typo", error);
        Assert.AreEqual("Shooter|Status", MinionTags.ToCsv(MinionTag.Shooter | MinionTag.Status));
    }

    [Test]
    public void CountMatching_CountsMinionsHavingTag()
    {
        var equipped = new[] { MinionTag.Shooter | MinionTag.Status, MinionTag.Zone | MinionTag.Status, MinionTag.Support };
        Assert.AreEqual(2, MinionTags.CountMatching(equipped, MinionTag.Status));
        Assert.AreEqual(1, MinionTags.CountMatching(equipped, MinionTag.Shooter));
        Assert.AreEqual(0, MinionTags.CountMatching(equipped, MinionTag.Heavy));
        Assert.AreEqual(0, MinionTags.CountMatching(equipped, MinionTag.None));
    }

    // 권능 matchTags: 태그 중 하나라도 가진 슬라임 수 (권능 정보 창 "해당 슬라임 N/5")
    [Test]
    public void Authority_CountMatchingMinions_AnyTag()
    {
        var equipped = new[] { MinionTag.Zone | MinionTag.Heavy, MinionTag.Shooter | MinionTag.Heavy, MinionTag.Zone, MinionTag.Support, MinionTag.Orbit | MinionTag.Status };
        Assert.AreEqual(4, AuthorityData.CountMatchingMinions(MinionTag.Zone | MinionTag.Heavy | MinionTag.Status, equipped));
        Assert.AreEqual(2, AuthorityData.CountMatchingMinions(MinionTag.Heavy, equipped));
        Assert.AreEqual(0, AuthorityData.CountMatchingMinions(MinionTag.None, equipped), "matchTags 없으면 0");
    }

    // 모든 슬라임: 아는 태그만, 형태는 정확히 하나
    [Test]
    public void Csv_EveryMinionHasOneForm()
    {
        var problems = new List<string>();
        foreach (var (key, raw) in CsvTags())
        {
            MinionTag tags = MinionTags.Parse(raw, out string error);
            if (error != null)
                problems.Add($"{key}: 모르는 태그 {error}");
            if (!MinionTags.HasSingleForm(tags))
                problems.Add($"{key}: 형태가 하나가 아님 ({raw})");
        }
        Assert.IsEmpty(problems, string.Join("\n", problems));
    }

    // 형태·특성마다 해당 슬라임이 최소 3종 (권능이 쓸모 있으려면)
    [Test]
    public void Csv_EveryTagHasEnoughMinions()
    {
        var all = CsvTags().Values.Select(raw => MinionTags.Parse(raw, out _)).ToList();
        var problems = MinionTags.Forms.Concat(MinionTags.Traits)
            .Select(tag => (tag, count: MinionTags.CountMatching(all, tag)))
            .Where(x => x.count < MIN_MINIONS_PER_TAG)
            .Select(x => $"{x.tag}: {x.count}종")
            .ToList();
        Assert.IsEmpty(problems, $"{MIN_MINIONS_PER_TAG}종 미만인 태그\n{string.Join("\n", problems)}");
    }

    // 에셋에 CSV 태그가 반영됐는지 (Tools/Data/Import MinionData, Play 시 자동)
    [Test]
    public void Assets_MatchCsv()
    {
        Dictionary<string, string> csv = CsvTags();
        var problems = new List<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:MinionData"))
        {
            var data = AssetDatabase.LoadAssetAtPath<MinionData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data == null || !csv.TryGetValue(data.key, out string raw))
                continue;
            MinionTag expected = MinionTags.Parse(raw, out _);
            if (data.tags != expected)
                problems.Add($"{data.key}: 에셋 {MinionTags.ToCsv(data.tags)} / CSV {raw}");
        }
        Assert.IsEmpty(problems, "에셋 태그가 CSV와 다름 (Import MinionData 필요)\n" + string.Join("\n", problems));
    }
}
