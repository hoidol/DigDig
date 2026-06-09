using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class OreStone : MonoBehaviour, IHittable, IHpUI, ITile
{
    public Transform Transform => transform;

    static readonly Stack<OreStone> pool = new();

    public Vector2Int[,] IndexArr => indexArr;
    public Vector2Int[,] indexArr;
    public int Size => 1;

    public bool BreakTileWhenSpawn => false;


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
    public int level;
    // public Vector2Int gridPos;
    // public GameObject gold;
    // bool isGoldStone;
    public void Init(int level, Color color, Vector2Int[,] idxArr)//, Vector2Int gridPos
    {
        this.level = level;

        RegisterTile(idxArr);
        
        float distance = Vector2.Distance(Vector2.zero, transform.position);
        float disMulti = distance / 6f;
        if (disMulti <= 1)
            disMulti = 1;

        this.maxHp = GameManager.Instance.stageData.oreHp * disMulti;

        curHp = maxHp;
        hpUI = null;
        GetComponentInChildren<SpriteRenderer>().color = color;

        // isGoldStone = Random.Range(0, 3) == 0;

    }

    DamageData lastDamage;

    public void TakeDamage(DamageData damage)
    {
        lastDamage = damage;
        curHp -= damage.damage;
        damage.Applyed(hpPoint.transform.position);


        if (hpUI == null || !hpUI.IsOwn(this))
        {
            hpUI = HpUI.Get(this);
            hpUI.transform.position = hpPoint.position;
        }


        hpUI.UpdateTime();

        if (curHp <= 0)
        {
            Destroyed(true);
        }
    }

    int Exp => level + 1;



    public void Destroyed(bool reward)
    {
        ReleaseTile();

        if (reward)
        {
            ExpText.SetText(transform.position, Exp.ToString());
            Player.Instance.AddExp(Exp);

            // if (isGoldStone)
            //     Gold.Dropped(transform.position);

            EffectManager.Instance.Play(EffectType.OreStoneBreak, transform.position);
            GameEventBus.Publish(new DestroyedStoneEvent(this, lastDamage));
        }

        Return();
    }

    public bool CanHit()
    {
        return curHp > 0;
    }


    public void RegisterTile(Vector2Int[,] idxArr)
    {
        indexArr = idxArr;
    }

    public void ReleaseTile()
    {
        MapManager.ReleaseTile(indexArr);

    }
    StatusEffectHandler statusEffectHandler;
    public void ApplyStatusEffect(StatusEffect effect)
    {
        if (statusEffectHandler == null)
        {
            statusEffectHandler = gameObject.AddComponent<StatusEffectHandler>();
        }
        statusEffectHandler.Apply(effect);
    }

}

public class DestroyedStoneEvent
{
    public OreStone oreStone;
    public DamageData lastDamage;
    public DestroyedStoneEvent(OreStone stone, DamageData lastDamage)
    {
        oreStone = stone;
        this.lastDamage = lastDamage;
    }
}