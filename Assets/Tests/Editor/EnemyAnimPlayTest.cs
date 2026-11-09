using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.TestTools;

// 적 애니메이션: Walk가 기본, 처치되면 Dead 재생 → 끝나면 비활성화 후 풀로 (경로 끝 도착은 바로 비활성화)
public class EnemyAnimPlayTest : InGamePlayTestBase
{
    const string ENEMY_PREFAB_FOLDER = "Assets/3.Prefabs/Stage/0/Enemy";
    const float SPAWN_TIMEOUT = 10f; // 실시간(초)

    static readonly DamageData KILL = new DamageData { damage = 1e12f, attackType = AttackType.AllRounder };

    // 일반·엘리트 적 프리팹: 자식 Animator + Walk/Dead 상태 + root 연결
    [Test]
    public void EnemyPrefabs_HaveWalkDeadStateAndRoot()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { ENEMY_PREFAB_FOLDER });
        int checkedCount = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null || go.GetComponent<Enemy>() == null)
                continue;
            checkedCount++;

            var so = new SerializedObject(go.GetComponent<Enemy>());
            Assert.IsNotNull(so.FindProperty("root").objectReferenceValue, $"{path}: root 미연결");
            Assert.IsNotNull(so.FindProperty("hpPoint").objectReferenceValue, $"{path}: hpPoint 미연결");

            Animator animator = go.GetComponentInChildren<Animator>(true);
            Assert.IsNotNull(animator, $"{path}: Animator 없음");
            var controller = animator.runtimeAnimatorController as AnimatorController;
            Assert.IsNotNull(controller, $"{path}: AnimatorController 없음");

            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            string[] states = sm.states.Select(s => s.state.name).ToArray();
            Assert.Contains("Walk", states, $"{path} ({controller.name}): Walk 상태 없음");
            Assert.Contains("Dead", states, $"{path} ({controller.name}): Dead 상태 없음");
            // 기본 상태는 검사하지 않음: 스폰 시 코드에서 Walk를 재생 (Enemy.Spawn)
        }
        Assert.Greater(checkedCount, 0, "검사한 적 프리팹 없음");
    }

    static Enemy WaitActiveEnemy(System.Func<Enemy, bool> filter = null)
    {
        return EnemyFieldController.Instance.activeEnemies.FirstOrDefault(e => e != null && e.CurHp > 0 && (filter == null || filter(e)));
    }

    static IEnumerator WaitForEnemy()
    {
        double end = EditorApplication.timeSinceStartup + SPAWN_TIMEOUT;
        while (WaitActiveEnemy() == null && EditorApplication.timeSinceStartup < end)
            yield return WaitRealtime(0f);
        yield return WaitRealtime(0.3f); // 등장 연출(0.15초) 끝나고 충돌 켜질 때까지
    }

    [UnityTest]
    public IEnumerator Enemy_Walk_Then_DeadAnim_Then_Deactivate()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return DeadBody();
    }

    IEnumerator DeadBody()
    {
        current = "적 처치";
        yield return WaitForEnemy();
        Enemy enemy = WaitActiveEnemy();
        Assert.IsNotNull(enemy, "적이 소환되지 않음");
        Assert.IsNotNull(enemy.animator, $"{enemy.name}: Animator 없음");

        // 이동 중엔 Walk
        Assert.AreEqual(Enemy.WALK_ANIM_HASH, enemy.animator.GetCurrentAnimatorStateInfo(0).shortNameHash, $"{enemy.name}: 이동 중 Walk가 아님");

        int countBefore = EnemyFieldController.Instance.ActiveEnemyCount;
        enemy.TakeDamage(KILL);

        // 죽은 직후: 활성 상태로 Dead 재생, 충돌 꺼짐, 타겟 불가, 활성 적 수에서는 빠짐
        Assert.IsTrue(enemy.IsDying, "죽은 직후 Dead 재생 상태가 아님");
        Assert.IsTrue(enemy.gameObject.activeSelf, "Dead 애니메이션 전에 바로 비활성화됨");
        Assert.IsFalse(enemy.GetComponent<Collider2D>().enabled, "죽은 적의 충돌이 켜져 있음");
        Assert.IsFalse(enemy.CanHit(), "죽은 적이 타겟 가능");
        Assert.IsFalse(EnemyFieldController.Instance.activeEnemies.Contains(enemy), "죽은 적이 activeEnemies에 남음");
        Assert.Less(EnemyFieldController.Instance.ActiveEnemyCount, countBefore + 1, "죽은 적이 활성 적 수에 남음");

        yield return WaitFrames(2);
        Assert.AreEqual(Enemy.DEAD_ANIM_HASH, enemy.animator.GetCurrentAnimatorStateInfo(0).shortNameHash, "Dead 애니메이션이 재생되지 않음");

        // Dead 재생 중 추가 피해·이동 없음
        Vector3 pos = enemy.transform.position;
        enemy.TakeDamage(KILL);
        yield return WaitFrames(5);
        Assert.Less(Vector3.Distance(pos, enemy.transform.position), 0.01f, "Dead 재생 중 이동함");

        // 애니메이션이 끝나면 비활성화 (클립 길이만큼은 보여 주고, 최대 대기 시간 전에)
        var deadInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        float deadLength = deadInfo.length / Mathf.Max(0.01f, deadInfo.speed * deadInfo.speedMultiplier * enemy.animator.speed);
        float deadStart = Time.time - deadInfo.normalizedTime * deadLength;
        double end = EditorApplication.timeSinceStartup + (Enemy.DEAD_ANIM_MAX_TIME + 1f) / Mathf.Max(0.1f, Time.timeScale);
        while (enemy.gameObject.activeSelf && EditorApplication.timeSinceStartup < end)
            yield return WaitRealtime(0f);
        float shown = Time.time - deadStart;
        Assert.IsFalse(enemy.gameObject.activeSelf, $"Dead 애니메이션 후 비활성화되지 않음 (클립 {deadLength:0.00}초)");
        Assert.IsFalse(enemy.IsDying);
        Assert.GreaterOrEqual(shown, Mathf.Min(deadLength, Enemy.DEAD_ANIM_MAX_TIME) - 0.1f, $"Dead 애니메이션이 끝나기 전에 비활성화됨 (클립 {deadLength:0.00}초, 표시 {shown:0.00}초)");
        Assert.AreEqual(0, EnemyFieldController.Instance.DyingEnemyCount, "Dead 끝난 적이 풀에 반환되지 않음");

        // 풀에서 재사용되면 다시 Walk부터, 충돌 켜짐
        current = "풀 재사용";
        double reuseEnd = EditorApplication.timeSinceStartup + SPAWN_TIMEOUT;
        while (!enemy.gameObject.activeSelf && EditorApplication.timeSinceStartup < reuseEnd)
            yield return WaitRealtime(0f);
        Assert.IsTrue(enemy.gameObject.activeSelf, "죽은 적이 풀에서 재사용되지 않음");
        yield return WaitRealtime(0.3f);
        Assert.IsFalse(enemy.IsDying, "재사용된 적이 Dead 상태");
        Assert.IsTrue(enemy.CanHit(), "재사용된 적 타겟 불가");
        Assert.IsTrue(enemy.GetComponent<Collider2D>().enabled, "재사용된 적 충돌 꺼짐");
        Assert.AreEqual(Enemy.WALK_ANIM_HASH, enemy.animator.GetCurrentAnimatorStateInfo(0).shortNameHash, "재사용된 적이 Walk가 아님");

        AssertNoErrors();
    }

    // 풀에서 꺼낸 적: 소환하는 순간 transform·rigidbody 모두 소환 위치 → 이전 위치(죽은 자리)에서 켜졌다가 한두 프레임 뒤 옮겨지지 않음
    [UnityTest]
    public IEnumerator PooledEnemy_SpawnsAtSpawnPosition_NoJump()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return PooledSpawnBody();
    }

    IEnumerator PooledSpawnBody()
    {
        current = "풀 재사용 위치";
        yield return WaitForEnemy();
        Enemy enemy = WaitActiveEnemy(e => e.enemyType == EnemyType.Normal);
        Assert.IsNotNull(enemy, "적이 소환되지 않음");

        // 소환 위치에서 멀리 떨어진 곳에서 죽고 바로 풀로
        Vector2 spawnPos = EnemyFieldController.Instance.GetSpawnPosition();
        Transform[] paths = EnemyFieldController.Instance.paths;
        Vector2 farPos = paths.Select(p => (Vector2)p.position).OrderByDescending(p => Vector2.Distance(p, spawnPos)).First();
        enemy.transform.position = farPos;
        enemy.Rigidbody2D.position = farPos; // transform만 바꾸면 rigidbody는 그대로 (Kinematic)
        yield return WaitFrames(2);
        Assert.Greater(Vector2.Distance(enemy.Rigidbody2D.position, spawnPos), 1f, "테스트 준비: 먼 위치로 옮겨지지 않음");
        enemy.TakeDamage(KILL);
        enemy.Deactivate();

        // 같은 프레임에 같은 종류를 꺼내면 방금 넣은 적(스택 맨 위)
        Enemy prefab = GameManager.Instance.stageData.GetEnemyPrefab(enemy.enemyType, enemy.defenceType);
        Enemy reused = EnemyFieldController.Instance.Instantiate(prefab);
        Assert.AreSame(enemy, reused, "풀에서 같은 적을 꺼내지 않음");
        Assert.IsFalse(reused.gameObject.activeSelf, "Spawn 전에 활성화됨 (이전 위치에서 켜짐)");
        reused.Spawn(spawnPos, 0);

        Assert.IsTrue(reused.gameObject.activeSelf);
        Assert.Less(Vector2.Distance(reused.transform.position, spawnPos), 0.01f, "소환 직후 transform 위치");
        Assert.Less(Vector2.Distance(reused.Rigidbody2D.position, spawnPos), 0.01f, "소환 직후 rigidbody 위치가 이전 자리");

        // 다음 몇 프레임 동안 이동량은 이동 속도만큼만 (튀지 않음)
        Vector2 prev = reused.transform.position;
        for (int i = 0; i < 5; i++)
        {
            yield return WaitFrames(1);
            Vector2 now = reused.transform.position;
            float maxStep = reused.enemyData.moveSpeed * Mathf.Max(Time.deltaTime, Time.fixedDeltaTime) * 2f + 0.05f;
            Assert.LessOrEqual(Vector2.Distance(prev, now), maxStep, $"{i + 1}프레임째 위치가 튐 ({prev} → {now})");
            prev = now;
        }

        AssertNoErrors();
    }

    [UnityTest]
    public IEnumerator Enemy_ReachPathEnd_DeactivateImmediately()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return PathEndBody();
    }

    IEnumerator PathEndBody()
    {
        current = "경로 끝 도착";
        yield return WaitForEnemy();
        Enemy enemy = WaitActiveEnemy();
        Assert.IsNotNull(enemy, "적이 소환되지 않음");

        // 마지막 지점에 바로 도착시킴
        Transform[] paths = EnemyFieldController.Instance.paths;
        enemy.pathIdx = paths.Length - 1;
        enemy.transform.position = paths[paths.Length - 1].position;
        double end = EditorApplication.timeSinceStartup + 2f;
        while (enemy.gameObject.activeSelf && EditorApplication.timeSinceStartup < end)
            yield return WaitFrames(1);

        Assert.IsFalse(enemy.gameObject.activeSelf, "경로 끝 도착 후 비활성화되지 않음 (Dead 재생 없이 바로)");
        Assert.IsFalse(enemy.IsDying, "경로 끝 도착인데 Dead 재생");
        Assert.IsFalse(EnemyFieldController.Instance.activeEnemies.Contains(enemy));

        // 부활(ClearAllEnemies) 시 Dead 재생 중인 적도 회수
        current = "부활 회수";
        yield return WaitForEnemy();
        Enemy dying = WaitActiveEnemy();
        Assert.IsNotNull(dying, "적이 소환되지 않음");
        dying.TakeDamage(KILL);
        Assert.IsTrue(dying.IsDying);
        EnemyFieldController.Instance.ClearAllEnemies();
        Assert.IsFalse(dying.gameObject.activeSelf, "부활 시 Dead 재생 중인 적이 남음");
        Assert.AreEqual(0, EnemyFieldController.Instance.DyingEnemyCount);
        Assert.AreEqual(0, EnemyFieldController.Instance.ActiveEnemyCount);

        AssertNoErrors();
    }
}
