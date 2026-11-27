using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

// (2026-11-27) 판 상태 저장·복구 (브레이크타임 시작 = 그 wave를 준비하기 전, 레벨업·카드 사용 전)
// 쓰는 곳: 로컬 판 이어하기(LocalGameSave, 멀티 대기 중 하던 싱글 판으로 돌아오기) · 기기 테스트 패널 웨이브 전환(WaveSnapshotSystem)
// 저장: 킹(칸·레벨·경험치·체력·주사위 코인·성장 능력(용사·정예 칸·스킬 해방·에너지·강화 카드)) · 미니언(칸·종·머지 레벨·진화 갈래·타겟/회전/버프 범위)
//   · 룬(칸·종·레벨) · 막힌 칸 · 미니언 강화 카드 레벨 · 필드 최대 미니언 수
// 카드(오퍼)는 저장 안 함 → 복구하면 그 wave 브레이크타임으로 가서 카드를 새로 받고, 주사위 코인은 저장값으로 맞춤
// 저장 안 함: 성장 룬 누적 공격력 · 미션 진행 · 피해 집계 · 받지 않은 룬 보상 · 박격포 깃발 위치
// JsonUtility로 파일 저장 가능 (능력 = AbilityData key, nullable 대신 has 플래그)
[System.Serializable]
public class GameSnapshot
{
    [System.Serializable]
    public class AbilitySnap
    {
        public string dataKey;
        public int level;
        public List<Vector2Int> heroOffsets = new List<Vector2Int>(); // 용사 정예 칸
        public int unlockedSkills;                                    // 용사 스킬 해방 수
        public int charges;                                           // 용사 에너지
        public List<Vector2Int> expandOffsets = new List<Vector2Int>(); // 정예 칸 추가 카드 (레벨마다 칸 하나)
    }

    [System.Serializable]
    public class MinionSnap
    {
        public Vector2Int tile;
        public string key;
        public int mergeLevel;
        public bool evolved;
        public string evolutionKey;
        public FindTargetType findTarget;
        public bool hasRotation;
        public RotationDirection rotation;
        public bool hasBuffShape;
        public BuffRangeShape buffShape;
    }

    [System.Serializable]
    public class RuneSnap
    {
        public Vector2Int tile;
        public string key;
        public int level;
    }

    [System.Serializable]
    public class EnhanceSnap
    {
        public string key;
        public StatType stat;
        public int lv;
    }

    public int nextWaveIdx;
    public bool hasKingTile;
    public Vector2Int kingTile;
    public int lv, exp, diceCoin, minionMaxCount;
    public float curHp;
    public List<AbilitySnap> abilities = new List<AbilitySnap>();
    public List<MinionSnap> minions = new List<MinionSnap>();
    public List<RuneSnap> runes = new List<RuneSnap>();
    public List<Vector2Int> blocked = new List<Vector2Int>();
    public List<EnhanceSnap> enhances = new List<EnhanceSnap>();

    #region 저장
    public static GameSnapshot Capture(int nextWaveIdx)
    {
        KingSlime king = KingSlime.Instance;
        TileManager tm = TileManager.Instance;
        if (king == null || tm == null)
            return null;
        var s = new GameSnapshot
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
        {
            s.hasKingTile = true;
            s.kingTile = kingTile.index;
        }

        foreach (KingAbility a in king.abilityInventory.Abilities)
        {
            if (a == null || a.data == null || string.IsNullOrEmpty(a.data.key))
                continue;
            var snap = new AbilitySnap { dataKey = a.data.key, level = a.level };
            if (a is HeroKingAbility hero)
            {
                if (hero.eliteOffsets != null)
                    snap.heroOffsets = new List<Vector2Int>(hero.eliteOffsets);
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
                var ms = new MinionSnap
                {
                    tile = t.index,
                    key = m.key,
                    mergeLevel = m.mergeLevel,
                    evolved = m.isEvolved,
                    evolutionKey = m.evolution != null ? m.evolution.key : null,
                    findTarget = m.findTargetType,
                };
                if (m is IRotationDirectionUnit r)
                {
                    ms.hasRotation = true;
                    ms.rotation = r.RotationDirection;
                }
                if (m is IBuffRangeUnit b)
                {
                    ms.hasBuffShape = true;
                    ms.buffShape = b.BuffRangeShape;
                }
                s.minions.Add(ms);
            }
            if (t.tileRune is TileRune rune)
                s.runes.Add(new RuneSnap { tile = t.index, key = rune.key, level = rune.upgradeLevel });
        }
        foreach (var e in MinionEnhanceSystem.GetAllLevels())
            s.enhances.Add(new EnhanceSnap { key = e.key, stat = e.stat, lv = e.lv });
        return s;
    }
    #endregion

