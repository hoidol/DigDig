using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// (2026-11-26) 테스트 전용 웨이브 저장·복구 (기기 테스트 패널 MobileTestPanel [웨이브 전환] 탭)
// 브레이크타임이 시작될 때마다(= 그 wave를 준비하기 전, 레벨업·카드 사용 전) 판 상태를 wave별로 저장:
//   킹(칸·레벨·경험치·체력·주사위 코인·성장 능력(용사·정예 칸·스킬 해방·에너지·강화 카드)) · 미니언(칸·종·머지 레벨·진화 갈래·타겟/회전/버프 범위 설정)
//   · 룬(칸·종·레벨) · 막힌 칸 · 미니언 강화 카드 레벨 · 필드 최대 미니언 수
//   카드(오퍼 미니언)는 저장 안 함 → 복구하면 그 wave 브레이크타임으로 가서 카드·주사위 코인을 새로 받고, 저장한 주사위 코인으로 맞춤
// 저장 안 함(복구 안 됨): 성장 룬 누적 공격력 · 미션 진행 · 피해 집계 · 받지 않은 룬 보상 · 박격포 깃발 위치
// BuildSetting.isTestMode일 때만 (MobileTestPanel이 Begin)
public static class WaveSnapshotSystem
{
    class AbilitySnap
    {
        public AbilityData data;
        public int level;
        public Vector2Int[] heroOffsets;      // 용사 정예병 칸
        public int unlockedSkills;            // 용사 스킬 해방 수
        public int charges;                   // 용사 에너지
        public List<Vector2Int> expandOffsets; // 정예 칸 추가 카드 (레벨마다 칸 하나)
    }

    class MinionSnap
    {
        public Vector2Int tile;
        public string key;
        public int mergeLevel;
        public bool evolved;
        public string evolutionKey;
        public FindTargetType findTarget;
        public RotationDirection? rotation;
        public BuffRangeShape? buffShape;
    }

    class RuneSnap
    {
        public Vector2Int tile;
        public string key;
        public int level;
    }

    class Snapshot
    {
        public int nextWaveIdx;
        public Vector2Int? kingTile;
        public int lv, exp, diceCoin, minionMaxCount;
        public float curHp;
        public List<AbilitySnap> abilities = new();
        public List<MinionSnap> minions = new();
        public List<RuneSnap> runes = new();
        public List<Vector2Int> blocked = new();
        public List<(string key, StatType stat, int lv)> enhances = new();
    }

    static readonly Dictionary<int, Snapshot> snapshots = new();
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

    #region 저장
    static void Capture(int nextWaveIdx)
    {
        KingSlime king = KingSlime.Instance;
        TileManager tm = TileManager.Instance;
        if (king == null || tm == null)
            return;
        var s = new Snapshot
        {
            nextWaveIdx = nextWaveIdx,
            lv = king.lv,
            exp = king.exp,
            diceCoin = king.diceCoin,
            curHp = king.health != null ? king.health.curHp : 0f,
            minionMaxCount = MinionFieldController.Instance != null ? MinionFieldController.Instance.minionMaxCount : 0,
        };
        Tile kingTile = tm.GetTile(king);
        if (kingTile != null)
            s.kingTile = kingTile.index;

        foreach (KingAbility a in king.abilityInventory.Abilities)
        {
            if (a == null || a.data == null)
                continue;
            var snap = new AbilitySnap { data = a.data, level = a.level };
            if (a is HeroKingAbility hero)
            {
                snap.heroOffsets = hero.eliteOffsets != null ? (Vector2Int[])hero.eliteOffsets.Clone() : null;
                snap.unlockedSkills = hero.UnlockedSkillCount;
                snap.charges = hero.Charges;
            }
            if (a is EliteAreaExpandKingAbility expand)
                snap.expandOffsets = new List<Vector2Int>(expand.Offsets);
            s.abilities.Add(snap);
        }

        foreach (Tile t in tm.tiles)
        {
            if (t == null)
                continue;
            if (t.isBlocked)
                s.blocked.Add(t.index);
            if (t.tileObject is Minion m)
            {
                s.minions.Add(new MinionSnap
                {
                    tile = t.index,
                    key = m.key,
                    mergeLevel = m.mergeLevel,
                    evolved = m.isEvolved,
                    evolutionKey = m.evolution != null ? m.evolution.key : null,
                    findTarget = m.findTargetType,
                    rotation = m is IRotationDirectionUnit r ? r.RotationDirection : null,
                    buffShape = m is IBuffRangeUnit b ? b.BuffRangeShape : null,
                });
            }
            if (t.tileRune is TileRune rune)
                s.runes.Add(new RuneSnap { tile = t.index, key = rune.key, level = rune.upgradeLevel });
        }
        s.enhances = MinionEnhanceSystem.GetAllLevels();
        snapshots[nextWaveIdx] = s;
        Debug.Log($"[WaveSnapshot] Wave {nextWaveIdx + 1} 저장: 미니언 {s.minions.Count} · 룬 {s.runes.Count} · 킹 Lv.{s.lv}");
    }
    #endregion

