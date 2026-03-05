using UnityEngine;

public class MeleeEnemy : Enemy
{

    public override void UpdateAttack()
    {

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player.Instance.TakeDamage(enemyData.GetAttackPower(1));
            gameObject.SetActive(false);
        }
    }
}