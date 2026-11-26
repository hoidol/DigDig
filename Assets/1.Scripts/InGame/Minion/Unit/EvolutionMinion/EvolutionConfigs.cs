using System;
using UnityEngine;

// (2026-11-26) 5성 진화 수치. 종마다 Config 하나 = MinionEvolutionSetting 에셋(Resources/MinionEvolutionSetting) 인스펙터 한 칸
// - 수치를 바꾸려면 에셋 인스펙터에서 (실행 중에도 바로 반영: 동작 클래스가 매번 Config를 읽음)
// - 새 종 진화 = ① 여기 Config 클래스(수치 + BehaviourType + GetDescArgs) ② 동작 클래스(MinionEvolution 상속, Behaviours/) ③ MinionEvolutionSetting에 칸 추가
// - 이름·설명 = MinionData.csv EvolutionName / EvolutionDesc ({0}{1}… = GetDescArgs 순서, 확률은 % 숫자로 넘김)
// - 확률은 0~1 (0.05 = 5%), 시간은 초, 피해 배율은 미니언 공격력 기준 (1 = 100%)
[Serializable]
public abstract class EvolutionConfig
{
    [Tooltip("진화하면 공격력 배율 (1 = 그대로)")] public float attackPowerMul = 1f;
    [Tooltip("진화하면 공속 배율 (1 = 그대로)")] public float attackSpeedMul = 1f;

    public abstract Type BehaviourType { get; }
    public virtual object[] GetDescArgs() => Array.Empty<object>();

    protected static object Pct(float chance01) => Mathf.RoundToInt(chance01 * 100f);
    protected static object Num(float v) => v.ToString("0.##");
}

// 불똥이 → 마그미: 탄이 맞은 자리에 마그마 스플래시 (주변 피해 + 화상) + 확률 스턴
[Serializable]
public class FlameEvolutionConfig : EvolutionConfig
{
    [Tooltip("스플래시 범위")] public float splashRadius = 1.1f;
    [Tooltip("스플래시 피해 (공격력 배율, 맞은 적 주변 적에게)")] public float splashDamageMul = 0.6f;
    [Tooltip("스플래시에 맞은 적도 화상")] public bool splashBurn = true;
    [Tooltip("스턴 확률 0~1 (맞은 적·스플래시 적마다)")] public float stunChance = 0.05f;
    [Tooltip("스턴 시간(초)")] public float stunDuration = 2f;
    public override Type BehaviourType => typeof(FlameEvolution);
    public override object[] GetDescArgs() => new[] { Num(splashRadius), Pct(splashDamageMul), Pct(stunChance), Num(stunDuration) };
}

// 뭉치: 발동 스킬(머리 위 버튼, 2026-11-26) — duration초 동안 공속 × skillAttackSpeedMul + 공격마다 부채꼴 fanCount발, 탄마다 피해 × fanDamageMul
[Serializable]
public class RandomEvolutionConfig : EvolutionConfig
{
    [Tooltip("쿨타임(초)")] public float cooldown = 10f;
    [Tooltip("발동 시간(초)")] public float duration = 3f;
    [Tooltip("발동 중 공속 배율 (빠르게 속사)")] public float skillAttackSpeedMul = 2f; // 부모 attackSpeedMul(진화 상시 배율)과 이름이 겹치면 직렬화 에러
    [Tooltip("발동 중 한 번에 쏘는 방향 수")] public int fanCount = 5;
    [Tooltip("방향 사이 각도(도)")] public float fanAngle = 14f;
    [Tooltip("발동 중 탄 피해 배율")] public float fanDamageMul = 0.5f;
    public override Type BehaviourType => typeof(RandomEvolution);
    public override object[] GetDescArgs() => new[] { Num(cooldown), Num(duration), (object)fanCount, Pct(fanDamageMul) };
}

