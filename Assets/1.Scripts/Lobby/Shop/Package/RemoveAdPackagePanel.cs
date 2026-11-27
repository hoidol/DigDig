// 광고 제거 칸 (버튼 key = RemoveAd): 구매하면 광고 없이 보상. 기본 게임 팩으로 이미 광고가 제거됐으면 숨김
public class RemoveAdPackagePanel : PackageProductPanel
{
    protected override string PurchasedMessageKey => "Purchase_RemoveAd";
    public override bool IsPurchased => UserDataManager.Instance.isAdRemoved;
}
