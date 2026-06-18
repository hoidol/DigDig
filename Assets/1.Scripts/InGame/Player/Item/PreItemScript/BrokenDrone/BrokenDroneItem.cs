using UnityEngine;

// 고장난 드론: 총알이 드론에 닿으면 랜덤 방향으로 1발 동시 발사 (데미지 50%)
public class BrokenDroneItem : Item
{
    const float DAMAGE_RATE = 0.5f;
    const int BULLET_COUNT = 1;

    public BrokenDrone drone;

    public override void OnEquip(Player player)
    {
        drone.gameObject.SetActive(true);
        drone.transform.parent = null;
        drone.transform.localPosition = Vector3.zero;
        drone.Init(DAMAGE_RATE, BULLET_COUNT);
        drone.Spawn(transform.position, BULLET_COUNT);
        drone.StartShooting();
    }

    public override void OnUnequip(Player player)
    {
        drone.StopShooting();
        drone.transform.parent = transform;
        drone.gameObject.SetActive(false);
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"총알이 드론에 닿으면 랜덤 방향으로 {BULLET_COUNT}발 동시 발사 (데미지 {DAMAGE_RATE * 100:0}%)";
    }
}
