using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 미니언이 적에게 준 피해를 종(key)별로 한 판 동안 합산
// (2026-11-27) 슬라임(슬라임) 피해도 화면 집계에 한 줄로 넣음 (GetEntries: 장착 미니언 + 슬라임, 전체 대비 % = 미니언 + 슬라임 합계). 미니언 진화 스킬·슬라임 용사 스킬 피해도 출처가 붙어 함께 셈
// (2026-11-25) 슬라임 피해는 따로 구분별로 합산 (SlimeDamage: 기본 공격 / 용사 1·2단계 스킬, AllyUnitDamageData.meterTag) → 용사 피해 측정·테스트용
// 집계 = Enemy.TakeDamage 한 곳(실제로 줄어든 체력), 출처 = DamageData.Attacker. 초기화 = GameManager.StartGame
// 피해마다 호출되므로 이벤트 발행 없이 값만 더하고, UI(DamageMeterSystemContainer)가 주기적으로 읽는다
public static class DamageMeterSystem
{
    static readonly Dictionary<string, double> damageDic = new();

    public static double TotalDamage { get; private set; }

    static readonly Dictionary<string, double> slimeDamageDic = new();
    public static double SlimeTotalDamage { get; private set; }

    public static void Record(DamageData data, float damage)
    {
        if (damage <= 0 || data == null)
            return;
        if (data.Attacker is Slime)
        {
            string tag = data is AllyUnitDamageData a && !string.IsNullOrEmpty(a.meterTag) ? a.meterTag : HeroSlimeAbility.TAG_BASIC;
            slimeDamageDic.TryGetValue(tag, out double v);
            slimeDamageDic[tag] = v + damage;
            SlimeTotalDamage += damage;
            return;
        }
        Record(data.Attacker, damage);
    }

    public static double GetSlimeDamage(string tag) => tag != null && slimeDamageDic.TryGetValue(tag, out double v) ? v : 0;

    public static void Record(IAllyUnit attacker, float damage)
    {
        if (damage <= 0 || attacker is not Minion minion || minion.minionData == null)
            return;

        string key = minion.minionData.key;
        damageDic.TryGetValue(key, out double value);
        damageDic[key] = value + damage;
        TotalDamage += damage;
    }

    public static double GetDamage(string key)
    {
        return key != null && damageDic.TryGetValue(key, out double value) ? value : 0;
    }

    // 슬라임 피해 중 용사 스킬(1·2단계) 몫 = 슬라임 전체 - 기본 공격
    public static double SlimeSkillDamage => System.Math.Max(0, SlimeTotalDamage - GetSlimeDamage(HeroSlimeAbility.TAG_BASIC));

    // 미니언 + 슬라임 전체
    public static double AllDamage => TotalDamage + SlimeTotalDamage;

    // 화면 한 줄 (피해 집계 창·결과 화면 공용)
    public struct Entry
    {
        public string name;
        public Sprite thum;
        public double damage;
        public string detail; // 슬라임 = "스킬 N%" (슬라임 피해 중 스킬 몫)
        public bool isSlime;
    }

    // 장착 미니언 + 슬라임, 피해 큰 순 (같으면 장착 순서, 슬라임은 맨 뒤)
    public static List<Entry> GetEntries()
    {
        var list = new List<Entry>();
        if (MinionManager.Instance != null && MinionManager.Instance.minionDataDic.Count > 0 && UserDataManager.Instance != null)
        {
            UserMinion[] equipped = UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions;
            foreach (MinionData data in (equipped ?? new UserMinion[0]).Where(e => e != null && e.MinionData != null).Select(e => e.MinionData).Distinct())
                list.Add(new Entry { name = data.Title, thum = data.thum, damage = GetDamage(data.key) });
        }
        Slime slime = Slime.Instance;
        if (slime != null)
        {
            HeroSlimeAbility hero = slime.abilityInventory != null ? slime.abilityInventory.Get<HeroSlimeAbility>() : null;
            bool hasHero = hero != null && hero.data != null;
            SpriteRenderer body = slime.GetComponentInChildren<SpriteRenderer>(true);
            list.Add(new Entry
            {
                name = hasHero ? hero.data.title : TranslateManager.GetText("DamageMeter_Slime"),
                thum = hasHero && hero.data.thum != null ? hero.data.thum : body != null ? body.sprite : null,
                damage = SlimeTotalDamage,
                detail = SlimeTotalDamage > 0 && SlimeSkillDamage > 0
                    ? string.Format(TranslateManager.GetText("DamageMeter_SlimeSkill"), $"{SlimeSkillDamage / SlimeTotalDamage * 100:0}") : null,
                isSlime = true,
            });
        }
        // 안정 정렬 (같은 피해면 원래 순서)
        return list.Select((e, i) => (e, i)).OrderByDescending(x => x.e.damage).ThenBy(x => x.i).Select(x => x.e).ToList();
    }

    public static void Clear()
    {
        damageDic.Clear();
        TotalDamage = 0;
        slimeDamageDic.Clear();
        SlimeTotalDamage = 0;
    }
}
