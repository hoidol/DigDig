// 권능 능력 하나 (CLAUDE.md "권능"). 클래스 이름 = {AuthorityData.key}Authority → AuthoritySystem이 key로 찾아 생성
// 판 하나 동안만 존재: Begin(게임 시작 전) → End(씬 종료). 수치는 각 권능 클래스의 const(임시)
public abstract class Authority
{
    public AuthorityData data;

    // 게임 시작 전 (타일·슬라임 생성 전에 불림). 이벤트 구독은 여기서
    public virtual void OnBegin() { }

    // 씬 종료. 구독 해제는 여기서
    public virtual void OnEnd() { }
}
