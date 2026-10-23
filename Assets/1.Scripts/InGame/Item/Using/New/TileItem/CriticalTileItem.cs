using UnityEngine;

// 치명타: 적중 시 chance% 확률로 데미지 critPower배 (판정은 AllyUnitDamageData.ApplyDamage)
public class CriticalTileItem : TileItem //타일 위에 설치되는 아이템
{
    public float chance = 10f;
    public float critPower = 2f;

    public override void ModifyDamageData(AllyUnitDamageData data)
    {
        data.critChance += chance;
        data.critPower = Mathf.Max(data.critPower, critPower); // 캐릭터 치명타 배율이 더 높으면 유지
    }
}
