using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

// 게임을 하고 로비로 돌아왔을 때의 연출 (2026-11-20)
// 씬 구성: BossMover(bossMoverTr) 하위에 Boss · NextBoss · BossTr · NextBossTr · WaveText · NextWaveText (글씨는 보스 하위가 아님). 킹은 따로.
// → 보스 쪽은 BossMover 하나만 움직여서 "킹이 올라가는 것처럼" 보이게 함 (킹은 제자리에서 콩콩)
// 스테이지에서 보스(wave 25·50, 2026-11-26)를 처음 처치하고 돌아오면:
//  1. 킹은 제자리에서 콩콩, BossMover가 내려와 처치한 보스가 킹 앞(stopDistance)까지
//  2. 킹이 뒤로 살짝(힘 모음) → 몸통 박치기 → 보스가 돌며 날아감
//  3. BossMover가 계속 내려옴(킹은 콩콩) → revealDistance(3)만큼 더 내려오면 NextBoss가 보임 → NextBoss가 보스 자리에 도착
//  4. 끝 = 평소 화면 (BossMover 원위치, Boss = 다음 보스)
// 평소 화면 = 지금 잡아야 할 보스만 (NextBoss 숨김). 마지막 보스까지 다 잡았으면 최종 보스(wave 50)
// BattleCanvas를 다시 열면(다른 로비 화면에 갔다 오면) 연출 중이어도 바로 평소 화면
public class LobbyReturnController : MonoSingleton<LobbyReturnController>
{
    [Header("자리 (고정)")]
    public Transform bossTr;       // 보스 자리 (BossMover 하위)
    public Transform nextBossTr;   // 다음 보스 자리 (BossMover 하위, 비우면 NextBoss 처음 자리)
    public Transform kingSlimeTr;
    public Transform bossMoverTr;  // 보스 쪽 전체를 움직이는 부모 (비우면 bossTr의 부모)
    [Header("보이는 것")]
    public LobbyBoss boss;      // 평소 = 지금 잡을 보스, 연출 = 처치한 보스(날아감)
    public LobbyBoss nextBoss;  // 연출 때 위에서 내려오는 다음 보스 (평소엔 숨김)
    public Transform kingSlime;
    public TMP_Text curWaveText;   // Boss의 wave ("Wave 20"). BossMover 하위라 BossMover와 같이 움직임 (보스가 날아가도 그 자리)
    public TMP_Text nextWaveText;  // NextBoss의 wave ("Wave 40"). 최종 보스를 잡는 연출이면 NextBoss 없이 "Clear" 글씨만. 평소엔 숨김

    [Header("연출 - 다가가기 (킹은 제자리 콩콩, 보스가 내려옴)")]
    public int hopCount = 3;            // 뛰는 횟수 (보스가 내려오는 시간 = hopCount × hopDuration)
    public float hopDuration = 0.4f;    // 한 번 뛰는 시간
    public float hopHeight = 0.5f;      // 뛰는 높이
    public float stopDistance = 1.2f;   // 보스가 멈추는 자리 = 킹에서 이 거리
    [Header("연출 - 박치기")]
    public float windupDistance = 0.3f; // 뒤로 물러나는 거리
    public float windupDuration = 0.3f;
    public float slamDuration = 0.12f;
    public float contactDistance = 0.4f; // 부딪히는 순간 킹 ~ 보스 거리
    [Header("연출 - 보스 날아감")]
    public Vector2 flyOffset = new Vector2(5f, 8f); // 보스 자리에서 날아가는 만큼
    public float flySpin = 720f;
    public float flyDuration = 0.7f;
    [Header("연출 - 다음 보스까지")]
    public float scrollDelay = 0.1f;
    public float scrollDuration = 1.5f;  // 날아간 뒤 NextBoss가 보스 자리에 올 때까지
    public float revealDistance = 3f;    // 보스가 날아간 뒤 이만큼 더 내려오면 NextBoss가 보임
    public float landDuration = 0.3f;    // 다음 보스 도착 출렁

