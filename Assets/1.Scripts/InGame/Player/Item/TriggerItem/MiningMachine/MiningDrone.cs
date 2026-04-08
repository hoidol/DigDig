using UnityEngine;

public class MiningDrone : MiningMachine
{
    public float moveSpeed = 4f;
    public float attackDist = 1f; // 이 거리 안에서만 공격

    protected override void Update()
    {
        FindAndMove();
        base.Update();
    }

    void FindAndMove()
    {
        if (targetOre == null) return;

        Vector2 dir = (targetOre.transform.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    protected override void Attack(OreStone ore)
    {
        float dist = Vector2.Distance(transform.position, ore.transform.position);
        if (dist > attackDist) return;

        ore.TakeDamage(new DamageData() { damage = attackPower });
    }
}
