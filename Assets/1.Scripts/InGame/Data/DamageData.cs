using UnityEngine;

public class DamageData
{
    public float damage;
    public virtual float ApplyDamage(Vector2 pos)
    {
        if (damage < 1)
            damage = 0;
        return damage;
    }
}


public class EnemyDamageData : DamageData
{
    public override float ApplyDamage(Vector2 pos)
    {
        if (damage < 1)
            damage = 0;
        string dText = $"-{(int)damage}";
        CharacterTakeDamageText.SetText(pos, dText);
        return damage;
    }
}


public class AllyUnitDamageData : DamageData
{
    public IAllyUnit allyUnit;

    public bool isCrt;

    public void Init(IAllyUnit allyUnit)
    {
        this.allyUnit = allyUnit;
        isCrt = false;
    }

    public override float ApplyDamage(Vector2 pos)
    {
        if (damage < 1)
            damage = 0;
        string dText = $"-{(int)damage}";

        if (!isCrt)
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

public class CharacterDamageData : AllyUnitDamageData
{
    public RaycastHit2D hit2D;
    public CharacterBulletObject characterBulletObject;
    public void Init()
    {
        isCrt = false;
    }
    public bool mustCrit;
    public void Calculate()
    {
        damage = Character.Instance.statMgr.AttackPower;
        isCrt = false;
        if (mustCrit || Random.Range(0f, 100f) <= Character.Instance.statMgr.CritChance)
        {
            isCrt = true;
            damage *= Character.Instance.statMgr.CritPower;
        }
    }

    public void Calculate(float d)
    {
        damage = d;
        isCrt = false;
        if (mustCrit || Random.Range(0f, 100f) <= Character.Instance.statMgr.CritChance)
        {
            isCrt = true;
            damage *= Character.Instance.statMgr.CritPower;
        }
    }
}