using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EliteEnemy : Enemy
{
    List<Enemy> enemies = new List<Enemy>();
    public override void Spawn(Vector2Int[,] idxArr)
    {
        //이 자리에 있는 적들 미리 잡아두기
        enemies.Clear();
        for(int x= 0; x < idxArr.GetLength(0); x++)
        {
            for(int y= 0; y < idxArr.GetLength(1); y++)
            {
                Vector2Int tileIndex = new Vector2Int(x,y);
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
        base.Spawn(idxArr);
    }

    public override void Apear()
    {
        base.Apear();
        //현재 위치에 있는 모든 적들을 제거
        for(int i = 0; i < enemies.Count; i++)
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
