using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    Rigidbody2D rg;
    Animator animator;
    Transform bodyRootTr;
    Character character;

    public Vector2 MoveDirection { get; private set; }
    public float maxDistance { get; private set; }
    float maxDistanceSqr;

    public void Init(Character character, Rigidbody2D rg, Animator animator, Transform bodyRootTr)
    {
        this.character = character;
        this.rg = rg;
        this.animator = animator;
        this.bodyRootTr = bodyRootTr;
        MoveDirection = Vector2.up;
        Restart();
    }

    public void Restart()
    {
        maxDistance = 0;
        maxDistanceSqr = 0f;
        animator.Play("Idle");
    }

    public void Move()
    {

        if (!GameManager.Instance.isPlaying)
        {
            rg.linearVelocity = Vector2.zero;
            return;
        }

#if UNITY_EDITOR || !UNITY_ANDROID && !UNITY_IOS
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        MoveDirection = new Vector2(x, y).normalized;
#else
        MoveDirection = moveJoystick.Direction;
#endif
        if (MoveDirection.magnitude > 0.1f)
        {
            bodyRootTr.localScale = new Vector3(character.weapon.GetAttackDirection().x >= 0 ? 1 : -1, 1, 1);
            animator.SetBool("Running", true);

            float sqrDist = ((Vector2)transform.position - Vector2.zero).sqrMagnitude;
            if (sqrDist > maxDistanceSqr)
            {
                maxDistanceSqr = sqrDist;
                maxDistance = (int)Mathf.Sqrt(sqrDist);
            }
        }
        else
        {
            animator.SetBool("Running", false);
        }


        rg.linearVelocity = MoveDirection * (character.statMgr.MoveSpeed / 5);
    }

    void FixedUpdate()
    {
        Vector2 pos = rg.position;
        float maxR = MapManager.MAX_RANGE_RADIUS;
        if (pos.sqrMagnitude > maxR * maxR)
        {
            rg.position = pos.normalized * maxR;
            if (Vector2.Dot(rg.linearVelocity, pos) > 0)
                rg.linearVelocity = Vector2.zero;
        }
    }
}
