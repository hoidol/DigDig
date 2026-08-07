using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 골드를 자동으로 수집. 골드 없으면 플레이어 오른쪽 상단에 대기.
public class PiggyBank : MonoBehaviour, IPicker
{
    public Transform Transform => transform;

    float moveSpeed = 4f;

    readonly List<Collider2D> claimedGolds = new();
    CancellationTokenSource cts;

    static readonly Vector3 IDLE_OFFSET = new Vector3(0.8f, 0.8f, 0);

    public void Init(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
        claimedGolds.Clear();

        cts?.Cancel();
        cts = new CancellationTokenSource();
        CollectLoop(cts.Token).Forget();
    }

    public void PickUp(IPickable pickable)
    {
        pickable.PickedUp();
        // Player.Instance.AddGold(1);
    }

    async UniTaskVoid CollectLoop(CancellationToken token)
    {
        int mask = LayerMask.GetMask("Pickable");

        while (!token.IsCancellationRequested)
        {
            Collider2D target = FindNearestGold(mask);

            if (target == null)
            {
                MoveToIdlePosition();
                await UniTask.Yield(token);
                continue;
            }

            IPickable pickable = target.GetComponent<IPickable>();
            await MoveToward(pickable, token);
            if (token.IsCancellationRequested) return;

            if (pickable != null && !pickable.IsTaken)
                pickable.Take(this);

            await UniTask.Yield(token);
        }
    }

    void MoveToIdlePosition()
    {
        Vector3 target = Character.Instance.transform.position + IDLE_OFFSET;
        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    Collider2D FindNearestGold(int mask)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, 20f, mask);

        Collider2D nearest = null;
        float minDist = float.MaxValue;

        foreach (var col in cols)
        {
            if (col.tag != "Gold") continue;

            IPickable pickable = col.GetComponent<IPickable>();
            if (pickable == null || pickable.IsTaken) continue;
            if (claimedGolds.Contains(col)) continue;

            float dist = Vector2.SqrMagnitude((Vector2)col.transform.position - (Vector2)transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = col;
            }
        }

        if (nearest != null)
            claimedGolds.Add(nearest);

        return nearest;
    }

    async UniTask MoveToward(IPickable pickable, CancellationToken token)
    {
        const float arriveDist = 0.3f;

        while (!token.IsCancellationRequested)
        {
            if (!pickable.Transform.gameObject.activeSelf || pickable.IsTaken) break;
            if (Vector2.Distance(transform.position, pickable.Transform.position) <= arriveDist) break;

            transform.position = Vector2.MoveTowards(
                transform.position, pickable.Transform.position, moveSpeed * Time.deltaTime);

            await UniTask.Yield(token);
        }
    }

    void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
