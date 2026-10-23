using UnityEngine;

// 거인 사냥: 엘리트/보스에게 데미지 ×damageMul (Enemy.TakeDamage에서 적용)
public class GiantSlayerTileItem : TileItem //타일 위에 설치되는 아이템
{
    public float damageMul = 2f;

    public override void ModifyDamageData(AllyUnitDamageData data)
    {
        data.eliteBossDamageMul = damageMul;
    }
}
