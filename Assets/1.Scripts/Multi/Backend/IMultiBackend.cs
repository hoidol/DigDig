using System;
using Cysharp.Threading.Tasks;

// (2026-11-27) 멀티 서버 통신 (Firebase / 에디터용 가짜). MultiMatchManager가 고름
public interface IMultiBackend
{
    string MyUid { get; }
    // 서버 시각(ms) 추정 (기기 시각 + 서버 시각 차이)
    long ServerNowMs { get; }

    // 빠른 매칭: 사람을 모으는 공개 방에 들어감 → 방 번호 (실패하면 error key)
    UniTask<MultiJoinResult> JoinAsync(string nickname);
    // 방만들기: 새 방(코드로만 입장)
    UniTask<MultiJoinResult> CreateRoomAsync(string nickname);
    // 코드입력: 그 코드의 방에 들어감
    UniTask<MultiJoinResult> JoinByCodeAsync(string nickname, string code);
    // 대기 중 나가기 (방이 차기 전만)
    UniTask LeaveAsync(string roomId);
    // 방 상태 구독 (바뀔 때마다 onChanged, 메인 스레드)
    void Listen(string roomId, Action<MultiRoomState> onChanged);
    void StopListen();
    UniTask SetReadyAsync(string roomId);

    // ---- 방장이 하는 일 (MultiMatchManager가 방장일 때만 부름)
    UniTask SetFullAtAsync(string roomId);                 // 8명이 차면 시작 기준 시각 (서버 시각)
    UniTask SetStartAtAsync(string roomId, long startAtMs); // Ready 수가 바뀔 때마다 시작 시각
    UniTask CleanupOldRoomsAsync();                         // 시작 5시간 지난 방 지우기

    UniTask AddAiPlayerAsync(string roomId, string aiUid, string nickname, int recordSlot); // AI 봇 자리 + 정보 (Ready 상태)

    // 판 상황 (wave·슬라임·미니언·배치). uid = null이면 나, 아니면 그 AI 봇 (방장)
    UniTask UpdateStatusAsync(string roomId, MultiStatus status, string uid = null);
    // 웨이브 50 보스 처치 → 도착 시각 기록 + 가장 먼저면 우승(true)
    UniTask<bool> ReportReachAsync(string roomId, string uid = null);
    // 판이 끝남 (도착 또는 탈락)
    UniTask ReportEndAsync(string roomId, string uid = null);
    // 멀티 판을 나감 (online = false → 방장이면 다음 사람이 이어받음)
    UniTask LeaveGameAsync(string roomId);
}

public class MultiJoinResult
{
    public string roomId;
    public string error; // 번역 key (Multi_JoinFailed · Multi_CodeNotFound · Multi_RoomFull · Multi_RoomStarted)
    public bool Ok => !string.IsNullOrEmpty(roomId);
    public static MultiJoinResult Fail(string error) => new MultiJoinResult { error = error };
    public static MultiJoinResult Success(string roomId) => new MultiJoinResult { roomId = roomId };
}

public class MultiStatus
{
    public int wave;
    public int slimeLv;
    public string heroKey;
    public string minions;
    public string board;
}
