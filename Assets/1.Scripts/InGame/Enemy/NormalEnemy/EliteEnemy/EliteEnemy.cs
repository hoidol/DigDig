using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using UnityEngine;

public class EliteEnemy : NormalEnemy, IEnemySpecialAttackPattern
{
    [SerializeField] EnemySpecialAttackPattern enemySpecialAttackPattern;
    CancellationTokenSource cts;
    public override void StartAttack()
    {
        base.StartAttack();

        cts = new CancellationTokenSource();
        ProcessAttack(cts.Token).Forget();
    }
    async UniTask ProcessAttack(CancellationToken ct)
    {
        await enemySpecialAttackPattern.Execute(this, () =>
        {

        });
        await UniTask.WaitForSeconds(enemySpecialAttackPattern.duration, cancellationToken: ct);
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
            enemies[i].OnDestroy();
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        BlessingStone.Instantiate(transform.position);
    }

    public float PlayAnim(string animName)
    {
        if (animator == null)
            return 0f;

        animator.Play(animName, -1, 0);
        float duration = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        return duration;

    }
}
