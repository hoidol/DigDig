using System.IO;
using System.Linq;
using NUnit.Framework;

// (2026-11-26) MinionData.csv EvolutionMinionDesc: 진화하는 종마다 설명이 있고, {0}… 자리가 MinionEvolutionSetting 수치 개수와 맞는지
public class EvolutionDescRuleTest
{
    [Test]
    public void EvolutionDesc_FormatsWithConfigArgs()
    {
        string[] lines = File.ReadAllText("Assets/Json/MinionData.csv").TrimStart('﻿').Replace("\r\n", "\n").Split('\n');
        string[] header = lines[0].Split('\t');
        int keyIdx = System.Array.IndexOf(header, "Key");
        int descIdx = System.Array.IndexOf(header, "EvolutionMinionDesc");
        Assert.GreaterOrEqual(descIdx, 0, "EvolutionMinionDesc 열 없음");
        var setting = UnityEngine.ScriptableObject.CreateInstance<MinionEvolutionSetting>(); // 코드 기본값 기준 (자리 수만 봄)
        foreach (string key in MinionEvolutionSetting.EVOLVING_KEYS)
        {
            string[] row = lines.Select(l => l.Split('\t')).FirstOrDefault(c => c.Length > keyIdx && c[keyIdx] == key);
            Assert.IsNotNull(row, $"{key} 행 없음");
            string desc = row.Length > descIdx ? row[descIdx] : "";
            Assert.IsNotEmpty(desc, $"{key} 진화 설명 없음");
            string text = null;
            Assert.DoesNotThrow(() => text = string.Format(desc, setting.Get(key).GetDescArgs()), $"{key}: {{n}} 자리가 수치 개수보다 많음");
            StringAssert.DoesNotContain("{", text, key);
        }
    }

    // (2026-11-26) 번역: Minion.{Key}.Evolution(이름) · Minion.{Key}.EvolutionDesc(설명) — 진화하는 종마다 한글·영어, 수치 자리 맞음
    [Test]
    public void EvolutionTranslation_KrEn()
    {
        var rows = File.ReadAllText("Assets/Json/Translate.csv").TrimStart('\uFEFF').Replace("\r\n", "\n").Split('\n')
            .Select(l => l.Split('\t')).Where(c => c.Length >= 3).ToDictionary(c => c[0], c => c, System.StringComparer.Ordinal);
        var setting = UnityEngine.ScriptableObject.CreateInstance<MinionEvolutionSetting>();
        foreach (string key in MinionEvolutionSetting.EVOLVING_KEYS)
        {
            Assert.IsTrue(rows.TryGetValue(MinionEvolutionData.NameKey(key), out var name), $"{key} 진화 이름 번역 없음");
            Assert.IsNotEmpty(name[1], $"{key} 진화 이름 Kr");
            Assert.IsNotEmpty(name[2], $"{key} 진화 이름 En");
            Assert.IsTrue(rows.TryGetValue(MinionEvolutionData.DescKey(key), out var desc), $"{key} 진화 설명 번역 없음");
            object[] args = setting.Get(key).GetDescArgs();
            for (int lang = 1; lang <= 2; lang++)
            {
                string text = null;
                Assert.DoesNotThrow(() => text = string.Format(desc[lang], args), $"{key} 진화 설명 {(lang == 1 ? "Kr" : "En")} 수치 자리");
                StringAssert.DoesNotContain("{", text);
            }
        }
    }
}
