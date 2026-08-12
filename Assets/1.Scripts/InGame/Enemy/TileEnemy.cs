using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using TMPro;
public abstract class TileEnemy : Enemy, IHittable, ITile
{
    [Header("생성 시 타일을 부수면서 등장함")]
    public bool breakTileWhenSpawn;

    public Vector2Int[,] TileIndexArr => tileIndexArr;
    protected Vector2Int[,] tileIndexArr; //현재 차지하고 있는 위치
    public Vector2Int Size => enemyData.size;

    public bool BreakTileWhenSpawn => breakTileWhenSpawn;


    //적 생성 시 호출
    // public override void Spawn(Vector2Int[,] idxArr)
    // {
    //     base.Spawn(idxArr);
    //     tileIndexArr = new Vector2Int[enemyData.size.x, enemyData.size.y];

    //     RegisterTile(idxArr);

    //     gameObject.SetActive(false);
    //     Vector2 pos = MapManager.TileIndexToCenterPosition(idxArr);
    //     EnemySpawnIndicator.Get(pos, null).PlayIndicator(tileIndexArr, apearTime, () =>
    //     {
    //         Apear();
    //     });

    // }

    // const float APEAR_POP_DURATION = 0.15f;
    // public override void Apear()
    // {
    //     gameObject.SetActive(true);

    //     if (col2d != null)
    //         col2d.enabled = false;

    //     transform.localScale = Vector3.zero;
    //     transform.DOScale(1f, APEAR_POP_DURATION).SetEase(Ease.OutBack)
    //         .OnComplete(() =>
    //         {
    //             if (col2d != null)
    //                 col2d.enabled = true;
    //         });
    // }



    public static Vector2Int[] FindPath(Vector2 start, Vector2 end)
    {
        Vector2 vec = end - start;
        Vector2Int[] dirs = new Vector2Int[2];
        if (Mathf.Abs(vec.normalized.x) > Mathf.Abs(vec.normalized.y))
        {
            dirs[0] = vec.normalized.x > 0 ? Vector2Int.right : Vector2Int.left;
            dirs[1] = vec.normalized.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
        else
        {
            dirs[0] = vec.normalized.y > 0 ? Vector2Int.up : Vector2Int.down;
            dirs[1] = vec.normalized.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        return dirs;
    }




    public async virtual UniTask MoveTo(Vector2Int dir, float delaySec = 2)
    {
        Vector2Int[,] newTiles = MapManager.GetIndexArray(tileIndexArr, dir);
        MapManager.RegisterTile(newTiles, this); // 이동 중 다른 오브젝트가 점유 못하게 선점

        Vector2 dest = MapManager.TileIndexToCenterPosition(newTiles);
        UpdateFacing(dir);
        // await UniTask.Delay(3000);
        transform.DOMove(dest, 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                MapManager.ReleaseTile(tileIndexArr);//현재 위치 해제
                RegisterTile(newTiles); //이동한 위치 등록
            });

        await UniTask.Delay(TimeSpan.FromSeconds(delaySec));
    }

    public override void Destroy()
    {
        base.Destroy();
        ReleaseTile();
    }
    public bool CanHit()
    {
        return curHp > 0;
    }

    public virtual void RegisterTile(Vector2Int[,] idxArr)
    {
        tileIndexArr = idxArr;
        MapManager.RegisterTile(idxArr, this);
    }

    public virtual void ReleaseTile()
    {
        transform.DOKill();
        MapManager.ReleaseTile(tileIndexArr);
        gameObject.SetActive(false);
        GameEventBus.Publish(new EnemyDeadEvent(this));
    }


}
