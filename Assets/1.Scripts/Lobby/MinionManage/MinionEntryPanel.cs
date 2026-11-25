using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Lobby
{
    // 미니언 관리 화면의 미니언 칸 (2026-11-24 Growth1MinionEntryPanel 통합)
    // 보유 / 구매 가능(유저 레벨 도달) / 잠김(레벨 미달) 상태마다 씬에서 연결한 오브젝트를 켜고 끔 (ownedObject·buyableObject·lockedObject)
    // 카드 어디를 눌러도 정보 창이 열리도록 칸 루트에서 클릭을 받는다.
    // (썸네일 버튼이 받는 클릭은 그 버튼이 처리하고, 카드 틀·글씨 등 나머지 영역의 클릭이 여기로 올라온다)
    public class MinionEntryPanel : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            // 스크롤 드래그 끝에 오는 클릭은 무시
            if (eventData.dragging)
                return;
            OnClickedPanel();
        }

        public MinionPanel minionPanel;
        public MinionData minionData;
        public  UserMinion userMinion;
        public void SetData(MinionData minionData)
        {
            this.minionData = minionData;
            if(minionData == null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
            userMinion = UserDataManager.Instance.userMinionManager.GetUserMinion(minionData.key); 
            if(minionPanel == null)
            {
                Debug.Log("MinionEntryPanel if(minionPanel == null)");
            }
            minionPanel.SetData(minionData);
        }

        
        // 보유·잠김 = 정보 창 / 구매 가능 = 구매 확인 팝업 → 구매 (2026-11-24 구매는 미니언 관리 화면 카드에서)
        public void OnClickedPanel()
        {
            if (minionData == null)
                return;
            if (UserDataManager.Instance.userMinionManager.GetBuyState(minionData.key) == MinionBuyState.Buyable)
            {
                OpenBuyConfirm();
                return;
            }
            MinionInfoCanvas.Instance.OpenCanvas(minionData);
        }

        void OpenBuyConfirm()
        {
            string currency = TranslateManager.GetText($"Currency.{minionData.buyCurrencyType}");
            string body = string.Format(TranslateManager.GetText("Minion_BuyConfirm_Body"), minionData.Title, currency, minionData.buyPrice);
            YesOrNoCanvas.Instance.OpenCanvas(TranslateManager.GetText("Minion_BuyConfirm_Title"), body, yes =>
            {
                if (yes)
                    Buy();
            });
        }

        void Buy()
        {
            MinionBuyResult result = UserDataManager.Instance.userMinionManager.BuyMinion(minionData.key);
            if (result == MinionBuyResult.NotEnoughCurrency)
            {
                SoundManager.Play(SFXType.Fail_NotEnough);
                ToastCanvas.Toast(string.Format(TranslateManager.GetText("Currency_NotEnough"),
                    TranslateManager.GetText($"Currency.{minionData.buyCurrencyType}")));
                return;
            }
            if (result != MinionBuyResult.Success)
                return;
            SoundManager.Play(SFXType.PurchaseComplete);
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("Minion_Bought"), minionData.Title));
            LobbyManager.Instance.GetLobbyCanvas(LobbyState.Minion).UpdateCanvas(); // 칸 상태·정렬 갱신
        }

        // ---- 구매 상태 표시 (2026-11-24): 씬에서 직접 만든 오브젝트를 연결. 비운 칸은 건너뜀 (실행 중 생성 없음) ----
        [Header("상태별로 켜지는 오브젝트")]
        [Tooltip("보유일 때만 켜짐 (선택)")]
        public GameObject ownedObject;
        [Tooltip("구매 가능(유저 레벨 도달)일 때만 켜짐. 가격 표시 묶음 등")]
        public GameObject buyableObject;
        [Tooltip("잠김(유저 레벨 미달)일 때만 켜짐. 자물쇠 등")]
        [UnityEngine.Serialization.FormerlySerializedAs("unownPanel")]
        public GameObject lockedObject;

        [Header("글씨·아이콘")]
        [Tooltip("구매 가격 숫자 (구매 가능일 때). 재화가 모자라면 notEnoughColor")]
        public TMP_Text priceText;
        [Tooltip("구매 재화 아이콘 (Resources/Icons/Currency/{재화})")]
        public Image currencyIcon;
        [Tooltip("해금 레벨 \"Lv.N\" (잠김일 때, 번역 Minion_UnlockLevel)")]
        public TMP_Text unlockLevelText;
        public Color priceColor = Color.white;
        public Color notEnoughColor = new Color(1f, 0.4f, 0.4f);

        public MinionBuyState BuyState { get; private set; }

        public void UpdatePanel()
        {
            if(minionData ==null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            BuyState = UserDataManager.Instance.userMinionManager.GetBuyState(minionData.key);
            SetActive(ownedObject, BuyState == MinionBuyState.Owned);
            SetActive(buyableObject, BuyState == MinionBuyState.Buyable);
            SetActive(lockedObject, BuyState == MinionBuyState.Locked);
            minionPanel.SetData(minionData); // 그림 색 (잠김만 검게)

            if (BuyState == MinionBuyState.Buyable)
            {
                bool enough = UserDataManager.Instance.userData.GetCurrency(minionData.buyCurrencyType) >= minionData.buyPrice;
                if (priceText != null)
                {
                    priceText.text = minionData.buyPrice.ToString("N0");
                    priceText.color = enough ? priceColor : notEnoughColor;
                }
                if (currencyIcon != null)
                {
                    Sprite icon = GetCurrencyIcon(minionData.buyCurrencyType);
                    if (icon != null)
                        currencyIcon.sprite = icon;
                }
            }
            else if (BuyState == MinionBuyState.Locked && unlockLevelText != null)
            {
                unlockLevelText.text = string.Format(TranslateManager.GetText("Minion_UnlockLevel"), UserMinionManager.GetUnlockLevel(minionData.key));
            }
        }

        static void SetActive(GameObject obj, bool active)
        {
            if (obj != null && obj.activeSelf != active)
                obj.SetActive(active);
        }

        public static Sprite GetCurrencyIcon(CurrencyType type) => Resources.Load<Sprite>($"Icons/Currency/{type}");
    }
}
