using UnityEngine;

// 묶음 1 (Lv.5) 판의 시작 + 묶음 4 장인의 망치 — 코인·보드 운영

// 개척자의 곡괭이: 시작 시 막힌 칸 UNBLOCK_COUNT칸 해제
public class PioneerPickAuthority : Authority
{
    public const int UNBLOCK_COUNT = 2;

    public override void OnBegin()
    {
        for (int i = 0; i < UNBLOCK_COUNT && TileManager.Instance.HasBlockedTile; i++)
            TileManager.Instance.UnblockRandomTile();
    }
}

// 상인의 장부: 첫 wave 동안 소환 가격 절반 (MinionSpawnButton.GetSpawnPrice)
public class MerchantLedgerAuthority : Authority
{
    public const float DISCOUNT_MUL = 0.5f;

    public static int ApplyDiscount(int price, int waveIdx) => waveIdx == 0 ? Mathf.CeilToInt(price * DISCOUNT_MUL) : price;
}

// 도박꾼의 동전: 소환 시 25% 머지 Lv1로 등장 / 25% 코인 2배 소모 (MinionSpawnButton)
public class GamblerCoinAuthority : Authority
{
    public const float LEVEL_UP_CHANCE = 0.25f;
    public const float DOUBLE_COST_CHANCE = 0.25f;

    // roll(0~1) → (머지 레벨, 낼 코인). 코인 2배를 못 내면 가진 만큼만
    public static (int lv, int cost) Roll(float roll, int price, int coin)
    {
        if (roll < LEVEL_UP_CHANCE)
            return (1, price);
        if (roll < LEVEL_UP_CHANCE + DOUBLE_COST_CHANCE)
            return (0, Mathf.Min(price * 2, coin));
        return (0, price);
    }
}

// 장인의 망치: 타일 아이템 뽑기 최저 확률 30%, 실패하면 낸 코인 절반 환불 (DrawItemButton / SelectItemCanvas)
public class ArtisanHammerAuthority : Authority
{
    public const int MIN_CHANCE = 30;
    public const float REFUND_RATIO = 0.5f;

    public static int ApplyMinChance(int chance) => Mathf.Max(chance, MIN_CHANCE);
    public static int GetRefund(int paid) => Mathf.FloorToInt(paid * REFUND_RATIO);
}
