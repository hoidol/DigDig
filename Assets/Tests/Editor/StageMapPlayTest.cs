using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-24) 스테이지 맵 Stage0~5: 실행 때 그 맵 프리팹 하나만 불러옴 + 타일 24칸 이상 index 중복 없음 + 적이 맵 경로로 소환·이동해 도착까지 감, 에러 없음
public class StageMapPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator Map_SpawnAndWalk([Values(0, 1, 2, 3, 4, 5)] int mapIndex)
    {
        if (!Application.isPlaying)
        {
            SessionState.SetInt(TileManager.FORCE_MAP_INDEX_TEST_KEY, mapIndex);
            BeforeEnterPlayMode();
            yield return new EnterPlayMode();
        }
        SessionState.EraseInt(TileManager.FORCE_MAP_INDEX_TEST_KEY);
        yield return WaitGameStart();
        yield return Body(mapIndex);
    }

    IEnumerator Body(int mapIndex)
    {
        current = $"맵 {mapIndex}";
        TileManager tm = TileManager.Instance;
        Stage stage = tm.ActiveStage;
        Assert.AreEqual(mapIndex, stage.mapIndex, "고른 맵이 아님");
        Assert.AreEqual(1, tm.GetComponentsInChildren<Stage>(true).Length, "맵은 하나만 불러옴");
        Assert.GreaterOrEqual(tm.tiles.Length, 24);
        Assert.IsTrue(tm.tiles.All(t => t.transform.IsChildOf(stage.transform)), "다른 맵 타일이 섞임");
        Assert.IsFalse(TileManager.HasDuplicateIndex(tm.tiles), "index 중복");

        EnemyFieldController field = EnemyFieldController.Instance;
        Assert.AreEqual(stage.routes.Length, field.Routes.Count, "경로 수");
        Vector2 end = field.Routes[0][field.Routes[0].Length - 1].position;
        foreach (Transform[] route in field.Routes)
            Assert.Less(Vector2.Distance(end, route[route.Length - 1].position), 0.01f, "도착 위치가 다름");

        // 브레이크타임 경로 표시(EnemyPathPreview): 선 + 달리는 화살표가 실제로 그려짐
        yield return WaitRealtime(0.8f);
        EnemyPathPreview preview = field.GetComponent<EnemyPathPreview>();
        Assert.IsNotNull(preview, "EnemyPathPreview 없음");
        LineRenderer[] lines = preview.GetComponentsInChildren<LineRenderer>(false);
        SpriteRenderer[] arrows = preview.GetComponentsInChildren<SpriteRenderer>(false);
        Assert.IsNotEmpty(lines, "경로선이 안 그려짐");
        Assert.IsTrue(lines.All(l => l.enabled && l.positionCount >= 2 && l.startColor.a > 0f), "경로선 상태");
        Assert.GreaterOrEqual(arrows.Length, 2, "달리는 화살표가 안 보임");
        Assert.IsTrue(arrows.Any(r => r.color.a > 0.1f), "화살표가 투명함");
        // 화살표가 맵 경로 근처에 있음
        Bounds mapBounds = stage.GetWorldBounds();
        mapBounds.Expand(1f);
        Assert.IsTrue(arrows.All(r => mapBounds.Contains(new Vector3(r.transform.position.x, r.transform.position.y, mapBounds.center.z))), "화살표가 맵 밖");

        // wave 시작 → 적이 갈래마다 출발 위치에서 나와 걷다가 도착(경로 끝)까지 감
        Time.timeScale = 8f;
        GameManager.Instance.EndBreakTime();
        int reached = 0;
        double timeout = EditorApplication.timeSinceStartup + 60;
        var seenRoutes = new System.Collections.Generic.HashSet<Transform[]>();
        Slime slime = Slime.Instance;
        while (EditorApplication.timeSinceStartup < timeout)
        {
            slime.enabled = false; // 슬라임 공격 끔 → 적이 경로 끝까지 감
            foreach (Enemy e in field.activeEnemies)
                if (e.route != null)
                    seenRoutes.Add(e.route);
            if (slime.health.curHp < slime.MaxHp)
                reached++; // 경로 끝 도착 = 슬라임 피해
            slime.health.curHp = slime.MaxHp;
            if (seenRoutes.Count >= field.Routes.Count && reached > 0)
                break;
            if (EditorApplication.isPaused)
                EditorApplication.isPaused = false;
            yield return null;
        }
        Time.timeScale = 1f;
        Assert.AreEqual(field.Routes.Count, seenRoutes.Count, "모든 경로로 적이 나오지 않음");
        Assert.Greater(reached, 0, "경로 끝에 도착한 적 없음");
        AssertNoErrors();
    }
}
