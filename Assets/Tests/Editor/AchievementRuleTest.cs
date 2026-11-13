using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// 업적 규칙 (CLAUDE.md "업적"): CSV 행이 전부 읽히는지, 목표 계산, 날짜 판단(DailyCheck), 이름 번역 key
public class AchievementRuleTest
{
    const string DAILY_CSV = "Assets/Resources/Json/DailyAchievement.csv";
    const string CUMULATIVE_CSV = "Assets/Resources/Json/CumulativeAchievement.csv";

    static string Read(string path)
    {
        TextAsset csv = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        Assert.IsNotNull(csv, $"{path} 없음");
        return csv.text;
    }

    static int RowCount(string text) => text.Split('\n').Skip(1).Count(l => !string.IsNullOrWhiteSpace(l));

    [TearDown]
    public void TearDown() => DailyCheck.NowForTest = null;

    [Test]
    public void DailyCsv_AllRowsLoad()
    {
        string text = Read(DAILY_CSV);
        List<DailyAchievementData> datas = DailyAchievementManager.ParseCsv(text);
        Assert.AreEqual(RowCount(text), datas.Count, "읽지 못한 행이 있음 (Type이 AchievementType에 없거나 RewardType 오타)");
        Assert.AreEqual(datas.Count, datas.Select(d => d.type).Distinct().Count(), "같은 Type이 두 번");
        foreach (DailyAchievementData d in datas)
        {
            Assert.Greater(d.goal, 0, $"{d.type} Goal");
            Assert.IsTrue(int.TryParse(d.rewardData.value, out int v) && v > 0, $"{d.type} RewardValue '{d.rewardData.value}'");
        }
    }

    [Test]
    public void CumulativeCsv_AllRowsLoad()
    {
        string text = Read(CUMULATIVE_CSV);
        List<CumulativeAchievementData> datas = CumulativeAchievementManager.ParseCsv(text);
        Assert.AreEqual(RowCount(text), datas.Count, "읽지 못한 행이 있음 (Type이 AchievementType에 없거나 RewardType 오타)");
        Assert.AreEqual(datas.Count, datas.Select(d => d.type).Distinct().Count(), "같은 Type이 두 번");
        foreach (CumulativeAchievementData d in datas)
        {
            Assert.Greater(d.initGoal, 0, $"{d.type} InitGoal");
            Assert.GreaterOrEqual(d.increaseGoal, 0, $"{d.type} IncreaseGoal");
            Assert.IsTrue(int.TryParse(d.rewardData.value, out int v) && v > 0, $"{d.type} RewardValue '{d.rewardData.value}'");
        }
    }

    [Test]
    public void Csv_UnknownType_SkippedWithError()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("알 수 없는 Type 'NoSuchType'"));
        var datas = DailyAchievementManager.ParseCsv("Type\tGoal\tRewardType\tRewardValue\nKillEnemy\t5\tGold\t10\nNoSuchType\t5\tGold\t10\n");
        Assert.AreEqual(1, datas.Count);
        Assert.AreEqual(AchievementType.KillEnemy, datas[0].type);
        Assert.AreEqual(5, datas[0].goal);
        Assert.AreEqual(RewardType.Gold, datas[0].rewardData.rewardType);
        Assert.AreEqual("10", datas[0].rewardData.value);
    }

    // 누적 목표 = InitGoal + IncreaseGoal × 클리어 횟수
    [Test]
    public void CumulativeGoal()
    {
        Assert.AreEqual(80, CumulativeAchievementData.GetGoal(80, 40, 0));
        Assert.AreEqual(120, CumulativeAchievementData.GetGoal(80, 40, 1));
        Assert.AreEqual(480, CumulativeAchievementData.GetGoal(80, 40, 10));
        Assert.AreEqual(2, CumulativeAchievementData.GetGoal(2, 0, 7), "IncreaseGoal 0이면 목표 고정");
    }

    [Test]
    public void FillAmount_Clamped()
    {
        Assert.AreEqual(0f, AchievementData.GetFillAmount(0, 10));
        Assert.AreEqual(0.5f, AchievementData.GetFillAmount(5, 10));
        Assert.AreEqual(1f, AchievementData.GetFillAmount(25, 10), "목표를 넘어도 1");
        Assert.AreEqual(1f, AchievementData.GetFillAmount(0, 0), "목표 0은 가득");
    }

    // 날짜 판단: 기기 시간 자정 기준
    [Test]
    public void DailyCheck_Midnight()
    {
        DailyCheck.NowForTest = new DateTime(2030, 3, 5, 23, 59, 59);
        string day = DailyCheck.Today;
        Assert.AreEqual("20300305", day);
        Assert.IsTrue(DailyCheck.IsToday(day));
        Assert.IsFalse(DailyCheck.IsToday(""), "저장된 날짜가 없으면 오늘이 아님");
        Assert.IsFalse(DailyCheck.IsToday((string)null));
        Assert.IsTrue(DailyCheck.IsToday(new DateTime(2030, 3, 5, 0, 0, 0)));

        DailyCheck.NowForTest = new DateTime(2030, 3, 6, 0, 0, 0);
        Assert.IsFalse(DailyCheck.IsToday(day), "자정이 지나면 다른 날");
        Assert.IsFalse(DailyCheck.IsToday(new DateTime(2030, 3, 5, 23, 59, 59)));
        Assert.AreEqual("20300306", DailyCheck.Today);
    }

    // 이름 = Translate `Achievement.{Type}`
    [Test]
    public void TitleKeys_InTranslateCsv()
    {
        var table = ScriptableObject.CreateInstance<TranslateDataTable>();
        table.LoadData();
        var keys = new HashSet<string>(table.rows.Select(r => r.key));
        UnityEngine.Object.DestroyImmediate(table);

        var missing = Enum.GetValues(typeof(AchievementType)).Cast<AchievementType>()
            .Select(AchievementData.GetTitleKey).Where(k => !keys.Contains(k)).ToList();
        Assert.IsEmpty(missing, "Translate.csv에 없는 key:\n" + string.Join("\n", missing));
    }
}
