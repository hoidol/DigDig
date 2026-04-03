using UnityEngine;

public class StatusEffectView : MonoBehaviour
{
    public string effectKey;

    public void Play()
    {
        gameObject.SetActive(true);
    }
    public void Stop()
    {
        gameObject.SetActive(false);
    }
}
