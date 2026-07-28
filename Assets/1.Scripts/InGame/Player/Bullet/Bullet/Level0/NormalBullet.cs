public class NormalBullet : Bullet
{
    public NormalBullet()
    {
        key = "Normal";
        bounceCount = Player.Instance.statMgr.Bounce;
    }
    public int bounceCount;
}
