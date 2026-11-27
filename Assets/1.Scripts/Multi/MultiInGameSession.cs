using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

// (2026-11-27) 멀티 판 진행 중 (멀티 InGame에 MultiMatchManager가 만듦, 씬과 같이 사라짐)
// - 내 판 상황 올리기: 브레이크타임 시작(다음 프레임, 카드 지급 후)·wave 시작 때 wave·킹 레벨·용사·미니언·판 배치
// - 웨이브 50 보스 처치(= 클리어) → 도착 기록(서버 시각) + 가장 먼저면 우승 → 전적 우승·클리어
// - 내 판이 끝남(클리어 또는 부활 안 하고 포기) → 종료 기록 + 판수 → 멀티 결과 창 (ResultCanvas 대신, 싱글 보상 없음)
public class MultiInGameSession : MonoBehaviour
{
    public static MultiInGameSession Instance { get; private set; }

    public bool Ended { get; private set; }
    public bool Reached { get; private set; }
    public bool IsChampion { get; private set; }

    void Awake() => Instance = this;

    void OnEnable()
    {
        GameEventBus.Subscribe<BreakTimeStartEvent>(OnBreakTimeStart);
        GameEventBus.Subscribe<WaveStartEvent>(OnWaveStart);
    }

    void OnDisable()
    {
        GameEventBus.Unsubscribe<BreakTimeStartEvent>(OnBreakTimeStart);
        GameEventBus.Unsubscribe<WaveStartEvent>(OnWaveStart);
        if (Instance == this)
            Instance = null;
    }

    void OnBreakTimeStart(BreakTimeStartEvent e) => StartCoroutine(SendNextFrame());
    void OnWaveStart(WaveStartEvent e) => SendStatus();

    IEnumerator SendNextFrame()
    {
        yield return null;
        SendStatus();
    }

    public void SendStatus()
    {
        if (!MultiGame.IsActive || Ended || !MultiMatchManager.Exists)
            return;
        MultiMatchManager.Instance.UpdateStatus(BuildStatus());
    }

    // ---- 내 판 상황
    public static MultiStatus BuildStatus()
    {
        GameManager gm = GameManager.Instance;
        KingSlime king = KingSlime.Instance;
        TileManager tm = TileManager.Instance;
        var status = new MultiStatus();
        if (gm != null)
            status.wave = (gm.isBreakTime ? gm.breakTimeNextWaveIdx : gm.waveIdx) + 1;
        if (king != null)
        {
            status.kingLv = king.lv;
            HeroKingAbility hero = king.abilityInventory != null ? king.abilityInventory.Get<HeroKingAbility>() : null;
            status.heroKey = hero != null ? hero.HeroDataKey : "";
        }
        if (tm != null)
        {
            var board = new MultiBoard();
            var minions = new System.Collections.Generic.List<(string, int)>();
            foreach (Tile t in tm.tiles)
            {
                if (t == null)
                    continue;
                var cell = new MultiBoard.Cell { x = t.index.x, y = t.index.y, blocked = t.isBlocked };
                bool any = t.isBlocked;
                if (t.tileObject is Minion m)
                {
                    cell.minion = m.key;
                    cell.lv = m.mergeLevel + 1;
                    cell.evolved = m.isEvolved;
                    minions.Add((m.key, m.mergeLevel + 1));
                    any = true;
                }
                else if (t.tileObject is KingSlime)
                {
                    cell.king = true;
                    any = true;
                }
                if (t.tileRune is TileRune rune)
                {
                    cell.rune = rune.key;
                    cell.runeLv = rune.upgradeLevel;
                    any = true;
                }
                if (any)
                    board.cells.Add(cell);
            }
            status.minions = MultiGame.FormatMinions(minions);
            status.board = JsonUtility.ToJson(board);
        }
        return status;
    }

    // ---- 판 끝 (ResultCanvas.OpenCanvas가 멀티면 대신 부름)
    public void OnLocalGameEnded(bool cleared)
    {
        if (Ended)
            return;
        Ended = true;
        PlayRecorder.Finish(cleared); // (2026-11-27) 플레이 기록: 로컬 백업 → 서버
        FinishAsync(cleared).Forget();
    }

    async UniTaskVoid FinishAsync(bool cleared)
    {
        MultiMatchManager mm = MultiMatchManager.Instance;
        if (cleared)
        {
            Reached = true;
            IsChampion = await mm.ReportReach();
        }
        await mm.ReportEnd();
        UserDataManager.Instance.AddMultiRecord(IsChampion, Reached);
        MultiResultPanel panel = FindFirstObjectByType<MultiResultPanel>(FindObjectsInactive.Include);
        if (panel != null)
            panel.Show(this);
        else
        {
            Debug.LogWarning("[Multi] MultiResultPanel 없음 — 메뉴 Tools/Multi/Create MultiGameUI Scene · 로비로");
            mm.ExitMultiGame();
        }
    }
}
