using UnityEngine;

public class DamageData
{
    public float damage;
    public bool isCrt;
    public void Applyed(Vector2 pos)
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

    }
}

public class CharacterBulletDamageData : DamageData
{
    public RaycastHit2D hit2D;
    public CharacterBulletObject characterBulletObject;
    public void Init(CharacterBulletObject pBObj)
    {
        characterBulletObject = pBObj;
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