// Firebase 이벤트 이름 모음. 같은 이벤트를 여러 곳에서 찍을 때 이름이 어긋나지 않도록 여기에 추가해서 쓴다.
// 규칙: 소문자 snake_case, 40자 이하 (FirebaseEvent 주석 참고)
public static class FirebaseEventName
{
    // 예시 — 필요한 것만 남기고 추가
    public const string TUTORIAL_BEGIN = "tutorial_begin";       // Firebase 권장 이벤트 이름
    public const string TUTORIAL_COMPLETE = "tutorial_complete"; // Firebase 권장 이벤트 이름
    public const string STAGE_START = "stage_start";
    public const string STAGE_END = "stage_end";
}
