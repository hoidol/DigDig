using System;
using Cysharp.Threading.Tasks;

// (2026-11-25) 카드 새로고침 횟수 (한 판 기준, GameManager.StartGame에서 ResetAll)
// 무료 FREE_PER_GAME번 → 그다음은 광고를 보고 AD_PER_GAME번까지. 다 쓰면 버튼 숨김
// 광고 제거 구매자: 광고 없이 바로 되지만 횟수 제한은 같음 (AdManager.Show가 광고 없이 성공 처리) → 무한 새로고침 방지
// 사용처: 레벨업 미니언 강화 카드(LevelUpCanvas) · 브레이크타임 미니언 카드(MinionOfferSystem)
public class RerollCounter
{
    public const int FREE_PER_GAME = 2;
    public const int AD_PER_GAME = 3;

    public static readonly RerollCounter MinionEnhance = new RerollCounter("reroll_minion_enhance");
    public static readonly RerollCounter BreakOffer = new RerollCounter("reroll_break_offer");

    readonly string adId;
    public int FreeLeft { get; private set; } = FREE_PER_GAME;
    public int AdLeft { get; private set; } = AD_PER_GAME;
    public bool WaitingAd { get; private set; }
    public bool CanReroll => FreeLeft > 0 || AdLeft > 0;
    public bool NeedsAd => FreeLeft <= 0 && AdLeft > 0;

    RerollCounter(string adId) { this.adId = adId; }

    public static void ResetAll()
    {
        MinionEnhance.Reset();
        BreakOffer.Reset();
    }

    public void Reset()
    {
        FreeLeft = FREE_PER_GAME;
        AdLeft = AD_PER_GAME;
        WaitingAd = false;
    }

    // 무료가 남으면 바로, 아니면 광고 → 성공하면 onReroll. 끝나면(성공·실패) onDone
    public void TryReroll(Action onReroll, Action onDone = null)
    {
        if (WaitingAd || !CanReroll)
            return;
        if (FreeLeft > 0)
        {
            FreeLeft--;
            onReroll?.Invoke();
            onDone?.Invoke();
            return;
        }
        WaitingAd = true;
        AdManager.Show(adId, AdType.RA, async success =>
        {
            await UniTask.SwitchToMainThread(); // 광고 SDK 콜백은 메인 스레드가 아닐 수 있음
            WaitingAd = false;
            if (success)
            {
                AdLeft--;
                onReroll?.Invoke();
            }
            onDone?.Invoke();
        });
    }

    static bool AdRemoved => UserDataManager.Instance != null && UserDataManager.Instance.IsAdRemovedEffective;

    // 버튼 글씨: "새로고침 (2/2)" / "광고 보고 새로고침 (3)" / 광고 제거 = "새로고침 (+3)"
    public string GetLabel(string verb)
    {
        if (FreeLeft > 0)
            return $"{verb} ({FreeLeft}/{FREE_PER_GAME})";
        return AdRemoved ? $"{verb} (+{AdLeft})" : $"광고 보고 {verb} ({AdLeft})";
    }
}
