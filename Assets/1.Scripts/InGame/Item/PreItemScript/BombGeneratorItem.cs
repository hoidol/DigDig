using UnityEngine;

public class BombGeneratorItem : TriggerItem
{
    public BlackBomb bombPrefab;
    public float bombDamage = 15f;

    public override void OnTrigger()
    {// 현재 플레이어가 바라보는 방향
        Vector2 dir = Character.Instance.bodyRootTr.localScale.x >= 0
            ? Vector2.right
            : Vector2.left;

        var bomb = Object.Instantiate(bombPrefab);
        bomb.transform.position = Character.Instance.transform.position;
        bomb.Shoot(dir, bombDamage * count);
    }

}
