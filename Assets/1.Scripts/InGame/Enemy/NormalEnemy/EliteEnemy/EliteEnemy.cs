using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using UnityEngine;
using System;

public class EliteEnemy : NormalEnemy, IEnemySpecialAttackPattern
{
    [SerializeField] float patternCoolTime;
    [SerializeField] EnemyAttackPattern enemyAttackPattern;
    CancellationTokenSource cts;
    public override void StartAttack()
    {
        base.StartAttack();

        cts = new CancellationTokenSource();
        ProcessAttack(cts.Token).Forget();
    }
    async UniTask ProcessAttack(CancellationToken ct)
    {
        await enemyAttackPattern.Execute(this, () =>
        {

        });

        await UniTask.Delay(TimeSpan.FromSeconds(patternCoolTime));

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

        //출현했을때 해당 자리에 있는 Enemy 제거
        // List<Enemy> enemies = new List<Enemy>();

        // enemies.Clear();
        // for (int x = 0; x < tileIndexArr.GetLength(0); x++)
        // {
        //     for (int y = 0; y < tileIndexArr.GetLength(1); y++)
        //     {
        //         Vector2Int tileIndex = new Vector2Int(x, y);
        //         if (!MapManager.CheckEmpty(tileIndex))
        //         {
        //             Enemy e = EnemySpawner.Instance.GetEnemyInTileIndex(tileIndex);
        //             if (e == this)
        //                 continue;

        //             if (!enemies.Contains(e))
        //                 enemies.Add(e);

        //         }
        //     }
        // }
        //현재 위치에 있는 모든 적들을 제거
        // for (int i = 0; i < enemies.Count; i++)
        // {
        //     enemies[i].Destroy();
        // }
    }

    public float PlayAnim(string animName)
    {
        if (animator == null)
            return 0f;

        animator.Play(animName, -1, 0);
        float duration = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        return duration;

    }
    public override void Reward()
    {
        base.Reward();
        EnhanceStone enhanceStone = EnhanceStone.Instantiate();
        enhanceStone.transform.position = transform.position;
    }
}
