using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 권능 18개를 모두 켜고 실제 전투 (CLAUDE.md "권능"): 에러 없음 + 권능별 효과가 게임에 들어가는지
public class AuthorityPlayInGameTest : InGamePlayTestBase
{
    // 장판·회전·버프·처치 감지·상태이상·강타 슬라임을 섞어 모든 권능 훅이 불리게
    static readonly string[] MINIONS = { "Mine", "Thunder", "Mortar", "Inferno", "Orbit", "Shield", "Mud", "Dopamine", "Berserker", "Scavenger", "Breeze", "Ice", "Flame", "Iron", "Hunter", "Random" };

    static string[] AllAuthorityKeys => AuthorityManager.Instance.authorityDatas.Select(r => r.key).ToArray();

    [UnityTest]
    public IEnumerator AllAuthorities_Battle_NoErrors()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return AllAuthorities_Battle_NoErrorsBody();
    }

    IEnumerator AllAuthorities_Battle_NoErrorsBody()
    {
        current = "권능 시작";
        int blockedBefore = TileManager.Instance.tiles.Count(t => t.isBlocked);
        AuthoritySystem.Begin(AllAuthorityKeys);
        Assert.AreEqual(AllAuthorityKeys.Length, AuthoritySystem.Actives.Count(), "모든 권능 생성");

        // 개척자의 곡괭이: 막힌 칸 2칸 해제
        int blockedAfter = TileManager.Instance.tiles.Count(t => t.isBlocked);
        Assert.AreEqual(Mathf.Max(0, blockedBefore - PioneerPickAuthority.UNBLOCK_COUNT), blockedAfter, "개척자의 곡괭이");

        // 상인의 장부: wave 0 소환 가격 절반 (버튼 표시 대신 계산식)
        Assert.AreEqual(MerchantLedgerAuthority.ApplyDiscount(10, 0), 5);

        // 슬라임 배치 (최대 수 제한 풀기)
        MinionFieldController.Instance.minionMaxCount = 99;
        foreach (string key in MINIONS)
        {
            Tile tile = TileManager.Instance.GetRandomEmptyTile();
            if (tile == null) break;
            current = key;
            SpawnMinion(key, tile, 1);
        }

        // 쌍둥이 반지: 같은 종 둘을 붙이면 둘 다 twinBonus 1
        current = "쌍둥이 반지";
        Tile a = TileManager.Instance.tiles.FirstOrDefault(t => t.IsEmpty && TileManager.Instance.GetTilesAround(t, new[] { Vector2Int.right }).Any(n => n.IsEmpty));
        if (a != null)
        {
            Tile b = TileManager.Instance.GetTilesAround(a, new[] { Vector2Int.right }).First(n => n.IsEmpty);
            Minion s1 = SpawnMinion("Random", a, 0);
            float before = s1.AttackPower;
            Minion s2 = SpawnMinion("Random", b, 0);
            yield return WaitFrames(2);
            Assert.AreEqual(1, s1.twinBonus, "붙은 같은 종 → 보너스");
            Assert.AreEqual(1, s2.twinBonus);
        }

        // 전투 (배속), 킹은 WaitRealtime이 계속 회복
        current = "전투";
        Time.timeScale = 4f;
        for (int i = 0; i < 3; i++)
            yield return SkipWave();
        yield return WaitRealtime(12f);

        // 잔불의 화로: 장판 슬라임이 터뜨린 자리에 불길이 생겼는지
        Assert.Greater(EmberFire.CreatedCount, 0, "잔불의 화로 불길이 한 번도 안 생김");

        // 모래시계: 킹 체력 30% 아래 → 전체 정지
        current = "모래시계";
        float maxHp = KingSlime.Instance.health.MaxHp;
        KingSlime.Instance.TakeDamage(new DamageData { damage = maxHp * 0.8f, attackType = AttackType.AllRounder });
        yield return WaitFrames(2);
        Assert.IsTrue(Enemy.IsFrozenAll, "모래시계가 발동하지 않음");
        KingSlime.Instance.AddHp(100000, false);

        Time.timeScale = 1f;
        AssertNoErrors();
    }
}
