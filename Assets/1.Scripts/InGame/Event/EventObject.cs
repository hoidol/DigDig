using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventObject : MonoBehaviour, IWayPointerTarget, ITile
{
    public EventType eventType;
    [SerializeField] protected float clearRadius = 5f;
    public Transform Transform => transform;

    public virtual Sprite GetThum() => thum;
    [SerializeField] Sprite thum;

    public float MaxTime => maxTime;
    [SerializeField] protected float maxTime;

    public float CurTimer => curTimer;

    public Vector2Int[,] TileIndexArr => indexArr;

    [SerializeField] protected float curTimer;

    Vector2Int[,] indexArr;

    public bool BreakTileWhenSpawn => true;

    public Vector2Int Size => Vector2Int.one;


    public bool interacting;
    // NpcManager.Spawn() 호출 시 위치가 결정된 뒤 실행됨
    public virtual void Appear(Vector2 spawnPos)
    {
        curTimer = maxTime;
        Debug.Log("EventObject OnAppear");
        interacting= false;
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
        if(interacting)
        return;

        if (curTimer > 0)
            curTimer -= Time.deltaTime;
    }
    public virtual void ClearArea(Vector2 pos)
    {
        MapManager.Instance.ClearTilesInRadius(pos, clearRadius, clearRadius);
    }


    public void RegisterTile(Vector2Int[,] idxArr)
    {
        indexArr = idxArr;
        MapManager.RegisterTile(idxArr,this);

    }

    public void ReleaseTile()
    {
        MapManager.ReleaseTile(indexArr);

    }

}
