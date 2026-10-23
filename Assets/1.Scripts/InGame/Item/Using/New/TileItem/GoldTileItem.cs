using UnityEngine;

// 코인: 이 타일 위 유닛의 공격으로 적을 처치하면 코인 +addCoin
public class GoldTileItem : TileItem //타일 위에 설치되는 아이템
{
    public int addCoin = 1;

    IBulletBehavior hitBehavior;
    public override IBulletBehavior HitBehavior => hitBehavior ??= new KillCoinBehavior(this);
}

public class KillCoinBehavior : IBulletBehavior
{
    public int ApplyOrder => -1; // 관통 등 다음 처리를 끊는 behavior보다 먼저
    readonly GoldTileItem item;

    public KillCoinBehavior(GoldTileItem item)
    {
        this.item = item;
    }

    // 적중 behavior는 TakeDamage 이후에 불림. 이미 죽은 적에 다른 탄이 또 맞아도 중복 지급되지 않도록
    // 처치한 공격의 유닛이 이 타일 위 유닛일 때만 1번 지급
    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
    {
        if (hit is not Enemy enemy || enemy.killedBy == null || item.tile == null)
            return true;
        if (!ReferenceEquals(enemy.killedBy.allyUnit, item.tile.tileObject))
            return true;

        enemy.killedBy = null;
        Character.Instance.AddCoin(item.addCoin);
        CoinText.Instantiate().SetCoinText(hit.Transform.position + Vector3.up * 0.4f, $"+{item.addCoin}");
        return true;
    }
}
