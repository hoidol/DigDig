using System.Collections;
using Cysharp.Threading.Tasks;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 타일 아이템 플레이 테스트 (CLAUDE.md "타일 아이템")
public class TileItemPlayTest : InGamePlayTestBase
{
    const string HOST_SLIME = "Ice";
    const string CROSS_BUFF_SLIME = "Breeze";
    const float ITEM_TEST_TIME = 4f; // 아이템 하나당 게임 시간(초)
    const float TIME_SCALE = 4f;

    // 타일 index는 전체 6*6 격자 기준 (그룹 경계를 넘어 이어짐). 십자 버프·버프 범위 확장이 이 index로 주변 칸을 찾는다
    [UnityTest]
    public IEnumerator TileIndices_AreUnique6x6()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();

        Tile[] tiles = TileManager.Instance.tiles;
        var duplicated = tiles.GroupBy(t => t.index).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.IsEmpty(duplicated, "같은 index 타일이 있음");
        Assert.AreEqual(36, tiles.Length);
        Assert.IsTrue(tiles.All(t => t.index.x >= 0 && t.index.x < 6 && t.index.y >= 0 && t.index.y < 6), "index가 0~5 범위 밖");
        foreach (Tile tile in tiles)
            Assert.AreSame(tile, TileManager.Instance.GetTile(tile.index), $"GetTile({tile.index})이 다른 타일을 반환");
    }

    // 모든 타일 아이템: 설치 → 슬라임 올려서 전투 → 판매(제거)까지 에러 없이
    [UnityTest]
    public IEnumerator AllTileItems_InstallFightRemove_NoErrors()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();

        Time.timeScale = TIME_SCALE;
        var itemDatas = ItemManager.Instance.allItemDatas.Where(d => d.itemPrefab is TileItem).ToList();
        Assert.IsNotEmpty(itemDatas, "로드된 타일 아이템 없음");

        foreach (ItemData data in itemDatas)
        {
            current = data.key;
            var tileItemPrefab = (TileItem)data.itemPrefab;

            // 개척 타일: 설치 없이 막힌 칸 1개 해제
            if (!tileItemPrefab.NeedTile)
            {
                int blockedBefore = TileManager.Instance.tiles.Count(t => t.isBlocked);
                if (blockedBefore == 0)
                    continue;
                KingSlime.Instance.AddItem(data.key).Forget();
                yield return WaitFrames(2);
                Assert.AreEqual(blockedBefore - 1, TileManager.Instance.tiles.Count(t => t.isBlocked), $"{data.key}: 막힌 칸이 1개 해제되지 않음");
                continue;
            }

            Tile tile = GetFreeTile();
            Assert.IsNotNull(tile, "빈 타일 없음");
            KingSlime.Instance.AddItem(data.key, tile).Forget();
            yield return WaitFrames(2);

            TileItem installed = tile.tileItem as TileItem;
            Assert.IsNotNull(installed, $"{data.key}: 타일에 아이템 오브젝트가 설치되지 않음");
            Assert.AreEqual(data.key, installed.key, $"{data.key}: 다른 아이템이 설치됨 ({installed.GetType().Name})");
            Assert.IsInstanceOf(tileItemPrefab.GetType(), installed, $"{data.key}: 프리팹과 다른 클래스");

            Slime slime = null;
            try
            {
                slime = SpawnSlime(installed is ExpandBuffRangeTileItem ? CROSS_BUFF_SLIME : HOST_SLIME, tile);
            }
            catch (System.Exception e)
            {
                errors.Add($"[{current}] 슬라임 올리는 중 예외: {e.GetType().Name} {e.Message.Split('\n')[0]}");
            }
            yield return WaitRealtime(ITEM_TEST_TIME / TIME_SCALE);

            try
            {
                if (slime != null)
                    SlimeFieldController.Instance.RemoveFieldSlime(slime);
                KingSlime.Instance.RemoveItem(installed);
            }
            catch (System.Exception e)
            {
                errors.Add($"[{current}] 제거 중 예외: {e.GetType().Name} {e.Message.Split('\n')[0]}");
            }
            yield return WaitFrames(2);
            Assert.IsNull(tile.tileItem, $"{data.key}: 판매 후에도 타일에 아이템이 남음");
        }

        AssertNoErrors("타일 아이템 테스트 중 에러");
    }

    // 스탯 버프형: 같은 슬라임을 아이템 없는 칸 / 아이템 칸에 올려 비교
    [UnityTest]
    public IEnumerator StatTileItems_ChangeStats()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();

        Tile plainTile = GetFreeTile();
        Slime plain = SpawnSlime(HOST_SLIME, plainTile);

        (string key, StatType stat, int sign)[] cases =
        {
            ("AttackPowerTile", StatType.AttackPower, +1),
            ("AttackSpeedTile", StatType.AttackSpeed, +1),
            ("RangeUpTile", StatType.AttackRange, +1),
            ("BerserkTile", StatType.AttackPower, +1),
            ("BerserkTile", StatType.AttackRange, -1),
        };
        foreach (var c in cases)
        {
            current = $"{c.key}/{c.stat}";
            Tile tile = GetFreeTile(plainTile);
            KingSlime.Instance.AddItem(c.key, tile).Forget();
            yield return WaitFrames(2);
            Slime onItem = SpawnSlime(HOST_SLIME, tile);
            yield return WaitFrames(2);

            float before = plain.statDic[c.stat];
            float after = onItem.statDic[c.stat];
            if (c.sign > 0)
                Assert.Greater(after, before, $"{current}: 스탯이 오르지 않음");
            else
                Assert.Less(after, before, $"{current}: 스탯이 내려가지 않음");

            SlimeFieldController.Instance.RemoveFieldSlime(onItem);
            KingSlime.Instance.RemoveItem((TileItem)tile.tileItem);
            yield return WaitFrames(2);
        }
        AssertNoErrors();
    }

    // 버프 범위 확장: 십자 버프 슬라임이 확장 타일 위에 있으면 대각선 칸 슬라임도 버프를 받는다 (같은 TileGroup 안)
    [UnityTest]
    public IEnumerator ExpandBuffRangeTile_BuffsDiagonal()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();

        // 대각선 칸이 같은 그룹이고 둘 다 비어 있는 자리
        Tile center = null, diagonal = null;
        foreach (Tile t in TileManager.Instance.tiles.Where(t => t.IsEmpty && t.CanAddTileItem))
        {
            diagonal = new[] { new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) }
                .Select(o => TileManager.Instance.GetTile(t.index + o))
                .FirstOrDefault(d => d != null && d.IsEmpty && d.tileGroup == t.tileGroup);
            if (diagonal != null) { center = t; break; }
        }
        Assert.IsNotNull(center, "테스트할 자리 없음 (대각선이 같은 그룹인 빈 칸 2개)");

        Slime target = SpawnSlime(HOST_SLIME, diagonal);
        Slime buffer = SpawnSlime(CROSS_BUFF_SLIME, center);
        yield return WaitFrames(3);
        float noExpand = target.AttackSpeed;
        Assert.IsFalse(((CrossBuffSlime)buffer).IsRangeExpanded);

        SlimeFieldController.Instance.RemoveFieldSlime(buffer);
        KingSlime.Instance.AddItem("MaxExpandBuffRangeTile", center).Forget();
        yield return WaitFrames(2);
        Assert.IsInstanceOf<ExpandBuffRangeTileItem>(center.tileItem, "버프 범위 확장 아이템이 설치되지 않음");

        buffer = SpawnSlime(CROSS_BUFF_SLIME, center);
        yield return WaitFrames(3);
        Assert.IsTrue(((CrossBuffSlime)buffer).IsRangeExpanded, "확장 타일 위인데 범위 확장 안 됨");
        Assert.Greater(target.AttackSpeed, noExpand, "대각선 슬라임이 버프를 받지 못함");

        // 타일에서 내리면 다시 4방향
        SlimeFieldController.Instance.RemoveFieldSlime(buffer);
        yield return WaitFrames(3);
        Assert.AreEqual(noExpand, target.AttackSpeed, 0.0001f, "버프 슬라임이 사라졌는데 버프가 남음");
        AssertNoErrors();
    }
}
