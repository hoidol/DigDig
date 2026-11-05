using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

// 아이템 선택 단계에서 광고를 보면 선택지 갱신. 한 판에 REFRESH_COUNT_PER_GAME번
public class AdRefreshItemButton : ButtonUI
{
    const int REFRESH_COUNT_PER_GAME = 2;
    public TMP_Text remainText;
    int remainCount = REFRESH_COUNT_PER_GAME; // 버튼이 판마다 새로 생기므로 판 단위로 리셋됨

    void OnEnable()
    {
        UpdateButton();
    }

    void UpdateButton()
    {
        if (remainText != null)
            remainText.text = $"{remainCount}/{REFRESH_COUNT_PER_GAME}";

        if(remainCount <= 0)
        {
            gameObject.SetActive(false);
        }       
            
    }

    public override void OnClickedBtn()
    {
        if (remainCount <= 0)
        {
            ToastCanvas.Toast("이번 판 갱신 횟수를 모두 사용했습니다");
            return;
        }

        // 광고 제거 구매자는 광고 없이 바로 갱신
        if (UserDataManager.Instance.isAdRemoved)
        {
            Refresh();
            return;
        }

        button.interactable = false; // 광고 보는 동안 중복 클릭 방지
        AdManager.Instance.Play("refresh_item", AdType.RA, async (success) =>
        {
            await UniTask.SwitchToMainThread(); // 광고 SDK 콜백은 메인 스레드가 아닐 수 있음
            if (success)
                Refresh();
            else
                UpdateButton();
        });
    }

    void Refresh()
    {
        if (SelectItemCanvas.Instance.RefreshItems())
            remainCount--;
        UpdateButton();
    }
}
