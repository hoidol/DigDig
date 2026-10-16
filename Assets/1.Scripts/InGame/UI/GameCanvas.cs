using UnityEngine;
using TMPro;

public class GameCanvas : MonoBehaviour
{
    public TMP_Text waveText;
    void Start()
    {
        
        waveText.text = $"WAVE 1";
        GameEventBus.Subscribe<PhaseStartEvent>(OnPhaseStartEvent);
    }
    void OnPhaseStartEvent(PhaseStartEvent e)
    {
        
        waveText.text = $"WAVE {e.phaseIdx+1}";
    }
}
