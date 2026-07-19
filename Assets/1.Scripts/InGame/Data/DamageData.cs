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
            DamageText.SetText(pos, dText);
        }
        else
        {
            CRTDamageText.SetText(pos, dText);
        }

    }
}

public class PlayerBulletDamageData : DamageData
{
    public RaycastHit2D hit2D;
    public PlayerBulletObject playerBulletObject;
    public void Init(PlayerBulletObject pBObj)
    {
        playerBulletObject = pBObj;
        isCrt = false;
        mustCrit = false;
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