using UnityEngine;

// Item과 Skill의 공통 베이스
// 플레이어를 강화하는 모든 요소가 상속
public abstract class PlayerEnhancement : MonoBehaviour
{
    public string key;
    public int count;

    public virtual string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return "설명 없음";
    }
    public virtual void OnEquip(Player player) { }
    public virtual void OnUnequip(Player player) { }
    public virtual void LevelUp()
    {
        count++;
    }
    public virtual void UpdateEnhancement() { }
}
