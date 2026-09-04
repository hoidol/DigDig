using UnityEngine;

[RequireComponent(typeof(Slime))]
public class SlimeMovement : MonoBehaviour
{

    public Rigidbody2D rg2D;


    public float outRange = 5;
    public float inRange = 1;
    public float moveSpeed;


    public LayerMask hittableLayer;
    const float STUCK_CHECK_INTERVAL = 0.35f;

    // float stuckCheckTimer;
    public Slime slime;
    public virtual void Awake()
    {
        rg2D = GetComponent<Rigidbody2D>();
        slime = GetComponent<Slime>();
    }

    public virtual void FixedUpdate()
    {
        Vector2 vec = Character.Instance.transform.position - transform.position;
        float tempMoveSpeed = moveSpeed;
        if (vec.magnitude < outRange)
        {
            tempMoveSpeed /= 3;
        }
        if (vec.magnitude <= inRange)
        {
            rg2D.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = vec.normalized;
        rg2D.linearVelocity = dir * tempMoveSpeed;
    }
    public virtual void Update()
    {

        // stuckCheckTimer -= Time.deltaTime;
        // if (stuckCheckTimer <= 0f)
        // {
        //     stuckCheckTimer = STUCK_CHECK_INTERVAL;
        //     if (Physics2D.OverlapPoint(transform.position, hittableLayer))
        //     {
        //         //순간 이동 이펙트
        //         transform.position = Character.Instance.transform.position;
        //         //순간 이동 이펙트
        //     }
        // }
    }
}
