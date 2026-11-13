using UnityEngine;
[System.Serializable]
public class DamageData
{
    public float damage;
    public AttackType attackType;
    // 만능(AllRounder) 공격은 방어 타입 배율 무시 (Enemy.TakeDamage에서 확인)
    public virtual bool IgnoreDefence => attackType == AttackType.AllRounder;
    public virtual float ApplyDamage(Vector2 pos, float multiplier = 1f)
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
    public override float ApplyDamage(Vector2 pos, float multiplier = 1f)
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

    // 발사 시 설정 (캐릭터 탄: 캐릭터 치명타 스탯, 타일 아이템: TileItem.ModifyDamageData)
    public float critChance;    // % 단위, 적중마다 판정
    public float critPower = 2f;
    public bool ignoreDefence;  // 만능 공격 타일: 원래 속성과 상관없이 AllRounder처럼 방어 타입 배율 무시
    public float eliteBossDamageMul = 1f; // 엘리트/보스에게 주는 데미지 배율 (거인 사냥 타일, EnemyDefence에서 적용). 강철 슬라임은 BigEnemyDamageForce

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
        eliteBossDamageMul = 1f;
    }

    public override float ApplyDamage(Vector2 pos, float multiplier = 1f)
    {
        float damage = this.damage * multiplier;

        // 탄 하나가 여러 번 맞을 수 있어 적중마다 판정
        bool crt = critChance > 0 && Random.Range(0f, 100f) <= critChance; // 100이면 확정
        if (crt)
            damage *= critPower;

        // 최소 피해 보장: 내성·배율로 1 미만이 돼도 1 (원래 피해가 0인 공격은 그대로 0)
        damage = GetMinGuaranteedDamage(this.damage, damage);
        string dText = $"{(int)damage}";

        if (!crt)
        {
            DamageText.SetText(pos, dText, ColorSetting.characterDamageColor);
        }
        else
        {
            CRTDamageText.SetText(pos, dText, ColorSetting.characterCrtDamageColor);
        }
        return damage;
    }
}
