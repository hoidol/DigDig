using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

// 덫 설치: 랜덤 방향으로 덫을 던짐 - 데미지 + 스턴
public class TrapItem : TriggerItem
{
    public Trap trapPrefab;
    public float throwDistance = 4f;
    public float damage = 10f;
    int maxTrapCount = 2;

    int tripCount;
    public override string GetDescription(int c = -1, bool detail = false)
    {
        return "랜덤 방향으로 덫을 던집니다.\n적이 밟으면 폭발 데미지와 스턴을 줍니다.";
    }

    public override void OnTrigger()
    {
        if (trapPrefab == null) return;
        if (tripCount >= maxTrapCount)
            return;

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector3 spawnPos = Player.Instance.transform.position + (Vector3)(dir * throwDistance);


        RaycastHit2D hit = Physics2D.Raycast(Player.Instance.transform.position, dir, throwDistance, LayerMask.GetMask("Hittable"));
        if (hit.collider != null && hit.collider.GetComponent<OreStone>() != null)
            spawnPos = Player.Instance.transform.position + (Vector3)(dir * (hit.distance - 0.5f));

        var trap = Instantiate(trapPrefab, spawnPos, Quaternion.identity);
        trap.Spawn(spawnPos, count);
        trap.Init(this);
        tripCount++;
    }

    protected override async UniTask WaitCooldown(CancellationToken token)
    {
        while (tripCount >= maxTrapCount)
        {
            CoolTimer = coolTime;
            await UniTask.Yield(token);
        }
        await base.WaitCooldown(token);
    }

    public void TripTrggered()
    {
        tripCount--;
    }
}
