using System;
using UnityEngine;

// 타일 위에 설치되는 아이템. 획득할 때마다 객체가 하나씩 새로 생겨 랜덤 타일에 설치된다.
// 수치는 타일(객체)마다 고정 → count로 강해지지 않음
public abstract class TileItem  : Item,ITileItem
{
    public Tile tile;

    public override void OnEquip()
    {
        tile = TileManager.Instance.GetRandomTileForItem();
        if (tile == null)
        {
            Debug.LogWarning($"TileItem {key} 설치할 타일 없음");
            return;
        }
        tile.AddTileItem(this);
        transform.SetParent(tile.transform, false); // 하이어라키에서 설치된 타일 아래에 둠 (타일 위치 기준)
    }
    public override void OnUnequip()
    {
        if (tile != null)
            tile.RemoveTileItem(this);
        tile = null;
    }

    // 뽑기 후보로 나올 수 있는지 (ItemManager.GetDrawTileItem)
    public virtual bool CanDraw() => true;

    // 위에 올라간 유닛이 탄을 쏠 때마다 호출 (Slime.FireBullet, Weapon.Shoot). 발사 직후라 데미지 데이터 초기화가 끝난 상태
    public virtual void OnFired(AllyBulletObject bullet)
    {
        ModifyDamageData(bullet.DamageData);
        IBulletBehavior hitBehavior = HitBehavior; // 확률형은 읽을 때 판정하므로 한 번만 읽음
        if (hitBehavior != null)
            bullet.AddBehavior(hitBehavior);
    }

    // 데미지 데이터 수정 (치명타, 만능 공격). 탄이 없는 Orbit은 적중마다 직접 호출
    public virtual void ModifyDamageData(AllyUnitDamageData data)
    {

    }

    // 적중 후 효과 (둔화, 스턴, 처치 코인). 상태가 없으므로 타일 객체마다 하나를 만들어 모든 탄이 공유
    // 확률형(둔화)은 읽을 때마다 판정해서 null일 수 있음 → 호출 측은 한 번만 읽을 것
    public virtual IBulletBehavior HitBehavior => null;

    public virtual void OnEnter(ITileObject tileObject)
    {

    }
    public virtual void OnExit(ITileObject tileObject)
    {

    }

}