    #region 복구
    // 저장해 둔 그 wave 브레이크타임으로 되돌림. 저장이 없으면 false
    public static bool Restore(int nextWaveIdx)
    {
        if (!snapshots.TryGetValue(nextWaveIdx, out Snapshot s))
            return false;
        GameManager gm = GameManager.Instance;
        KingSlime king = KingSlime.Instance;
        TileManager tm = TileManager.Instance;
        if (gm == null || king == null || tm == null)
            return false;

        restoring = true;
        try
        {
            CloseOpenWindows(king);

            // 1) wave·적 정리 + 판 비우기
            GameTestUtil.ClearFieldMinions();
            foreach (TileRune r in king.runeInventory.curRunes.OfType<TileRune>().ToList())
                king.RemoveRune(r);
            foreach (Tile t in tm.tiles)
            {
                if (t == null)
                    continue;
                bool block = s.blocked.Contains(t.index);
                if (block && !t.isBlocked && t.IsEmpty)
                    t.Block();
                else if (!block && t.isBlocked)
                    t.Unblock();
            }

            // 2) 킹 칸
            if (s.kingTile.HasValue)
            {
                Tile kt = tm.GetTile(s.kingTile.Value);
                Tile cur = tm.GetTile(king);
                if (kt != null && kt != cur && kt.IsEmpty)
                {
                    tm.RemoveTileObject(king);
                    kt.SetTileObject(king);
                }
            }

            // 3) 킹 레벨·성장 능력 (용사가 레벨을 보므로 레벨 먼저)
            KingAbilityInventory inv = king.abilityInventory;
            foreach (KingAbility a in inv.Abilities.ToList())
                inv.Remove(a.data);
            king.lv = s.lv;
            king.exp = s.exp;
            king.pendingLevelUps = 0;
            foreach (AbilitySnap a in s.abilities)
                RestoreAbility(inv, a);
            inv.Get<HeroKingAbility>()?.RefreshHeroLevel();

            // 4) 미니언 강화 카드 → 미니언 (UpdateMinion이 강화 레벨을 읽음)
            MinionEnhanceSystem.RestoreLevels(s.enhances);
            foreach (MinionSnap m in s.minions)
                RestoreMinion(tm, m);

            // 5) 룬
            foreach (RuneSnap r in s.runes)
            {
                Tile t = tm.GetTile(r.tile);
                if (t == null || t.tileRune != null)
                    continue;
                king.AddRune(r.key, t).Forget();
                if (t.tileRune is TileRune installed)
                    while (installed.upgradeLevel < r.level && installed.ForceUpgrade()) { }
            }

            // 6) 그 wave 브레이크타임으로 이동 → 카드·주사위 코인 새로 지급 후 저장한 코인으로 맞춤
            king.bonusDiceCoinNextBreak = 0;
            king.pendingRuneRewards = 0;
            MinionOfferSystem.Instance?.ResetDealForTest();
            gm.GoToBreakTimeForTest(nextWaveIdx);
            king.AddDiceCoin(s.diceCoin - king.diceCoin);
            if (MinionFieldController.Instance != null)
                MinionFieldController.Instance.minionMaxCount = s.minionMaxCount;
            if (king.health != null)
                king.health.curHp = Mathf.Min(s.curHp, king.MaxHp);
            king.UpdateCharacter();
            GameEventBus.Publish(new ExpChangedEvent(king.exp, king.MaxExp, king.lv));
            KingSlimeManageCanvas.Instance?.UpdateCanvas();
        }
        finally
        {
            restoring = false;
        }
        Debug.Log($"[WaveSnapshot] Wave {nextWaveIdx + 1} 복구");
        return true;
    }

