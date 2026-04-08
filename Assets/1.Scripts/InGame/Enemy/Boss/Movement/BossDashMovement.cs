using UnityEngine;
using System;

// Boss GameObject에 부착
// 공격 후 플레이어 주변 랜덤 위치로 빠르게 이동
// 이동 중 플레이어 접촉 시 데미지는 Boss.OnTriggerEnter2D에서 처리
public class BossDashMovement : MonoBehaviour, IBossMovement
{
    [SerializeField] float dashSpeed = 18f;
    [SerializeField] float minDist = 5f;
    [SerializeField] float maxDist = 10f;
    [SerializeField] float arrivalThreshold = 0.3f;

    Boss boss;
    Action onComplete;
    bool isDashing;
    Vector2 targetPos;

    public void StartMove(Boss boss, Action onComplete)
    {
        this.boss = boss;
        this.onComplete = onComplete;
        targetPos = GetRandomPosAroundPlayer();
        isDashing = true;
    }

    void Update()
    {
        if (!isDashing) return;

        Vector2 dir = targetPos - (Vector2)boss.transform.position;
        if (dir.magnitude <= arrivalThreshold)
        {
            Arrive();
            return;
        }

        boss.Rigidbody2D.linearVelocity = dir.normalized * dashSpeed;
        boss.SetFacing(dir.x);
    }

    void Arrive()
    {
        isDashing = false;
        boss.Rigidbody2D.linearVelocity = Vector2.zero;
        onComplete?.Invoke();
    }

    public void Cancel()
    {
        if (!isDashing) return;
        isDashing = false;
        if (boss != null)
            boss.Rigidbody2D.linearVelocity = Vector2.zero;
    }

    Vector2 GetRandomPosAroundPlayer()
    {
        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float dist = UnityEngine.Random.Range(minDist, maxDist);
        return (Vector2)Player.Instance.transform.position
               + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
    }
}
