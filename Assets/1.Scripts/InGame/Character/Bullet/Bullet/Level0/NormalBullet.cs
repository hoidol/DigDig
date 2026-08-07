public class NormalBullet : Bullet
{
    public NormalBullet()
    {
        key = "Normal";
        bounceCount = Character.Instance.statMgr.Bounce;
    }
    public int bounceCount;
}
