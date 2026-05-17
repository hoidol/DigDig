using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class StatStone : MonoBehaviour, IHittable, IWayPointerTarget
{
    static readonly Stack<StatStone> pool = new();
    public static StatStone prefab;
    public static StatStone Get()
    {
        if (prefab == null)
        {
            prefab = Resources.Load<StatStone>("Prefabs/StatStone");
        }
        StatStone statUp = pool.Count > 0 ? pool.Pop() : Instantiate(prefab);
        statUp.gameObject.SetActive(true);
        return statUp;
    }


    public void Return()
    {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
        pool.Push(this);
    }

    public Transform Transform => transform;

    public Sprite Thum => statData.thum;

    public float MaxTime => 120;

    public float CurTimer => curTimer;
    public float curTimer;


    public float curHp;
    public float maxHp;
    public Transform maskTr;
    public TMP_Text statInfoText;
    public Transform damageTextPoint;
    StatData statData;
    public void Spawn(Vector2 pos, StatData statData, int lv)
    {
        this.statData = statData;
        destroying = false;
        transform.position = pos;
        float distance = Vector2.Distance(Vector2.zero, transform.position);
        float disMulti = distance / 4.5f;
        if (disMulti <= 1)
            disMulti = 1;

        this.maxHp = GameManager.Instance.stageData.oreHp * disMulti * 3;
        curHp = maxHp;
        maskTr.localScale = new Vector3(1, 0, 1);
        curTimer = MaxTime;
        statInfoText.text = statData.GetDescription(lv);

        Appear(pos);
    }

    void Update()
    {
        if (curTimer > 0)
            curTimer -= Time.deltaTime;
        else
            Destroy();
    }
    public bool CanHit()
    {
        return !destroying;
    }

    public void TakeDamage(DamageData damage)
    {
        if (!CanHit())
            return;
        damage.Applyed(damageTextPoint.position);
        curHp = Mathf.Max(0, curHp - damage.damage);

        //maskTr.localScale 체려 비율에 따라 0->1으로 줄어들게
        float hpRatio = (maxHp - curHp) / maxHp;
        maskTr.localScale = new Vector3(1, hpRatio, 1);
        if (curHp <= 0)
        {
            Destroy();
        }
    }
    bool destroying = false;
    public void Destroy()
    {
        destroying = true;
        Player.Instance.statInventory.AddStat(statData);

        WayPointerCanvas.Instance?.Remove(this);
        Return();
    }

    public void Appear(Vector2 spawnPos)
    {
        WayPointerCanvas.Instance.AddWayPoint(this);
        ClearArea(transform.position);
    }

    public void ClearArea(Vector2 pos)
    {
        MapManager.Instance.ClearTilesInRadius(pos, 1f);
    }

}
