using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class FreeMiniMeMovement : MiniMeMovement
{
    public Vector2 dir; // 이거를 4~7초 마다 랜덤하게 변경되게 하고, 변경되기 전에는 clock에 따라서 각도가 조금씩 변경되게 처리, 플레이어랑 7보다 멀리 떨어져 있으면 플레이어 방향으로 이동하게 설정
    public bool clock;// 참이면 시계 방향 - 5~8초 사이로 랜덤하게 변경되게 처리

    public float leashRange = 7f;
    public float rotateSpeed = 40f; // 초당 회전 각도

    public float minSpeed = 1f;
    public float maxSpeed = 3f;
    public float speedChangeSpeed = 2f; // moveSpeed가 초당 변할 수 있는 최대량 (급변 방지)

    float targetSpeed;
    float speedTimer;

    float dirTimer;
    float clockTimer;

    public override void Awake()
    {
        base.Awake();
        dir = Random.insideUnitCircle.normalized;
        clock = Random.value > 0.5f;
        dirTimer = Random.Range(4f, 7f);
        clockTimer = Random.Range(5f, 8f);

        targetSpeed = moveSpeed = Random.Range(minSpeed, maxSpeed);
        speedTimer = Random.Range(2f, 4f);
    }

    public override void Update()
    {
        base.Update();

        speedTimer -= Time.deltaTime;
        if (speedTimer <= 0f)
        {
            targetSpeed = Random.Range(minSpeed, maxSpeed);
            speedTimer = Random.Range(2f, 4f);
        }
        moveSpeed = Mathf.MoveTowards(moveSpeed, targetSpeed, speedChangeSpeed * Time.deltaTime);

        clockTimer -= Time.deltaTime;
        if (clockTimer <= 0f)
        {
            clock = Random.value > 0.5f;
            clockTimer = Random.Range(5f, 8f);
        }

        dirTimer -= Time.deltaTime;
        if (dirTimer <= 0f)
        {
            Vector2 toPlayer = (Vector2)Character.Instance.transform.position - (Vector2)transform.position;
            if (toPlayer.magnitude > leashRange)
            {
                dir = toPlayer.normalized;
                return;
            }
            else
            {
                dir = Random.insideUnitCircle.normalized;    
            }
            
            dirTimer = Random.Range(4f, 7f);
            return;
        }

        float angle = rotateSpeed * Time.deltaTime * (clock ? -1f : 1f);
        dir = Quaternion.Euler(0, 0, angle) * dir;
    }
    
    public override void FixedUpdate()
    {
        //base.FixedUpdate();
        rg2D.linearVelocity = dir* moveSpeed;
    }
}
