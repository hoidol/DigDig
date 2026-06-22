using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using UnityEngine;

public class EliteEnemy : NormalEnemy
{
    [SerializeField] AttackPatternInfo attackPatternInfo;
    CancellationTokenSource cts;
    protected override void StartAttack()
    {
        base.StartAttack();

        cts = new CancellationTokenSource();
        ProcessAttack(cts.Token).Forget();
    }
    async UniTaskVoid ProcessAttack(CancellationToken ct)
    {
        animator.Play(attackPatternInfo.readyAnimName);
        await UniTask.WaitForSeconds(attackPatternInfo.readyTime, cancellationToken: ct);
        
        attackPatternInfo.attackPattern.Execute(this, () =>
        {
            
        });
        await UniTask.WaitForSeconds(attackPatternInfo.attackPattern.duration, cancellationToken: ct);
        EndAttack();
    }

    public override void CancelAttack()
    {
        base.CancelAttack();
        EndAttack();
    }

    public override void Apear()
    {
        base.Apear();

        List<Enemy> enemies = new List<Enemy>();

        enemies.Clear();
        for (int x = 0; x < tileIndexArr.GetLength(0); x++)
        {
            for (int y = 0; y < tileIndexArr.GetLength(1); y++)
            {
                Vector2Int tileIndex = new Vector2Int(x, y);
                if (!MapManager.CheckEmpty(tileIndex))
                {
                    Enemy e = EnemyManager.Instance.GetEnemyInTileIndex(tileIndex);
                    if (!enemies.Contains(e))
                    {
                        enemies.Add(e);
                    }
                }
            }
        }
        //현재 위치에 있는 모든 적들을 제거
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].OnDead();
        }
    }

    public override void OnDead()
    {
        base.OnDead();
        BlessingStone.Instantiate(transform.position);
    }
}