    // 보스 그림. index = 보스 순번 - 1 (0 = wave 20, 1 = wave 40 …). 모자라면 마지막 그림, 비어 있으면 LobbyBoss 그림 그대로
    public Sprite[] bossSprites;

    // 결과 화면 → 로비로 넘기는 값 (저장 안 함, UserMyInfoManager.unshownGainExp와 같은 방식)
    // 이번 판에 처음 처치한 보스 중 가장 늦은 wave 번호(표시 번호). 0 = 없음
    public static int unshownDefeatedBossWave;

    bool isPlaying;
    public bool IsPlaying => isPlaying;

    // ---------- 규칙 (static, 테스트용) ----------

    // 처치한 보스 수. waveIdx = 저장값 maxWave(= 마지막으로 진행한 waveIdx). 보스 wave를 깨면 다음 waveIdx로 넘어가므로 waveIdx / 20
    public static int GetDefeatedBossCount(int waveIdx, bool isCleared)
    {
        if (isCleared)
            return GameSetting.LAST_WAVE / GameSetting.BOSS_WAVE_INTERVAL;
        return Mathf.Max(0, waveIdx) / GameSetting.BOSS_WAVE_INTERVAL;
    }

    // 다음에 잡을 보스 wave(표시 번호). 마지막 보스까지 잡았으면 0
    public static int GetNextBossWave(int defeatedBossCount)
    {
        int wave = (defeatedBossCount + 1) * GameSetting.BOSS_WAVE_INTERVAL;
        return wave <= GameSetting.LAST_WAVE ? wave : 0;
    }

    // 처음 처치한 보스가 있으면 그 wave(표시 번호), 없으면 0
    public static int GetNewlyDefeatedBossWave(int prevDefeatedBossCount, int waveIdx, bool isCleared)
    {
        int count = GetDefeatedBossCount(waveIdx, isCleared);
        return count > prevDefeatedBossCount ? count * GameSetting.BOSS_WAVE_INTERVAL : 0;
    }

    // 결과 화면에서 호출. prevDefeatedBossCount = 이번 판 기록을 저장하기 전 값
    public static void RecordGameResult(int prevDefeatedBossCount, int waveIdx, bool isCleared)
    {
        int wave = GetNewlyDefeatedBossWave(prevDefeatedBossCount, waveIdx, isCleared);
        if (wave > unshownDefeatedBossWave)
            unshownDefeatedBossWave = wave;
    }

    // ---------- 평소 화면 ----------

    Sequence playingSeq;
    bool homeSaved;
    Vector3 moverHome;
    Vector3 bossHomeLocal, nextBossHomeLocal, kingHomeScale, bossHomeScale, nextBossHomeScale;
    Quaternion bossHomeRot, nextBossHomeRot;

    Transform Mover => bossMoverTr != null ? bossMoverTr : (bossTr != null ? bossTr.parent : null);

    // 처음 자리·크기 기억 (연출 후·중단 시 되돌림)
    void SaveHome()
    {
        if (homeSaved)
            return;
        homeSaved = true;
        if (Mover != null) moverHome = Mover.localPosition;
        if (boss != null) { bossHomeLocal = boss.transform.localPosition; bossHomeRot = boss.transform.localRotation; bossHomeScale = boss.transform.localScale; }
        if (nextBoss != null) { nextBossHomeLocal = nextBoss.transform.localPosition; nextBossHomeRot = nextBoss.transform.localRotation; nextBossHomeScale = nextBoss.transform.localScale; }
        if (kingSlime != null) kingHomeScale = kingSlime.localScale;
    }

    void Awake()
    {
        SaveHome();
    }

