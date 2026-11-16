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


    // 대상(미니언 등)을 따라다니며 duration초 재생. 대상이 꺼지면 같이 끝남
    Transform followTarget;
    public Transform FollowTarget => followTarget;

    public void PlayFollow(Transform target, float playDuration)
    {
        followTarget = target;
        Play(target.position);
        timer = playDuration;
    }

    public override void EndEffect()
    {
        followTarget = null;
        base.EndEffect();
    }

    void LateUpdate()
    {
        if (followTarget == null)
            return;
        if (!followTarget.gameObject.activeInHierarchy)
        {
            EndEffect();
            return;
        }
        transform.position = followTarget.position;
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
