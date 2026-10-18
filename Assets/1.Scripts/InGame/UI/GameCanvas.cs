using UnityEngine;
using TMPro;

public class GameCanvas : MonoBehaviour
{
    public TMP_Text waveText;
    void Start()
    {

        waveText.text = $"WAVE 1";
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
    }
    void OnWaveStartEvent(WaveStartEvent e)
    {

        waveText.text = $"WAVE {e.waveIdx + 1}";
    }
}
