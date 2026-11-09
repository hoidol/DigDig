using System;
using System.Collections.Generic;
using System.Linq;

// 슬라임 유형 태그 (SlimeData.csv Tags 열, "Shooter|Status"처럼 | 로 구분).
// 장비가 "장착 슬라임 구성"에 반응하는 기준 (CLAUDE.md "장비").
// 형태는 슬라임마다 정확히 하나, 특성은 0개 이상. 태그는 기본 효과 기준 (부가 효과는 안 셈)
[Flags]
public enum SlimeTag
{
    None = 0,

    // 형태: 공격 방식 (하나만)
    Shooter = 1 << 0,   // 발사: 탄을 쏘는 게 주 공격
    Zone = 1 << 1,      // 장판: 경로나 지정 위치에 피해 (지뢰·지옥불·박격포·낙뢰 등)
    Orbit = 1 << 2,     // 회전: 회전체로 공격
    Support = 1 << 3,   // 버프: 십자 버프
    Special = 1 << 4,   // 특수: 공격보다 고유 효과가 핵심

    // 특성
    Status = 1 << 8,      // 상태이상을 기본으로 검
    Chance = 1 << 9,      // 성공/실패 확률 판정이 핵심
    KillTrigger = 1 << 10, // 주변 처치에 반응
    Heavy = 1 << 11,      // 엘리트·보스 대응용 강타
    Transform = 1 << 12,  // 종·머지 레벨을 바꿈
}

public static class SlimeTags
{
    public const SlimeTag FORM_MASK = SlimeTag.Shooter | SlimeTag.Zone | SlimeTag.Orbit | SlimeTag.Support | SlimeTag.Special;
    public const SlimeTag TRAIT_MASK = SlimeTag.Status | SlimeTag.Chance | SlimeTag.KillTrigger | SlimeTag.Heavy | SlimeTag.Transform;

    public static IEnumerable<SlimeTag> Forms => Split(FORM_MASK);
    public static IEnumerable<SlimeTag> Traits => Split(TRAIT_MASK);

    public static SlimeTag GetForm(SlimeTag tags) => tags & FORM_MASK;

    // 형태가 정확히 하나인지
    public static bool HasSingleForm(SlimeTag tags) => Split(GetForm(tags)).Count() == 1;

    // tag의 비트가 모두 있으면 true (tag가 여러 개면 전부)
    public static bool Has(SlimeTag tags, SlimeTag tag) => tag != SlimeTag.None && (tags & tag) == tag;

    // 장착 슬라임 중 tag에 해당하는 종 수 (로비 "N종 해당", 장비 조건)
    public static int CountMatching(IEnumerable<SlimeTag> slimeTags, SlimeTag tag) => slimeTags.Count(t => Has(t, tag));

    // 낱개 플래그로 분리
    public static IEnumerable<SlimeTag> Split(SlimeTag tags)
    {
        foreach (SlimeTag t in Enum.GetValues(typeof(SlimeTag)))
        {
            if (t != SlimeTag.None && (tags & t) == t)
                yield return t;
        }
    }

    // CSV "Shooter|Status" → 플래그. 모르는 이름은 error에 담고 무시
    public static SlimeTag Parse(string raw, out string error)
    {
        error = null;
        SlimeTag result = SlimeTag.None;
        if (string.IsNullOrWhiteSpace(raw))
            return result;
        foreach (string part in raw.Split('|'))
        {
            string name = part.Trim();
            if (name.Length == 0)
                continue;
            if (Enum.TryParse(name, out SlimeTag tag) && Enum.IsDefined(typeof(SlimeTag), tag))
                result |= tag;
            else
                error = (error == null ? "" : error + ", ") + name;
        }
        return result;
    }

    public static string ToCsv(SlimeTag tags) => string.Join("|", Split(tags));
}
