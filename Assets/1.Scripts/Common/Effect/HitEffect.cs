using UnityEngine;

public class HitEffect : ParticleEffect
{

    public override void Play(Vector2 point, Vector2 dir)
    {
        Play(point);
        transform.right = -dir;
    }
}
