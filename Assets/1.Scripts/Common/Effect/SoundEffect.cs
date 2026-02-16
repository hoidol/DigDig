using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    public AudioSource audioSource;
    public bool playOnAwake;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnEnable()
    {
        if (playOnAwake)
        {
            audioSource.Play();
        }
    }
}
