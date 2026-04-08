using UnityEngine;

public class MagmaBall : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IHittable hittable))
        {
            //hittable.TakeDamage(Player.Instance.statMgr.AttackPower);
        }
    }
}