    #region 복구
    public static bool IsRestoring { get; private set; }

    // 저장한 그 wave 브레이크타임으로 되돌림 (게임이 시작된 상태여야 함)
    public bool Apply()
    {
        GameManager gm = GameManager.Instance;
        KingSlime king = KingSlime.Instance;
        TileManager tm = TileManager.Instance;
        if (gm == null || king == null || tm == null)
            return false;

        IsRestoring = true;
        try
        {
            CloseOpenWindows(king);

            // 1) 판 비우기
            GameTestUtil.ClearFieldMinions();
            foreach (TileRune r in king.runeInventory.curRunes.OfType<TileRune>().ToList())
                king.RemoveRune(r);
            foreach (Tile t in tm.tiles)
            {
                if (t == null)
                    continue;
                bool block = blocked.Contains(t.index);
                if (block && !t.isBlocked && t.IsEmpty)
                    t.Block();
                else if (!block && t.isBlocked)
                    t.Unblock();
            }

            // 2) 킹 칸
            if (hasKingTile)
            {
                Tile kt = tm.GetTile(kingTile);
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
            king.lv = lv;
            king.exp = exp;
            king.pendingLevelUps = 0;
            foreach (AbilitySnap a in abilities)
                RestoreAbility(inv, a);
            inv.Get<HeroKingAbility>()?.RefreshHeroLevel();

            // 4) 미니언 강화 카드 → 미니언 (UpdateMinion이 강화 레벨을 읽음)
            MinionEnhanceSystem.RestoreLevels(enhances.Select(e => (e.key, e.stat, e.lv)));
            foreach (MinionSnap m in minions)
                RestoreMinion(tm, m);

            // 5) 룬
            foreach (RuneSnap r in runes)
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
            king.AddDiceCoin(diceCoin - king.diceCoin);
            if (MinionFieldController.Instance != null)
                MinionFieldController.Instance.minionMaxCount = minionMaxCount;
            if (king.health != null)
                king.health.curHp = Mathf.Min(curHp, king.MaxHp);
            king.UpdateCharacter();
            GameEventBus.Publish(new ExpChangedEvent(king.exp, king.MaxExp, king.lv));
            KingSlimeManageCanvas.Instance?.UpdateCanvas();
        }
        finally
        {
            IsRestoring = false;
        }
        return true;
    }

    // 능력 key → AbilityData (용사·용사 강화 카드, 그 밖은 Resources/KingGrowth 아래 전부)
    public static AbilityData FindAbilityData(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;
        KingGrowthSetting setting = KingGrowthSetting.Instance;
        if (setting != null)
        {
            foreach (AbilityData[] list in new[] { setting.heroAbilities, setting.heroBoostAbilities })
                if (list != null)
                    foreach (AbilityData d in list)
                        if (d != null && d.key == key)
                            return d;
        }
        return Resources.LoadAll<AbilityData>("KingGrowth").FirstOrDefault(d => d != null && d.key == key);
    }

    static void RestoreAbility(KingAbilityInventory inv, AbilitySnap a)
    {
        AbilityData data = FindAbilityData(a.dataKey);
        if (data == null || data.prefab == null)
        {
            Debug.LogWarning($"[GameSnapshot] 능력 '{a.dataKey}'을(를) 찾지 못해 건너뜀");
            return;
        }
        if (a.expandOffsets != null && a.expandOffsets.Count > 0)
        {
            // 정예 칸 추가: 레벨마다 칸 하나 (추가 전에 extraOffset 지정)
            for (int i = 0; i < a.expandOffsets.Count; i++)
            {
                Vector2Int off = a.expandOffsets[i];
                if (i == 0)
                    inv.Add(data, ab => { if (ab is EliteAreaExpandKingAbility e) e.extraOffset = off; });
                else
                {
                    if (inv.Get(data) is EliteAreaExpandKingAbility e)
                        e.extraOffset = off;
                    inv.Add(data);
                }
            }
            return;
        }
        inv.Add(data, ab =>
        {
            if (ab is HeroKingAbility h && a.heroOffsets != null && a.heroOffsets.Count > 0)
                h.eliteOffsets = a.heroOffsets.ToArray();
        });
        for (int lv = 1; lv < a.level; lv++)
            inv.Add(data);
        if (inv.Get(data) is HeroKingAbility hero)
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
        if (m.hasRotation && minion is IRotationDirectionUnit r)
            r.SetRotationDirection(m.rotation);
        if (m.hasBuffShape && minion is IBuffRangeUnit b)
            b.SetBuffRangeShape(m.buffShape);
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
