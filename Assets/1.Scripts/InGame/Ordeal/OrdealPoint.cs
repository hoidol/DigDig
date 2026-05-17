using UnityEngine;
using UnityEngine.AI;

public class OrdealPoint : MonoBehaviour, IWayPointerTarget
{
    OrdealData ordealData;

    public Transform Transform => transform;

    public Sprite Thum => thum;
    [SerializeField] Sprite thum;

    public float MaxTime => OrdealManager.WAIT_TIME;


    public float CurTimer => curTimer;
    [SerializeField] protected float curTimer;
    [SerializeField] protected float clearRadius = 5f;


    public void Appear(Vector2 spawnPos)
    {
        curTimer = OrdealManager.WAIT_TIME;
        WayPointerCanvas.Instance.AddWayPoint(this, true, 2);
        ClearArea(transform.position);
    }

    public void Destroy()
    {
        Destroy(gameObject);
        if (!entered)
            WayPointerCanvas.Instance?.Remove(this);
    }

    public void ClearArea(Vector2 pos)
    {
        MapManager.Instance.ClearTilesInRadius(pos, clearRadius);
    }
    void Update()
    {
        if (curTimer > 0)
            curTimer -= Time.deltaTime;
    }
    public void Spawn(OrdealData ordealData)
    {
        entered = false;
        this.ordealData = ordealData;
        Appear(transform.position);
    }
    bool entered;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (entered)
            return;
        if (collision.CompareTag("Player"))
        {
            entered = true;
            // GameEventBus.Publish(new ReachedOrdealEvent(ordealData, transform.position));
            WayPointerCanvas.Instance.Remove(this);
        }
    }
}

// public class ReachedOrdealEvent
// {
//     public OrdealData ordealData;
//     public Vector2 pos;
//     public ReachedOrdealEvent(OrdealData ordealData, Vector2 pos)
//     {
//         this.ordealData = ordealData;
//         this.pos = pos;
//     }
// }
