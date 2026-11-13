using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 타일 아이템 강화 (CLAUDE.md "타일 아이템 강화"): 타일 객체마다, 주사위 코인 = DiceCoin × (현재 레벨 + 1), 성공 70% → 50%, 최대 = MaxCount
public class TileItemUpgradePlayTest : InGamePlayTestBase
{
    [Test]
    public void Rules_CostAndChance()
    {
        Assert.AreEqual(6, TileItem.GetUpgradeCost(3, 1), "Lv1→2 = DiceCoin × 2");
        Assert.AreEqual(9, TileItem.GetUpgradeCost(3, 2), "Lv2→3 = DiceCoin × 3");
        Assert.AreEqual(70f, TileItem.GetUpgradeChance(1));
        Assert.AreEqual(50f, TileItem.GetUpgradeChance(2));
        Assert.AreEqual(new[] { 1.5f, 1.75f, 2f }, ItemData.ParseLevelValues("1.5/1.75/2"));
        Assert.IsEmpty(ItemData.ParseLevelValues(""));
    }

    // 데이터: DiceCoin > 0 인 아이템은 레벨 수치가 최대 레벨만큼 있어야 함, DiceCoin 0 은 강화 불가
    [Test]
    public void Data_UpgradableItemsHaveValuesPerLevel()
    {
        var datas = UnityEditor.AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/AddressableResources/ItemData" })
            .Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)))
            .Where(d => d.itemPrefab is TileItem).ToList();
        Assert.IsNotEmpty(datas);
        foreach (ItemData d in datas.Where(d => d.diceCoin > 0))
        {
            Assert.IsTrue(d.CanUpgradeItem, $"{d.key}: 강화 비용은 있는데 Values 없음");
            Assert.AreEqual(d.MaxLevel, d.levelValues.Length, $"{d.key}: Values 수 ≠ 최대 레벨(MaxCount)");
        }
        foreach (ItemData d in datas.Where(d => d.diceCoin <= 0))
            Assert.IsFalse(d.CanUpgradeItem, $"{d.key}: DiceCoin 0인데 강화 가능");
    }

    static string TileItemTestFormat(float mul) => ((mul - 1f) * 100f).ToString("0.##") + "%";

    // 모든 타일 아이템 설명: Values가 있으면 {0}이 채워져야 함 (Desc에 {0}이 없거나 포맷 깨짐 방지)
    [Test]
    public void Data_DescriptionsFormatted()
    {
        var datas = UnityEditor.AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/AddressableResources/ItemData" })
            .Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)))
            .Where(d => d.itemPrefab is TileItem && d.HasLevelValues).ToList();
        foreach (ItemData d in datas)
        {
            StringAssert.Contains("{0}", d.desc, $"{d.key}: Values가 있는데 Desc에 {{0}} 없음");
            StringAssert.DoesNotContain("{0}", ((TileItem)d.itemPrefab).GetDescription(d), d.key);
        }
    }

    [UnityTest]
    public IEnumerator Upgrade_ButtonFlow()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Upgrade_ButtonFlowBody();
    }

    IEnumerator Upgrade_ButtonFlowBody()
    {
        const string KEY = "AttackPowerTile";
        ItemData data = ItemManager.Instance.GetItemData(KEY);
        Tile tile = GetFreeTile();
        KingSlime.Instance.AddItem(KEY, tile).Forget();
        yield return WaitFrames(2);
        var item = (TileItem)tile.tileItem;
        Minion minion = SpawnMinion("Ice", tile);
        yield return WaitFrames(2);
        Assert.AreEqual(1, item.upgradeLevel);
        float atkLv1 = minion.statDic[StatType.AttackPower];
        // Desc "공격력 {0}% 증가" → 현재 레벨 수치 (배율 1.5 → 50%)
        StringAssert.Contains(TileItemTestFormat(data.GetLevelValue(1)), item.GetDescription());
        StringAssert.DoesNotContain("{0}", item.GetDescription());

        var button = Object.FindObjectsByType<UpgradeTileItemButton>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
        Assert.IsNotNull(button, "InGame 씬에 강화 버튼 없음 (TileItemInfoPanel.upgradeTileItemButton)");
        var panel = button.GetComponentInParent<TileItemInfoPanel>(true);
        panel.SetTile(tile);
        Assert.IsTrue(button.gameObject.activeSelf, "강화 가능한 아이템인데 버튼 숨김");

        // 코인 부족 → 아무 일 없음
        KingSlime.Instance.AddDiceCoin(-KingSlime.Instance.diceCoin);
        button.OnClickedBtn();
        Assert.AreEqual(1, item.upgradeLevel);
        Assert.AreEqual(0, KingSlime.Instance.diceCoin);

        // 성공할 때까지 (실패는 코인만 소모, 레벨 그대로)
        int tries = 0;
        while (item.upgradeLevel == 1 && tries++ < 50)
        {
            int cost = item.UpgradeCost;
            Assert.AreEqual(data.diceCoin * 2, cost);
            KingSlime.Instance.AddDiceCoin(cost);
            button.OnClickedBtn();
            Assert.AreEqual(0, KingSlime.Instance.diceCoin, "강화 비용을 내지 않음");
        }
        Assert.AreEqual(2, item.upgradeLevel, "강화 성공 안 됨");
        yield return WaitFrames(2);
        float atkLv2 = minion.statDic[StatType.AttackPower];
        Assert.Greater(atkLv2, atkLv1, "올라간 슬라임 공격력이 레벨 수치로 갱신 안 됨");
        StringAssert.Contains(TileItemTestFormat(data.GetLevelValue(2)), item.GetDescription(), "설명이 레벨 수치로 안 바뀜");
        Assert.AreEqual(atkLv1 / data.GetLevelValue(1) * data.GetLevelValue(2), atkLv2, 0.01f, "버프 중복 적용");

        // 최대 레벨 → 버튼 숨김
        tries = 0;
        while (!item.IsMaxLevel && tries++ < 50)
        {
            KingSlime.Instance.AddDiceCoin(item.UpgradeCost);
            button.OnClickedBtn();
        }
        Assert.AreEqual(data.MaxLevel, item.upgradeLevel);
        panel.SetTile(tile);
        Assert.IsFalse(button.gameObject.activeSelf, "최대 레벨인데 버튼 보임");

        // 강화 불가 아이템 → 버튼 숨김
        Tile other = GetFreeTile(tile);
        KingSlime.Instance.AddItem("TrueDamageTile", other).Forget();
        yield return WaitFrames(2);
        panel.SetTile(other);
        Assert.IsFalse(button.gameObject.activeSelf, "DiceCoin 0 아이템에 강화 버튼");

        AssertNoErrors();
    }
}
