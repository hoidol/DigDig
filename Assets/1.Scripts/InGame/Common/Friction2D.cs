using UnityEngine;

// 중력 없는 환경에서 Rigidbody2D에 마찰 효과 적용
[RequireComponent(typeof(Rigidbody2D))]
public class Friction2D : MonoBehaviour
{
    [Range(0f, 1f)]
    public float friction = 0.95f; // 매 프레임 속도에 곱해지는 감속 계수 (1 = 감속 없음)

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity *= friction;
    }
}
