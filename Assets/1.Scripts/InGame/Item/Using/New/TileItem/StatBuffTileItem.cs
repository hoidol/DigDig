using UnityEngine;

// 타일 위 오브젝트(Slime, Character)에 스탯 버프를 거는 타일 아이템. 수치는 프리팹 인스펙터에서 설정
public abstract class StatBuffTileItem : TileItem
{
    public StatOpType opType = StatOpType.Multiply;
    public float value = 1.2f;

    protected abstract StatType StatType { get; }

    IBuffable buffTarget;
    Buff appliedBuff; // RemoveBuff는 참조로 지우므로 적용한 인스턴스를 들고 있음

    public override void OnEnter(ITileObject tileObject)
    {
        if (tileObject is not IBuffable target)
            return;

        buffTarget = target;
        appliedBuff = new Buff(StatType, value, opType);
        buffTarget.AddBuff(appliedBuff);
    }

    public override void OnExit(ITileObject tileObject)
    {
        if (buffTarget == null || !ReferenceEquals(buffTarget, tileObject))
            return;

        buffTarget.RemoveBuff(appliedBuff);
        buffTarget = null;
        appliedBuff = null;
    }
}
