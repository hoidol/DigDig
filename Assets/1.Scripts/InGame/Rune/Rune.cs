using System;
using UnityEngine;

// 아이템 = 능력(Ability)의 한 종류. 데이터는 RuneData(itemPrefab이 이 컴포넌트를 참조)
public abstract class Rune : Ability
{
    public override string GetDescription()
    {
        return runeData.desc;
    }
    public RuneData runeData => RuneManager.Instance.GetRuneData(key);

    public bool CanMerge()
    {
        return true;
    }

    public override void OnEquip()
    {
        UpdateRune();
    }

    public virtual void UpdateRune()
    {

    }


}
