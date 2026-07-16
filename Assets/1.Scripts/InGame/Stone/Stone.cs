using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Stone : MonoBehaviour, IHittable, IHpUI, ITile
{
    public Transform Transform => transform;

    static readonly Stack<Stone> pool = new();

    public Vector2Int[,] TileIndexArr => tileIndexArr;
    public Vector2Int[,] tileIndexArr;
    public Vector2Int Size => Vector2Int.one;

#if UNITY_EDITOR
    [SerializeField] Vector2Int tileIndex;
#endif
    public bool BreakTileWhenSpawn => false;

    static Stone stonePrefab;
    public static Stone Get(Vector3 pos, Transform parent)
    {
        if (stonePrefab == null)
            stonePrefab = Resources.Load<Stone>("Prefabs/Stone");
        Stone ore = pool.Count > 0 ? pool.Pop() : Instantiate(stonePrefab, parent);
        ore.transform.position = pos;
        ore.gameObject.SetActive(true);
        return ore;
    }


    public Transform hpPoint;
    HpUI hpUI = null;
    public float curHp;
    public float maxHp;

    public float MaxHp => maxHp;
    public float CurHp => curHp;
    Vector3 IHpUI.HpUIPosition => hpPoint.position;
    public int level;
    // public Vector2Int gridPos;
    // public GameObject gold;
    // bool isGoldStone;
    public virtual void Init(int level, Color color, Vector2Int[,] idxArr)//, Vector2Int gridPos
    {
        this.level = level;

        RegisterTile(idxArr);

#if UNITY_EDITOR
        tileIndex = idxArr[0, 0];
#endif

        float distance = Vector2.Distance(Vector2.zero, transform.position);
        float disMulti = distance / 10f;
        if (disMulti <= 1)
            disMulti = 1;

        this.maxHp = GameManager.Instance.stageData.oreHp * disMulti;

        curHp = maxHp;
        hpUI = null;

        GetComponentInChildren<SpriteRenderer>().color = color;
    }

    DamageData lastDamage;

    public virtual void TakeDamage(DamageData damage)
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
            Reward();
            OnDestroy();
        }
    }

    int exp => 2 + (int)(level * 1.5f);


    public void Reward()
    {
        Exp.Instantiate(transform.position, exp, 1);
    }

    public virtual void OnDestroy()
    {
        EffectManager.Instance.Play(EffectType.StoneBreak, transform.position);
        GameEventBus.Publish(new DestroyedStoneEvent(this, lastDamage));
        ReleaseTile();
    }

    public virtual bool CanHit()
    {
        return curHp > 0;
    }


    public virtual void RegisterTile(Vector2Int[,] idxArr)
    {
        tileIndexArr = idxArr;
        MapManager.RegisterTile(tileIndexArr, this);
    }

    public virtual void ReleaseTile()
    {
        MapManager.ReleaseTile(tileIndexArr);

        if (!gameObject.activeSelf) return;
        hpUI?.Release();
        hpUI = null;
        gameObject.SetActive(false);
        pool.Push(this);
    }

    StatusEffectHandler statusEffectHandler;
    public virtual void ApplyStatusEffect(StatusEffect effect)
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
    public Stone stone;
    public DamageData lastDamage;
    public DestroyedStoneEvent(Stone stone, DamageData lastDamage)
    {
        stone = stone;
        this.lastDamage = lastDamage;
    }
}