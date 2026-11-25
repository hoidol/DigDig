using UnityEngine;

// (2026-11-24) 5성 진화 갈래의 특수 동작 (선택). MinionEvolutionData.prefab에 이 컴포넌트를 붙인 프리팹을 넣으면
// 진화할 때 미니언 자식으로 생성 → OnEquip, 미니언이 사라지거나(풀 반납) 새 미니언이 되면 OnUnequip 후 삭제
// 진화해도 미니언 클래스·key는 그대로 (피해 집계·미션·합성 등 key 기준 시스템 유지). 갈래마다 다른 동작은 이 클래스를 상속해서 만든다
public abstract class MinionEvolution : MonoBehaviour
{
    protected Minion minion;

    public virtual void OnEquip(Minion owner)
    {
        minion = owner;
    }

    public virtual void OnUnequip()
    {
    }
}
