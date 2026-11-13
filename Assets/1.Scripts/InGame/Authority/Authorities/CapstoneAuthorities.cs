using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 묶음 6 (Lv.30) 빌드 완성 — 장착 슬라임 구성 조건이 맞으면 강한 효과

// 순혈의 왕관: 장착 5종의 공격 속성이 모두 같으면(만능 제외) 약점 배율 1.5배 (EnemyDefence.GetMultiplier)
public class PureCrownAuthority : Authority
{
    public const float WEAKNESS_MUL = 1.5f;
    public bool active; // 조건 충족 (게임 시작 시 판정)

    public static bool IsPure(IEnumerable<AttackType> types)
    {
        var list = types.ToList();
        return list.Count > 0 && list.All(t => t == list[0]) && list[0] != AttackType.AllRounder;
    }

    // 약점(> 1)일 때만 키움. 내성(< 1)·보통은 그대로
    public static float Apply(float multiplier) => multiplier > 1f ? multiplier * WEAKNESS_MUL : multiplier;

    public override void OnBegin() => active = IsPure(AuthoritySystem.EquippedMinionDatas().Select(d => d.attackType));
}

// 왕의 인장: 장착 5종의 형태가 모두 다르면 모든 슬라임 공속 ×1.2 (Minion.UpdateMinion)
public class KingSealAuthority : Authority
{
    public const float ATTACK_SPEED_MUL = 1.2f;
    public const int REQUIRED_COUNT = 5;
    public bool active;

    public static bool AllDifferentForms(IEnumerable<MinionTag> tags)
    {
        var forms = tags.Select(MinionTags.GetForm).ToList();
        return forms.Count >= REQUIRED_COUNT && forms.All(f => f != MinionTag.None) && forms.Distinct().Count() == forms.Count;
    }

    public override void OnBegin() => active = AllDifferentForms(AuthoritySystem.EquippedMinionDatas().Select(d => d.tags));
}

// 혈통의 플라스크: 합성할 때 50% 확률로 결과가 랜덤 종 대신 같은 종 유지 (Minion.Merge)
public class BloodlineFlaskAuthority : Authority
{
    public const float KEEP_CHANCE = 0.5f;

    public static string PickResultKey(string ownKey, string randomKey, float roll) => roll < KEEP_CHANCE ? ownKey : randomKey;
}
