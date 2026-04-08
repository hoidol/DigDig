using UnityEngine;

public class DamageData
{
    public float damage;
    public bool isCrt;
    public Transform cause;//데미지 전달자 또는 원인자
    public void Applyed(Vector2 pos)
    {
        if (damage < 1)
            damage = 0;
        string dText = ((int)damage).ToString();
        if (!isCrt)
        {
            DamageText.SetText(pos, dText);
        }
        else
        {
            CRTDamageText.SetText(pos, dText);
        }

    }
}

public class PlayerDamageData : DamageData
{
    public void Init()
    {
        isCrt = false;
        mustCrit = false;
        cause = null;
    }
    public bool mustCrit;
    public void Calculate()
    {
        damage = Player.Instance.statMgr.AttackPower;
        isCrt = false;
        if (mustCrit || Random.Range(0f, 100f) <= Player.Instance.statMgr.CritChance)
        {
            isCrt = true;
            damage *= Player.Instance.statMgr.CritPower;
        }
    }

    public void Calculate(float d)
    {
        damage = d;
        isCrt = false;
        if (mustCrit || Random.Range(0f, 100f) <= Player.Instance.statMgr.CritChance)
        {
            isCrt = true;
            damage *= Player.Instance.statMgr.CritPower;
        }
    }
}