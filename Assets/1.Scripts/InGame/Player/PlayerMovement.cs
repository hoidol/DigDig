using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Joystick moveJoystick;
    Rigidbody2D rg;
    Animator animator;
    Transform bodyRootTr;
    Player player;

    public Vector2 MoveDirection { get; private set; }
    public float maxDistance { get; private set; }
    float maxDistanceSqr;

    public void Init(Player player, Rigidbody2D rg, Animator animator, Transform bodyRootTr, Joystick moveJoystick)
    {
        this.player = player;
        this.rg = rg;
        this.animator = animator;
        this.bodyRootTr = bodyRootTr;
        this.moveJoystick = moveJoystick;
    }

    public void ResetOnUndergroundStart()
    {
        maxDistance = 0;
        maxDistanceSqr = 0f;
    }

    public void Move()
    {
#if UNITY_EDITOR || !UNITY_ANDROID && !UNITY_IOS
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        MoveDirection = new Vector2(x, y).normalized;
#else
        MoveDirection = moveJoystick.Direction;
#endif
        if (MoveDirection.magnitude > 0.1f)
        {
            bodyRootTr.localScale = new Vector3(MoveDirection.x >= 0 ? 1 : -1, 1, 1);
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

        rg.linearVelocity = MoveDirection * player.statMgr.MoveSpeed;
    }
}
