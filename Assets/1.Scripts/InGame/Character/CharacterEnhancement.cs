using UnityEngine;

// Item과 Skill의 공통 베이스
// 플레이어를 강화하는 모든 요소가 상속
public abstract class CharacterEnhancement : MonoBehaviour
{
    public string key;
    public bool equipped;
    // public int count = 1;

    public virtual string GetDescription(bool detail = false)
    {
        return "설명 없음";
    }
    // public virtual void LevelUp() { count++; }
    public virtual void OnEquip(Character character)
    {
        equipped = true;
    }
    public virtual void OnUnequip(Character character)
    {
        equipped = false;
    }
    public virtual void UpdateEnhancement() { }
}
