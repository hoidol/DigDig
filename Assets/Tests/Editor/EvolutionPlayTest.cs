using System.Collections;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

// 진화 타일(EvolutionTileItem) → Slime.Evolve → 슬라임별 [각성] 발현 검사
// - 진화 타일 흐름: 최대 머지 레벨만 진화, 진화시키면 타일 사라짐, 옮겨도 유지, 이미 올라가 있는 슬라임에 설치해도 진화, 희생·성장·조커는 진화 안 함
// - 각성 효과: 상태 값(개수·확률), 발사 탄 스펙(효과·배율), 실제 피해(가시방패·화염 각성 스킬), 포위 진형 버프, 시체수집 보너스
public class EvolutionPlayTest : InGamePlayTestBase
{
    const float TIME_SCALE = 4f;
    const int MAX = SlimeData.MAX_MERGE_LEVEL;
    const string ELITE_PREFAB_PATH = "Assets/3.Prefabs/Stage/0/Enemy/EliteEnemy/NormalEliteEnemy.prefab";
    const float SPAWN_TIMEOUT = 10f; // 실시간(초)

    // ── 진화 타일 흐름 ─────────────────────────────────────

    [UnityTest]
    public IEnumerator EvolutionTile_EvolvesOnlyMaxLevel()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return EvolutionTileBody();
        AssertNoErrors();
    }

    // 진화 타일은 진화시킨 다음 프레임에 제거됨
    static IEnumerator WaitTileItemRemoved(Tile tile)
    {
        float end = Time.realtimeSinceStartup + 2f;
        while (tile.tileItem != null && Time.realtimeSinceStartup < end)
            yield return WaitFrames(1);
    }

    IEnumerator EvolutionTileBody()
    {
        Tile evoTile = GetFreeTile();
        Assert.IsNotNull(evoTile, "빈 타일 없음");
        KingSlime.Instance.AddItem("EvolutionTile", evoTile).Forget();
        yield return WaitFrames(2);
        Assert.IsInstanceOf<EvolutionTileItem>(evoTile.tileItem, "진화 타일이 설치되지 않음");

        // 최대 레벨 미만은 진화 안 함
        Slime low = SpawnSlime("Iron", evoTile, MAX - 1);
        yield return WaitFrames(2);
        Assert.IsFalse(low.isEvolved, "최대 레벨 미만인데 진화함");
        SlimeFieldController.Instance.RemoveFieldSlime(low);
        yield return WaitFrames(2);

        // 최대 레벨은 올라가면 진화, 진화 타일은 사라짐(1회용)
        Slime max = SpawnSlime("Iron", evoTile, MAX);
        yield return WaitFrames(2);
        Assert.IsTrue(max.isEvolved, "최대 레벨인데 진화 타일 위에서 진화하지 않음");
        Assert.IsFalse(max.CanEvolve, "진화 후에도 CanEvolve가 true");
        yield return WaitTileItemRemoved(evoTile);
        Assert.IsNull(evoTile.tileItem, "진화 후에도 진화 타일이 남아 있음");
        Assert.IsFalse(KingSlime.Instance.itemInventory.curItems.Any(i => i is EvolutionTileItem), "진화 타일이 인벤토리에 남아 있음");

        // 다른 칸으로 옮겨도 진화 유지
        Tile other = GetFreeTile(evoTile);
        evoTile.Clear();
        other.SetTileObject(max);
        yield return WaitFrames(2);
        Assert.IsTrue(max.isEvolved, "진화 타일에서 내려오자 진화가 풀림");

        // 사라진 칸에 다른 최대 레벨 슬라임을 올려도 진화 안 함
        Slime second = SpawnSlime("Bounce", evoTile, MAX);
        yield return WaitFrames(2);
        Assert.IsFalse(second.isEvolved, "진화 타일이 사라진 칸에서 진화함");
        SlimeFieldController.Instance.RemoveFieldSlime(second);
        yield return WaitFrames(2);

        // 희생·성장·조커는 진화하지 않음 (최대 레벨에서 쓸 각성이 없음), 진화 타일도 남음
        KingSlime.Instance.AddItem("EvolutionTile", evoTile).Forget();
        yield return WaitFrames(2);
        foreach (string key in new[] { "Sacrifice", "GrowUp", "Joker" })
        {
            Slime s = SpawnSlime(key, evoTile, MAX);
            yield return WaitFrames(2);
            Assert.IsFalse(s.CanEvolve, $"{key}: CanEvolve가 true");
            Assert.IsFalse(s.isEvolved, $"{key}: 진화함");
            SlimeFieldController.Instance.RemoveFieldSlime(s);
            yield return WaitFrames(2);
        }
        Assert.IsInstanceOf<EvolutionTileItem>(evoTile.tileItem, "진화하지 않는 슬라임인데 진화 타일이 사라짐");

        // 진화 타일 뽑기 후보 수에 진화한 슬라임·진화 안 하는 슬라임은 안 셈
        Slime growUp = SpawnSlime("GrowUp", GetFreeTile(evoTile, other), MAX);
        yield return WaitFrames(2);
        Assert.AreEqual(0, EvolutionTileItem.EvolvableSlimeCount, "진화할 수 없는 슬라임이 진화 후보로 셈");
        SlimeFieldController.Instance.RemoveFieldSlime(growUp);

        // 진화된 슬라임을 없애고 같은 종을 다시 소환하면(풀 재사용 포함) 진화 안 된 상태
        SlimeFieldController.Instance.RemoveFieldSlime(max);
        yield return WaitFrames(2);
        Slime fresh = SpawnSlime("Iron", other, MAX);
        yield return WaitFrames(2);
        Assert.IsFalse(fresh.isEvolved, "새로 소환한 슬라임이 진화 상태");

        // 이미 슬라임이 올라가 있는 칸에 진화 타일을 설치해도 바로 진화
        KingSlime.Instance.AddItem("EvolutionTile", other).Forget();
        yield return WaitFrames(2);
        Assert.IsTrue(fresh.isEvolved, "슬라임 위에 진화 타일을 설치했는데 진화하지 않음");
        yield return WaitTileItemRemoved(other);
        Assert.IsNull(other.tileItem, "슬라임 위에 설치해 진화시킨 진화 타일이 남아 있음");
        SlimeFieldController.Instance.RemoveFieldSlime(fresh);
        yield return WaitFrames(2);
    }

    // ── 상태 값으로 보이는 각성 ─────────────────────────────

    [UnityTest]
    public IEnumerator Evolve_ChangesSlimeValues()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return ValuesBody();
        AssertNoErrors();
    }

    IEnumerator ValuesBody()
    {
        // 빙석: 눈 1개 → 2개
        HailSlime hail = (HailSlime)SpawnSlime("Hail", GetFreeTile(), MAX);
        yield return WaitFrames(2);
        Assert.AreEqual(1, hail.hailCount);
        hail.Evolve();
        yield return WaitFrames(3);
        Assert.AreEqual(HailSlime.EVOLVE_HAIL_COUNT, hail.hailCount, "빙석: 눈 개수");
        Assert.GreaterOrEqual(hail.GetComponentsInChildren<HailStone>(true).Length, HailSlime.EVOLVE_HAIL_COUNT, "빙석: 눈 오브젝트가 늘지 않음");
        SlimeFieldController.Instance.RemoveFieldSlime(hail);

        // 진흙: 구슬 최대 +1
        MudSlime mud = (MudSlime)SpawnSlime("Mud", GetFreeTile(), MAX);
        int orbs = mud.maxOrbCount;
        mud.Evolve();
        Assert.AreEqual(orbs + MudSlime.EVOLVE_ADD_ORB_COUNT, mud.maxOrbCount, "진흙: 구슬 최대 수");
        SlimeFieldController.Instance.RemoveFieldSlime(mud);

        // 광란: 추가 탄 2발
        DopamineSlime dopamine = (DopamineSlime)SpawnSlime("Dopamine", GetFreeTile(), MAX);
        dopamine.Evolve();
        Assert.AreEqual(DopamineSlime.EVOLVE_EXTRA_SHOT_COUNT, dopamine.extraShotCount, "광란: 추가 탄 수");
        SlimeFieldController.Instance.RemoveFieldSlime(dopamine);

        // 지뢰: 한 번에 2개
        MineSlime mine = (MineSlime)SpawnSlime("Mine", GetFreeTile(), MAX);
        Assert.AreEqual(1, mine.ShootCount);
        mine.Evolve();
        Assert.AreEqual(MineSlime.EVOLVE_MINES_PER_THROW, mine.ShootCount, "지뢰: 한 번에 던지는 수");
        SlimeFieldController.Instance.RemoveFieldSlime(mine);

        // 사냥꾼: 2회마다 저격
        HunterSlime hunter = (HunterSlime)SpawnSlime("Hunter", GetFreeTile(), MAX);
        Assert.Greater(hunter.snipeInterval, HunterSlime.EVOLVE_SNIPE_INTERVAL);
        hunter.Evolve();
        Assert.AreEqual(HunterSlime.EVOLVE_SNIPE_INTERVAL, hunter.snipeInterval, "사냥꾼: 저격 주기");
        SlimeFieldController.Instance.RemoveFieldSlime(hunter);

        // 박격포: 슬로우 확률 50%
        MortarSlime mortar = (MortarSlime)SpawnSlime("Mortar", GetFreeTile(), MAX);
        Assert.Less(mortar.slowChance, MortarSlime.EVOLVE_SLOW_CHANCE);
        mortar.Evolve();
        Assert.AreEqual(MortarSlime.EVOLVE_SLOW_CHANCE, mortar.slowChance, 0.0001f, "박격포: 슬로우 확률");
        SlimeFieldController.Instance.RemoveFieldSlime(mortar);

        // 회전: 모든 구슬에 스턴
        OrbitSlime orbit = (OrbitSlime)SpawnSlime("Orbit", GetFreeTile(), MAX);
        Assert.IsFalse(orbit.orbitMachine.orbitOrbs.Any(o => o.bulletBehaviors.OfType<StunOnHitBehavior>().Any()), "회전: 진화 전에 스턴이 있음");
        orbit.Evolve();
        Assert.IsTrue(orbit.orbitMachine.orbitOrbs.Count > 0 && orbit.orbitMachine.orbitOrbs.All(o => o.bulletBehaviors.OfType<StunOnHitBehavior>().Any()), "회전: 구슬에 스턴이 붙지 않음");
        SlimeFieldController.Instance.RemoveFieldSlime(orbit);

        // 철벽: 모든 방패에 가시
        ShieldSlime shield = (ShieldSlime)SpawnSlime("Shield", GetFreeTile(), MAX);
        shield.Evolve();
        Assert.IsTrue(shield.orbitMachine.orbitOrbs.Count > 0 && shield.orbitMachine.orbitOrbs.OfType<ShieldOrbitOrb>().All(o => o.thornMaxHpRatio > 0), "철벽: 방패에 가시가 붙지 않음");
        SlimeFieldController.Instance.RemoveFieldSlime(shield);

        // 버서커: 최대 중첩일 때 공격력 ×1.2
        BerserkerSlime berserker = (BerserkerSlime)SpawnSlime("Berserker", GetFreeTile(), MAX);
        berserker.stack = berserker.maxStack;
        berserker.UpdateSlime();
        float ap = berserker.statDic[StatType.AttackPower];
        berserker.Evolve();
        Assert.AreEqual(ap * BerserkerSlime.EVOLVE_MAX_STACK_ATTACK_POWER_MUL, berserker.statDic[StatType.AttackPower], ap * 0.001f, "버서커: 최대 중첩 공격력");
        SlimeFieldController.Instance.RemoveFieldSlime(berserker);
        yield return WaitFrames(2);
    }

    // ── 발사 탄에 붙는 각성 ────────────────────────────────

    [UnityTest]
    public IEnumerator Evolve_AddsBulletEffects()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return BulletBody();
        AssertNoErrors();
    }

    IEnumerator BulletBody()
    {
        // 탄성·관통: 피해 보너스 force
        CheckBullet("Bounce", spec => spec.bulletForces.OfType<DamageBoostForce>().Any(), "탄성: 피해 보너스");
        CheckBullet("Pierce", spec => spec.bulletForces.OfType<DamageBoostForce>().Any(), "관통: 피해 보너스");
        // 냉기: 튕김, 번개: 감속, 천둥: 확률 감속
        CheckBullet("Ice", spec => spec.bulletBehaviors.OfType<BounceBehavior>().Any(), "냉기: 튕김");
        CheckBullet("Lightning", spec => spec.bulletBehaviors.OfType<SlowOnHitBehavior>().Any(), "번개: 감속");
        CheckBullet("Thunder", spec => spec.bulletBehaviors.OfType<SlowOnHitBehavior>().Any(), "천둥: 감속");
        // 강철: 엘리트·보스 배율 +0.2
        CheckBullet("Iron", spec => GetIronMul(spec) > 0f, "강철: 배율", compare: true);
        // 낙인: 최대 중첩 5
        CheckBullet("Stigma", spec => GetStigmaMaxStack(spec) >= StigmaSlime.EVOLVE_MAX_STACK, "낙인: 최대 중첩");
        yield return WaitFrames(2);
    }

    float ironBefore;

    static float GetIronMul(BulletSpec spec) => spec.bulletForces.OfType<BigEnemyDamageForce>().Select(f => f.mul).DefaultIfEmpty(0f).First();

    static int GetStigmaMaxStack(BulletSpec spec)
    {
        StigmaOnHitBehavior b = spec.bulletBehaviors.OfType<StigmaOnHitBehavior>().FirstOrDefault();
        if (b == null)
            return 0;
        return (int)typeof(StigmaOnHitBehavior).GetField("maxStack", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(b);
    }

    // 진화 전 탄은 조건 불충족, 진화 후 탄은 충족 (강철은 진화 전후 배율 비교)
    void CheckBullet(string key, System.Func<BulletSpec, bool> evolvedCheck, string label, bool compare = false)
    {
        current = key;
        Slime slime = SpawnSlime(key, GetFreeTile(), MAX);
        AllyBulletObject before = slime.GetBullet();
        Assert.IsNotNull(before, $"{label}: 탄 없음");
        if (compare)
            ironBefore = GetIronMul(before.spec);
        else
            Assert.IsFalse(evolvedCheck(before.spec), $"{label}: 진화 전에 이미 적용됨");
        before.gameObject.SetActive(false);

        slime.Evolve();
        AllyBulletObject after = slime.GetBullet();
        if (compare)
            Assert.AreEqual(ironBefore + IronSlime.EVOLVE_ELITE_BOSS_MUL_ADD, GetIronMul(after.spec), 0.0001f, label);
        else
            Assert.IsTrue(evolvedCheck(after.spec), $"{label}: 진화 후 적용 안 됨");
        after.gameObject.SetActive(false);
        SlimeFieldController.Instance.RemoveFieldSlime(slime);
    }

    // ── 실제 피해로 보는 각성 ──────────────────────────────

    [UnityTest]
    public IEnumerator Evolve_ThornAndFlameSkill_DealDamage()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return DamageBody();
        AssertNoErrors();
    }

    IEnumerator DamageBody()
    {
        Time.timeScale = TIME_SCALE;
        double end = EditorApplication.timeSinceStartup + SPAWN_TIMEOUT;
        while (FindEnemy() == null && EditorApplication.timeSinceStartup < end)
            yield return WaitRealtime(0f);
        Enemy enemy = FindEnemy();
        Assert.IsNotNull(enemy, "적이 소환되지 않음");
        MakeTanky(enemy);
        enemy.AddBlocker(); // 제자리에 묶어 둠

        // 가시방패: 막힌 적에게 1초마다 최대 체력 1% (피해는 철벽 슬라임 누적 피해로 확인 → 킹 공격과 구분)
        ShieldSlime shield = (ShieldSlime)SpawnSlime("Shield", GetFreeTile(), MAX);
        ShieldOrbitOrb orb = shield.orbitMachine.orbitOrbs.OfType<ShieldOrbitOrb>().First();
        typeof(ShieldOrbitOrb).GetMethod("TryBlock", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(orb, new object[] { enemy });
        Assert.AreEqual(1, orb.BlockedCount, "방패가 적을 막지 못함");
        shield.AccumulatedDamage = 0;
        yield return WaitRealtime(1.5f / TIME_SCALE);
        Assert.AreEqual(0f, shield.AccumulatedDamage, "진화 전인데 가시 피해");
        shield.Evolve();
        yield return WaitRealtime(2.5f / TIME_SCALE);
        Assert.GreaterOrEqual(shield.AccumulatedDamage, enemy.MaxHp * ShieldSlime.EVOLVE_THORN_MAX_HP_RATIO * 0.99f, "가시방패 피해가 없음");
        SlimeFieldController.Instance.RemoveFieldSlime(shield);
        yield return WaitFrames(2);

        // 화염 각성: 5초마다 사거리 안 적에게 피해 + 화상 (일반 공격은 공속 0으로 막고 확인)
        enemy.AddBlocker();
        FlameSlime flame = (FlameSlime)SpawnSlime("Flame", GetFreeTile(), MAX);
        enemy.transform.position = flame.transform.position;
        flame.statDic[StatType.AttackSpeed] = 0;
        flame.AccumulatedDamage = 0;
        yield return WaitRealtime((FlameSlime.EVOLVE_SKILL_COOLTIME + 1f) / TIME_SCALE);
        Assert.AreEqual(0f, flame.AccumulatedDamage, "진화 전인데 화염 피해");

        flame.Evolve();
        flame.statDic[StatType.AttackSpeed] = 0;
        enemy.transform.position = flame.transform.position;
        yield return WaitRealtime((FlameSlime.EVOLVE_SKILL_COOLTIME + 1f) / TIME_SCALE);
        Assert.Greater(flame.AccumulatedDamage, 0f, "화염 각성 스킬 피해 없음");
        Assert.IsTrue(enemy.GetComponent<StatusEffectHandler>().HasEffect("Flame"), "화염 각성 스킬 화상 없음");
        SlimeFieldController.Instance.RemoveFieldSlime(flame);
        Time.timeScale = 1f;
        yield return WaitFrames(2);
    }

    static Enemy FindEnemy()
    {
        foreach (Enemy e in EnemyFieldController.Instance.activeEnemies)
            if (e != null && e.CurHp > 0 && !e.IsEliteOrBoss && e.GetComponent<Collider2D>().enabled)
                return e;
        return null;
    }

    static void MakeTanky(Enemy enemy)
    {
        const float HP = 1e7f;
        enemy.maxHp = HP;
        enemy.curHp = HP;
    }

    // ── 포위 진형 (십자 버프 슬라임 각성) ──────────────────

    [UnityTest]
    public IEnumerator Evolve_CrossBuffSurround()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return SurroundBody();
        AssertNoErrors();
    }

    static bool IsUsable(Tile t) => t.tileItem == null && (t.tileObject == null || t.tileObject is Slime);

    static readonly Vector2Int[] CROSS = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };

    IEnumerator SurroundBody()
    {
        // 십자 4칸이 모두 같은 그룹인 칸 (3*3 그룹 가운데). 막힌 칸이면 풀어서 사용
        // 아이템 칸(앞 테스트의 진화 타일 등)·킹이 있는 칸은 제외
        Tile center = null;
        foreach (Tile t in TileManager.Instance.tiles)
        {
            Tile[] around = CROSS.Select(o => TileManager.Instance.GetTile(t.index + o)).ToArray();
            if (around.All(a => a != null && a.tileGroup == t.tileGroup) && around.Append(t).All(IsUsable))
            {
                center = t;
                break;
            }
        }
        Assert.IsNotNull(center, "3*3 그룹 가운데 칸 없음");
        Tile[] cross = CROSS.Select(o => TileManager.Instance.GetTile(center.index + o)).ToArray();
        foreach (Tile t in cross.Append(center))
        {
            if (t.isBlocked)
                t.Unblock();
            if (t.tileObject is Slime s)
                SlimeFieldController.Instance.RemoveFieldSlime(s);
        }
        yield return WaitFrames(2);

        Slime[] hosts = cross.Select(t => SpawnSlime("Iron", t, 0)).ToArray();
        yield return WaitFrames(2);
        float baseAp = hosts[0].statDic[StatType.AttackPower];

        CrossBuffSlime rage = (CrossBuffSlime)SpawnSlime("Rage", center, MAX);
        yield return WaitFrames(3);
        float buffed = hosts[0].statDic[StatType.AttackPower];
        Assert.Greater(buffed, baseAp, "격노 버프가 걸리지 않음");
        Assert.IsTrue(rage.IsSurrounded(), "십자 4칸이 다 찼는데 포위 판정 실패");
        Assert.AreEqual(rage.buffValue, rage.EffectiveBuffValue, 0.0001f, "진화 전인데 포위 진형 보너스");

        rage.Evolve();
        yield return WaitFrames(3);
        float expected = 1f + (rage.buffValue - 1f) * (1f + CrossBuffSlime.EVOLVE_SURROUND_BONUS);
        Assert.AreEqual(expected, rage.EffectiveBuffValue, 0.0001f, "포위 진형 버프 값");
        Assert.AreEqual(baseAp * expected, hosts[0].statDic[StatType.AttackPower], baseAp * 0.001f, "포위 진형 버프가 주변 슬라임에 적용 안 됨");

        // 한 칸이 비면 포위 해제 → 원래 버프 값
        SlimeFieldController.Instance.RemoveFieldSlime(hosts[3]);
        yield return WaitFrames(3);
        Assert.IsFalse(rage.IsSurrounded());
        Assert.AreEqual(buffed, hosts[0].statDic[StatType.AttackPower], baseAp * 0.001f, "포위가 풀렸는데 보너스가 남음");

        SlimeFieldController.Instance.RemoveFieldSlime(rage);
        for (int i = 0; i < 3; i++)
            SlimeFieldController.Instance.RemoveFieldSlime(hosts[i]);
        yield return WaitFrames(2);
    }

    // ── 시체수집 각성: 엘리트·보스 처치 보너스 ─────────────

    [UnityTest]
    public IEnumerator Evolve_ScavengerEliteBonus()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return ScavengerBody();
        AssertNoErrors();
    }

    IEnumerator ScavengerBody()
    {
        // Awake 없이 쓰는 엘리트 (비활성 부모 아래 생성 → 체력 0 = 처치 상태)
        GameObject holder = new GameObject("EliteHolder");
        holder.SetActive(false);
        Enemy elite = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(ELITE_PREFAB_PATH), holder.transform).GetComponent<Enemy>();
        Assert.IsTrue(elite.IsEliteOrBoss);
        elite.curHp = 0f;
        Assert.LessOrEqual(elite.CurHp, 0f);

        ScavengerSlime normal = (ScavengerSlime)SpawnSlime("Scavenger", GetFreeTile(), MAX);
        ScavengerSlime evolved = (ScavengerSlime)SpawnSlime("Scavenger", GetFreeTile(), MAX);
        evolved.Evolve();
        yield return WaitFrames(2);

        MethodInfo onDead = typeof(ScavengerSlime).GetMethod("OnEnemyDead", BindingFlags.NonPublic | BindingFlags.Instance);
        int coinPerKill = ((ScavengerSlimeData)SlimeManager.Instance.GetSlimeData("Scavenger")).coinPerKill;

        elite.transform.position = normal.transform.position;
        int coin = KingSlime.Instance.coin;
        onDead.Invoke(normal, new object[] { new EnemyDeadEvent(elite) });
        Assert.AreEqual(coin + coinPerKill, KingSlime.Instance.coin, "진화 전 시체수집 엘리트 처치 코인");

        elite.transform.position = evolved.transform.position;
        coin = KingSlime.Instance.coin;
        onDead.Invoke(evolved, new object[] { new EnemyDeadEvent(elite) });
        Assert.AreEqual(coin + coinPerKill + ScavengerSlime.EVOLVE_ELITE_BOSS_BONUS_COIN, KingSlime.Instance.coin, "진화 후 엘리트 보너스 코인");

        Object.Destroy(holder);
        SlimeFieldController.Instance.RemoveFieldSlime(normal);
        SlimeFieldController.Instance.RemoveFieldSlime(evolved);
        yield return WaitFrames(2);
    }
}
