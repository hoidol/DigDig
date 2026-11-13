using System.Collections;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 슬라임을 다른 슬라임 위에 놓았을 때: 머지가 되면 합성, 안 되면 두 슬라임 자리 교체 (MinionFieldController.Merge)
// 드래그 입력 대신 놓은 뒤 호출되는 Merge(source, target, originPos)를 직접 호출
public class MergeSwapPlayTest : InGamePlayTestBase
{
    const int MAX = MinionData.MAX_MERGE_LEVEL;

    [UnityTest]
    public IEnumerator DropOnMinion_MergeOrSwap()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    // 킹 ↔ 슬라임: 서로 위에 놓으면 자리 교체 (MinionFieldController.DropAt)
    [UnityTest]
    public IEnumerator KingSlime_SwapWithMinion()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return KingBody();
        AssertNoErrors();
    }

    static readonly MethodInfo DROP_AT = typeof(MinionFieldController).GetMethod("DropAt",
        BindingFlags.NonPublic | BindingFlags.Instance);

    // 드래그해서 target 칸 위에 놓은 것처럼: 손으로 옮긴 위치에서 DropAt 호출
    static IEnumerator DragDrop(ITileObject source, Tile target)
    {
        Vector3 origin = source.Transform.position;
        source.StartDrag();
        source.Transform.position = target.transform.position;
        DROP_AT.Invoke(MinionFieldController.Instance, new object[] { source, target.transform.position, origin });
        yield return WaitFrames(3);
    }

    IEnumerator KingBody()
    {
        Assert.IsNotNull(DROP_AT, "MinionFieldController.DropAt 없음");
        KingSlime king = KingSlime.Instance;
        Tile kingTile = TileManager.Instance.GetTile(king);
        Assert.IsNotNull(kingTile, "킹이 타일 위에 없음");

        // 1) 킹을 슬라임 위에 놓음 → 교체
        Tile minionTile = GetFreeTile(kingTile);
        Minion minion = SpawnMinion("Iron", minionTile, 0);
        yield return WaitFrames(2);
        yield return DragDrop(king, minionTile);
        Assert.AreSame(minionTile, TileManager.Instance.GetTile(king), "킹 → 슬라임: 킹이 슬라임 칸으로 안 감");
        Assert.AreSame(kingTile, TileManager.Instance.GetTile(minion), "킹 → 슬라임: 슬라임이 킹 칸으로 안 옴");
        Assert.AreEqual((Vector2)minionTile.transform.position, (Vector2)king.transform.position, "킹 위치");
        Assert.AreEqual((Vector2)kingTile.transform.position, (Vector2)minion.transform.position, "슬라임 위치");
        Assert.IsTrue(KingSlime.Instance.minionInventory.curMinions.Contains(minion), "교체인데 슬라임이 사라짐");

        // 2) 슬라임을 킹 위에 놓음 → 다시 교체 (원래 자리로)
        yield return DragDrop(minion, minionTile);
        Assert.AreSame(kingTile, TileManager.Instance.GetTile(king), "슬라임 → 킹: 킹이 원래 칸으로 안 감");
        Assert.AreSame(minionTile, TileManager.Instance.GetTile(minion), "슬라임 → 킹: 슬라임이 킹 칸으로 안 감");
        Assert.AreEqual((Vector2)kingTile.transform.position, (Vector2)king.transform.position, "킹 위치");
        Assert.AreEqual((Vector2)minionTile.transform.position, (Vector2)minion.transform.position, "슬라임 위치");

        // 3) 킹이 타일 아이템 칸의 슬라임과 교체 → 아이템 효과가 킹에게, 슬라임에선 빠짐
        Tile itemTile = GetFreeTile(kingTile, minionTile);
        KingSlime.Instance.AddItem("AttackPowerTile", itemTile).Forget();
        yield return WaitFrames(2);
        Assert.IsNotNull(itemTile.tileItem, "공격력 타일 설치 안 됨");
        Minion onItem = SpawnMinion("Bounce", itemTile, 0);
        yield return WaitFrames(2);
        float buffed = onItem.statDic[StatType.AttackPower];
        yield return DragDrop(king, itemTile);
        Assert.AreSame(itemTile, TileManager.Instance.GetTile(king), "아이템 칸: 킹이 안 감");
        Assert.AreSame(kingTile, TileManager.Instance.GetTile(onItem), "아이템 칸: 슬라임이 안 옴");
        Assert.Less(onItem.statDic[StatType.AttackPower], buffed, "아이템 칸에서 나간 슬라임에 효과가 남음");
        Assert.AreSame(itemTile.tileItem, king.shotModifier?.tileItem, "킹에 아이템 칸 연결 안 됨");

        // 4) 빈 칸 이동은 그대로
        Tile empty = GetFreeTile(kingTile, minionTile, itemTile);
        yield return DragDrop(king, empty);
        Assert.AreSame(empty, TileManager.Instance.GetTile(king), "킹 빈 칸 이동 안 됨");
        Assert.IsNull(itemTile.tileObject, "킹이 떠난 칸이 비지 않음");

        MinionFieldController.Instance.RemoveFieldMinion(minion);
        MinionFieldController.Instance.RemoveFieldMinion(onItem);
        yield return WaitFrames(2);
    }

    static readonly MethodInfo DROP =typeof(MinionFieldController).GetMethod("Merge",
        BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(Minion), typeof(Minion), typeof(Vector3) }, null);

    static IEnumerator Drop(Minion source, Minion target)
    {
        DROP.Invoke(MinionFieldController.Instance, new object[] { source, target, source.transform.position });
        yield return WaitFrames(3);
    }

    IEnumerator Body()
    {
        Assert.IsNotNull(DROP, "MinionFieldController.Merge(source, target, originPos) 없음");

        // 1) 다른 종 → 자리 교체 + 타일 아이템 효과도 따라 바뀜 (공격력 타일)
        Tile itemTile = GetFreeTile();
        KingSlime.Instance.AddItem("AttackPowerTile", itemTile).Forget();
        yield return WaitFrames(2);
        Assert.IsNotNull(itemTile.tileItem, "공격력 타일 설치 안 됨");
        Tile other = GetFreeTile(itemTile);

        Minion onItem = SpawnMinion("Iron", itemTile, 0);
        Minion source = SpawnMinion("Bounce", other, 0);
        yield return WaitFrames(2);
        float ironBuffed = onItem.statDic[StatType.AttackPower];
        float bounceBase = source.statDic[StatType.AttackPower];

        yield return Drop(source, onItem);
        Assert.AreSame(itemTile, TileManager.Instance.GetTile(source), "다른 종: 끌어온 슬라임이 상대 칸으로 안 감");
        Assert.AreSame(other, TileManager.Instance.GetTile(onItem), "다른 종: 상대 슬라임이 원래 칸으로 안 옴");
        Assert.AreEqual((Vector2)itemTile.transform.position, (Vector2)source.transform.position, "끌어온 슬라임 위치");
        Assert.AreEqual((Vector2)other.transform.position, (Vector2)onItem.transform.position, "상대 슬라임 위치");
        Assert.Greater(source.statDic[StatType.AttackPower], bounceBase, "교체 후 아이템 칸 슬라임에 효과가 안 붙음");
        Assert.Less(onItem.statDic[StatType.AttackPower], ironBuffed, "교체 후 아이템 칸에서 나간 슬라임에 효과가 남음");
        Assert.IsTrue(KingSlime.Instance.minionInventory.curMinions.Contains(source) && KingSlime.Instance.minionInventory.curMinions.Contains(onItem), "교체인데 슬라임이 사라짐");
        MinionFieldController.Instance.RemoveFieldMinion(source);
        MinionFieldController.Instance.RemoveFieldMinion(onItem);
        yield return WaitFrames(2);

        // 2) 같은 종·다른 머지 레벨 → 교체
        Tile a = GetFreeTile(), b = GetFreeTile(a);
        Minion low = SpawnMinion("Iron", a, 0);
        Minion high = SpawnMinion("Iron", b, 1);
        yield return Drop(low, high);
        Assert.AreSame(b, TileManager.Instance.GetTile(low), "다른 레벨: 교체 안 됨");
        Assert.AreSame(a, TileManager.Instance.GetTile(high), "다른 레벨: 교체 안 됨");
        MinionFieldController.Instance.RemoveFieldMinion(low);
        MinionFieldController.Instance.RemoveFieldMinion(high);
        yield return WaitFrames(2);

        // 3) 최대 레벨끼리(같은 종) → 머지 불가 → 교체
        a = GetFreeTile(); b = GetFreeTile(a);
        Minion max1 = SpawnMinion("Iron", a, MAX);
        Minion max2 = SpawnMinion("Iron", b, MAX);
        yield return Drop(max1, max2);
        Assert.AreSame(b, TileManager.Instance.GetTile(max1), "최대 레벨: 교체 안 됨");
        Assert.AreSame(a, TileManager.Instance.GetTile(max2), "최대 레벨: 교체 안 됨");
        MinionFieldController.Instance.RemoveFieldMinion(max1);
        MinionFieldController.Instance.RemoveFieldMinion(max2);
        yield return WaitFrames(2);

        // 4) 같은 종·같은 레벨 → 합성 (교체 아님): 놓인 칸에 레벨 +1 슬라임, 끌어온 칸은 빔
        a = GetFreeTile(); b = GetFreeTile(a);
        Minion m1 = SpawnMinion("Iron", a, 0);
        Minion m2 = SpawnMinion("Iron", b, 0);
        int mergeCount = MinionFieldController.Instance.mergeCount;
        yield return Drop(m1, m2);
        Assert.AreEqual(mergeCount + 1, MinionFieldController.Instance.mergeCount, "같은 종·같은 레벨인데 합성 안 됨");
        Assert.IsTrue(a.IsEmpty, "합성 후 끌어온 칸이 비지 않음");
        Minion merged = b.tileObject as Minion;
        Assert.IsNotNull(merged, "합성 결과 슬라임 없음");
        Assert.AreEqual(1, merged.mergeLevel);
        MinionFieldController.Instance.RemoveFieldMinion(merged);
        yield return WaitFrames(2);

        // 5) 희생은 다른 종과도 합성 (교체 아님)
        a = GetFreeTile(); b = GetFreeTile(a);
        Minion sac = SpawnMinion("Sacrifice", a, 0);
        Minion bounce = SpawnMinion("Bounce", b, 0);
        mergeCount = MinionFieldController.Instance.mergeCount;
        yield return Drop(sac, bounce);
        Assert.AreEqual(mergeCount + 1, MinionFieldController.Instance.mergeCount, "희생 합성이 교체로 처리됨");
        Minion sacResult = b.tileObject as Minion;
        Assert.IsNotNull(sacResult);
        Assert.AreEqual("Bounce", sacResult.key);
        MinionFieldController.Instance.RemoveFieldMinion(sacResult);
        yield return WaitFrames(2);
    }
}
