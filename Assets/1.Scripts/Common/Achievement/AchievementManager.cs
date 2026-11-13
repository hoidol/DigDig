using System.Collections.Generic;
using UnityEngine;

// 업적: 일일(DailyAchievement.csv) / 누적(CumulativeAchievement.csv). 한 행 = 업적 하나(Type)
// 집계는 행동이 일어나는 곳에서 AchievementManager.Report(type, 횟수)를 직접 호출한다 (씬 이벤트 구독 없음)
// 날짜가 바뀌면(DailyCheck, 기기 시간 자정) 일일 업적 초기화 + Login 1회
public class AchievementManager : MonoSingleton<AchievementManager>
{
    public DailyAchievementManager dailyAchievementManager;
    public CumulativeAchievementManager cumulativeAchievementManager;
    public List<SubAchievementManager> subAchievementManagers = new List<SubAchievementManager>();

    bool isDirty; // 처치처럼 자주 오르는 값은 프레임 끝에 한 번만 저장

    void Awake()
    {
        dailyAchievementManager = GetComponentInChildren<DailyAchievementManager>();
        cumulativeAchievementManager = GetComponentInChildren<CumulativeAchievementManager>();

        subAchievementManagers.Add(dailyAchievementManager);
        subAchievementManagers.Add(cumulativeAchievementManager);

        dailyAchievementManager.Init();
        cumulativeAchievementManager.Init();

        CheckNewDay();
    }

    public SubAchievementManager GetSubAchievementManager(AchievementCategory category)
    {
        for (int i = 0; i < subAchievementManagers.Count; i++)
        {
            if (subAchievementManagers[i].category == category)
            {
                return subAchievementManagers[i];
            }
        }
        return null;
    }

    // 업적 집계는 이걸로. 매니저가 없으면(테스트 등) 아무 일도 안 함
    public static void Report(AchievementType achievementType, int count = 1)
    {
        if (Instance == null)
            return;
        Instance.Achieve(achievementType, count);
    }

    public void Achieve(AchievementType achievementType, int count = 1)
    {
        if (count <= 0)
            return;
        CheckNewDay();
        AddValue(achievementType, count);
    }

    void AddValue(AchievementType achievementType, int count)
    {
        for (int i = 0; i < subAchievementManagers.Count; i++)
        {
            subAchievementManagers[i].Achieve(achievementType, count);
        }
        isDirty = true;
        GameEventBus.Publish(new UpdateAchievementEvent());
    }

    // 날짜가 바뀌었으면 일일 업적 초기화 + 접속(Login) 1회. 집계 전·업적 창을 열 때·앱으로 돌아올 때 확인
    public void CheckNewDay()
    {
        if (!dailyAchievementManager.ResetIfNewDay())
            return;
        AddValue(AchievementType.Login, 1);
        SaveIfDirty();
    }

    void LateUpdate()
    {
        SaveIfDirty();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            CheckNewDay();
    }

    void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
            SaveIfDirty();
        else
            CheckNewDay();
    }

    void OnDestroy()
    {
        SaveIfDirty();
    }

    public void SaveIfDirty()
    {
        if (!isDirty)
            return;
        isDirty = false;
        dailyAchievementManager.userDailyAchievementManager.SaveData();
        cumulativeAchievementManager.userCumulativeAchievementManager.SaveData();
    }
}
public enum AchievementCategory
{
    Daily,
    Cumulative
}
// 저장 데이터에 번호로 들어가므로 순서를 바꾸지 말고 뒤에 추가한다. CSV Type 열 = 이 이름, 표시 이름 = Translate `Achievement.{Type}`
public enum AchievementType
{
    KillEnemy,        // 적 처치 (경로 끝 도착 제외)
    ClearStage,       // 사용 안 함 (무한 웨이브)
    Login,            // 접속한 날 수 (하루 1회)
    LevelUpMinion,     // 로비 슬라임 강화
    TryStage,         // 한 판 = 결과 화면
    DrawMinion,        // 로비 슬라임 뽑기 (뽑은 장수)
    KillBonusElite,   // 보너스 엘리트 처치
    DrawItem,         // 인게임 타일 아이템 뽑기 시도 (확률 실패 포함)
    UpgradeReinforce, // 인게임 직접 강화
    Watch_Ad,         // 광고 보상 받기 (광고 제거 구매자 포함)
    SpawnMinion,       // 소환 버튼으로 소환 (무료 소환 제외)
    MergeMinion        // 슬라임 합성
}
[System.Serializable]
public abstract class AchievementData
{
    public AchievementType type;
    public ConditionData conditionData = new ConditionData();
    public RewardData rewardData;
    public string Title => TranslateManager.GetText(GetTitleKey(type));
    public static string GetTitleKey(AchievementType type) => $"Achievement.{type}";
    public string Desc() => $"{GetDisplayValue()}/{GetGoal()}";
    public float fillAmount => GetFillAmount(GetUserAchievement().value, GetGoal());

    public static float GetFillAmount(int value, int goal) => goal > 0 ? Mathf.Clamp01((float)value / goal) : 1f;

    // 진행 글씨에 보이는 값 (일일은 목표를 넘지 않게)
    protected virtual int GetDisplayValue() => GetUserAchievement().value;

    public abstract UserAchievement GetUserAchievement();
    public abstract bool CheckCanClear();
    public abstract int GetGoal();
}

public class UpdateAchievementEvent
{

}

// 업적 CSV(탭 구분) 공용 읽기
public static class AchievementCsv
{
    public class Row
    {
        Dictionary<string, string> cols = new Dictionary<string, string>();
        public int line;
        public void Set(string header, string value) => cols[header] = value;
        public string Get(string header) => cols.TryGetValue(header, out string v) ? v : "";
        public int GetInt(string header) => int.TryParse(Get(header), out int v) ? v : 0;
    }

    public static List<Row> Parse(string text)
    {
        List<Row> rows = new List<Row>();
        string[] lines = text.Split('\n');
        string[] headers = lines[0].Trim().Split('\t');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] cols = lines[i].Trim('\r', '\n').Split('\t');
            Row row = new Row { line = i + 1 };
            for (int c = 0; c < headers.Length && c < cols.Length; c++)
                row.Set(headers[c].Trim(), cols[c].Trim());
            rows.Add(row);
        }
        return rows;
    }

    // Type·RewardType이 enum에 없으면 에러 로그 후 false (그 행은 건너뜀)
    public static bool TryGetTypes(Row row, string fileName, out AchievementType type, out RewardData rewardData)
    {
        rewardData = null;
        if (!System.Enum.TryParse(row.Get("Type"), out type))
        {
            Debug.LogError($"[{fileName}] {row.line}행: 알 수 없는 Type '{row.Get("Type")}' (AchievementType에 추가 필요)");
            return false;
        }
        if (!System.Enum.TryParse(row.Get("RewardType"), out RewardType rewardType))
        {
            Debug.LogError($"[{fileName}] {row.line}행: 알 수 없는 RewardType '{row.Get("RewardType")}'");
            return false;
        }
        rewardData = new RewardData { rewardType = rewardType, value = row.Get("RewardValue") };
        return true;
    }
}
