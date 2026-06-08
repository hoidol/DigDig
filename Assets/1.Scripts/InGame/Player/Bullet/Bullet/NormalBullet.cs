public class NormalBullet : Bullet
{
    // public int baseBounceCount = 0;


    // public override void OnBulletFired(PlayerBullet bullet)
    // {
    //     bullet.AddBehavior(new BounceBehavior(baseBounceCount));
    // }

    public override PlayerBulletObject GetBulletObject()
    {
        return null;
    }
}
