using UnityEngine;

// [개조된 총]
// TriggerItem. 쿨타임마다 플레이어 공격 방향으로 4발 연속 발사. 데미지는 공격력과 동일.
public class FixedGunItem : TriggerItem
{
    const int SHOT_COUNT = 4;

    public override void OnTrigger()
    {
        base.OnTrigger();
        Vector2 dir = Player.Instance.weapon.dirTr.right;

        for (int i = 0; i < SHOT_COUNT; i++)
        {
            // var bullet = PlayerBulletObject.Instantiate();
            // bullet.transform.position = Player.Instance.transform.position;
            // bullet.Shoot(dir, Player.Instance.statMgr.AttackPower);
        }
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"공격 방향으로 {coolTime}초마다 {SHOT_COUNT}발 추가 발사";
    }
}
