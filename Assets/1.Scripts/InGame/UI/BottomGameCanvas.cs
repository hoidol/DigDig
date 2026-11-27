using UnityEngine.Serialization;
using UnityEngine;
using TMPro;

public class BottomGameCanvas : MonoBehaviour
{
    public GameObject buttonObject; // 기존 버튼 줄(Buttons_Hor). 카드 소환 모드에선 항상 숨김
    public GameObject offerButtonObject; // 카드 소환 모드 버튼 줄(OfferButtons: 카드·Next·주사위 코인·경험치 구매). 타일 선택 중·wave 중 숨김
    public AllySettingPanel allySettingPanel;
    [FormerlySerializedAs("tileItemInfoPanel")] public TileRuneInfoPanel tileRuneInfoPanel;
    public GameObject hideGameObject; // allySettingPanel이 보이는 동안 숨김. 비우면 하위의 "HideGame"을 찾음
    // (2026-11-24) wave 중 하단 UI (슬라임 체력·용사 에너지·스킬 버튼). 비우면 실행 중 OfferButtons 자리에 생성
    public HeroSkillHud heroSkillHud;
    bool waveActive;

    void Start()
    {
        if (hideGameObject == null)
            hideGameObject = FindChild("HideGame");

        GameEventBus.Subscribe<TileSelectedEvent>(OnTileSelectedEvent);
        GameEventBus.Subscribe<TileDeselectedEvent>(OnTileDeselectedEvent);
        // (2026-11-19) 카드 소환 모드: 기존 버튼 줄은 게임 내내 숨기고, 타일 선택에 따른 숨김/표시는 OfferButtons가 대신함.
        // OfferButtons는 게임 시작 때 숨겨 두고 브레이크타임(카드 지급, BreakTimeStartEvent)에만 보임
        // (2026-11-22) wave 중엔 OfferButtons 전체를 끔 (WaveStartEvent)
        if (UseOfferButtons)
        {
            buttonObject.SetActive(false);
            if (heroSkillHud == null)
                heroSkillHud = HeroSkillHud.Create(offerButtonObject.transform as RectTransform);
            GameEventBus.Subscribe<BreakTimeStartEvent>(OnBreakTimeStartEvent);
            GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
        }
        else
            offerReady = true;
        UpdateButtonObject();
        // 시작 시 allySettingPanel은 닫혀 있음(AllySettingPanel.Start가 끄지만 Start 순서가 보장되지 않아 직접 켬)
        if (hideGameObject != null)
            hideGameObject.SetActive(true);
    }

    bool UseOfferButtons => GameSetting.USE_WAVE_MINION_OFFER && offerButtonObject != null;
    GameObject ToggleButtonObject => UseOfferButtons ? offerButtonObject : buttonObject;
    bool offerReady; // 카드 소환 모드: 브레이크타임에만 true (첫 카드 지급 전·wave 중엔 OfferButtons 숨김)
    bool tileSelected;

    void OnDestroy()
    {
        GameEventBus.Unsubscribe<BreakTimeStartEvent>(OnBreakTimeStartEvent);
        GameEventBus.Unsubscribe<WaveStartEvent>(OnWaveStartEvent);
    }

    void OnWaveStartEvent(WaveStartEvent e)
    {
        offerReady = false;
        waveActive = true;
        UpdateButtonObject();
    }

    void OnBreakTimeStartEvent(BreakTimeStartEvent e)
    {
        offerReady = true;
        waveActive = false;
        UpdateButtonObject();
    }

    // 버튼 줄 = 시작 준비가 됐고 타일 선택 중이 아닐 때만 보임
    void UpdateButtonObject()
    {
        ToggleButtonObject.SetActive(offerReady && !tileSelected);
        // wave 중 하단 = 슬라임 체력·에너지·스킬 (타일 선택 중엔 유닛 정보에 자리를 내줌)
        if (heroSkillHud != null)
            heroSkillHud.gameObject.SetActive(waveActive && !tileSelected);
    }

    GameObject FindChild(string childName)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            if (t.name == childName)
                return t.gameObject;
        Debug.LogWarning($"[BottomGameCanvas] 하위에 {childName} 오브젝트가 없습니다.");
        return null;
    }

    // allySettingPanel이 보이면 HideGame 끄고, 닫히면 다시 켬
    void UpdateHideGame()
    {
        if (hideGameObject == null)
            return;
        hideGameObject.SetActive(!allySettingPanel.obj.activeSelf);
    }

    // 타일 선택 시 위 유닛 정보 + 타일(아이템) 정보. 빈 타일(유닛·아이템 없음)은 반응 없음 (슬라임 버프 칸이면 버프 칸 정보)
    void OnTileSelectedEvent(TileSelectedEvent e)
    {
        if (e.tile.tileObject == null && e.tile.tileRune == null && !EliteArea.IsEliteTile(e.tile))
        {
            OnTileDeselectedEvent(null);
            return;
        }

        tileSelected = true;
        UpdateButtonObject();
        allySettingPanel.SetMinion(e.tile.tileObject as IAllyUnit);
        tileRuneInfoPanel.SetTile(e.tile);
        UpdateHideGame();
    }

    void OnTileDeselectedEvent(TileDeselectedEvent e)
    {
        tileSelected = false;
        UpdateButtonObject();
        allySettingPanel.SetMinion(null);
        tileRuneInfoPanel.SetTile(null);
        UpdateHideGame();
    }

}
