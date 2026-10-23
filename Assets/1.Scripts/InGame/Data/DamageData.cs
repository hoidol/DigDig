using UnityEngine;
[System.Serializable]
public class DamageData
{
    public float damage;
    public AttackType attackType;
    public virtual float ApplyDamage(Vector2 pos, float multiplier = 1f)
    {
        float damage = this.damage * multiplier;
        if (damage < 1)
            damage = 0;

        string dText = $"-{(int)damage}";
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

        string dText = $"-{(int)damage}";
        CharacterTakeDamageText.SetText(pos, dText);
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
    public bool ignoreDefence;  // 만능 공격: 방어 타입 배율 무시
    public float eliteBossDamageMul = 1f; // 엘리트/보스에게 주는 데미지 배율 (Enemy.TakeDamage에서 확인)

    public void Init(IAllyUnit allyUnit)
    {
        this.allyUnit = allyUnit;
        critChance = 0;
        critPower = 2f;
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

        if (damage < 1)
            damage = 0;
        string dText = $"-{(int)damage}";

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
