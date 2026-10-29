using UnityEngine;
using TMPro;

public class GameCanvas : MonoBehaviour
{
    public TMP_Text waveText;
    public GameObject buttonObject;
    public AllySettingPanel allySettingPanel;
    public TileItemInfoPanel tileItemInfoPanel;
    void Start()
    {

        waveText.text = $"WAVE 1";
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
        GameEventBus.Subscribe<TileSelectedEvent>(OnTileSelectedEvent);
        GameEventBus.Subscribe<TileDeselectedEvent>(OnTileDeselectedEvent);
        buttonObject.SetActive(true);
    }

    // 타일 선택 시 위 유닛 정보 + 타일(아이템) 정보. 빈 타일(유닛·아이템 없음)은 반응 없음
    void OnTileSelectedEvent(TileSelectedEvent e)
    {
        if (e.tile.tileObject == null && e.tile.tileItem == null)
        {
            OnTileDeselectedEvent(null);
            return;
        }

        buttonObject.SetActive(false);
        allySettingPanel.SetSlime(e.tile.tileObject as IAllyUnit);
        tileItemInfoPanel.SetTile(e.tile);
    }

    void OnTileDeselectedEvent(TileDeselectedEvent e)
    {
        buttonObject.SetActive(true);
        allySettingPanel.SetSlime(null);
        tileItemInfoPanel.SetTile(null);
    }

    void OnWaveStartEvent(WaveStartEvent e)
    {

        waveText.text = $"WAVE {e.waveIdx + 1}";
    }
}
