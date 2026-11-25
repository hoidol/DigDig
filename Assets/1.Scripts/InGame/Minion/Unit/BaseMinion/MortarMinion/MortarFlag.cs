using UnityEngine;

// 폭격 슬라임의 깃발: 폭탄이 떨어지는 지점. 드래그로 옮김 (슬라임 사거리 안으로 제한)
// 슬라임의 자식이 아니라 월드에 따로 둠 → 슬라임을 다른 칸으로 옮겨도 깃발 자리는 유지 (사거리 밖이면 당겨짐)
// 프리팹: 깃발 스프라이트 + rangeTr(폭발 범위 원, 슬라임 사거리 rangeTr와 같은 규칙: scale = 반지름)
public class MortarFlag : DragHandle
{
    public Transform rangeTr; // 폭발 범위 표시 (소유 슬라임 선택 중 또는 깃발 드래그 중에만)

    MortarMinion owner;
    public override IAllyUnit Owner => owner;

    public void Init(MortarMinion owner)
    {
        this.owner = owner;
        ShowRange(false);
    }

    public void SetRangeRadius(float radius)
    {
        if (rangeTr != null)
            rangeTr.localScale = Vector3.one * radius;
    }

    public void ShowRange(bool show)
    {
        if (rangeTr != null)
            rangeTr.gameObject.SetActive(show);
    }

    public override Vector2 ClampPosition(Vector2 pos) => owner != null ? owner.ClampFlagPosition(pos) : pos;

    public override void BeginHandleDrag()
    {
        base.BeginHandleDrag();
        ShowRange(true);
    }
}
