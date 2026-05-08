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


    protected virtual void OnDestroy()
    {
        EventManager.Instance?.RemoveEventObject(this);
        gameObject.SetActive(false);

        WayPointerCanvas.Instance.Remove(this);
    }

    // NpcManager.Spawn() 호출 시 위치가 결정된 뒤 실행됨
    public virtual void OnAppear(Vector2 spawnPos)
    {
        curTimer = maxTime;
        Debug.Log("EventObject OnAppear");
        WayPointerCanvas.Instance.AddWayPoint(this);
    }

    protected void ClearArea(Vector2 pos)
    {
        MapManager.Instance.ClearTilesInRadius(pos, clearRadius);
    }
}