    // 선택한 스테이지 기록 기준으로 지금 잡을 보스만 보임 (BattleCanvas가 열릴 때·스테이지를 바꿀 때 호출)
    // 연출 중이면 연출을 끝내고 바로 평소 화면
    public void ShowStage(UserStage userStage)
    {
        if (userStage == null)
            return;
        if (isPlaying && playingSeq != null && playingSeq.IsActive())
            playingSeq.Kill(); // PlayBossDefeatAsync가 이어서 평소 화면으로 (아래 ApplyLayout과 같은 결과)
        int nextWave = GetNextBossWave(GetDefeatedBossCount(userStage.maxWave, userStage.isClear));
        ApplyLayout(nextWave);
    }

    // 평소 화면에 보이는 보스 wave: 다음에 잡을 보스, 다 잡았으면 최종 보스
    public static int GetDisplayBossWave(int nextBossWave) => nextBossWave > 0 ? nextBossWave : GameSetting.LAST_WAVE;

    // 평소 화면: BossMover·보스·킹 원위치, Boss = 지금 잡을 보스(다 잡았으면 최종 보스), NextBoss 숨김
    void ApplyLayout(int nextBossWave)
    {
        SaveHome();
        int bossWave = GetDisplayBossWave(nextBossWave);
        if (Mover != null)
            Mover.localPosition = moverHome;
        if (kingSlime != null)
        {
            PlaceAt(kingSlime, kingSlimeTr);
            kingSlime.localScale = kingHomeScale;
        }
        if (boss != null)
        {
            boss.transform.localPosition = bossTr != null && bossTr.parent == boss.transform.parent ? bossTr.localPosition : bossHomeLocal;
            boss.transform.localRotation = bossHomeRot;
            boss.transform.localScale = bossHomeScale;
            boss.Show(GetBossSprite(bossWave));
            boss.SetSpriteVisible(true);
        }
        if (curWaveText != null)
        {
            curWaveText.gameObject.SetActive(true);
            curWaveText.text = GetWaveLabel(bossWave);
        }
        if (nextWaveText != null)
            nextWaveText.gameObject.SetActive(false);
        if (nextBoss != null)
        {
            ResetNextBoss();
            nextBoss.Hide();
        }
    }

    void ResetNextBoss()
    {
        nextBoss.transform.localPosition = nextBossTr != null && nextBossTr.parent == nextBoss.transform.parent ? nextBossTr.localPosition : nextBossHomeLocal;
        nextBoss.transform.localRotation = nextBossHomeRot;
        nextBoss.transform.localScale = nextBossHomeScale;
    }

    // ---------- 연출 ----------

    // 로비 시작 때 LobbyManager가 호출. 보여줄 게 있으면 페이드가 끝난 뒤 연출하고 끝날 때까지 기다림
    public static async UniTask PlayPendingAsync(UniTask fadeDone)
    {
        int wave = unshownDefeatedBossWave;
        unshownDefeatedBossWave = 0;
        if (wave <= 0)
            return;

        LobbyReturnController controller = Instance;
        if (controller == null)
            return;
        await fadeDone;
        await controller.PlayBossDefeatAsync(wave);
    }

    // defeatedBossWave = 처치한 보스 wave(표시 번호)
    public async UniTask PlayBossDefeatAsync(int defeatedBossWave)
    {
        SaveHome();
        int nextWave = GetNextBossWave(defeatedBossWave / GameSetting.BOSS_WAVE_INTERVAL);
        Transform mover = Mover;
        if (kingSlime == null || boss == null || nextBoss == null || mover == null || kingSlimeTr == null)
        {
            Debug.LogWarning("[LobbyReturnController] kingSlime·boss·nextBoss·bossMoverTr·kingSlimeTr 연결이 비어 연출 생략");
            ApplyLayout(nextWave);
            return;
        }
        if (isPlaying && playingSeq != null && playingSeq.IsActive())
            playingSeq.Kill();

        // 시작 = 평소 화면에서 Boss만 처치한 보스로, NextBoss = 다음 보스(최종이었으면 그림 없이 "Clear")
        ApplyLayout(nextWave);
        boss.Show(GetBossSprite(defeatedBossWave));
        if (curWaveText != null)
            curWaveText.text = GetWaveLabel(defeatedBossWave);
        if (nextWaveText != null)
            nextWaveText.text = GetNextWaveLabel(defeatedBossWave);
        if (nextWave > 0)
        {
            nextBoss.Show(GetBossSprite(nextWave));
            nextBoss.SetSpriteVisible(true);
        }
        nextBoss.Hide(); // NextBoss·NextWaveText는 revealDistance만큼 더 내려오면 보임 (최종 보스였으면 NextBoss 없이 Clear 글씨만)

        isPlaying = true;
        Sequence seq = BuildDefeatSequence(mover, nextWave > 0);
        playingSeq = seq;

        var done = new UniTaskCompletionSource();
        _ = seq.OnKill(() => done.TrySetResult()); // 끝나거나, BattleCanvas를 다시 열어 중단되거나, 씬이 바뀌어도
        await done.Task;
        isPlaying = false;
        playingSeq = null;
        if (this == null)
            return; // 연출 중 씬이 바뀜

        // 끝 = 평소 화면 (NextBoss가 도착한 자리 = 보스 자리라 Boss로 바꿔도 같은 모습)
        ApplyLayout(nextWave);
    }

