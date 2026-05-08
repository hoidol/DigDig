using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

// 실제 채광 담당 - 근처 광석으로 이동해서 공격
public class Pickaxe : Ally
{
    const float SEARCH_RADIUS = 5f;
    const float MOVE_SPEED = 8f;
    const float ARRIVE_DIST = 0.3f;
    const float ATTACK_INTERVAL = 0.8f;

    readonly DamageData damageData = new();
    CancellationTokenSource cts;
    public OreStone target;
    HashSet<OreStone> claimedTargets;

    public void Init(HashSet<OreStone> shared)
    {
        claimedTargets = shared;
    }


    public void StartMining()
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        MineLoop(cts.Token).Forget();
    }

    public void StopMining()
    {
        ReleaseTarget();
        cts?.Cancel();
        cts = null;
    }

    void ClaimTarget(OreStone ore)
    {
        ReleaseTarget();
        target = ore;
        claimedTargets?.Add(ore);
    }

    void ReleaseTarget()
    {
        if (target != null)
        {
            claimedTargets?.Remove(target);
            target = null;
        }
    }

    async UniTaskVoid MineLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (target == null || !target.gameObject.activeSelf)
            {
                ReleaseTarget();
                var found = FindNearestOre();
                if (found != null) ClaimTarget(found);
                if (token.IsCancellationRequested) return;
                await UniTask.Yield(token);
            }
            else
            {
                await MoveToward(target.transform.position, token);
                if (token.IsCancellationRequested) return;

                damageData.damage = Player.Instance.statMgr.MagicPower;
                target.TakeDamage(damageData);
                if (!target.CanHit())
                    ReleaseTarget();
                await UniTask.Delay((int)(ATTACK_INTERVAL * 1000), cancellationToken: token);
            }
        }
    }

    async UniTask MoveToward(Vector2 dest, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (Vector2.Distance(transform.position, dest) <= ARRIVE_DIST) break;
            transform.position = Vector2.MoveTowards(transform.position, dest, MOVE_SPEED * Time.deltaTime);
            await UniTask.Yield(token);
        }
    }

    OreStone FindNearestOre()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(
            Player.Instance.transform.position, SEARCH_RADIUS, LayerMask.GetMask("Hittable"));

        OreStone nearest = null;
        float minDist = float.MaxValue;

        foreach (var col in cols)
        {
            OreStone ore = col.GetComponent<OreStone>();
            if (ore == null || ore.curHp <= 0) continue;
            if (claimedTargets != null && claimedTargets.Contains(ore)) continue;

            float dist = ((Vector2)ore.transform.position - (Vector2)Player.Instance.transform.position).sqrMagnitude;
            if (dist > SEARCH_RADIUS * SEARCH_RADIUS) continue;
            if (dist < minDist)
            {
                minDist = dist;
                nearest = ore;
            }
        }
        return nearest;
    }

    private void OnDestroy()
    {
        cts?.Cancel();
    }
}
