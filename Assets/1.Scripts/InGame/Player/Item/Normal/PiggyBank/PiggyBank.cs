using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 골드를 줍고 플레이어에게 전달한 뒤 사라지는 저금통
public class PiggyBank : MonoBehaviour, IPicker
{
    public Transform Transform => transform;

    int maxPickCount;
    float moveSpeed;
    Action onFinished;

    int pickedCount;
    readonly List<Collider2D> claimedGolds = new();
    CancellationTokenSource cts;

    static readonly int pickableLayer = -1; // 초기화는 Init에서

    public void Init(int maxPickCount, float moveSpeed, Action onFinished)
    {
        this.maxPickCount = maxPickCount;
        this.moveSpeed = moveSpeed;
        this.onFinished = onFinished;
        pickedCount = 0;
        claimedGolds.Clear();

        cts = new CancellationTokenSource();
        CollectLoop(cts.Token).Forget();
    }

    public void UpdateStats(int newMaxPickCount, float newMoveSpeed)
    {
        maxPickCount = newMaxPickCount;
        moveSpeed = newMoveSpeed;
    }

    public void PickUp(IPickable pickable)
    {
        pickable.PickedUp();
        Player.Instance.AddGold(1);
        pickedCount++;

        if (pickedCount >= maxPickCount)
        {
            cts?.Cancel();
            Destroy(gameObject);
            onFinished?.Invoke();
        }
    }

    async UniTaskVoid CollectLoop(CancellationToken token)
    {
        int mask = LayerMask.GetMask("Pickable");

        while (!token.IsCancellationRequested)
        {
            Collider2D target = FindNearestGold(mask);

            if (target == null)
            {
                await UniTask.Yield(token);
                continue;
            }

            IPickable pickable = target.GetComponent<IPickable>();

            // 목표 골드로 이동
            await MoveToward(pickable, token);
            if (token.IsCancellationRequested) return;

            if (pickable != null && !pickable.IsTaken)
                pickable.Take(this);

            await UniTask.Yield(token);
        }
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
