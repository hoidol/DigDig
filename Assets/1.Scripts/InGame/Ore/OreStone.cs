using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OreStone : MonoBehaviour, IHittable, IHpUI
{
    public const float SIZE = 1.46f;
    public Transform Transform => transform;

    static readonly Stack<OreStone> pool = new();

    public static OreStone Get(OreStone prefab, Vector3 pos, Transform parent)
    {
        OreStone ore = pool.Count > 0 ? pool.Pop() : Instantiate(prefab, parent);
        ore.transform.SetParent(parent);
        ore.transform.position = pos;
        ore.gameObject.SetActive(true);
        return ore;
    }

    public void Return()
    {
        if (!gameObject.activeSelf) return;
        hpUI?.Release();
        hpUI = null;
        gameObject.SetActive(false);
        pool.Push(this);
    }

    public Transform hpPoint;
    HpUI hpUI = null;
    public float curHp;
    public float maxHp;

    float IHpUI.MaxHp => maxHp;
    float IHpUI.CurHp => curHp;
    Vector3 IHpUI.HpUIPosition => hpPoint.position;
    public int idx;
    public Vector2Int gridPos;
    public GameObject gold;
    bool isGoldStone;
    public void Init(int idx, Color color, Vector2Int gridPos)
    {
        this.idx = idx;
        this.gridPos = gridPos;

        float distance = Vector2.Distance(Vector2.zero, transform.position);
        float disMulti = distance / 6;
        if (disMulti <= 1)
            disMulti = 1;

        this.maxHp = GameManager.Instance.stageData.GetOrdealProgressData().oreHp * disMulti;

        curHp = maxHp;
        hpUI = null;
        GetComponentInChildren<SpriteRenderer>().color = color;

        isGoldStone = Random.Range(0, 3) == 0;

        gold.SetActive(isGoldStone);
    }

    DamageData lastDamage;

    public void TakeDamage(DamageData damage)
    {
        lastDamage = damage;
        curHp -= damage.damage;
        damage.Applyed(hpPoint.transform.position);


        if (hpUI == null || !hpUI.IsOwn(this))
            hpUI = HpUI.Get(this);
        hpUI.UpdateTime();

        if (curHp <= 0)
        {
            Destroyed(true);
        }
    }

    int Exp => (GameManager.Instance.ordealClearCount - 1) * 4 + idx + 1;
    public void Destroyed(bool reward)
    {
        MapManager.Instance.RegisterDestroyed(gridPos);

        if (reward)
        {
            ExpText.SetText(transform.position, Exp.ToString());
            if (isGoldStone)
                Gold.Dropped(transform.position, idx.ToString());
            GameManager.Instance.AddDestroyOreStone();
            EffectManager.Instance.Play(EffectType.OreStoneBreak, transform.position);
            Player.Instance.AddExp(idx + 1);
            GameEventBus.Publish(new OreStoneDestroyedEvent(this, lastDamage));
        }

        Return();
    }

    public bool CanHit()
    {
        return curHp > 0;
    }
}

public class OreStoneDestroyedEvent
{
    public OreStone oreStone;
    public DamageData lastDamage;
    public OreStoneDestroyedEvent(OreStone stone, DamageData lastDamage)
    {
        oreStone = stone;
        this.lastDamage = lastDamage;
    }
}