using System.Collections;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 슬라임을 다른 슬라임 위에 놓았을 때: 머지가 되면 합성, 안 되면 두 슬라임 자리 교체 (SlimeFieldController.Merge)
// 드래그 입력 대신 놓은 뒤 호출되는 Merge(source, target, originPos)를 직접 호출
public class MergeSwapPlayTest : InGamePlayTestBase
{
    const int MAX = SlimeData.MAX_MERGE_LEVEL;

    [UnityTest]
    public IEnumerator DropOnSlime_MergeOrSwap()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    // 킹 ↔ 슬라임: 서로 위에 놓으면 자리 교체 (SlimeFieldController.DropAt)
    [UnityTest]
    public IEnumerator KingSlime_SwapWithSlime()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return KingBody();
        AssertNoErrors();
    }

    static readonly MethodInfo DROP_AT = typeof(SlimeFieldController).GetMethod("DropAt",
        BindingFlags.NonPublic | BindingFlags.Instance);

    // 드래그해서 target 칸 위에 놓은 것처럼: 손으로 옮긴 위치에서 DropAt 호출
    static IEnumerator DragDrop(ITileObject source, Tile target)
    {
        Vector3 origin = source.Transform.position;
        source.StartDrag();
        source.Transform.position = target.transform.position;
        DROP_AT.Invoke(SlimeFieldController.Instance, new object[] { source, target.transform.position, origin });
        yield return WaitFrames(3);
    }

    IEnumerator KingBody()
    {
        Assert.IsNotNull(DROP_AT, "SlimeFieldController.DropAt 없음");
        KingSlime king = KingSlime.Instance;
        Tile kingTile = TileManager.Instance.GetTile(king);
        Assert.IsNotNull(kingTile, "킹이 타일 위에 없음");

        // 1) 킹을 슬라임 위에 놓음 → 교체
        Tile slimeTile = GetFreeTile(kingTile);
        Slime slime = SpawnSlime("Iron", slimeTile, 0);
        yield return WaitFrames(2);
        yield return DragDrop(king, slimeTile);
        Assert.AreSame(slimeTile, TileManager.Instance.GetTile(king), "킹 → 슬라임: 킹이 슬라임 칸으로 안 감");
        Assert.AreSame(kingTile, TileManager.Instance.GetTile(slime), "킹 → 슬라임: 슬라임이 킹 칸으로 안 옴");
        Assert.AreEqual((Vector2)slimeTile.transform.position, (Vector2)king.transform.position, "킹 위치");
        Assert.AreEqual((Vector2)kingTile.transform.position, (Vector2)slime.transform.position, "슬라임 위치");
        Assert.IsTrue(KingSlime.Instance.slimeInventory.curSlimes.Contains(slime), "교체인데 슬라임이 사라짐");

        // 2) 슬라임을 킹 위에 놓음 → 다시 교체 (원래 자리로)
        yield return DragDrop(slime, slimeTile);
        Assert.AreSame(kingTile, TileManager.Instance.GetTile(king), "슬라임 → 킹: 킹이 원래 칸으로 안 감");
        Assert.AreSame(slimeTile, TileManager.Instance.GetTile(slime), "슬라임 → 킹: 슬라임이 킹 칸으로 안 감");
        Assert.AreEqual((Vector2)kingTile.transform.position, (Vector2)king.transform.position, "킹 위치");
        Assert.AreEqual((Vector2)slimeTile.transform.position, (Vector2)slime.transform.position, "슬라임 위치");

        // 3) 킹이 타일 아이템 칸의 슬라임과 교체 → 아이템 효과가 킹에게, 슬라임에선 빠짐
        Tile itemTile = GetFreeTile(kingTile, slimeTile);
        KingSlime.Instance.AddItem("AttackPowerTile", itemTile).Forget();
        yield return WaitFrames(2);
        Assert.IsNotNull(itemTile.tileItem, "공격력 타일 설치 안 됨");
        Slime onItem = SpawnSlime("Bounce", itemTile, 0);
        yield return WaitFrames(2);
        float buffed = onItem.statDic[StatType.AttackPower];
        yield return DragDrop(king, itemTile);
        Assert.AreSame(itemTile, TileManager.Instance.GetTile(king), "아이템 칸: 킹이 안 감");
        Assert.AreSame(kingTile, TileManager.Instance.GetTile(onItem), "아이템 칸: 슬라임이 안 옴");
        Assert.Less(onItem.statDic[StatType.AttackPower], buffed, "아이템 칸에서 나간 슬라임에 효과가 남음");
        Assert.AreSame(itemTile.tileItem, king.shotModifier?.tileItem, "킹에 아이템 칸 연결 안 됨");

        // 4) 빈 칸 이동은 그대로
        Tile empty = GetFreeTile(kingTile, slimeTile, itemTile);
        yield return DragDrop(king, empty);
        Assert.AreSame(empty, TileManager.Instance.GetTile(king), "킹 빈 칸 이동 안 됨");
        Assert.IsNull(itemTile.tileObject, "킹이 떠난 칸이 비지 않음");

        SlimeFieldController.Instance.RemoveFieldSlime(slime);
        SlimeFieldController.Instance.RemoveFieldSlime(onItem);
        yield return WaitFrames(2);
    }

    static readonly MethodInfo DROP =typeof(SlimeFieldController).GetMethod("Merge",
        BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(Slime), typeof(Slime), typeof(Vector3) }, null);

    static IEnumerator Drop(Slime source, Slime target)
    {
        DROP.Invoke(SlimeFieldController.Instance, new object[] { source, target, source.transform.position });
        yield return WaitFrames(3);
    }

    IEnumerator Body()
    {
        Assert.IsNotNull(DROP, "SlimeFieldController.Merge(source, target, originPos) 없음");

        // 1) 다른 종 → 자리 교체 + 타일 아이템 효과도 따라 바뀜 (공격력 타일)
        Tile itemTile = GetFreeTile();
        KingSlime.Instance.AddItem("AttackPowerTile", itemTile).Forget();
        yield return WaitFrames(2);
        Assert.IsNotNull(itemTile.tileItem, "공격력 타일 설치 안 됨");
        Tile other = GetFreeTile(itemTile);

        Slime onItem = SpawnSlime("Iron", itemTile, 0);
        Slime source = SpawnSlime("Bounce", other, 0);
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
        Assert.IsTrue(KingSlime.Instance.slimeInventory.curSlimes.Contains(source) && KingSlime.Instance.slimeInventory.curSlimes.Contains(onItem), "교체인데 슬라임이 사라짐");
        SlimeFieldController.Instance.RemoveFieldSlime(source);
        SlimeFieldController.Instance.RemoveFieldSlime(onItem);
        yield return WaitFrames(2);

        // 2) 같은 종·다른 머지 레벨 → 교체
        Tile a = GetFreeTile(), b = GetFreeTile(a);
        Slime low = SpawnSlime("Iron", a, 0);
        Slime high = SpawnSlime("Iron", b, 1);
        yield return Drop(low, high);
        Assert.AreSame(b, TileManager.Instance.GetTile(low), "다른 레벨: 교체 안 됨");
        Assert.AreSame(a, TileManager.Instance.GetTile(high), "다른 레벨: 교체 안 됨");
        SlimeFieldController.Instance.RemoveFieldSlime(low);
        SlimeFieldController.Instance.RemoveFieldSlime(high);
        yield return WaitFrames(2);

        // 3) 최대 레벨끼리(같은 종) → 머지 불가 → 교체
        a = GetFreeTile(); b = GetFreeTile(a);
        Slime max1 = SpawnSlime("Iron", a, MAX);
        Slime max2 = SpawnSlime("Iron", b, MAX);
        yield return Drop(max1, max2);
        Assert.AreSame(b, TileManager.Instance.GetTile(max1), "최대 레벨: 교체 안 됨");
        Assert.AreSame(a, TileManager.Instance.GetTile(max2), "최대 레벨: 교체 안 됨");
        SlimeFieldController.Instance.RemoveFieldSlime(max1);
        SlimeFieldController.Instance.RemoveFieldSlime(max2);
        yield return WaitFrames(2);

        // 4) 같은 종·같은 레벨 → 합성 (교체 아님): 놓인 칸에 레벨 +1 슬라임, 끌어온 칸은 빔
        a = GetFreeTile(); b = GetFreeTile(a);
        Slime m1 = SpawnSlime("Iron", a, 0);
        Slime m2 = SpawnSlime("Iron", b, 0);
        int mergeCount = SlimeFieldController.Instance.mergeCount;
        yield return Drop(m1, m2);
        Assert.AreEqual(mergeCount + 1, SlimeFieldController.Instance.mergeCount, "같은 종·같은 레벨인데 합성 안 됨");
        Assert.IsTrue(a.IsEmpty, "합성 후 끌어온 칸이 비지 않음");
        Slime merged = b.tileObject as Slime;
        Assert.IsNotNull(merged, "합성 결과 슬라임 없음");
        Assert.AreEqual(1, merged.mergeLevel);
        SlimeFieldController.Instance.RemoveFieldSlime(merged);
        yield return WaitFrames(2);

        // 5) 희생은 다른 종과도 합성 (교체 아님)
        a = GetFreeTile(); b = GetFreeTile(a);
        Slime sac = SpawnSlime("Sacrifice", a, 0);
        Slime bounce = SpawnSlime("Bounce", b, 0);
        mergeCount = SlimeFieldController.Instance.mergeCount;
        yield return Drop(sac, bounce);
        Assert.AreEqual(mergeCount + 1, SlimeFieldController.Instance.mergeCount, "희생 합성이 교체로 처리됨");
        Slime sacResult = b.tileObject as Slime;
        Assert.IsNotNull(sacResult);
        Assert.AreEqual("Bounce", sacResult.key);
        SlimeFieldController.Instance.RemoveFieldSlime(sacResult);
        yield return WaitFrames(2);
    }
}
