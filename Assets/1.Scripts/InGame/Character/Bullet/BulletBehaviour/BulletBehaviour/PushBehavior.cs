using UnityEngine;
public class PushBehavior : IBulletBehavior
{
    float power;
    public PushBehavior(float power) {this.power = power;}

    public bool OnHit(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 pushDir)
    {
        Enemy enemy = hit.Transform.GetComponent<Enemy>();
        if(enemy != null)
            enemy.Push(pushDir,power);
        // if (remaining-- <= 0)
        //     return true;
        // hit.
        // bullet.Bounce(hit2D);
        return false;
    }
}
