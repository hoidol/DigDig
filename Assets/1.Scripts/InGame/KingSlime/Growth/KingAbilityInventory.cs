using System.Collections.Generic;
using UnityEngine;

// (2026-11-20) 킹이 한 판 동안 얻은 성장 능력 목록. 같은 능력을 다시 얻으면 level +1
// 능력 오브젝트는 킹 아래에 생성 (씬과 같이 사라짐)
public class KingAbilityInventory
{
    readonly KingSlime king;
    readonly List<KingAbility> abilities = new List<KingAbility>();
    public IReadOnlyList<KingAbility> Abilities => abilities;

    public KingAbilityInventory(KingSlime king)
    {
        this.king = king;
    }

    public KingAbility Get(AbilityData data)
    {
        foreach (var a in abilities)
            if (a.data == data)
                return a;
        return null;
    }

    // 0 = 없음
    public int GetLevel(AbilityData data)
    {
        KingAbility a = Get(data);
        return a != null ? a.level : 0;
    }

    public bool IsMaxLevel(AbilityData data) => GetLevel(data) >= data.maxLevel;

    // 그 종류 능력을 하나라도 가졌는지 (종족·직업은 한 판에 하나)
    public bool HasCategory(AbilityCategory category)
    {
        foreach (var a in abilities)
            if (a.data != null && a.data.category == category)
                return true;
        return false;
    }

    public T Get<T>() where T : KingAbility
    {
        foreach (var a in abilities)
            if (a is T t)
                return t;
        return null;
    }

    public void Add(AbilityData data)
    {
        if (data == null || data.prefab == null)
        {
            Debug.LogWarning($"[KingAbilityInventory] prefab 없음: {(data != null ? data.key : "null")}");
            return;
        }

        KingAbility ability = Get(data);
        if (ability != null)
        {
            if (ability.level >= data.maxLevel)
                return;
            ability.level++;
            ability.OnLevelChanged();
            return;
        }

        ability = Object.Instantiate(data.prefab, king.transform);
        ability.name = data.key;
        ability.Init(king, data);
        abilities.Add(ability);
        ability.OnEquip();
    }

    // Weapon.Shoot에서 호출
    public void OnKingFired(AllyBulletObject bullet)
    {
        for (int i = 0; i < abilities.Count; i++)
            abilities[i].OnKingFired(bullet);
    }
}
