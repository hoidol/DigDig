using System.Collections.Generic;

// (2026-11-27) 플레이 기록 한 판 (스테이지 1 보통만, 싱글·멀티): 브레이크타임마다 상태 한 장(Frame) → 판이 끝나면 로컬 백업 후 서버(공용 DB, 최대 100판)
// 쓰는 곳: 멀티 방에 사람이 모자라면 방장이 이 기록을 시간대로 재생하는 AI 봇으로 채움 (MultiAiDirector)
[System.Serializable]
public class PlayRecord
{
    public const int VERSION = 1;

    [System.Serializable]
    public class Frame
    {
        public float t;          // 판 시작부터 실제 시간(초) — AI 봇 재생 기준
        public long at;          // 이 브레이크타임이 시작된 서버 시각 (ms, Unix UTC, ServerClock)
        public int wave;         // 다음 wave (브레이크타임)
        public int kingLv;
        public string heroKey;
        public float hp;         // 킹 체력 비율 0~1
        public int diceCoin;
        public string minions;   // "Key:표시레벨,…" (MultiGame.FormatMinions)
        public string board;     // MultiBoard JSON
    }

    public int version = VERSION;
    public string id;            // 기기에서 만든 고유 값
    public string mode;          // "Single" / "Multi"
    public int stageOrder;
    public DifficultyType difficulty;
    public long startedAt;       // 판 시작 서버 시각 (ms, Unix UTC, ServerClock)
    public float playTime;       // 끝난 시각 (판 시작부터 실제 시간, 초)
    public int finalWave;        // 마지막 wave 번호
    public bool cleared;         // 마지막 wave 보스 처치
    public bool abandoned;       // 끝내지 않고 버려진 판 (다음 판을 새로 시작함)
    public List<Frame> frames = new List<Frame>();

    public const string MODE_SINGLE = "Single";
    public const string MODE_MULTI = "Multi";
    public bool IsMulti => mode == MODE_MULTI;
}

// 서버 목록용 요약 (index/{칸}) — AI 봇이 고를 때 이것만 받아 거름
[System.Serializable]
public class PlayRecordSummary
{
    public int slot;
    public string id;
    public string mode;
    public int stageOrder;
    public DifficultyType difficulty;
    public int finalWave;
    public bool cleared;
    public float playTime;
    public long startedAt;
    public int frameCount;
    public string owner;     // (2026-11-27) 올린 유저 uid의 해시 (uid·닉네임은 안 남김) — 유저당 칸 수 제한
    public long uploadedAt;  // 올린 서버 시각 (ms)
}
