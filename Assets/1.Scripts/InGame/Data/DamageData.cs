using UnityEngine;
[System.Serializable]
public class DamageData
{
    public float damage;
    public AttackType attackType;
    // 피해를 준 아군 (피해 집계 DamageMeterSystem용). AllyUnitDamageData가 아닌 피해(화상·번개 연쇄 등)는 여기에 넣는다
    public IAllyUnit source;
    public virtual IAllyUnit Attacker => source;
    // 방어 타입 배율 무시 여부 (Enemy.TakeDamage에서 확인). (2026-11-21) 카오스(Chaos)도 배율표를 따름 → 무시는 만능 공격 타일(ignoreDefence)만
    public virtual bool IgnoreDefence => false;
    // typeMultiplier = 방어 타입 표 배율(약점·내성 표시용), targetMaxHp = 맞은 대상 최대 체력(피해 숫자 크기용, 0이면 기본 크기)
    public virtual float ApplyDamage(Vector2 pos, float multiplier = 1f, float typeMultiplier = 1f, float targetMaxHp = 0f)
    {
        float damage = this.damage * multiplier;
        if (damage < 1)
            damage = 0;

        string dText = $"{(int)damage}";
        DamageText.SetText(pos, dText, ColorSetting.enemyDamageColor);
        return damage;
    }
}

[System.Serializable]
public class EnemyDamageData : DamageData
{
    public override float ApplyDamage(Vector2 pos, float multiplier = 1f, float typeMultiplier = 1f, float targetMaxHp = 0f)
    {
        float damage = this.damage * multiplier;
        if (damage < 1)
            damage = 1;

        string dText = $"{(int)damage}";
        KingSlimeTakeDamageText.SetText(pos, dText);
        return damage;
    }
}

[System.Serializable]
public class AllyUnitDamageData : DamageData
{
    public IAllyUnit allyUnit;
    public override IAllyUnit Attacker => allyUnit ?? source;
    // (2026-11-25) 킹 피해 집계 구분 (HeroKingAbility.TAG_*, null = 기본 공격). DamageMeterSystem.Record
    public string meterTag;

    // 발사 시 설정 (캐릭터 탄: 캐릭터 치명타 스탯, 타일 아이템: TileRune.ModifyDamageData)
    public float critChance;    // % 단위, 적중마다 판정
    public float critPower = 2f;
    public bool lastHitCrit; // 마지막 적중이 치명타였는지 (ApplyDamage에서 설정, 적중 후 효과용: 엘프 정밀 사격 등)
    public bool ignoreDefence;  // 만능 공격 타일: 원래 속성과 상관없이 방어 타입 배율 무시
    public float eliteBossDamageMul = 1f; // 엘리트/보스에게 주는 데미지 배율 (거인 사냥 타일, EnemyDefence에서 적용). 강철 슬라임은 BigEnemyDamageForce
    // (2026-11-21) 방어 타입별 주는 피해 +% (종족 버프 등, StatType.DamageTo*). EnemyDefence에서 맞은 적의 방어 타입 것만 곱함
    public float lightDamageMul = 1f, normalDamageMul = 1f, armoredDamageMul = 1f, etherealDamageMul = 1f;

    public float GetDefenceDamageMul(DefenceType defenceType)
    {
        switch (defenceType)
        {
            case DefenceType.Light: return lightDamageMul;
            case DefenceType.Normal: return normalDamageMul;
            case DefenceType.Armored: return armoredDamageMul;
            case DefenceType.Ethereal: return etherealDamageMul;
            default: return 1f;
        }
    }

    // 방어 타입 → 그 타입 상대 피해 +% 스탯
    public static StatType GetDefenceDamageStat(DefenceType defenceType)
    {
        switch (defenceType)
        {
            case DefenceType.Light: return StatType.DamageToLight;
            case DefenceType.Armored: return StatType.DamageToArmored;
            case DefenceType.Ethereal: return StatType.DamageToEthereal;
            default: return StatType.DamageToNormal;
        }
    }

    public override bool IgnoreDefence => ignoreDefence || base.IgnoreDefence;

