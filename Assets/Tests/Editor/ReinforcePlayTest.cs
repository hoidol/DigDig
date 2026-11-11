using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 인게임 직접 강화 창 (ReinforceCanvas.reinforcePanels): 장착 5종이 패널에 순서대로, 코인으로 종 단위 강화
public class ReinforcePlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator ReinforcePanels_EquippedSlimes()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    // 씬 패널이 아직 5개가 안 되면 테스트용으로 채움
    static void EnsurePanels(ReinforceCanvas canvas)
    {
        if (canvas.reinforcePanels != null && canvas.reinforcePanels.Count(p => p != null) >= GameSetting.SLIME_SLOT_COUNT)
            return;
        canvas.reinforcePanels = new ReinforcePanel[GameSetting.SLIME_SLOT_COUNT];
        for (int i = 0; i < canvas.reinforcePanels.Length; i++)
        {
            var go = new GameObject($"ReinforcePanel{i}(Test)", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            canvas.reinforcePanels[i] = go.AddComponent<ReinforcePanel>();
        }
    }

    IEnumerator Body()
    {
        ReinforceCanvas canvas = ReinforceCanvas.Instance;
        Assert.IsNotNull(canvas, "InGame 씬에 ReinforceCanvas 없음");
        EnsurePanels(canvas);
        canvas.OpenCanvas();
        yield return WaitFrames(1);

        UserSlime[] equipped = UserDataManager.Instance.userSlimeManager.userSlimeData.equiptedSlimes;
        for (int i = 0; i < GameSetting.SLIME_SLOT_COUNT; i++)
        {
            ReinforcePanel panel = canvas.reinforcePanels[i];
            Assert.IsTrue(panel.gameObject.activeSelf, $"{i}번 패널이 꺼져 있음");
            Assert.IsNotNull(panel.SlimeData, $"{i}번 패널 슬라임 없음");
            Assert.AreEqual(equipped[i].key, panel.SlimeData.key, $"{i}번 패널이 장착 순서와 다름");
        }
        Assert.IsTrue(canvas.gameObject.activeSelf, "열자마자 닫힘 (슬롯머신 자동 닫기가 남아 있음)");

        // 필드에 없는 종도 강화 가능, 가격 30 → 60
        ReinforcePanel first = canvas.reinforcePanels[0];
        string key = first.SlimeData.key;
        KingSlimeStatManager stat = KingSlime.Instance.statMgr;
        int lv = stat.GetReinforceLevel(key);
        int price = ReinforcePanel.GetPrice(lv);
        Assert.AreEqual(GameSetting.REINFORCE_PRICE_PER_LEVEL * (lv + 1), price);

        KingSlime.Instance.AddCoin(price - KingSlime.Instance.coin); // 딱 한 번 강화할 만큼
        first.OnClickReinforce();
        Assert.AreEqual(lv + 1, stat.GetReinforceLevel(key), "강화 레벨이 오르지 않음");
        Assert.AreEqual(0, KingSlime.Instance.coin, "가격만큼 코인이 빠지지 않음");
        Assert.AreEqual(ReinforcePanel.GetPrice(lv + 1), first.GetPrice(), "다음 가격");

        // 코인 부족 → 그대로
        first.OnClickReinforce();
        Assert.AreEqual(lv + 1, stat.GetReinforceLevel(key), "코인이 없는데 강화됨");
        Assert.AreEqual(0, KingSlime.Instance.coin);

        // 다른 종은 영향 없음
        string otherKey = canvas.reinforcePanels[1].SlimeData.key;
        if (otherKey != key)
            Assert.AreEqual(0, stat.GetReinforceLevel(otherKey), "다른 종 레벨이 오름");

        canvas.CloseCanvas();
    }
}