    Sequence BuildDefeatSequence(Transform mover, bool hasNextBoss)
    {
        Vector3 kingPos = kingSlimeTr.position;
        Vector3 kingScale = kingHomeScale;
        Vector3 bossPos = boss.transform.position;
        Vector3 moverStart = mover.position;
        float dist = Vector3.Distance(kingPos, bossPos);
        Vector3 dir = dist > 0.0001f ? (bossPos - kingPos) / dist : Vector3.up;
        Vector3 meetPos = kingPos + dir * Mathf.Min(stopDistance, dist);              // 보스가 내려와 멈추는 자리
        Vector3 contactPos = meetPos - dir * Mathf.Min(contactDistance, stopDistance); // 박치기 순간 킹 자리
        Vector3 moverMeet = moverStart + (meetPos - bossPos);
        // NextBoss가 보스 자리(처음 Boss 자리)에 오는 BossMover 위치
        Vector3 moverEnd = moverStart + (bossPos - nextBoss.transform.position);

        Sequence seq = DOTween.Sequence().SetLink(gameObject);

        // 1. 킹은 제자리 콩콩, BossMover가 내려와 보스가 킹 앞까지
        float approachDuration = Mathf.Max(1, hopCount) * hopDuration;
        seq.Append(mover.DOMove(moverMeet, approachDuration).SetEase(Ease.Linear));
        InsertHops(seq, 0f, approachDuration, kingPos);

        // 2. 뒤로 살짝 물러나며 웅크림 → 몸통 박치기
        seq.Append(kingSlime.DOMove(kingPos - dir * windupDistance, windupDuration).SetEase(Ease.OutQuad));
        seq.Join(kingSlime.DOScale(new Vector3(kingScale.x * 1.15f, kingScale.y * 0.85f, kingScale.z), windupDuration));
        seq.Append(kingSlime.DOMove(contactPos, slamDuration).SetEase(Ease.InQuad));
        seq.Join(kingSlime.DOScale(new Vector3(kingScale.x * 0.85f, kingScale.y * 1.2f, kingScale.z), slamDuration));

        // 3. 부딪힘: 킹 납작 → 원래 크기·제자리, 보스(+글씨)는 돌며 날아감 → 숨김
        seq.Append(kingSlime.DOScale(new Vector3(kingScale.x * 1.25f, kingScale.y * 0.8f, kingScale.z), 0.06f));
        seq.Append(kingSlime.DOScale(kingScale, 0.2f).SetEase(Ease.OutBack));
        seq.Join(boss.transform.DOMove(meetPos + (Vector3)flyOffset, flyDuration).SetEase(Ease.OutQuad));
        seq.Join(boss.transform.DORotate(new Vector3(0f, 0f, -flySpin), flyDuration, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear));
        seq.Join(kingSlime.DOMove(kingPos, flyDuration).SetEase(Ease.OutQuad));
        seq.AppendCallback(() => boss.Hide());

        // 4. BossMover가 계속 내려옴 (킹은 콩콩) → revealDistance만큼 내려오면 NextBoss 보임 → 보스 자리 도착 + 출렁
        seq.AppendInterval(scrollDelay);
        float scrollStart = seq.Duration(false);
        bool revealed = false;
        seq.Append(mover.DOMove(moverEnd, scrollDuration).SetEase(Ease.OutQuad));
        seq.OnUpdate(() =>
        {
            if (!revealed && seq.Elapsed(false) >= scrollStart && Vector3.Distance(mover.position, moverMeet) >= revealDistance)
            {
                revealed = true;
                RevealNext(hasNextBoss);
            }
        });
        seq.AppendCallback(() => RevealNext(hasNextBoss)); // 거리가 revealDistance보다 짧아도 도착하면 보임
        InsertHops(seq, scrollStart, scrollDuration, kingPos);
        if (hasNextBoss)
            seq.Append(nextBoss.transform.DOPunchScale(new Vector3(0.15f, -0.12f, 0f), landDuration, 2, 0.5f));
        return seq;
    }

