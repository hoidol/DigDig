using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChaseTargetMiniMeMovement : MiniMeMovement
{
    [SerializeField] Transform target;
    [SerializeField] float chaseRange;
    public override void Awake()
    {
        base.Awake();
        miniMe.attackBehaviour.onTargetListener += OnTargetListener;
    }

    void OnTargetListener(Transform tr)
    {
        target = tr;
    }
    public override void FixedUpdate()
    {
        if(target == null)
        { 
            base.FixedUpdate();
            return;
        }
        Vector2 vec = target.position - transform.position;
        
        if (vec.magnitude < chaseRange)
        {
            rg2D.linearVelocity = Vector2.zero;
            return;
        }
        rg2D.linearVelocity = vec.normalized * moveSpeed;
    }

}
