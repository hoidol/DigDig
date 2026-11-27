using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-24) 타겟 방식 First(가장 먼저 나온 적) = 기본값, 얼음은 가까운 적 기본
// 머지(드래그 합성)·카드 합성 결과는 놓인 칸(타일 위) 미니언의 타겟 방식을 유지
public class FindTargetPlayTest : InGamePlayTestBase
{
    static readonly MethodInfo DROP = typeof(MinionFieldController).GetMethod("Merge",
        BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Minion), typeof(Minion), typeof(Vector3) }, null);

    [UnityTest]
    public IEnumerator FirstTarget_DefaultAndMergeKeepsTileMinionSetting()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        // ── 기본값 ──
        current = "기본값";
        Assert.AreEqual(FindTargetType.First, Slime.Instance.FindTargetType, "슬라임 기본 = 먼저 온 적");
        Minion pierce = SpawnMinion("Pierce", GetFreeTile(), 0);
        Minion ice = SpawnMinion("Ice", GetFreeTile(), 0);
        yield return WaitFrames(2);
        Assert.AreEqual(FindTargetType.First, pierce.FindTargetType, "미니언 기본 = 먼저 온 적");
        Assert.AreEqual(FindTargetType.Closest, ice.FindTargetType, "얼음 기본 = 가까운 적");
        MinionFieldController.Instance.RemoveFieldMinion(pierce);
        MinionFieldController.Instance.RemoveFieldMinion(ice);
        yield return WaitFrames(2);

        // ── First = 사거리 안에서 가장 먼저 소환된 적 (멀어도) ──
        current = "First 선택";
        Enemy.FreezeAll(30f); // 적이 움직이지 않게
        Vector2 center = Slime.Instance.transform.position;
        Enemy prefab = GameManager.Instance.stageData.GetEnemyPrefab(EnemyType.Normal, DefenceType.Normal);
        Enemy early = EnemyFieldController.Instance.Instantiate(prefab);
        early.Spawn(center + new Vector2(1.5f, 0f), 0); // 먼저 소환, 멀리
        Enemy late = EnemyFieldController.Instance.Instantiate(prefab);
        late.Spawn(center + new Vector2(0.5f, 0f), 0);  // 나중 소환, 가까이
        yield return WaitRealtime(0.6f); // 등장 연출(0.15초)이 끝나야 충돌체가 켜짐 (Enemy.Apear)
        yield return WaitFrames(2);
        Assert.Less(early.SpawnOrder, late.SpawnOrder, "소환 순서");
        LayerMask mask = 1 << 3; // 미니언 기본 targetLayerMask와 같은 적 레이어
        Assert.AreEqual(early.transform, InGameUtil.FindTarget(center, 3f, mask, FindTargetType.First), "First = 먼저 나온 적");
        Assert.AreEqual(late.transform, InGameUtil.FindTarget(center, 3f, mask, FindTargetType.Closest), "Closest = 가까운 적");
        var two = InGameUtil.FindTargets(center, 3f, mask, FindTargetType.First, 2);
        CollectionAssert.AreEqual(new[] { early.transform, late.transform }, two, "여러 타겟도 소환 순서대로");
        early.TakeDamage(KILL);
        late.TakeDamage(KILL);
        Enemy.ClearFreezeAll();
        yield return WaitFrames(2);

        // ── 드래그 합성: 놓인 칸(target) 미니언의 설정 유지 ──
        current = "드래그 합성";
        Assert.IsNotNull(DROP, "MinionFieldController.Merge(source, target, originPos) 없음");
        Minion onTile = SpawnMinion("Ice", GetFreeTile(), 0);
        Minion dragged = SpawnMinion("Ice", GetFreeTile(), 0);
        yield return WaitFrames(2);
        onTile.SetFindTargetType(FindTargetType.HighHp);
        dragged.SetFindTargetType(FindTargetType.First);
        Tile mergeTile = TileManager.Instance.GetTile(onTile);
        DROP.Invoke(MinionFieldController.Instance, new object[] { dragged, onTile, dragged.transform.position });
        yield return WaitFrames(3);
        Minion merged = mergeTile.tileObject as Minion;
        Assert.IsNotNull(merged, "합성 결과가 놓인 칸에 없음");
        Assert.AreEqual(1, merged.mergeLevel, "합성 결과 머지 레벨");
        Assert.AreEqual(FindTargetType.HighHp, merged.FindTargetType, "합성 결과 = 놓인 칸 미니언 설정 (얼음 기본값 Closest가 아님)");

        // ── 카드 합성(같은 칸 교체): 그 칸 미니언 설정 유지 ──
        current = "카드 합성";
        merged.SetFindTargetType(FindTargetType.First);
        Minion replaced = MinionFieldController.Instance.ReplaceMinion(merged, "Ice", merged.mergeLevel + 1);
        yield return WaitFrames(2);
        Assert.AreEqual(FindTargetType.First, replaced.FindTargetType, "카드 합성 결과 = 그 칸 미니언 설정");
        MinionFieldController.Instance.RemoveFieldMinion(replaced);
        yield return WaitFrames(2);
    }
}
