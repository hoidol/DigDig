using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class NormalEnemy : Enemy
{
    //static readonly LayerMask physicAreaMask = LayerMask.GetMask("PhysicArea");
    const int CHECK_INTERVAL_MS = 200;
    [SerializeField] bool pathClear;
    CancellationTokenSource cts;

    public override void Spawn(Vector2 pos)
    {
        base.Spawn(pos);
        pathClear = false;
        cts = new CancellationTokenSource();
        PathCheckLoop(cts.Token).Forget();
    }
    protected override void OnDead(DamageData damage)
    {
        base.OnDead(damage);
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    async UniTaskVoid PathCheckLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (Player.Instance != null)
            {
                Vector2 toPlayer = (Vector2)Player.Instance.transform.position - (Vector2)transform.position;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, toPlayer.magnitude, LayerMask.GetMask("PhysicArea"));
                pathClear = hit.collider == null;
            }

            await UniTask.Delay(CHECK_INTERVAL_MS, cancellationToken: token);
        }
    }

    public override void UpdateApproaching()
    {


        Vector2 vec = Player.Instance.transform.position - transform.position;
        if (vec.magnitude <= enemyData.attackRange && pathClear)
        {
            ChangeState(EnemyState.Attack);
            return;
        }
        SetFacing(vec.x);
        rg2d.linearVelocity = vec.normalized * (enemyData.moveSpeed * statusEffectHandler.SlowRate);
    }
}
