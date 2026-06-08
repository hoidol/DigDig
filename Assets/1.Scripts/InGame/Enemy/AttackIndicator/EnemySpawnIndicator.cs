using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemySpawnIndicator : AreaIndicator
{
    static readonly Stack<EnemySpawnIndicator> pool = new();

    public static EnemySpawnIndicator prefab;
    public static EnemySpawnIndicator Get(Vector3 pos, Transform parent)
    {
        if (prefab == null)
            prefab = Resources.Load<EnemySpawnIndicator>("UI/EnemySpawnIndicator");
        EnemySpawnIndicator obj = pool.Count > 0 ? pool.Pop() : Instantiate(prefab, parent);
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return()
    {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
        pool.Push(this);
    }

    public Transform warningTr;
    public override void PlayIndicator(float size, float sec, Action end)
    {
        Debug.Log("EnemySpawnIndicator PlayIndicator Start");
        gameObject.SetActive(true);
        warningTr.localScale = Vector2.zero;
        warningTr.DOScale(size, sec).SetEase(Ease.InCubic).OnComplete(() =>
        {
            Debug.Log("EnemySpawnIndicator PlayIndicator End");
            end.Invoke();
            StopIndicator();
        });
    }

    public override void StopIndicator()
    {
        warningTr.DOKill();
        Return();
    }
}