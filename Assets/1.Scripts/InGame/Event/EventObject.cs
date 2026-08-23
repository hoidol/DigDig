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
    public virtual void Appear(Vector2 spawnPos)
    {
        curTimer = maxTime;
        indexArr = new Vector2Int[Size.x, Size.y];
        indexArr[0, 0] = MapManager.PositionToTileIndex(spawnPos);
        Debug.Log($"EventObject OnAppear x {indexArr[0, 0].x} y {indexArr[0, 0].y}");


        interacting = false;
        WayPointerCanvas.Instance.AddWayPoint(this);
        ClearArea(transform.position);
        RegisterTile(indexArr);
    }

    void Update()
    {
        if (interacting)
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
        gameObject.name = $"{eventType} {indexArr[0, 0].x} {indexArr[0, 0].y}";
        MapManager.RegisterTile(idxArr, this);

    }
    public virtual void Destroy()
    {
        EventManager.Instance?.RemoveEventObject(this);
        Destroy(gameObject);

        WayPointerCanvas.Instance.Remove(this);
        ReleaseTile();
    }

    public void ReleaseTile()
    {
        MapManager.ReleaseTile(indexArr);

    }

}
