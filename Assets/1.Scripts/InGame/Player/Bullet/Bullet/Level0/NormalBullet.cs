public class NormalBullet : Bullet
{
    public NormalBullet()
    {
        key = "Normal";
        bounceCount = Player.Instance.bounce;
    }
    public int bounceCount;
}
