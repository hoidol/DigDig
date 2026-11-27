using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// (2026-11-26) 테스트 전용 웨이브 저장·복구 (기기 테스트 패널 MobileTestPanel [웨이브 전환] 탭)
// 브레이크타임이 시작될 때마다(= 그 wave를 준비하기 전, 레벨업·카드 사용 전) 판 상태를 wave별로 저장:
//   슬라임(칸·레벨·경험치·체력·주사위 코인·성장 능력(용사·정예 칸·스킬 해방·에너지·강화 카드)) · 미니언(칸·종·머지 레벨·진화 갈래·타겟/회전/버프 범위 설정)
//   · 룬(칸·종·레벨) · 막힌 칸 · 미니언 강화 카드 레벨 · 필드 최대 미니언 수
//   카드(오퍼 미니언)는 저장 안 함 → 복구하면 그 wave 브레이크타임으로 가서 카드·주사위 코인을 새로 받고, 저장한 주사위 코인으로 맞춤
// 저장 안 함(복구 안 됨): 성장 룬 누적 공격력 · 미션 진행 · 피해 집계 · 받지 않은 룬 보상 · 박격포 깃발 위치
// BuildSetting.isTestMode일 때만 (MobileTestPanel이 Begin)
public static class WaveSnapshotSystem
{
    // (2026-11-27) 저장 내용·복구는 GameSnapshot (로컬 판 이어하기와 공용)
    static readonly Dictionary<int, GameSnapshot> snapshots = new();
    static MonoBehaviour runner;
    static bool restoring;
    static bool subscribed;

    public static IEnumerable<int> SavedWaves => snapshots.Keys.OrderBy(k => k);
    public static bool Has(int nextWaveIdx) => snapshots.ContainsKey(nextWaveIdx);

    // MobileTestPanel.Start에서 호출 (씬마다 새로)
    public static void Begin(MonoBehaviour owner)
    {
        snapshots.Clear();
        runner = owner;
        restoring = false;
        GameEventBus.Unsubscribe<BreakTimeStartEvent>(OnBreakTimeStart);
        GameEventBus.Subscribe<BreakTimeStartEvent>(OnBreakTimeStart);
        subscribed = true;
        // 게임 시작 브레이크타임(wave 1)이 구독보다 먼저 왔으면 지금 저장
        GameManager gm = GameManager.Instance;
        if (gm != null && gm.isBreakTime)
            runner.StartCoroutine(CaptureNextFrame(gm.breakTimeNextWaveIdx));
    }

    public static void End()
    {
        if (subscribed)
            GameEventBus.Unsubscribe<BreakTimeStartEvent>(OnBreakTimeStart);
        subscribed = false;
        snapshots.Clear();
        runner = null;
    }

    static void OnBreakTimeStart(BreakTimeStartEvent e)
    {
        if (restoring || runner == null)
            return;
        // 한 프레임 뒤에 저장: 카드 지급·주사위 코인 초기화(MinionOfferSystem)가 끝난 상태
        runner.StartCoroutine(CaptureNextFrame(e.nextWaveIdx));
    }

    static IEnumerator CaptureNextFrame(int nextWaveIdx)
    {
        yield return null;
        if (restoring)
            yield break;
        Capture(nextWaveIdx);
    }

    static void Capture(int nextWaveIdx)
    {
        GameSnapshot snap = GameSnapshot.Capture(nextWaveIdx);
        if (snap == null)
            return;
        snapshots[nextWaveIdx] = snap;
        Debug.Log($"[WaveSnapshot] Wave {nextWaveIdx + 1} 저장: 미니언 {snap.minions.Count} · 룬 {snap.runes.Count} · 슬라임 Lv.{snap.lv}");
    }

    // 저장해 둔 그 wave 브레이크타임으로 되돌림. 저장이 없으면 false
    public static bool Restore(int nextWaveIdx)
    {
        if (!snapshots.TryGetValue(nextWaveIdx, out GameSnapshot s))
            return false;
        restoring = true;
        bool ok;
        try { ok = s.Apply(); }
        finally { restoring = false; }
        if (ok)
            Debug.Log($"[WaveSnapshot] Wave {nextWaveIdx + 1} 복구");
        return ok;
    }
}
