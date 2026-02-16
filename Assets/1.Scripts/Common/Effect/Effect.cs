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
    public virtual void Show(Vector3 point)
    {
        gameObject.SetActive(true);
        transform.position = point;
    }

    public virtual void Show(Vector3 point, Color color)
    {
        Show(point);
    }

    public virtual void EndEffect()
    {
        gameObject.SetActive(false);
    }
}
