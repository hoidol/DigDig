using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class DroneMovement : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float radius = 3f;
    public Vector2 stayTimes = new(0.5f, 1.5f);
    public LayerMask oreMask;

    const int MAX_CANDIDATE = 10;
    const float CHECK_RADIUS = 0.3f;

    public Vector2 targetPos;
    Tween moveTween;

    public void Spawn(Vector2 pos)
    {
        transform.position = pos;
        MoveToNext();
    }

    void SetTargetPosition()
    {
        Vector2 playerPos = Player.Instance.transform.position;

        for (int i = 0; i < MAX_CANDIDATE; i++)
        {
            Vector2 candidate = playerPos + Random.insideUnitCircle * radius;
            if (Physics2D.OverlapCircle(candidate, CHECK_RADIUS, oreMask) == null)
            {
                targetPos = candidate;
                return;
            }
        }
        targetPos = playerPos;
    }

    void MoveToNext()
    {
        SetTargetPosition();
        float dist = Vector2.Distance(transform.position, targetPos);
        float duration = dist / moveSpeed;

        moveTween?.Kill();
        moveTween = transform.DOMove(targetPos, duration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                float wait = Random.Range(stayTimes.x, stayTimes.y);
                moveTween = DOVirtual.DelayedCall(wait, MoveToNext);
            });
    }

    void OnDestroy()
    {
        moveTween?.Kill();
    }
}
