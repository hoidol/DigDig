using UnityEngine;
using TMPro;

public class GameCanvas : MonoBehaviour
{
    public TMP_Text waveText;
    public GameObject buttonObject;
    public AllySettingPanel allySettingPanel;
    void Start()
    {

        waveText.text = $"WAVE 1";
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
        GameEventBus.Subscribe<SetTargetEvent>(OnSetTargetEvent);
        GameEventBus.Subscribe<ReleaseTargetEvent>(OnReleaseTargetEvent);
        buttonObject.SetActive(true);
    }

    void OnSetTargetEvent(SetTargetEvent e)
    {
        buttonObject.SetActive(false);
        allySettingPanel.SetSlime(e.unit);
    }

    void OnReleaseTargetEvent(ReleaseTargetEvent e)
    {
        buttonObject.SetActive(true);
        allySettingPanel.SetSlime(null);
    }

    void OnWaveStartEvent(WaveStartEvent e)
    {

        waveText.text = $"WAVE {e.waveIdx + 1}";
    }
}
