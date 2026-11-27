using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-25 갱신) 카드 소환 모드(MinionFieldController.LockMinionPosition): 한 번 놓은 미니언은 자리 고정
// - 미니언을 머지가 안 되는 미니언 위에 놓음 → 자리 교체 없이 원위치 (이전: 교체)
// - 같은 종·같은 머지 레벨 → 합성
// - 미니언을 빈 칸에 놓음 → 브레이크타임 = 주사위 코인 2개로 이동(확인 팝업, 모자라면 토스트) / wave 중 = 원위치
// - 슬라임 ↔ 미니언 자리 교체 없음 (원위치), 슬라임은 빈 칸으로 자유롭게 이동
public class MergeSwapPlayTest : InGamePlayTestBase
{
    const int MAX = MinionData.MAX_MERGE_LEVEL;

    [UnityTest]
    public IEnumerator DropOnMinion_MergeOrStay()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        Assert.IsTrue(MinionFieldController.LockMinionPosition, "카드 소환 모드가 아니면 이 테스트 규칙이 다름");
        yield return Body();
        AssertNoErrors();
    }

    [UnityTest]
    public IEnumerator Slime_NoSwap_MovesToEmpty()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return SlimeBody();
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

    IEnumerator SlimeBody()
    {
        Assert.IsNotNull(DROP_AT, "MinionFieldController.DropAt 없음");
        Slime slime = Slime.Instance;
        Tile slimeTile = TileManager.Instance.GetTile(slime);
        Assert.IsNotNull(slimeTile, "슬라임이 타일 위에 없음");

        // 1) 슬라임을 미니언 위에 놓음 → 교체 없이 원위치
        Tile minionTile = GetFreeTile(slimeTile);
        Minion minion = SpawnMinion("Iron", minionTile, 0);
        yield return WaitFrames(2);
        yield return DragDrop(slime, minionTile);
        Assert.AreSame(slimeTile, TileManager.Instance.GetTile(slime), "슬라임 → 미니언: 슬라임은 원래 칸");
        Assert.AreSame(minionTile, TileManager.Instance.GetTile(minion), "슬라임 → 미니언: 미니언은 그대로");
        Assert.AreEqual((Vector2)slimeTile.transform.position, (Vector2)slime.transform.position, "슬라임 위치 원위치");

        // 2) 미니언을 슬라임 위에 놓음 → 원위치
        yield return DragDrop(minion, slimeTile);
        Assert.AreSame(slimeTile, TileManager.Instance.GetTile(slime), "미니언 → 슬라임: 슬라임 그대로");
        Assert.AreSame(minionTile, TileManager.Instance.GetTile(minion), "미니언 → 슬라임: 미니언 원위치");
        Assert.AreEqual((Vector2)minionTile.transform.position, (Vector2)minion.transform.position, "미니언 위치 원위치");

        // 3) 슬라임은 빈 칸으로 이동
        Tile empty = GetFreeTile(slimeTile, minionTile);
        yield return DragDrop(slime, empty);
        Assert.AreSame(empty, TileManager.Instance.GetTile(slime), "슬라임 빈 칸 이동 안 됨");
        Assert.IsTrue(slimeTile.IsEmpty, "슬라임이 떠난 칸이 비지 않음");

        MinionFieldController.Instance.RemoveFieldMinion(minion);
        yield return WaitFrames(2);
    }

    static readonly MethodInfo DROP = typeof(MinionFieldController).GetMethod("Merge",
        BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(Minion), typeof(Minion), typeof(Vector3) }, null);

    static IEnumerator Drop(Minion source, Minion target, Vector3 origin)
    {
        DROP.Invoke(MinionFieldController.Instance, new object[] { source, target, origin });
        yield return WaitFrames(3);
    }

    // 머지가 안 되면 둘 다 그대로 + 끌어온 미니언은 원래 위치로
    IEnumerator AssertStay(Minion source, Minion target, string label)
    {
        Tile sa = TileManager.Instance.GetTile(source), tb = TileManager.Instance.GetTile(target);
        Vector3 origin = source.transform.position;
        source.transform.position = target.transform.position; // 끌어다 놓은 상태
        int mergeCount = MinionFieldController.Instance.mergeCount;
        yield return Drop(source, target, origin);
        Assert.AreEqual(mergeCount, MinionFieldController.Instance.mergeCount, $"{label}: 합성되면 안 됨");
        Assert.AreSame(sa, TileManager.Instance.GetTile(source), $"{label}: 끌어온 미니언은 원래 칸");
        Assert.AreSame(tb, TileManager.Instance.GetTile(target), $"{label}: 놓인 미니언은 그대로");
        Assert.AreEqual((Vector2)sa.transform.position, (Vector2)source.transform.position, $"{label}: 원위치");
    }

    IEnumerator Body()
    {
        Assert.IsNotNull(DROP, "MinionFieldController.Merge(source, target, originPos) 없음");

        // 1) 다른 종 → 교체 없이 원위치
        Tile a = GetFreeTile(), b = GetFreeTile(a);
        Minion iron = SpawnMinion("Iron", a, 0);
        Minion bounce = SpawnMinion("Bounce", b, 0);
        yield return WaitFrames(2);
        yield return AssertStay(bounce, iron, "다른 종");
        MinionFieldController.Instance.RemoveFieldMinion(iron);
        MinionFieldController.Instance.RemoveFieldMinion(bounce);
        yield return WaitFrames(2);

        // 2) 같은 종·다른 머지 레벨 → 원위치
        a = GetFreeTile(); b = GetFreeTile(a);
        Minion low = SpawnMinion("Iron", a, 0);
        Minion high = SpawnMinion("Iron", b, 1);
        yield return WaitFrames(2);
        yield return AssertStay(low, high, "다른 레벨");
        MinionFieldController.Instance.RemoveFieldMinion(low);
        MinionFieldController.Instance.RemoveFieldMinion(high);
        yield return WaitFrames(2);

        // 3) 최대 레벨끼리 → 머지 불가 → 원위치
        a = GetFreeTile(); b = GetFreeTile(a);
        Minion max1 = SpawnMinion("Iron", a, MAX);
        Minion max2 = SpawnMinion("Iron", b, MAX);
        yield return WaitFrames(2);
        yield return AssertStay(max1, max2, "최대 레벨");
        MinionFieldController.Instance.RemoveFieldMinion(max1);
        MinionFieldController.Instance.RemoveFieldMinion(max2);
        yield return WaitFrames(2);

        // 4) 같은 종·같은 레벨 → 합성: 놓인 칸에 레벨 +1, 끌어온 칸은 빔
        a = GetFreeTile(); b = GetFreeTile(a);
        Minion m1 = SpawnMinion("Iron", a, 0);
        Minion m2 = SpawnMinion("Iron", b, 0);
        yield return WaitFrames(2);
        int mergeCount = MinionFieldController.Instance.mergeCount;
        yield return Drop(m1, m2, m1.transform.position);
        Assert.AreEqual(mergeCount + 1, MinionFieldController.Instance.mergeCount, "같은 종·같은 레벨인데 합성 안 됨");
        Assert.IsTrue(a.IsEmpty, "합성 후 끌어온 칸이 비지 않음");
        Minion merged = b.tileObject as Minion;
        Assert.IsNotNull(merged, "합성 결과 미니언 없음");
        Assert.AreEqual(1, merged.mergeLevel);
        MinionFieldController.Instance.RemoveFieldMinion(merged);
        yield return WaitFrames(2);

        // 5) (2026-11-26) 브레이크타임에 미니언을 빈 칸에 놓음 → 주사위 코인 2개로 이동 (확인 팝업)
        Assert.IsTrue(GameManager.Instance.isBreakTime, "게임 시작 = 브레이크타임");
        Slime slime = Slime.Instance;
        a = GetFreeTile(); b = GetFreeTile(a);
        Minion moving = SpawnMinion("Iron", a, 0);
        yield return WaitFrames(2);
        //  코인 부족 → 팝업 없이 원위치
        slime.diceCoin = GameSetting.MOVE_MINION_DICE_COIN - 1;
        yield return DragDrop(moving, b);
        Assert.AreSame(a, TileManager.Instance.GetTile(moving), "코인 부족이면 원위치");
        Assert.IsFalse(YesOrNoCanvas.Instance != null && YesOrNoCanvas.Instance.gameObject.activeInHierarchy, "코인 부족이면 팝업 없음");
        Assert.AreEqual(GameSetting.MOVE_MINION_DICE_COIN - 1, slime.diceCoin);
        //  코인 충분 → 팝업 → No = 그대로
        slime.diceCoin = 5;
        yield return DragDrop(moving, b);
        Assert.IsTrue(YesOrNoCanvas.Instance.gameObject.activeInHierarchy, "이동 확인 팝업");
        Assert.AreSame(a, TileManager.Instance.GetTile(moving), "Yes 전엔 원래 칸");
        YesOrNoCanvas.Instance.OnClickedNo();
        yield return WaitFrames(2);
        Assert.AreSame(a, TileManager.Instance.GetTile(moving), "No = 이동 안 함");
        Assert.AreEqual(5, slime.diceCoin, "No = 코인 그대로");
        //  Yes = 코인 2개 쓰고 이동
        yield return DragDrop(moving, b);
        YesOrNoCanvas.Instance.OnClickedYes();
        yield return WaitFrames(2);
        Assert.AreSame(b, TileManager.Instance.GetTile(moving), "Yes = 이동");
        Assert.IsTrue(a.IsEmpty, "떠난 칸이 빔");
        Assert.AreEqual(5 - GameSetting.MOVE_MINION_DICE_COIN, slime.diceCoin, "주사위 코인 2개 소비");
        Assert.AreEqual((Vector2)b.transform.position, (Vector2)moving.transform.position);

        // 6) wave 중엔 빈 칸에 놓아도 원위치 (자리 고정)
        GameManager.Instance.EndBreakTime();
        yield return WaitFrames(2);
        Assert.IsFalse(GameManager.Instance.isBreakTime);
        slime.diceCoin = 5;
        yield return DragDrop(moving, a);
        Assert.AreSame(b, TileManager.Instance.GetTile(moving), "wave 중엔 이동 안 됨");
        Assert.AreEqual(5, slime.diceCoin, "wave 중엔 코인 안 씀");
        MinionFieldController.Instance.RemoveFieldMinion(moving);
        yield return WaitFrames(2);
    }
}
