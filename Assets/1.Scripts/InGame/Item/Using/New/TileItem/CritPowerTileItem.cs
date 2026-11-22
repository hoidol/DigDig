using UnityEngine;

// (2026-11-21) 크리티컬 룬: chance% 확률로 치명타, 치명타 피해 ×레벨 값(ItemData Values 2/2.5/3)
// 미니언은 기본 치명타 확률이 0이라 확률도 같이 줌. 다른 치명타(종족·직업·설의)와 확률은 더하고 배율은 큰 값
public class CritPowerTileItem : TileItem
{
    public float chance = 10f;
    public float critPower = 2f;

    protected override void ApplyLevelValue(float v) => critPower = v;
    protected override string FormatLevelValue(float v) => FormatNumber(v);

    public override void ModifyDamageData(AllyUnitDamageData data)
    {
        data.critChance += chance;
        data.critPower = Mathf.Max(data.critPower, critPower);
    }
}
