using UnityEngine;

public class ParticleEffect : Effect
{
    public ParticleSystem[] particleSystems;
    public float duration;
    [SerializeField] protected float timer = 0;
    public override void Init()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        if (duration <= 0)
        {
            duration = particleSystems[0].main.duration;
        }
    }
    public override void Play()
    {
        base.Play();
        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Play();
        }
        timer = duration;
    }
    public override void Play(Vector2 point)
    {
        base.Play(point);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Play();
        }
        timer = duration;
    }


    private void Update()
    {
        if (timer <= 0)
        {
            EndEffect();
            return;
        }
        timer -= Time.deltaTime;
    }
}
