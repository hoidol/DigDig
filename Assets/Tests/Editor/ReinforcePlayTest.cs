using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

// 인게임 직접 강화 (CLAUDE.md "인게임 직접 강화"): 장착 5종이 패널에 순서대로, 코인으로 종 단위 강화, 공격력 % 합연산
public class ReinforcePlayTest : InGamePlayTestBase
{
    // 규칙: 가격·코인 차감·종 단위 레벨 (씬 패널이 모자라도 테스트용 패널로 확인)
    [UnityTest]
    public IEnumerator ReinforcePanels_EquippedMinions()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    // 효과: 필드 슬라임·이후 소환분 공격력, 다른 종 영향 없음, 0마리가 돼도 레벨 유지
    [UnityTest]
    public IEnumerator Reinforce_AttackPowerEffect()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return EffectBody();
        AssertNoErrors();
    }

    // 실제 씬 연결: 강화 버튼 → 창 → 칸 버튼 클릭으로 강화 → 닫기 (테스트용 패널 없이)
    [UnityTest]
    public IEnumerator Reinforce_SceneUI()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return SceneUIBody();
        AssertNoErrors();
    }

    // 씬 패널이 아직 5개가 안 되면 테스트용으로 채움
    static void EnsurePanels(ReinforceCanvas canvas)
    {
        if (canvas.reinforcePanels != null && canvas.reinforcePanels.Count(p => p != null) >= GameSetting.MINION_SLOT_COUNT)
            return;
        canvas.reinforcePanels = new ReinforcePanel[GameSetting.MINION_SLOT_COUNT];
        for (int i = 0; i < canvas.reinforcePanels.Length; i++)
        {
            var go = new GameObject($"ReinforcePanel{i}(Test)", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            canvas.reinforcePanels[i] = go.AddComponent<ReinforcePanel>();
        }
    }

    static UserMinion[] Equipped => UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions;
    static KingSlimeStatManager Stat => KingSlime.Instance.statMgr;
    static float Power(Minion minion) => minion.statDic[StatType.AttackPower];

    static void SetCoin(int coin) => KingSlime.Instance.AddCoin(coin - KingSlime.Instance.coin);

    IEnumerator Body()
    {
        ReinforceCanvas canvas = ReinforceCanvas.Instance;
        Assert.IsNotNull(canvas, "InGame 씬에 ReinforceCanvas 없음");
        EnsurePanels(canvas);
        canvas.OpenCanvas();
        yield return WaitFrames(1);

        UserMinion[] equipped = Equipped;
        for (int i = 0; i < GameSetting.MINION_SLOT_COUNT; i++)
        {
            ReinforcePanel panel = canvas.reinforcePanels[i];
            Assert.IsTrue(panel.gameObject.activeSelf, $"{i}번 패널이 꺼져 있음");
            Assert.IsNotNull(panel.MinionData, $"{i}번 패널 슬라임 없음");
            Assert.AreEqual(equipped[i].key, panel.MinionData.key, $"{i}번 패널이 장착 순서와 다름");
        }
        Assert.IsTrue(canvas.gameObject.activeSelf, "열자마자 닫힘 (슬롯머신 자동 닫기가 남아 있음)");

        // 필드에 없는 종도 강화 가능, 가격 30 → 60 → 90
        ReinforcePanel first = canvas.reinforcePanels[0];
        string key = first.MinionData.key;
        Assert.IsFalse(Stat.HasMinionOnField(key), "테스트 전제: 필드에 그 종이 없어야 함");
        Assert.AreEqual(0, Stat.GetReinforceLevel(key), "게임 시작 시 강화 레벨 0");
        Assert.AreEqual(30, ReinforcePanel.GetPrice(0));
        Assert.AreEqual(60, ReinforcePanel.GetPrice(1));
        Assert.AreEqual(90, ReinforcePanel.GetPrice(2));

        SetCoin(30); // 딱 한 번 강화할 만큼
        first.OnClickReinforce();
        Assert.AreEqual(1, Stat.GetReinforceLevel(key), "강화 레벨이 오르지 않음");
        Assert.AreEqual(0, KingSlime.Instance.coin, "가격만큼 코인이 빠지지 않음");
        Assert.AreEqual(60, first.GetPrice(), "다음 가격");

        // 코인 부족 → 그대로 (1 모자람)
        SetCoin(59);
        first.OnClickReinforce();
        Assert.AreEqual(1, Stat.GetReinforceLevel(key), "코인이 모자란데 강화됨");
        Assert.AreEqual(59, KingSlime.Instance.coin, "강화 실패인데 코인이 빠짐");

        // 연속 강화 60 + 90
        SetCoin(150);
        first.OnClickReinforce();
        first.OnClickReinforce();
        Assert.AreEqual(3, Stat.GetReinforceLevel(key), "연속 강화");
        Assert.AreEqual(0, KingSlime.Instance.coin, "60 + 90 차감");

        // 다른 종은 영향 없음
        for (int i = 1; i < GameSetting.MINION_SLOT_COUNT; i++)
        {
            string otherKey = canvas.reinforcePanels[i].MinionData.key;
            if (otherKey != key)
                Assert.AreEqual(0, Stat.GetReinforceLevel(otherKey), $"다른 종({otherKey}) 레벨이 오름");
        }

        // 창을 닫았다 열어도 레벨·가격 유지
        canvas.CloseCanvas();
        canvas.OpenCanvas();
        yield return WaitFrames(1);
        Assert.AreEqual(ReinforcePanel.GetPrice(3), canvas.reinforcePanels[0].GetPrice(), "다시 열었을 때 가격");
        canvas.CloseCanvas();
    }

    IEnumerator EffectBody()
    {
        string keyA = Equipped[0].key;
        string keyB = Equipped.First(e => e.key != keyA).key;
        MinionData dataA = Equipped[0].MinionData;
        float percent = dataA.reinforceAttackPowerPercent;
        Assert.Greater(percent, 0f, $"{keyA} 강화 공격력 %가 0 (MinionData.csv ReinforceAttackPower)");

        // 서로 붙지 않는 칸에 (쌍둥이 반지 같은 인접 효과 배제)
        Tile[] tiles = TileManager.Instance.tiles.Where(t => t.IsEmpty && t.CanAddTileItem).ToArray();
        Minion a0 = SpawnMinion(keyA, tiles[0]);
        Minion a4 = SpawnMinion(keyA, tiles[tiles.Length - 1], MinionData.MAX_MERGE_LEVEL);
        Minion b = SpawnMinion(keyB, tiles[tiles.Length / 2]);
        yield return WaitFrames(2);
        a0.UpdateMinion(); a4.UpdateMinion(); b.UpdateMinion();
        float baseA0 = Power(a0), baseA4 = Power(a4), baseB = Power(b);
        Assert.Greater(baseA0, 0f, "기본 공격력");

        // 1레벨: 필드의 그 종 전체가 바로 +percent%
        Stat.LevelUpMinion(keyA);
        yield return WaitFrames(1);
        Assert.AreEqual(baseA0 * (1f + percent / 100f), Power(a0), baseA0 * 0.001f, $"{keyA} Lv1 공격력 (머지 0)");
        Assert.AreEqual(baseA4 * (1f + percent / 100f), Power(a4), baseA4 * 0.001f, $"{keyA} Lv1 공격력 (머지 4)");
        Assert.AreEqual(baseB, Power(b), baseB * 0.001f, $"다른 종({keyB}) 공격력이 바뀜");

        // 3레벨: 합연산 (1 + 3 × %), 복리 아님
        Stat.LevelUpMinion(keyA);
        Stat.LevelUpMinion(keyA);
        yield return WaitFrames(1);
        float mul = 1f + percent / 100f * 3;
        Assert.AreEqual(baseA0 * mul, Power(a0), baseA0 * 0.001f, $"{keyA} Lv3 공격력은 합연산");
        Assert.AreEqual(baseA0 * mul, Power(a0), baseA0 * 0.001f, "UpdateMinion을 다시 불러도 중복 적용 없음");
        a0.UpdateMinion();
        a0.UpdateMinion();
        Assert.AreEqual(baseA0 * mul, Power(a0), baseA0 * 0.001f, "UpdateMinion 반복 호출에 강화가 중복 적용됨");

        // 이후 소환분에도 적용
        Minion a0b = SpawnMinion(keyA, tiles[2]);
        yield return WaitFrames(2);
        a0b.UpdateMinion();
        Assert.AreEqual(Power(a0), Power(a0b), baseA0 * 0.001f, "강화 뒤 소환한 슬라임에 강화가 안 붙음");

        // 실제 전투: 강화된 슬라임이 싸워도 에러 없음
        current = "강화 후 전투";
        Time.timeScale = 4f;
        yield return WaitRealtime(3f);
        Time.timeScale = 1f;
        current = "";

        // 그 종이 필드에서 0마리가 돼도 레벨 유지 → 다시 소환하면 적용
        foreach (Minion minion in new[] { a0, a4, a0b })
        {
            Tile tile = TileManager.Instance.tiles.FirstOrDefault(t => ReferenceEquals(t.tileObject, minion));
            if (tile != null)
                tile.SetTileObject(null);
            KingSlime.Instance.RemoveMinion(minion); // 합성으로 사라질 때와 같은 경로
        }
        yield return WaitFrames(2);
        Assert.IsFalse(Stat.HasMinionOnField(keyA), "테스트 전제: 그 종이 필드에 없어야 함");
        Assert.AreEqual(3, Stat.GetReinforceLevel(keyA), "필드에서 0마리가 되자 강화 레벨이 사라짐");
        Minion again = SpawnMinion(keyA, tiles[0]);
        yield return WaitFrames(2);
        again.UpdateMinion();
        Assert.AreEqual(baseA0 * mul, Power(again), baseA0 * 0.001f, "0마리 뒤 다시 소환한 슬라임 공격력");

        // 종마다 % 가 다름 (데이터)
        var percents = MinionManager.Instance.minionDataDic.Values.Select(d => d.reinforceAttackPowerPercent).Distinct().ToArray();
        Assert.Greater(percents.Length, 1, "강화 %가 모든 종이 같음 (기획: 종마다 다르게)");
        Assert.IsTrue(MinionManager.Instance.minionDataDic.Values.All(d => d.reinforceAttackPowerPercent > 0f),
            "강화 %가 0인 종: " + string.Join(", ", MinionManager.Instance.minionDataDic.Values.Where(d => d.reinforceAttackPowerPercent <= 0f).Select(d => d.key)));
    }

    IEnumerator SceneUIBody()
    {
        ReinforceCanvas canvas = ReinforceCanvas.Instance;
        Assert.IsNotNull(canvas, "InGame 씬에 ReinforceCanvas 없음");
        // 씬에서 켜 둔 채 저장하면 게임 시작부터 창이 떠 있다 → 나머지를 끝까지 확인한 뒤 마지막에 실패 처리
        bool openAtStart = canvas.gameObject.activeSelf;
        if (openAtStart)
            canvas.gameObject.SetActive(false);

        // 강화 버튼 → 창
        ReinforceOpenButton open = Object.FindFirstObjectByType<ReinforceOpenButton>(FindObjectsInactive.Include);
        Assert.IsNotNull(open, "InGame 씬에 강화 버튼(ReinforceOpenButton) 없음");
        Assert.IsTrue(open.gameObject.activeInHierarchy, "강화 버튼이 화면에 안 보임 (부모가 꺼져 있음)");
        open.GetComponent<Button>().onClick.Invoke();
        yield return WaitFrames(2);
        Assert.IsTrue(canvas.gameObject.activeInHierarchy, "강화 버튼을 눌러도 창이 안 열림");

        // 칸 5개 + 표시
        ReinforcePanel[] panels = canvas.reinforcePanels.Where(p => p != null).ToArray();
        Assert.AreEqual(GameSetting.MINION_SLOT_COUNT, panels.Length, "씬의 ReinforcePanel 수 (장착 5종)");
        for (int i = 0; i < panels.Length; i++)
        {
            ReinforcePanel panel = panels[i];
            Assert.AreEqual(Equipped[i].key, panel.MinionData.key, $"{i}번 칸이 장착 순서와 다름");
            Assert.IsNotNull(panel.lvText, $"{i}번 칸 lvText 연결 안 됨");
            Assert.IsNotNull(panel.priceText, $"{i}번 칸 priceText 연결 안 됨");
            Assert.IsNotNull(panel.thumImage, $"{i}번 칸 thumImage 연결 안 됨");
            Assert.IsNotNull(panel.reinforceButton, $"{i}번 칸 reinforceButton 연결 안 됨");
            Assert.AreEqual(0, panel.reinforceButton.onClick.GetPersistentEventCount(), $"{i}번 칸 버튼 인스펙터 OnClick이 따로 걸려 있음 (두 번 강화됨)");
            Assert.AreEqual("Lv.0", panel.lvText.text, $"{i}번 칸 레벨 글씨");
            Assert.AreEqual("30", panel.priceText.text, $"{i}번 칸 가격 글씨");
            Assert.AreEqual(Equipped[i].MinionData.thum, panel.thumImage.sprite, $"{i}번 칸 썸네일");
        }

        // 칸 버튼 클릭 한 번 = 강화 한 번
        string key = panels[1].MinionData.key;
        SetCoin(100);
        panels[1].reinforceButton.onClick.Invoke();
        yield return WaitFrames(1);
        Assert.AreEqual(1, Stat.GetReinforceLevel(key), "칸 버튼 클릭 한 번에 강화 한 번");
        Assert.AreEqual(70, KingSlime.Instance.coin, "칸 버튼 클릭 코인 차감");
        Assert.AreEqual("Lv.1", panels[1].lvText.text, "강화 후 레벨 글씨");
        Assert.AreEqual("60", panels[1].priceText.text, "강화 후 가격 글씨");
        Assert.AreEqual("Lv.0", panels[0].lvText.text, "다른 칸 글씨가 바뀜");

        // 닫기 버튼
        Button close = canvas.GetComponentsInChildren<Button>(true).FirstOrDefault(b => b.name.StartsWith("CloseButton"));
        Assert.IsNotNull(close, "강화 창 닫기 버튼 없음");
        close.onClick.Invoke();
        yield return WaitFrames(2);
        Assert.IsFalse(canvas.gameObject.activeSelf, "닫기 버튼을 눌러도 창이 안 닫힘");
        Assert.IsFalse(openAtStart, "게임 시작 시 강화 창이 열려 있음 (InGame 씬에서 ReinforceCanvas를 꺼 두고 저장해야 함)");
    }
}
