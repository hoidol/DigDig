using UnityEngine;

// (2026-11-20) 능력 공용 부모. 게임에 붙어서 효과를 내는 것들은 모두 이걸 상속한다
// - 타일 아이템(룬): Item → TileItem (ItemData.itemPrefab)
// - 킹 성장(레벨업 카드): KingAbility → StatKingAbility 등 (AbilityData.prefab)
// 데이터(SO)가 프리팹의 Ability 컴포넌트를 참조 → 참조만 바꾸면 능력이 바뀐다
public abstract class Ability : MonoBehaviour
{
    public string key;
    public string Key => key;

    public virtual string GetDescription() => "";

    // 붙을 때 / 떨어질 때
    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }
}
