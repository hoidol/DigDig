using UnityEngine;

public class BlackBomb : MonoBehaviour
{
    public Rigidbody2D rg2d;
    float damage;
    public void Shoot(Vector2 dir, float damage)
    {
        rg2d.AddForce(dir);
        this.damage = damage;
    }


}