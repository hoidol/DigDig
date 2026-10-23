using UnityEngine;

// 만능 공격: 방어 타입 배율을 무시하고 모든 방어 타입에 데미지 100% (Enemy.TakeDamage에서 확인)
public class TrueDamageTileItem : TileItem //타일 위에 설치되는 아이템
{
    public override void ModifyDamageData(AllyUnitDamageData data)
    {
        data.ignoreDefence = true;
    }
}
