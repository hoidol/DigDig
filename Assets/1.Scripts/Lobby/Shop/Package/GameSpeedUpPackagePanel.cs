// 게임 속도 증가 패키지 (버튼 key = GameSpeedUp): 구매하면 인게임 배속을 5배까지 쓸 수 있다 (SpeedUpButton)
public class GameSpeedUpPackagePanel : PackageProductPanel
{
    protected override string PurchasedMessageKey => "Purchase_GameSpeedUp";
}
