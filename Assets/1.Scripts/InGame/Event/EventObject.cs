using System;
using UnityEngine;

public abstract class EventObject : MonoBehaviour, IWayPointerTarget
{
    public EventType eventType;
    [SerializeField] protected float clearRadius = 5f;
    public Transform Transform => transform;

    public Sprite Thum => thum;
    [SerializeField] Sprite thum;

    public float MaxTime => maxTime;
    [SerializeField] protected float maxTime;

    public float CurTimer => curTimer;
    [SerializeField] protected float curTimer;



    // NpcManager.Spawn() 호출 시 위치가 결정된 뒤 실행됨
    public virtual void Appear(Vector2 spawnPos)
    {
        curTimer = maxTime;
        Debug.Log("EventObject OnAppear");
        WayPointerCanvas.Instance.AddWayPoint(this);
        ClearArea(transform.position);
    }

    public virtual void Destroy()
    {
        EventManager.Instance?.RemoveEventObject(this);
        Destroy(gameObject);

        WayPointerCanvas.Instance.Remove(this);
    }
    void Update()
    {
        if (curTimer > 0)
            curTimer -= Time.deltaTime;
    }
    public void ClearArea(Vector2 pos)
    {
        MapManager.Instance.ClearTilesInRadius(pos, clearRadius);
    }

}
