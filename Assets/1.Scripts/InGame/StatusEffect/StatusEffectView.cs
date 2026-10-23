using UnityEngine;

public class StatusEffectView : MonoBehaviour
{
    public string effectKey;

    public virtual void Play()
    {
        gameObject.SetActive(true);
    }
    public virtual void Stop()
    {
        gameObject.SetActive(false);
    }
}
