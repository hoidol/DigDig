// (2026-11-27) 기본 게임 팩 칸 (버튼 key = BaseGamePack): 영구 광고 제거 + 게임 속도 + 용사 패스 30일 (HeroPass.IsBundleActive). 한 번만, 사면 숨김
public class BaseGamePackPanel : PackageProductPanel
{
    protected override string PurchasedMessageKey => "Purchase_BaseGamePack";
}
