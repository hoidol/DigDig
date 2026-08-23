using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class MiniMeMovement : MonoBehaviour
{

    public Rigidbody2D rg2D;

    public float inRange = 5;
    public float moveSpeed;


    public LayerMask hittableLayer;
    const float STUCK_CHECK_INTERVAL = 0.35f;
    // float stuckCheckTimer;

    public virtual void Awake()
    {
        rg2D = GetComponent<Rigidbody2D>();
    }

    public virtual void FixedUpdate()
    {
        Vector2 vec = Character.Instance.transform.position - transform.position;
        if (vec.magnitude < inRange)
        {
            rg2D.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = vec.normalized;
        rg2D.linearVelocity = dir * moveSpeed;
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
