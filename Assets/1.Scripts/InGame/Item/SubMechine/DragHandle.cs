using System.Collections.Generic;
using UnityEngine;

// 필드에서 끌어 옮길 수 있는 손잡이 공용 (폭격 슬라임 깃발 등)
// - 입력은 SlimeSpawner 한 곳에서 처리: 누른 위치에서 grabRadius 안의 손잡이를 찾아 드래그
// - 소유 유닛이 선택돼 있으면 그 손잡이가 슬라임/캐릭터보다 우선, 아니면 유닛이 없는 자리에서만 잡힘
// - 위치 제한은 하위 클래스의 ClampPosition에서
public abstract class DragHandle : MonoBehaviour
{
    static readonly List<DragHandle> actives = new List<DragHandle>();

    public float grabRadius = 0.5f;
    public abstract IAllyUnit Owner { get; }
    public bool IsDragging { get; private set; }

    protected virtual void OnEnable()
    {
        if (!actives.Contains(this))
            actives.Add(this);
    }

    protected virtual void OnDisable()
    {
        actives.Remove(this);
        IsDragging = false;
    }

    // pos에서 잡히는 손잡이 (preferOwner의 손잡이가 범위 안이면 그것, 아니면 가장 가까운 것)
    public static DragHandle FindAt(Vector2 pos, IAllyUnit preferOwner)
    {
        DragHandle closest = null;
        float minDist = float.MaxValue;
        foreach (DragHandle handle in actives)
        {
            float dist = Vector2.Distance(pos, handle.transform.position);
            if (dist > handle.grabRadius)
                continue;
            if (preferOwner != null && handle.Owner == preferOwner)
                return handle;
            if (dist < minDist)
            {
                minDist = dist;
                closest = handle;
            }
        }
        return closest;
    }

    public virtual Vector2 ClampPosition(Vector2 pos) => pos;

    public virtual void BeginHandleDrag()
    {
        IsDragging = true;
    }

    public virtual void DragTo(Vector2 pos)
    {
        transform.position = ClampPosition(pos);
    }

    public virtual void EndHandleDrag()
    {
        IsDragging = false;
    }
}