    void RevealNext(bool hasNextBoss)
    {
        if (hasNextBoss)
            nextBoss.gameObject.SetActive(true);
        if (nextWaveText != null)
            nextWaveText.gameObject.SetActive(true);
    }

    // duration 동안 킹이 제자리에서 콩콩 (hopDuration마다 한 번)
    void InsertHops(Sequence seq, float at, float duration, Vector3 kingPos)
    {
        int hops = Mathf.Max(1, Mathf.RoundToInt(duration / Mathf.Max(0.05f, hopDuration)));
        float hopTime = duration / hops;
        for (int i = 0; i < hops; i++)
        {
            float t = at + i * hopTime;
            seq.Insert(t, kingSlime.DOJump(kingPos, hopHeight, 1, hopTime).SetEase(Ease.Linear));
            seq.Insert(t, kingSlime.DOPunchScale(new Vector3(0.12f, -0.12f, 0f), hopTime, 2, 0.5f));
        }
    }

    // 테스트: 로비 Play 중 인스펙터에서 컴포넌트 ⋮ 메뉴 → "연출 테스트" (wave 20 보스를 처음 잡고 돌아온 것처럼)
    [ContextMenu("연출 테스트 (wave 20 보스)")]
    void TestPlayWave20() => TestPlay(20);
    [ContextMenu("연출 테스트 (wave 50 최종 보스, Clear)")]
    void TestPlayWave60() => TestPlay(GameSetting.LAST_WAVE);

    void TestPlay(int bossWave)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[LobbyReturnController] 연출 테스트는 Play 중에만 됩니다.");
            return;
        }
        if (isPlaying)
            return;
        PlayBossDefeatAsync(bossWave).Forget();
    }

    // ---------- wave 글씨 ----------

    public const string CLEAR_TEXT = "Clear";

    public static string GetWaveLabel(int wave) => $"Wave {wave}";

    // NextBoss 글씨: 처치한 보스 다음 보스 "Wave 40", 최종 보스였으면 "Clear", 0이면 null
    public static string GetNextWaveLabel(int bossWave)
    {
        if (bossWave <= 0)
            return null;
        int next = bossWave + GameSetting.BOSS_WAVE_INTERVAL;
        return next <= GameSetting.LAST_WAVE ? GetWaveLabel(next) : CLEAR_TEXT;
    }

    Sprite GetBossSprite(int bossWave)
    {
        if (bossSprites == null || bossSprites.Length == 0)
            return null;
        int idx = Mathf.Clamp(bossWave / GameSetting.BOSS_WAVE_INTERVAL - 1, 0, bossSprites.Length - 1);
        return bossSprites[idx];
    }

    static void PlaceAt(Transform target, Transform anchor)
    {
        if (target == null || anchor == null)
            return;
        target.SetPositionAndRotation(anchor.position, anchor.rotation);
    }
}
