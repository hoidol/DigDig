using UnityEngine;

// 고장난 드론: 총알이 드론에 닿으면 랜덤 방향으로 count발 동시 발사
public class BrokenDroneItem : Item
{
    static readonly float[] damageRates = { 0.5f, 0.7f, 1.0f };

    //public BrokenDrone dronePrefab;

    public BrokenDrone drone;

    float DamageRate => damageRates[UnityEngine.Mathf.Clamp(count - 1, 0, damageRates.Length - 1)];

    public override void OnEquip(Player player)
    {
        drone.gameObject.SetActive(true);
        drone.transform.parent = null;
        drone.transform.localPosition = Vector3.zero;
        drone.Init(DamageRate, count);
        drone.Spawn(transform.position, count);
        drone.StartShooting();
    }

    public override void UpdateItem()
    {
        if (drone == null) return;
        drone.Init(DamageRate, count);
    }

    public override void OnUnequip(Player player)
    {
        drone.StopShooting();
        drone.transform.parent = transform;
        drone.gameObject.SetActive(false);
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        float rate = damageRates[UnityEngine.Mathf.Clamp(c - 1, 0, damageRates.Length - 1)];
        return $"총알이 드론에 닿으면 랜덤 방향으로 {c}발 동시 발사 (데미지 {rate * 100:0}%)";
    }
}
