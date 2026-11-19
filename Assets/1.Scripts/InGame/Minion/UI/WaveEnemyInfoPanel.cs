using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 다음(또는 지금) wave 일반 적의 방어 타입 안내
// (2026-11-20) 카드 소환 모드: 브레이크타임이 되면 다음 wave 정보를 띄움 → X(closeButton)로 닫기, wave 시작 시 자동으로 닫힘,
// 필드의 일반 적(NormalEnemy)을 누르면 다시 보임 (MinionFieldController.BeginDrag → ShowByEnemyClick)
// 카드 소환 모드가 아니면 이전처럼 wave 시작 시 SHOW_TIME초 동안 보임
public class WaveEnemyInfoPanel : MonoBehaviour
{
    public static WaveEnemyInfoPanel Instance { get; private set; }

    public GameObject obj;
    public DefenceTypePanel defenceTypePanel;
    public Button closeButton; // X 버튼 (비워도 됨)
    // public TMP_Text waveText;
    // public TMP_Text defenceTypeDesc;
    // public Image thumImage;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        obj.SetActive(false);
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStartEvent);
        GameEventBus.Subscribe<BreakTimeStartEvent>(OnBreakTimeStartEvent);
    }

    void OnDestroy()
    {
        GameEventBus.Unsubscribe<WaveStartEvent>(OnWaveStartEvent);
        GameEventBus.Unsubscribe<BreakTimeStartEvent>(OnBreakTimeStartEvent);
        if (Instance == this)
            Instance = null;
    }

    const float SHOW_TIME = 10f;
    Coroutine showCo;

    void OnWaveStartEvent(WaveStartEvent e)
    {
        if (GameSetting.USE_WAVE_MINION_OFFER)
        {
            Hide();
            return;
        }
        SetWaveInfo(e.waveIdx);
        StopShowCo();
        showCo = StartCoroutine(ShowCo());
    }

    void OnBreakTimeStartEvent(BreakTimeStartEvent e)
    {
        Show(e.nextWaveIdx);
    }

    // 브레이크타임 = 다음 wave, wave 중 = 지금 wave 정보
    public void ShowByEnemyClick()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || !gm.isPlaying)
            return;
        Show(gm.isBreakTime ? gm.breakTimeNextWaveIdx : gm.waveIdx);
    }

    public void Show(int waveIdx)
    {
        StopShowCo();
        SetWaveInfo(waveIdx);
        obj.SetActive(true);
    }

    public void Hide()
    {
        StopShowCo();
        obj.SetActive(false);
    }

    void StopShowCo()
    {
        if (showCo != null)
            StopCoroutine(showCo);
        showCo = null;
    }

    // obj를 SHOW_TIME초 동안 보여준 뒤 숨김 (도중에 다시 호출되면 처음부터 다시 셈)
    IEnumerator ShowCo()
    {
        obj.SetActive(true);
        yield return new WaitForSeconds(SHOW_TIME);
        obj.SetActive(false);
        showCo = null;
    }

    // 방어 타입은 wave 번호로 정해지므로 아직 시작 전인 wave도 표시 가능
    public void SetWaveInfo(int w)
    {
        // waveText.text = string.Format(TranslateManager.GetText("WaveEnemyInfo"), w+1);
        defenceTypePanel.SetDefenceType(GameManager.GetWaveDefenceType(w));
    }
}
