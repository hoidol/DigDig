using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 묶음 5 (Lv.25) 위기 대응

// 사냥꾼의 부적: 엘리트·보스가 있으면 강타(Heavy) 슬라임이 먼저 노림 (Slime.FindTargets),
// 엘리트를 처치하면 타일 아이템 무료 뽑기 (성공 100%)
public class HunterCharmAuthority : Authority
{
    public const int MAX_CANDIDATES = 64;

    public static bool IsHeavy(SlimeData data) => data != null && SlimeTags.Has(data.tags, SlimeTag.Heavy);

    // 기존 우선순위를 유지한 채 엘리트·보스를 앞으로 (OrderBy는 안정 정렬)
    public static List<Transform> PreferEliteBoss(List<Transform> candidates, int count)
    {
        return candidates.OrderBy(t => t.TryGetComponent(out Enemy e) && e.IsEliteOrBoss ? 0 : 1).Take(count).ToList();
    }

    public override void OnBegin() => GameEventBus.Subscribe<EnemyDeadEvent>(OnEnemyDead);
    public override void OnEnd() => GameEventBus.Unsubscribe<EnemyDeadEvent>(OnEnemyDead);

    void OnEnemyDead(EnemyDeadEvent e)
    {
        // 경로 끝 도착(체력 남음)은 처치 아님
        if (!(e.enemy is EliteEnemy) || e.enemy.CurHp > 0)
            return;
        if (SelectItemCanvas.Instance == null || SelectItemCanvas.Instance.gameObject.activeSelf)
            return; // 이미 뽑기 중이면 생략
        if (ItemManager.Instance.GetDrawTileItems(1).Count == 0 || TileManager.Instance.GetRandomTileForItem() == null)
            return;
        ToastCanvas.Toast(TranslateManager.GetText("Authority_HunterCharmDraw"));
        SelectItemCanvas.Instance.OpenDraw(100);
    }
}

// 혼혈의 문장: 장착 슬라임에 물리·마법이 섞여 있으면, 방어 타입 wave 시작 시 약점 속성 슬라임 1마리 무료 소환
public class MixedCrestAuthority : Authority
{
    public static bool IsMixed(IEnumerable<AttackType> types)
    {
        var list = types.ToList();
        return list.Contains(AttackType.Physic) && list.Contains(AttackType.Magic);
    }

    // 이 방어 타입에 약점(배율 > 1)인 공격 속성의 장착 슬라임 key
    public static List<string> GetWeakKeys(IEnumerable<SlimeData> equipped, DefenceType defenceType)
    {
        return equipped.Where(d => d.attackType != AttackType.AllRounder && DefenceInfo.Get(d.attackType, defenceType) > 1f)
            .Select(d => d.key).ToList();
    }

    public override void OnBegin() => GameEventBus.Subscribe<WaveStartEvent>(OnWaveStart);
    public override void OnEnd() => GameEventBus.Unsubscribe<WaveStartEvent>(OnWaveStart);

    void OnWaveStart(WaveStartEvent e)
    {
        var equipped = AuthoritySystem.EquippedSlimeDatas();
        if (!IsMixed(equipped.Select(d => d.attackType)))
            return;
        DefenceType defenceType = GameManager.GetWaveDefenceType(e.waveIdx);
        if (defenceType == DefenceType.Normal)
            return;
        List<string> keys = GetWeakKeys(equipped, defenceType);
        if (keys.Count == 0)
            return;
        if (AuthoritySystem.SpawnFreeSlime(keys[Random.Range(0, keys.Count)]) != null)
            ToastCanvas.Toast(TranslateManager.GetText("Authority_MixedCrestSpawn"));
    }
}

// 모래시계: 킹 체력이 처음으로 30% 아래가 되면 5초간 모든 적 정지 (한 판에 1번, Enemy.Update)
public class HourglassAuthority : Authority
{
    public const float HP_RATIO = 0.3f;
    public const float FREEZE_TIME = 5f;
    bool used;

    public static bool ShouldTrigger(bool used, float curHp, float maxHp) => !used && curHp > 0 && maxHp > 0 && curHp < maxHp * HP_RATIO;

    public override void OnBegin() => GameEventBus.Subscribe<KingSlimeHpChangedEvent>(OnHpChanged);
    public override void OnEnd() => GameEventBus.Unsubscribe<KingSlimeHpChangedEvent>(OnHpChanged);

    void OnHpChanged(KingSlimeHpChangedEvent e)
    {
        if (!ShouldTrigger(used, e.curHp, e.maxHp))
            return;
        used = true;
        Enemy.FreezeAll(FREEZE_TIME);
        ToastCanvas.Toast(TranslateManager.GetText("Authority_HourglassFreeze"));
    }
}
