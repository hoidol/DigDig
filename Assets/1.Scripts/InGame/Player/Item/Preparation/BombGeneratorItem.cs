using UnityEngine;

public class BombGeneratorItem : TriggerItem
{
    public BlackBomb bombPrefab;
    public float bombDamage = 15f;

    public override void OnUnequip(Player player)
    {
        base.OnUnequip(player);
    }
    public override void OnTrigger()
    {// 현재 플레이어가 바라보는 방향
        Vector2 dir = Player.Instance.bodyRootTr.localScale.x >= 0
            ? Vector2.right
            : Vector2.left;

        var bomb = Object.Instantiate(bombPrefab);
        bomb.transform.position = Player.Instance.transform.position;
        bomb.Shoot(dir, bombDamage * count);
    }

}
