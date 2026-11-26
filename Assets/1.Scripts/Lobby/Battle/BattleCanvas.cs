
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

    // (2026-11-24) 스테이지 이용권: 이용권이 필요한 스테이지면 Play 버튼 자리에 구매 버튼 → 누르면 바로 결제
    public Button playButton; // 비우면 OnClickedBtn이 연결된 하위 버튼 중 스테이지 이동 버튼이 아닌 것
    public Button stagePassButton; // 비우면 Play 버튼을 복사해 실행 중 생성
    public TMP_Text stagePassText; // 구매 버튼 글씨 (StagePass_Buy, {0} = 가격). 비우면 구매 버튼의 첫 글씨
    bool isPurchasingStagePass;

    // (2026-11-26) 하단 클리어 팁 띠지 (지금 스테이지·난이도). 씬에 만들어 연결 (Tools/Lobby/Add ClearTip Band To Lobby Scene)
    public ClearTipTicker clearTipTicker;

    void Awake()
    {
        if (clearTipTicker == null)
            clearTipTicker = GetComponent<ClearTipTicker>();
        // 씬의 스테이지 이동 버튼에 Play(OnClickedBtn)가 같이 연결돼 있으면 끔 (> 를 누르면 게임이 시작되던 문제)
        DisablePersistentCalls(preStageButton);
        DisablePersistentCalls(nextStageButton);
        EnsureStagePassButton();
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
        UpdateStagePassButton();
        if (clearTipTicker != null && clearTipTicker.isActiveAndEnabled)
            clearTipTicker.Refresh(); // 스테이지·난이도가 바뀌면 그 팁으로
    }

    // 클리어 해금은 됐는데 이용권이 없으면 Play 대신 이용권 구매 버튼
    void UpdateStagePassButton()
    {
        bool needPass = stageData.ChekcUnlock() && stageData.IsMissingStagePass;
        if (playButton != null)
            playButton.gameObject.SetActive(!needPass);
        if (stagePassButton == null)
            return;
        stagePassButton.gameObject.SetActive(needPass);
        if (needPass && stagePassText != null)
        {
            ProductData productData = GetStagePassProduct();
            string price = productData != null ? productData.GetPriceToString() : "";
            stagePassText.text = string.Format(TranslateManager.GetText("StagePass_Buy"), price);
        }
    }

    static void DisablePersistentCalls(Button button)
    {
        if (button == null)
            return;
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            if (button.onClick.GetPersistentMethodName(i) == nameof(OnClickedBtn))
                button.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
    }

    // Play 버튼 찾기 + 이용권 구매 버튼이 없으면 Play 버튼을 복사해 만듦 (에너지 표시·번역 글씨는 뺌)
    void EnsureStagePassButton()
    {
        if (playButton == null)
            foreach (Button button in GetComponentsInChildren<Button>(true))
            {
                if (button == preStageButton || button == nextStageButton)
                    continue;
                for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                    if (button.onClick.GetPersistentTarget(i) == this && button.onClick.GetPersistentMethodName(i) == nameof(OnClickedBtn))
                    {
                        playButton = button;
                        break;
                    }
                if (playButton != null)
                    break;
            }

        if (stagePassButton == null && playButton != null)
        {
            stagePassButton = Instantiate(playButton, playButton.transform.parent);
            stagePassButton.name = "StagePassButton";
            stagePassButton.transform.SetSiblingIndex(playButton.transform.GetSiblingIndex() + 1);
            stagePassButton.onClick = new Button.ButtonClickedEvent(); // 복사된 OnClickedBtn 연결 제거
            foreach (TranslateText translate in stagePassButton.GetComponentsInChildren<TranslateText>(true))
                Destroy(translate);
            Transform energe = FindChild(stagePassButton.transform, "EnergePanel");
            if (energe != null)
                energe.gameObject.SetActive(false);
            stagePassButton.gameObject.SetActive(false);
        }
        if (stagePassButton == null)
            return;
        if (stagePassText == null)
            foreach (TMP_Text text in stagePassButton.GetComponentsInChildren<TMP_Text>(true))
                if (!FindParent(text.transform, "EnergePanel"))
                {
                    stagePassText = text;
                    break;
                }
        stagePassButton.onClick.AddListener(BuyStagePass);
    }

    static Transform FindChild(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name)
                return t;
        return null;
    }

    static bool FindParent(Transform t, string name)
    {
        for (; t != null; t = t.parent)
            if (t.name == name)
                return true;
        return false;
    }

    static ProductData GetStagePassProduct()
        => ProductManager.Instance != null ? ProductManager.Instance.GetProductData(InAppProductInfo.STAGE_PASS_KEY) : null;

    public void OnClickedBtn()
    {
        if (!stageData.ChekcUnlock())
        {
            ToastCanvas.Toast(stageData.GetLockedMessage());
            return;
        }
        // 스테이지 2부터는 이용권 필요 (보통은 구매 버튼이 Play 자리를 대신함)
        if (stageData.IsMissingStagePass)
        {
            BuyStagePass();
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

    // 이용권 구매 버튼 → 바로 결제 → 구매 기록(UserPurchaseManager) → Play 버튼으로 바뀜 (게임은 바로 시작하지 않음)
    public void BuyStagePass()
    {
        if (isPurchasingStagePass || StageData.HasStagePass())
            return;
        ProductData productData = GetStagePassProduct();
        if (productData == null)
        {
            ToastCanvas.Toast(TranslateManager.GetText("Purchase_Unavailable"));
            return;
        }
        isPurchasingStagePass = true;
        BlockCanvas.Instance.OpenCanvas(TranslateManager.GetText("Purchasing"));
        ProductManager.Instance.TryToBuy(productData, async success =>
        {
            await UniTask.SwitchToMainThread(); // 결제 SDK 콜백은 메인 스레드가 아닐 수 있음
            isPurchasingStagePass = false;
            BlockCanvas.Instance.CloseCanvas();
            if (!success)
                return;
            if (!StageData.HasStagePass()) // 구매 복원과 겹쳐도 기록은 하나
                UserDataManager.Instance.userPurchaseManager.AddUserPurchase(productData);
            SoundManager.Play(SFXType.PurchaseComplete);
            ToastCanvas.Toast(TranslateManager.GetText("StagePass_Purchased"));
            UpdateCanvas();
        });
    }
}
