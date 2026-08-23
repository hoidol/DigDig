using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    public SFXData sfxData;
    public AudioSource[] audioSources;
    public void Start()
    {
        int count = 5;
        audioSources = new AudioSource[count];
        for (int i = 0; i < count; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].clip = sfxData.audioClip;
            audioSources[i].playOnAwake = sfxData.playOnAwake;
            audioSources[i].loop = sfxData.loop;
            audioSources[i].pitch = sfxData.pitch;
            audioSources[i].volume = sfxData.volume;

        }
    }

    public void Play()
    {
        AudioSource audioSource = GetAudioSource();
        audioSource.Play();

    }

    AudioSource GetAudioSource()
    {
        foreach (var source in audioSources)
        {
            if (!source.isPlaying)
                return source;
        }

        AudioSource shortest = audioSources[0];
        float shortestRemaining = shortest.clip.length - shortest.time;
        foreach (var source in audioSources)
        {
            float remaining = source.clip.length - source.time;
            if (remaining < shortestRemaining)
            {
                shortest = source;
                shortestRemaining = remaining;
            }
        }
        return shortest;
    }
}