// 눈송이: 발동 스킬 — 큰 눈이 내 칸 묶음 주변 경로를 돌며 맞은 적 100% 빙결 + 확률 스턴
[Serializable]
public class IceEvolutionConfig : EvolutionConfig
{
    [Tooltip("쿨타임(초)")] public float cooldown = 10f;
    [Tooltip("눈 유지 시간(초)")] public float hailDuration = 8f;
    [Tooltip("눈 개수")] public int hailCount = 1;
    [Tooltip("눈 피해 (공격력 배율)")] public float hailDamageMul = 1.5f;
    [Tooltip("눈 이동 속도 (초당 거리)")] public float hailSpeed = 3f;
    [Tooltip("같은 적을 다시 때리는 간격(초)")] public float hitCooldown = 0.5f;
    [Tooltip("빙결 확률 0~1")] public float iceChance = 1f;
    [Tooltip("빙결 시간(초)")] public float iceDuration = 1.5f;
    [Tooltip("스턴 확률 0~1")] public float stunChance = 0.05f;
    [Tooltip("스턴 시간(초)")] public float stunDuration = 2f;
    [Tooltip("눈 프리팹 (HailStone)")] public HailStone hailPrefab;
    public override Type BehaviourType => typeof(IceEvolution);
    public override object[] GetDescArgs() => new[] { Num(cooldown), Num(hailDuration), Pct(iceChance), Pct(stunChance), Num(stunDuration) };
}

// 찌릿이: 필드의 다른 찌릿이와 번개 선으로 연결 → 선에 닿은 적 피해 + 감속
[Serializable]
public class LightningEvolutionConfig : EvolutionConfig
{
    [Tooltip("선 피해 간격(초)")] public float tickInterval = 0.25f;
    [Tooltip("선 피해 (공격력 배율, 간격마다)")] public float lineDamageMul = 0.4f;
    [Tooltip("선 판정 두께")] public float lineWidth = 0.35f;
    [Tooltip("감속 시간(초)")] public float slowDuration = 1f;
    [Tooltip("선 색")] public Color lineColor = new Color(1f, 0.95f, 0.35f, 0.9f);
    public override Type BehaviourType => typeof(LightningEvolution);
    public override object[] GetDescArgs() => new[] { Pct(lineDamageMul), Num(tickInterval), Num(slowDuration) };
}

// 빙빙이: 발동 스킬 — 일정 시간 회전체가 스플래시 피해 + 감속
[Serializable]
public class OrbitEvolutionConfig : EvolutionConfig
{
    [Tooltip("쿨타임(초)")] public float cooldown = 10f;
    [Tooltip("발동 시간(초)")] public float duration = 5f;
    [Tooltip("스플래시 범위")] public float splashRadius = 0.9f;
    [Tooltip("스플래시 피해 (회전체 피해 배율)")] public float splashDamageMul = 0.6f;
    [Tooltip("감속 시간(초)")] public float slowDuration = 1f;
    public override Type BehaviourType => typeof(OrbitEvolution);
    public override object[] GetDescArgs() => new[] { Num(cooldown), Num(duration), Pct(splashDamageMul) };
}

// 콩콩이: 맞으면 확률 스턴 + 발동 스킬(머리 위 버튼, 2026-11-26) = buffDuration초 공속 버프
[Serializable]
public class BounceEvolutionConfig : EvolutionConfig
{
    [Tooltip("스턴 확률 0~1")] public float stunChance = 0.05f;
    [Tooltip("스턴 시간(초)")] public float stunDuration = 1f;
    [Tooltip("공속 버프 쿨타임(초, 머리 위 버튼)")] public float buffCooldown = 7f;
    [Tooltip("공속 버프 시간(초)")] public float buffDuration = 4f;
    [Tooltip("공속 버프 배율")] public float buffAttackSpeedMul = 2f;
    public override Type BehaviourType => typeof(BounceEvolution);
    public override object[] GetDescArgs() => new[] { Pct(stunChance), Num(buffCooldown), Num(buffDuration), Num(buffAttackSpeedMul) };
}

// 철컹이: 엘리트·보스 피해 배율 상승 + 보스를 때리면 주변 적에게 스플래시
[Serializable]
public class IronEvolutionConfig : EvolutionConfig
{
    [Tooltip("엘리트·보스 피해 배율 (기본 철컹이 배율 대신)")] public float eliteBossDamageMul = 4f;
    [Tooltip("보스를 때릴 때 주변 적 스플래시 = 보스에게 준 피해 × 이 비율")] public float bossSplashRatio = 0.3f;
    [Tooltip("보스 스플래시 범위")] public float bossSplashRadius = 1.5f;
    [Tooltip("엘리트를 때려도 스플래시")] public bool splashOnElite = false;
    public override Type BehaviourType => typeof(IronEvolution);
    public override object[] GetDescArgs() => new[] { Num(eliteBossDamageMul), Pct(bossSplashRatio) };
}

