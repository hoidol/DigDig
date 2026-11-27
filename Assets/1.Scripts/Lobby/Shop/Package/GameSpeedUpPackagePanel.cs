// 게임 속도 칸 (버튼 key = GameSpeedUp): 구매하면 인게임 배속을 5배까지 (SpeedUpButton). 기본 게임 팩으로 이미 가졌으면 숨김
public class GameSpeedUpPackagePanel : PackageProductPanel
{
    protected override string PurchasedMessageKey => "Purchase_GameSpeedUp";
    public override bool IsPurchased => UserDataManager.Instance.IsGameSpeedUp;
}
