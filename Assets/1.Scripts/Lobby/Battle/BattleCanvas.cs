
using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Serialization;
public class BattleCanvas : BaseLobbyCanvas
{
    // < 이전 스테이지 / > 다음 스테이지. 그 방향에 스테이지 데이터가 없으면 숨김 (출시 땐 스테이지 1만 → 둘 다 숨김)
    // > 는 다음 스테이지가 잠겨 있어도 보이고, 누르면 클리어 조건 토스트
    public Button preStageButton;
    public Button nextStageButton;
    // public StageRewardContainer stageRewardContainer;

    [SerializeField] UserStage curUserStage;
    public StageData stageData;
    [FormerlySerializedAs("titleText")]
    public TMP_Text stageText;      // "스테이지 N" (Stage_Title). 난이도 이름은 DifficultyDisplayButton이 표시


    public Image stageThumImage;
    public GameObject stageLockedGO;

    // (2026-11-27) 스테이지 이용권 삭제 (모든 스테이지 무료, 클리어 해금만)

    // (2026-11-26) 하단 클리어 팁 띠지 (지금 스테이지·난이도). 씬에 만들어 연결 (Tools/Lobby/Add ClearTip Band To Lobby Scene)
    public ClearTipTicker clearTipTicker;

    void Awake()
    {
        if (clearTipTicker == null)
            clearTipTicker = GetComponent<ClearTipTicker>();
        // 씬의 스테이지 이동 버튼에 Play(OnClickedBtn)가 같이 연결돼 있으면 끔 (> 를 누르면 게임이 시작되던 문제)
        DisablePersistentCalls(preStageButton);
        DisablePersistentCalls(nextStageButton);
        if (preStageButton != null)
            preStageButton.onClick.AddListener(() => MoveStage(-1));
        if (nextStageButton != null)
            nextStageButton.onClick.AddListener(() => MoveStage(1));
    }

    void Start()
    {
        GameEventBus.Subscribe<ChangedDifficultyEvent>(OnChangedDifficultyEvent);
    }

    void OnDestroy()
    {
        GameEventBus.Unsubscribe<ChangedDifficultyEvent>(OnChangedDifficultyEvent);
    }

    void OnChangedDifficultyEvent(ChangedDifficultyEvent e)
    {
        RefreshStage();
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        RefreshStage();
    }

    // dir = -1 이전 / +1 다음 스테이지. 난이도는 그 스테이지에 있으면 유지, 없으면 Normal
    public void MoveStage(int dir)
    {
        int order = StageManager.Instance.GetNeighborStageOrder(UserDataManager.stageOrder, dir);
        if (order < 0)
            return;
        // 다음 스테이지가 아직 잠김(이 스테이지 Normal 미클리어) → 이동하지 않고 클리어 필요 토스트
        if (dir > 0)
        {
            StageData nextNormal = StageManager.Instance.GetStageData(order, DifficultyType.Normal);
            if (nextNormal != null && !nextNormal.ChekcUnlock())
            {
                ToastCanvas.Toast(nextNormal.GetLockedMessage());
                SoundManager.Play(SFXType.Fail_NotEnough);
                return;
            }
        }

        UserDataManager.stageOrder = order;
        SoundManager.Play(SFXType.TabChange);
        if (StageManager.Instance.GetStageData(order, UserDataManager.difficultyType) == null)
        {
            // 그 스테이지에 지금 난이도가 없으면 Normal → DifficultyDisplayButton도 갱신되게 이벤트로 (구독한 RefreshStage가 화면 갱신)
            UserDataManager.difficultyType = DifficultyType.Normal;
            GameEventBus.Publish(new ChangedDifficultyEvent(DifficultyType.Normal));
            return;
        }
        RefreshStage();
    }

    // 선택한 스테이지·난이도 데이터가 없으면 스테이지 1로 되돌림
    void RefreshStage()
    {
        StageManager stageManager = StageManager.Instance;
        if (stageManager.GetCurrentStageData() == null)
        {
            if (!stageManager.HasStage(UserDataManager.stageOrder))
                UserDataManager.stageOrder = StageData.FIRST_STAGE_ORDER;
            if (stageManager.GetCurrentStageData() == null)
                UserDataManager.difficultyType = DifficultyType.Normal;
        }
        SetUserStage(UserDataManager.Instance.userStageManager.GetCurrentStage());
    }

    public void SetUserStage(UserStage userStage)
    {
        curUserStage = userStage;
        UserStageManager.TryParseKey(curUserStage.key, out int order, out DifficultyType difficultyType);
        stageData = StageManager.Instance.GetStageData(order, difficultyType);
        // 로비 보스 = 이 스테이지에서 지금 잡을 보스 (연출 중이면 연출을 끝내고 바로 평소 화면)
        LobbyReturnController controller = LobbyReturnController.Instance;
        if (controller != null)
            controller.ShowStage(curUserStage);
        // stageRewardContainer.SetStageData(curUserStage, stageData);
        UpdateCanvas();
    }

    public override void UpdateCanvas()
    {
        if (stageData == null)
            return;
        // stageRewardContainer.UpdateContainer();
        if (stageText != null)
            stageText.text = string.Format(TranslateManager.GetText("Stage_Title"), stageData.DisplayNumber);

        if (preStageButton != null)
            preStageButton.gameObject.SetActive(StageManager.Instance.GetNeighborStageOrder(stageData.order, -1) >= 0);
        if (nextStageButton != null)
            nextStageButton.gameObject.SetActive(StageManager.Instance.GetNeighborStageOrder(stageData.order, 1) >= 0);

        stageLockedGO.SetActive(!stageData.ChekcUnlock());
        if (clearTipTicker != null && clearTipTicker.isActiveAndEnabled)
            clearTipTicker.Refresh(); // 스테이지·난이도가 바뀌면 그 팁으로
    }

    static void DisablePersistentCalls(Button button)
    {
        if (button == null)
            return;
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            if (button.onClick.GetPersistentMethodName(i) == nameof(OnClickedBtn))
                button.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
    }

    public void OnClickedBtn()
    {
        if (!stageData.ChekcUnlock())
        {
            ToastCanvas.Toast(stageData.GetLockedMessage());
            return;
        }
        // 게임 시작 = 에너지 PLAY_COST_ENERGE 소모. 모자라면 시작 안 함
        if (!EnergeManager.TryUsePlayCost())
        {
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("Energe_NotEnough"), EnergeManager.CHARGE_INTERVAL_MINUTES));
            SoundManager.Play(SFXType.Fail_NotEnough);
            return;
        }
        SceneManager.LoadScene("InGame");
    }
}