// 뾰족이: 탄이 화면 끝에서 반사(관통 횟수 1 소모) + 발동 스킬(머리 위 버튼, 2026-11-26) = 여러 방향 관통탄
[Serializable]
public class PierceEvolutionConfig : EvolutionConfig
{
    [Tooltip("탄 하나가 화면 끝에서 반사되는 최대 횟수")] public int maxScreenBounce = 3;
    [Tooltip("반사할 때 관통 횟수 소모")] public bool bounceConsumesPierce = true;
    [Tooltip("관통탄 스킬 쿨타임(초, 머리 위 버튼)")] public float skillCooldown = 10f;
    [Tooltip("관통탄 스킬 방향 수 (원형)")] public int skillBulletCount = 5;
    public override Type BehaviourType => typeof(PierceEvolution);
    public override object[] GetDescArgs() => new[] { Num(skillCooldown), (object)skillBulletCount };
}

// 늑돌이: 발동 스킬 — 일정 시간 공격 범위 안 적 전부를 간격마다 할큄 (강한 피해 + 감속)
[Serializable]
public class BerserkerEvolutionConfig : EvolutionConfig
{
    [Tooltip("쿨타임(초)")] public float cooldown = 10f;
    [Tooltip("발동 시간(초)")] public float duration = 4f;
    [Tooltip("할퀴는 간격(초)")] public float interval = 0.5f;
    [Tooltip("할퀴기 피해 (공격력 배율)")] public float damageMul = 2f;
    [Tooltip("감속 시간(초)")] public float slowDuration = 1f;
    [Tooltip("할퀴기 색")] public Color clawColor = new Color(1f, 0.55f, 0.45f, 1f);
    public override Type BehaviourType => typeof(BerserkerEvolution);
    public override object[] GetDescArgs() => new[] { Num(cooldown), Num(duration), Num(interval), Pct(damageMul) };
}

// 달그락: 발동 스킬(머리 위 버튼, 2026-11-26) = 깃발에서 적이 오는 쪽(경로 반대)으로 폭탄 여러 발 (확률 스턴)
[Serializable]
public class MortarEvolutionConfig : EvolutionConfig
{
    [Tooltip("쿨타임(초, 머리 위 버튼)")] public float cooldown = 15f;
    [Tooltip("폭탄 수")] public int bombCount = 3;
    [Tooltip("폭탄 간격 (폭발 범위 배율)")] public float spacingMul = 1.2f;
    [Tooltip("폭탄 사이 시간(초)")] public float dropInterval = 0.2f;
    [Tooltip("스턴 확률 0~1")] public float stunChance = 0.5f;
    [Tooltip("스턴 시간(초)")] public float stunDuration = 1f;
    public override Type BehaviourType => typeof(MortarEvolution);
    public override object[] GetDescArgs() => new[] { Num(cooldown), (object)bombCount, Pct(stunChance) };
}

// 둥둥이 (2026-11-26): 발동 스킬(머리 위 버튼) — duration초 동안 구름 cloudCount개가 몸 주변을 돌며 strikeInterval초마다 구름 아래로 번개
// 번개 = 구름 자리 반경 strikeRadius 안 적 전부 공격력 × damageMul + stunDuration초 스턴. 구름 모양 = cloudPrefab (에디터에서 그림 교체)
[Serializable]
public class ThunderEvolutionConfig : EvolutionConfig
{
    [Tooltip("쿨타임(초)")] public float cooldown = 10f;
    [Tooltip("발동 시간(초)")] public float duration = 5f;
    [Tooltip("구름 수")] public int cloudCount = 3;
    [Tooltip("구름이 도는 반경 (미니언 중심)")] public float orbitRadius = 1.3f;
    [Tooltip("구름 회전 속도 (도/초)")] public float orbitSpeed = 120f;
    [Tooltip("구름마다 번개 간격(초)")] public float strikeInterval = 1f;
    [Tooltip("번개 피해 (공격력 배율)")] public float damageMul = 2f;
    [Tooltip("번개 범위 (구름 자리 반경)")] public float strikeRadius = 0.7f;
    [Tooltip("스턴 시간(초)")] public float stunDuration = 1f;
    [Tooltip("구름이 땅(번개 떨어지는 자리)보다 위로 떠 있는 높이")] public float cloudHeight = 0.45f;
    [Tooltip("구름 프리팹 (3.Prefabs/Minion/Evolution/ThunderCloud)")] public UnityEngine.GameObject cloudPrefab;
    public override Type BehaviourType => typeof(ThunderEvolution);
    public override object[] GetDescArgs() => new[] { Num(cooldown), Num(duration), (object)cloudCount, Num(strikeInterval), Pct(damageMul), Num(stunDuration) };
}
