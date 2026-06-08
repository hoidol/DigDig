using DG.Tweening;
using UnityEngine;

public class NormalEnemy : Enemy
{
    //static readonly LayerMask physicAreaMask = LayerMask.GetMask("PhysicArea");

    //일정 시간(60초) 동안 파격 안받거나  플레이어와 너무 멀면 자동으로 사라지기

    public override void UpdateWaiting()
    {
        Vector2 vec = Player.Instance.transform.position - transform.position;

        if (vec.magnitude > enemyData.moveRange)
        {
            ChangeState(EnemyState.Moving);
            return;
        }
        if (vec.magnitude <= enemyData.attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }
        SetFacing(vec.x);
    }
    bool moving;
    public override void UpdateMoving()
    {
        Vector2Int curIndex = indexs[0];
        Vector2 vec = Player.Instance.transform.position - transform.position;
        //vec.normalized; //상하좌우로만 움직임
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

        for (int i = 0; i < dirs.Length; i++)
        {
            Vector2Int nextIndex = curIndex + dirs[i];
            if (MapManager.CheckEmpty(nextIndex))
            {
                MoveTo(nextIndex);
                return;
            }
        }


        //타일 격자로 움직여야돼
        if (vec.magnitude <= enemyData.attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }
        SetFacing(vec.x);

    }
    void MoveTo(Vector2Int nextIndex)
    {
        if (moving) return;
        moving = true;

        transform.DOMove(MapManager.IndexToPosition(nextIndex), 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                indexs[0] = nextIndex;
                moving = false;
            });
    }

    //너무 멀면 다가가기
}
