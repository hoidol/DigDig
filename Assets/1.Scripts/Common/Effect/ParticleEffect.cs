using UnityEngine;

public class ParticleEffect : Effect
{
    ParticleSystem[] particleSystems;
    public float duration;
    public override void Init()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        if(duration <= 0)
        {
            duration = particleSystems[0].main.duration;
        }
    }

    public override void Show(Vector3 point)
    {
        base.Show(point);
        for(int i =0;i< particleSystems.Length; i++)
        {
            particleSystems[i].Play();
        }
        timer = duration;
    }

    float timer = 0;
    private void Update()
    {
        if(timer <= 0)
        {
            EndEffect();
            return;
        }
        timer -= Time.deltaTime;
    }
}