    static void RestoreAbility(KingAbilityInventory inv, AbilitySnap a)
    {
        if (a.data == null || a.data.prefab == null)
            return;
        if (a.expandOffsets != null && a.expandOffsets.Count > 0)
        {
            // 정예 칸 추가: 레벨마다 칸 하나 (추가 전에 extraOffset 지정)
            for (int i = 0; i < a.expandOffsets.Count; i++)
            {
                Vector2Int off = a.expandOffsets[i];
                if (i == 0)
                    inv.Add(a.data, ab => { if (ab is EliteAreaExpandKingAbility e) e.extraOffset = off; });
                else
                {
                    if (inv.Get(a.data) is EliteAreaExpandKingAbility e)
                        e.extraOffset = off;
                    inv.Add(a.data);
                }
            }
            return;
        }
        inv.Add(a.data, ab =>
        {
            if (ab is HeroKingAbility h && a.heroOffsets != null)
                h.eliteOffsets = (Vector2Int[])a.heroOffsets.Clone();
        });
        for (int lv = 1; lv < a.level; lv++)
            inv.Add(a.data);
        if (inv.Get(a.data) is HeroKingAbility hero)
        {
            for (int i = 0; i < a.unlockedSkills; i++)
                hero.UnlockNextSkill();
            hero.SetChargesForTest(a.charges);
        }
    }

    static void RestoreMinion(TileManager tm, MinionSnap m)
    {
        Tile t = tm.GetTile(m.tile);
        if (t == null || !t.IsEmpty || MinionManager.Instance == null || !MinionManager.Instance.minionDataDic.ContainsKey(m.key))
            return;
        Minion minion = KingSlime.Instance.AddMinion(m.key, m.mergeLevel);
        t.SetTileObject(minion);
        if (m.evolved && minion.CanEvolve)
        {
            var branches = minion.minionData.GetEvolutions();
            MinionEvolutionData branch = branches.FirstOrDefault(b => b != null && b.key == m.evolutionKey);
            minion.Evolve(branch != null ? branch : (branches.Count > 0 ? branches[0] : null));
        }
        minion.SetFindTargetType(m.findTarget);
        if (m.rotation.HasValue && minion is IRotationDirectionUnit r)
            r.SetRotationDirection(m.rotation.Value);
        if (m.buffShape.HasValue && minion is IBuffRangeUnit b)
            b.SetBuffRangeShape(m.buffShape.Value);
    }

    // 열려 있는 레벨업·룬 선택 창 닫기 (밀린 레벨업은 버림)
    static void CloseOpenWindows(KingSlime king)
    {
        king.pendingLevelUps = 0;
        if (InGame.LevelUpCanvas.Instance != null && InGame.LevelUpCanvas.Instance.gameObject.activeSelf)
            InGame.LevelUpCanvas.Instance.gameObject.SetActive(false);
        if (SelectRuneCanvas.Instance != null && SelectRuneCanvas.Instance.gameObject.activeSelf)
            SelectRuneCanvas.Instance.gameObject.SetActive(false);
    }
    #endregion
}
