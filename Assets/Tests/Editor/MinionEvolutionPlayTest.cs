using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// (2026-11-26) 5성 진화: 진화하는 종마다 5성 소환 → 진화 선택 창(1장) → 눌러서 진화 → 동작 클래스 붙음
// 발동 스킬(머리 위 버튼)은 쿨타임을 채워 바로 쓰고, 멈춘 적 옆에서 싸워 피해·에러 확인. 수치 = MinionEvolutionSetting
public class MinionEvolutionPlayTest : InGamePlayTestBase
{
    const float TIME_SCALE = 3f;
    const float FIGHT_TIME = 5f; // 게임 시간(초)

    [UnityTest]
    public IEnumerator AllEvolutions_SelectAndFight_NoErrors()
    {
        if (!Application.isPlaying)
        {
            BeforeEnterPlayMode();
            DataTableImporter.ImportByName("MinionData");    // 진화 이름·설명 (EvolutionName/EvolutionDesc 열)
            DataTableImporter.ImportByName("TranslateData");
            yield return new EnterPlayMode();
        }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        if (GameManager.Instance.isBreakTime)
            GameManager.Instance.EndBreakTime();
        Time.timeScale = TIME_SCALE;
        var setting = MinionEvolutionSetting.Instance;
        Assert.IsNotNull(setting.ice.hailPrefab, "눈송이 진화 눈 프리팹 (Resources/MinionEvolutionSetting ice.hailPrefab)");

        foreach (string key in MinionEvolutionSetting.EVOLVING_KEYS)
        {
            current = key;
            KingSlime.Instance.health.curHp = KingSlime.Instance.MaxHp;
            if (!MinionManager.Instance.minionDataDic.ContainsKey(key))
            {
                Debug.LogWarning($"[EvolutionTest] {key} 데이터 없음 → 건너뜀");
                continue;
            }
            GameTestUtil.ClearFieldMinions();
            ClearEnemies();
            yield return WaitFrames(2);

            Tile tile = GetCenterFreeTile();
            Assert.IsNotNull(tile, "빈 타일 없음");
            Minion minion = SpawnMinion(key, tile, MinionData.MAX_MERGE_LEVEL);
            Minion partner = null;
            if (key == "Lightning") // 찌릿이 진화 = 다른 찌릿이와 연결
                partner = SpawnMinion(key, GetFreeTile(tile), 0);
            yield return WaitFrames(3);

            // 진화 선택 창: 1장, 누르면 진화 (요청은 한 프레임 뒤에 처리 → 최대 1초 기다림)
            var canvas = MinionEvolutionCanvas.Instance;
            for (double end = UnityEditor.EditorApplication.timeSinceStartup + 1.0; UnityEditor.EditorApplication.timeSinceStartup < end && !canvas.panel.activeInHierarchy;)
                yield return null;
            Assert.IsTrue(canvas.panel.activeInHierarchy, $"{key}: 진화 선택 창이 안 뜸 (lv {minion.mergeLevel}, CanEvolve {minion.CanEvolve}, isEvolved {minion.isEvolved}, active {minion.gameObject.activeInHierarchy}, 갈래 {minion.minionData.GetEvolutions().Count}, 타입 {minion.GetType().Name}, 진화 동작 {minion.EvolutionAbility})");
            var shown = canvas.options.Where(o => o.gameObject.activeSelf).ToArray();
            Assert.AreEqual(1, shown.Length, $"{key}: 진화 선택지 1장");
            StringAssert.DoesNotContain("{", shown[0].descText.text, $"{key}: 설명 수치 자리 남음");
            Assert.IsNotEmpty(shown[0].descText.text, $"{key}: 진화 설명 (MinionData.csv EvolutionDesc)");
            shown[0].button.onClick.Invoke();
            yield return WaitFrames(2);
            yield return WaitRealtime(0.1f);
            Assert.IsTrue(minion.isEvolved, $"{key}: 눌렀는데 진화 안 됨");
            Assert.IsNotNull(minion.EvolutionAbility, $"{key}: 진화 동작 없음");
            Assert.AreEqual(setting.Get(key).BehaviourType, minion.EvolutionAbility.GetType(), $"{key}: 진화 동작 클래스");
            Assert.IsFalse(canvas.panel.activeInHierarchy, $"{key}: 진화 후 창이 안 닫힘");

            // (2026-11-26) 발동 스킬은 적이 없어도 발동 → 적을 놓기 전에 씀
            if (minion.EvolutionAbility is ActiveSkillEvolution active)
            {
                Assert.IsTrue(active.HasActiveSkill);
                active.FillCooldownForTest();
                Assert.IsTrue(active.IsActiveSkillReady, $"{key}: 쿨타임을 채웠는데 준비 안 됨");
                Assert.IsTrue(active.UseActiveSkill(), $"{key}: 적이 없는데 발동 스킬을 못 씀");
                Assert.IsTrue(active.IsActiveSkillRunning, $"{key}: 발동 중이 아님");
            }

            // 멈춘 적을 미니언 옆에 (피해는 적을 놓기 전부터 셈: 세서 금방 다 잡을 수 있음)
            double before = DamageMeterSystem.GetDamage(key); // 피해 집계 (회전체 적중 등 모든 경로)
            SpawnFrozenEnemiesNear(minion, 6);
            yield return WaitRealtime(0.2f); // 옮긴 위치가 물리에 반영되도록

            yield return WaitRealtime(FIGHT_TIME / TIME_SCALE);
            Assert.Greater(DamageMeterSystem.GetDamage(key), before, $"{key}: 진화 후 피해 없음");
            if (partner != null)
                Assert.IsTrue(minion.EvolutionAbility.GetComponentsInChildren<LineRenderer>().Any(l => l.gameObject.activeSelf), "찌릿이: 다른 찌릿이와 번개 선");
        }
        GameTestUtil.ClearFieldMinions();
        ClearEnemies();
        Enemy.ClearFreezeAll();
        Time.timeScale = 1f;
    }

    // 6*6 가운데 쪽 빈 칸 (경로가 그룹 둘레라 어느 칸이든 근처에 경로)
    static Tile GetCenterFreeTile()
    {
        foreach (var idx in new[] { new Vector2Int(2, 2), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2) })
        {
            Tile t = TileManager.Instance.GetTile(idx);
            if (t == null)
                continue;
            if (t.isBlocked)
                t.Unblock();
            if (t.IsEmpty)
                return t;
        }
        return GetFreeTile();
    }

    static void SpawnFrozenEnemiesNear(Minion minion, int count)
    {
        MinionTestWindow.SpawnEnemies(EnemyType.Normal, DefenceType.Normal, count);
        Enemy.FreezeAll(9999f);
        // 미니언이 때릴 수 있는 자리: 박격포 = 깃발 둘레, 회전체 = 회전 반경, 그 외 = 공격 사거리 절반
        Vector2 center = minion is MortarMinion mortar ? mortar.FlagPos : (Vector2)minion.transform.position;
        float radius = minion is MortarMinion m2 ? m2.boomRange * 0.5f
            : minion is OrbitMinion orbit ? orbit.orbitMachine.radius
            : Mathf.Min(0.9f, minion.AttackRange * 0.5f);
        int i = 0;
        foreach (Enemy e in EnemyFieldController.Instance.activeEnemies.ToList())
        {
            float angle = i * Mathf.PI * 2f / count;
            e.transform.position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            i++;
        }
    }

    static void ClearEnemies()
    {
        foreach (Enemy e in EnemyFieldController.Instance.activeEnemies.ToList())
            if (e != null)
                e.TakeDamage(KILL);
    }
}
