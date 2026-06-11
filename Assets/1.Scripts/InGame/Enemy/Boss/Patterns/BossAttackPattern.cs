using UnityEngine;
using System;

// 보스 개별 공격 패턴 기본 클래스
// 각 패턴은 MonoBehaviour로 보스 프리팹의 자식 오브젝트에 붙여서 사용
public class BossAttackPattern : MonoBehaviour
{
    // 패턴 실행 - 끝나면 onEnd 호출
    [Header("해당 공격 총 시간")]
    [SerializeField] float duration;
    public virtual void Execute(Boss boss, Action onEnd)
    {
        
    }

    // 페이즈 전환 등으로 강제 중단 시 호출
    public virtual void Cancel()
    {
        
    }
}