    public const float MIN_DAMAGE = 1f;
    public static float GetMinGuaranteedDamage(float baseDamage, float finalDamage)
    {
        if (baseDamage <= 0f)
            return 0f;
        return Mathf.Max(MIN_DAMAGE, finalDamage);
    }

    public void Init(IAllyUnit allyUnit)
    {
        this.allyUnit = allyUnit;
        // 슬라임 치명타 확률 스탯(설의 버프 등). 연발 탄도 SetBullet → Init을 거치므로 같이 적용됨. 캐릭터 탄은 KingSlimeBulletObject에서 덮어씀
        critChance = allyUnit is Minion minion ? minion.CritChance : 0;
        critPower = allyUnit is Minion ? GameSetting.INIT_CRIT_POWER : 2f; // 슬라임 치명타 배율 (캐릭터 탄은 발사 시 캐릭터 스탯으로 덮어씀)
        ignoreDefence = false;
        // 엘리트·보스 피해 +% 스탯 (미니언 = 버프, 킹 = 스탯). 거인 사냥 타일은 OnFired에서 여기에 곱함
        float eliteBossBonus = allyUnit is Minion m ? m.EliteBossDamage
            : allyUnit is KingSlime k && k.statMgr != null ? k.statMgr.GetValue(StatType.EliteBossDamage) : 0f;
        eliteBossDamageMul = 1f + eliteBossBonus / 100f;
        lightDamageMul = 1f + GetBonusStat(allyUnit, StatType.DamageToLight) / 100f;
        normalDamageMul = 1f + GetBonusStat(allyUnit, StatType.DamageToNormal) / 100f;
        armoredDamageMul = 1f + GetBonusStat(allyUnit, StatType.DamageToArmored) / 100f;
        etherealDamageMul = 1f + GetBonusStat(allyUnit, StatType.DamageToEthereal) / 100f;
    }

    // 버프로만 오르는 % 스탯 (미니언 = statDic, 킹 = statMgr). 없으면 0
    static float GetBonusStat(IAllyUnit allyUnit, StatType statType)
    {
        if (allyUnit is Minion m)
            return m.statDic.TryGetValue(statType, out float v) ? v : 0f;
        if (allyUnit is KingSlime k && k.statMgr != null)
            return k.statMgr.GetValue(statType);
        return 0f;
    }

    public override float ApplyDamage(Vector2 pos, float multiplier = 1f, float typeMultiplier = 1f, float targetMaxHp = 0f)
    {
        float damage = this.damage * multiplier;

        // 탄 하나가 여러 번 맞을 수 있어 적중마다 판정
        bool crt = critChance > 0 && Random.Range(0f, 100f) <= critChance; // 100이면 확정
        lastHitCrit = crt;
        if (crt)
            damage *= critPower;

        // 최소 피해 보장: 내성·배율로 1 미만이 돼도 1 (원래 피해가 0인 공격은 그대로 0)
        damage = GetMinGuaranteedDamage(this.damage, damage);
        string dText = $"{(int)damage}";

        float size = GetDamageTextSize(damage, targetMaxHp);
        DamageTextType type = GetDamageTextType(typeMultiplier);
        if (!crt)
            DamageText.SetText(pos, dText, size, type);
        else
            CRTDamageText.SetText(pos, dText, size, type);
        return damage;
    }

    // 피해 숫자 크기: 대상 최대 체력 대비 피해 비율로 1 ~ 1.5배 (최대 체력의 절반 이상이면 1.5배). 최대 체력을 모르면 1
    public const float DAMAGE_TEXT_MAX_SIZE = 1.5f;
    public static float GetDamageTextSize(float damage, float targetMaxHp)
    {
        if (targetMaxHp <= 0f)
            return 1f;
        float ratio = Mathf.Clamp01(damage / targetMaxHp * 2f);
        return Mathf.Lerp(1f, DAMAGE_TEXT_MAX_SIZE, ratio);
    }

    // 방어 타입 표 배율 > 1 = 약점, < 1 = 내성
    public static DamageTextType GetDamageTextType(float typeMultiplier)
    {
        if (typeMultiplier > 1.001f)
            return DamageTextType.Weak;
        if (typeMultiplier < 0.999f)
            return DamageTextType.Resist;
        return DamageTextType.Normal;
    }
}
