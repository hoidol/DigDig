using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// 방어 타입 적(갑옷·정령)이 실제 게임에서 배율대로 피해를 받는지 (CLAUDE.md "적 방어 타입")
// wave → 프리팹 선택 → 풀 생성 → EnemyDefence 컴포넌트 → 슬라임 탄 데미지 데이터 → 체력 감소까지 실제 경로로 확인
public class DefenceTypePlayTest : InGamePlayTestBase
{
    const float DAMAGE = 1000f;

    [UnityTest]
    public IEnumerator DefenceEnemies_TakeMultipliedDamage()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    IEnumerator Body()
    {
        const int N = GameSetting.DEFENCE_WAVE_INTERVAL;
        int armoredWaveIdx = N * 2 - 1; // 두 번째 방어 wave = 갑옷
        Assert.AreEqual(DefenceType.Ethereal, GameManager.GetWaveDefenceType(N - 1), $"wave {N} = 정령");
        Assert.AreEqual(DefenceType.Armored, GameManager.GetWaveDefenceType(armoredWaveIdx), $"wave {N * 2} = 갑옷");

        Slime physic = SpawnSlime("Iron", GetFreeTile());
        Slime magic = SpawnSlime("Ice", GetFreeTile());
        yield return WaitFrames(2);
        Assert.AreEqual(AttackType.Physic, physic.AttackType, "Iron 공격 속성");
        Assert.AreEqual(AttackType.Magic, magic.AttackType, "Ice 공격 속성");

        foreach (DefenceType defenceType in new[] { DefenceType.Normal, DefenceType.Armored, DefenceType.Ethereal })
        {
            current = defenceType.ToString();
            Enemy prefab = GameManager.Instance.stageData.GetEnemyPrefab(EnemyType.Normal, defenceType);
            Assert.AreEqual(defenceType, prefab.defenceType, $"{defenceType}: 다른 방어 타입 프리팹이 선택됨 ({prefab.name})");

            foreach (Slime attacker in new[] { physic, magic })
            {
                // 풀에서 꺼낸 개체도 같은지 보려고 두 번 생성
                for (int n = 0; n < 2; n++)
                {
                    Enemy enemy = EnemyFieldController.Instance.Instantiate(prefab);
                    enemy.Spawn(EnemyFieldController.Instance.GetSpawnPosition(), armoredWaveIdx);
                    enemy.maxHp = enemy.curHp = DAMAGE * 10f;
                    yield return WaitFrames(1);

                    EnemyDefence defence = enemy.GetComponent<EnemyDefence>();
                    Assert.IsNotNull(defence, $"{defenceType}: EnemyDefence 없음");
                    Assert.AreEqual(1, enemy.GetComponents<EnemyDefence>().Length, $"{defenceType}: EnemyDefence가 여러 개");
                    Assert.AreEqual(defenceType, defence.DefenceType, $"{defenceType}: 붙은 EnemyDefence가 다름 ({defence.GetType().Name})");

                    var data = new AllyUnitDamageData();
                    data.Init(attacker);
                    data.damage = DAMAGE;
                    data.attackType = attacker.AttackType;
                    data.critChance = 0;

                    float before = enemy.curHp;
                    enemy.TakeDamage(data);
                    float dealt = before - enemy.curHp;
                    float expected = DAMAGE * DefenceInfo.Get(attacker.AttackType, defenceType);
                    Debug.Log($"[DefenceTypePlayTest] {defenceType} ← {attacker.key}({attacker.AttackType}) {dealt} (기대 {expected})");
                    Assert.AreEqual(expected, dealt, 0.01f, $"{defenceType} ← {attacker.AttackType}: 피해량");

                    enemy.TakeDamage(KILL);
                    yield return WaitRealtime(1.5f); // Dead 애니메이션 후 풀로 → 두 번째는 풀에서 재사용될 수 있음
                }
            }
        }

        // wave 8에 실제로 스폰되는 적이 갑옷형인지
        current = "armored wave spawn";
        var wave = GameManager.Instance.CurrentWave as BattleWave;
        Assert.IsNotNull(wave, "현재 wave가 BattleWave가 아님");
        int prevIdx = wave.waveIdx;
        DefenceType prevType = wave.waveDefenceType;
        wave.waveIdx = armoredWaveIdx;
        wave.waveDefenceType = GameManager.GetWaveDefenceType(armoredWaveIdx);
        var beforeSpawn = new System.Collections.Generic.HashSet<Enemy>(EnemyFieldController.Instance.activeEnemies);
        wave.Spawn(EnemyType.Normal);
        wave.waveIdx = prevIdx;
        wave.waveDefenceType = prevType;
        Enemy spawned = EnemyFieldController.Instance.activeEnemies.Single(e => !beforeSpawn.Contains(e)); // HashSet이라 순서 없음
        Assert.AreEqual(DefenceType.Armored, spawned.DefenceType, "갑옷 wave에 스폰된 적이 갑옷형이 아님");
        Assert.IsInstanceOf<ArmoredEnemyDefence>(spawned.GetComponent<EnemyDefence>(), "갑옷 wave 적의 EnemyDefence");
    }
}
