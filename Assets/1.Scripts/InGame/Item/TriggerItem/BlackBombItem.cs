using UnityEngine;

public class BlackBombItem : TriggerItem
{
    public BlackBomb bombPrefab;
    public float bombDamage = 20f;
    public float bombSpeed = 8f;

    public override void OnTrigger()
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        var bomb = Instantiate(bombPrefab);
        bomb.transform.position = Player.Instance.transform.position;
        bomb.Shoot(randomDir, bombDamage);
    }
}
