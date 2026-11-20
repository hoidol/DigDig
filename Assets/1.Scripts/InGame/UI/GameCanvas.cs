using UnityEngine;
using TMPro;

public class GameCanvas : MonoBehaviour
{
    public TMP_Text waveText;
    // (2026-11-20) 킹 경험치바 = GameCanvas 화면 최상단. 씬 하위에 ExpBar가 있으면 그걸 쓰고, 없으면 Panel 맨 위에 생성
    public ExpBar expBar;
    const float EXP_BAR_HEIGHT = 26f;

    void Start()
    {

        waveText.text = $"WAVE 1";
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
        EnsureExpBar();
    }

    void OnWaveStartEvent(WaveStartEvent e)
    {

        waveText.text = $"WAVE {e.waveIdx + 1}";
    }

    void EnsureExpBar()
    {
        if (expBar == null)
            expBar = GetComponentInChildren<ExpBar>(true);
        if (expBar != null)
            return;

        Transform parent = waveText != null ? waveText.transform.parent : transform;
        GameObject go = new GameObject("ExpBar", typeof(RectTransform), typeof(ExpBar));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.SetAsFirstSibling();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.sizeDelta = new Vector2(0, EXP_BAR_HEIGHT);
        rt.anchoredPosition = Vector2.zero;
        expBar = go.GetComponent<ExpBar>();
    }
}
