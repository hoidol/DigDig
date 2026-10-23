using UnityEngine;

// 광폭: 공격력 ×attackPowerMul, 사거리 ×rangeMul (기본 공격력 +100%, 사거리 -50%)
public class BerserkTileItem : TileItem //타일 위에 설치되는 아이템
{
    public float attackPowerMul = 2f;
    public float rangeMul = 0.5f;

    IBuffable buffTarget;
    Buff attackPowerBuff;
    Buff rangeBuff;

    public override void OnEnter(ITileObject tileObject)
    {
        if (tileObject is not IBuffable target)
            return;

        buffTarget = target;
        attackPowerBuff = new Buff(StatType.AttackPower, attackPowerMul, StatOpType.Multiply);
        rangeBuff = new Buff(StatType.AttackRange, rangeMul, StatOpType.Multiply);
        buffTarget.AddBuff(attackPowerBuff);
        buffTarget.AddBuff(rangeBuff);
    }

    public override void OnExit(ITileObject tileObject)
    {
        if (buffTarget == null || !ReferenceEquals(buffTarget, tileObject))
            return;

        buffTarget.RemoveBuff(attackPowerBuff);
        buffTarget.RemoveBuff(rangeBuff);
        buffTarget = null;
        attackPowerBuff = null;
        rangeBuff = null;
    }
}
