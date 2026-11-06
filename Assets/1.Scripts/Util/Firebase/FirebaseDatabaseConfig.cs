// Realtime Database 인스턴스 URL 모음. 서버 주소는 여기 한 곳에서만 관리한다.
//
// 유저 데이터 샤딩
// - 유저마다 처음 저장할 때 샤드(USERDATA_URLS 인덱스)를 배정하고, 랭킹 인스턴스의 shardMap/{uid}에 기록해 고정한다.
//   → 인스턴스를 추가해도 기존 유저는 원래 샤드 그대로. 옮기지 않는다.
// - 새 유저 배정 대상은 ASSIGNABLE_SHARDS. 이 안에서 uid 해시로 골고루 나눈다.
//
// 인스턴스 추가하는 법
// 1. Firebase 콘솔에서 인스턴스 생성 + users 보안 규칙 복사
// 2. USERDATA_URLS **맨 뒤에** URL 추가 (순서/인덱스 절대 변경·삭제 금지: shardMap에 인덱스로 저장됨)
// 3. ASSIGNABLE_SHARDS 조정 (예: 새 인스턴스에만 몰기 { 2 } / 전체에 골고루 { 0, 1, 2 })
// 4. 꽉 찬 인스턴스는 ASSIGNABLE_SHARDS에서 빼면 새 유저가 더 이상 안 들어간다
public static class FirebaseDatabaseConfig
{
#if DEV_SERVER
    // TODO: Dev 인스턴스 생성 후 입력
    public const string RANKING_URL = "";
    public static readonly string[] USERDATA_URLS = { };
    public static readonly int[] ASSIGNABLE_SHARDS = { };
#else
    public const string RANKING_URL = "https://dreamofslime-ranking.asia-southeast1.firebasedatabase.app/";
    public static readonly string[] USERDATA_URLS =
    {
        "https://dreamofslime-userdatas-1.asia-southeast1.firebasedatabase.app/", // 0
        "https://dreamofslime-userdatas-2.asia-southeast1.firebasedatabase.app/", // 1
    };
    public static readonly int[] ASSIGNABLE_SHARDS = { 0, 1 };
#endif

    // 유저 → 샤드 배정 기록 위치 (따로 인스턴스 두지 않고 랭킹 인스턴스에 둔다)
    public const string SHARD_MAP_URL = RANKING_URL;
    public const string SHARD_MAP_PATH = "shardMap";

    public static Firebase.Database.FirebaseDatabase Ranking => Firebase.Database.FirebaseDatabase.GetInstance(RANKING_URL);
    public static Firebase.Database.FirebaseDatabase ShardMap => Firebase.Database.FirebaseDatabase.GetInstance(SHARD_MAP_URL);
    public static Firebase.Database.FirebaseDatabase UserData(int shard) => Firebase.Database.FirebaseDatabase.GetInstance(USERDATA_URLS[shard]);

    // 새 유저 샤드 선택 : ASSIGNABLE_SHARDS 중 uid 해시로 고름.
    // string.GetHashCode()는 런타임마다 값이 달라질 수 있어 고정 해시(FNV-1a) 사용
    public static int PickShardForNewUser(string uid)
    {
        if (ASSIGNABLE_SHARDS.Length == 0)
            return -1;

        uint hash = 2166136261;
        foreach (char c in uid)
        {
            hash ^= c;
            hash *= 16777619;
        }
        return ASSIGNABLE_SHARDS[(int)(hash % (uint)ASSIGNABLE_SHARDS.Length)];
    }
}
