using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

// 곡괭이 - 근처 광석으로 직접 이동해서 공격, 없으면 플레이어 위치로 복귀
public class PickaxeItem : Item
{
    const float SEARCH_RADIUS = 5f;
    const float MOVE_SPEED = 8f;
    const float ARRIVE_DIST = 0.3f;
    const float ATTACK_INTERVAL = 0.8f;

    readonly DamageData damageData = new();
    CancellationTokenSource cts;
    float[] multiples = { 0.5f, 1, 1.5f };
    public override void OnEquip(Player player)
    {
        cts = new CancellationTokenSource();
        transform.parent = null;
        MineLoop(cts.Token).Forget();
    }

    public override void OnUnequip(Player player)
    {
        cts?.Cancel();
        cts = null;
    }

    async UniTaskVoid MineLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            OreStone target = FindNearestOre();

            if (target != null)
            {
                await MoveToward(target.transform.position, token);
                if (token.IsCancellationRequested) return;

                damageData.damage = Player.Instance.statMgr.AttackPower * multiples[count - 1];
                target.TakeDamage(damageData);
                await UniTask.Delay((int)(ATTACK_INTERVAL * 1000), cancellationToken: token);
            }
            else
            {
                //await MoveToward(Player.Instance.transform.position, token);
                if (token.IsCancellationRequested) return;
                await UniTask.Yield(token);
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

            float dist = ((Vector2)ore.transform.position - (Vector2)Player.Instance.transform.position).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                nearest = ore;
            }
        }
        return nearest;
    }
}
