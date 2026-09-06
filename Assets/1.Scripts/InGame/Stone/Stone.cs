using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Stone : MonoBehaviour, IHittable, ITile
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
    public Transform spriteRootTr;

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
    public TMP_Text hpText;
    public float curHp;
    public float maxHp;

    public float MaxHp => maxHp;
    public float CurHp => curHp;
    public int level;

    public SpriteRenderer innerSpriteRdr;
    public virtual void Init(int level, Color color, Vector2Int[,] idxArr)//, Vector2Int gridPos
    {
        this.level = level;

        RegisterTile(idxArr);

#if UNITY_EDITOR
        tileIndex = idxArr[0, 0];
#endif
        spriteRootTr.localRotation = Quaternion.identity;
        float distance = Vector2.Distance(Vector2.zero, transform.position);
        float disMulti = distance / 7f;
        if (disMulti <= 1)
            disMulti = 1;
        float defaultHp = GameManager.Instance.stageData.oreHp * disMulti;
        this.maxHp = defaultHp + defaultHp * level * 0.5f;

        curHp = maxHp;

        hpText.text = ((int)curHp).ToString();
        innerSpriteRdr.color = color;
    }

    DamageData lastDamage;

    public virtual void TakeDamage(DamageData damage)
    {
        lastDamage = damage;
        Debug.Log($"Stone TakeDamage {damage.damage}");
        curHp -= damage.ApplyDamage(hpPoint.transform.position);


        spriteRootTr.DOKill();
        spriteRootTr.DOShakeRotation(0.2f, new Vector3(0f, 0f, 5f), 100, 90f, false).OnComplete(() =>
        {
            spriteRootTr.localRotation = Quaternion.identity;
        });

        if (curHp > 1)
            hpText.text = ((int)curHp).ToString();
        else if (0 < curHp && curHp <= 1)
            hpText.text = "1";

        if (curHp <= 0)
        {
            Reward();
            Destroy();
        }
    }

    int exp => 2 + (int)(level * 1.5f);


    public void Reward()
    {
        HealPiece.Instantiate(transform.position);
        //Exp.Instantiate(transform.position, exp, 1);
        Coin.Instantiate(transform.position, level + 1, 0.5f);
    }

    public virtual void Destroy()
    {
        Debug.Log("Stone 어디서 불리냐");
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
        // MapManager.RegisterTile(tileIndexArr, this);
    }

    public virtual void ReleaseTile()
    {
        // MapManager.ReleaseTile(tileIndexArr);

        if (!gameObject.activeSelf) return;
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
    public DestroyedStoneEvent(Stone s, DamageData lastDamage)
    {
        this.stone = s;
        this.lastDamage = lastDamage;
    }
}