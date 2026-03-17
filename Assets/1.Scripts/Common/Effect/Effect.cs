using UnityEngine;

public class Effect : MonoBehaviour
{
    public EffectType effectType;
    private void Awake()
    {
        Init();
    }
    public virtual void Init()
    {

    }
    public virtual void Play(Vector2 point)
    {
        gameObject.SetActive(true);
        transform.position = point;
    }

    public virtual void Play(Vector2 point, Color color)
    {
        Play(point);
    }
    public virtual void Play(Vector2 point, Vector2 dir)
    {
        Play(point);
    }

    public virtual void EndEffect()
    {
        gameObject.SetActive(false);
    }
}
