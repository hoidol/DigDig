using UnityEngine;
using UnityEngine.InputSystem;

public class OreStone : MonoBehaviour, IHittable
{
    public const float SIZE = 1.46f;
    public Transform Transform
    {
        get
        {
            return transform;
        }
    }
    public Transform hpPoint;
    HpUI hpUI = null;
    public float curHp;
    public float maxHp;
    public int idx;
    public GameObject gold;
    bool isGoldStone;
    public void Init(int idx, Color color)
    {
        this.idx = idx;

        float distance = Vector2.Distance(Vector2.zero, transform.position);
        float disMulti = distance / 6;
        if (disMulti <= 1)
            disMulti = 1;
        this.maxHp = (8 + idx * 10) * disMulti;


        curHp = maxHp;
        hpUI = null;
        GetComponentInChildren<SpriteRenderer>().color = color;

        isGoldStone = Random.Range(0, 3) == 0;

        gold.SetActive(isGoldStone);
    }

    public void TakeDamage(DamageData damage)
    {
        curHp -= damage.damage;
        damage.Applyed(hpPoint.transform.position);


        if (hpUI == null || hpUI.hittable != this)
        {
            hpUI = HpUI.GetHpUI(this);
            hpUI.transform.position = hpPoint.position;
        }
        hpUI.SetRate(curHp / maxHp);
        if (curHp <= 0)
        {
            Destroyed();
        }
    }


    public void Destroyed()
    {
        gameObject.SetActive(false);
        ExpText expText = ExpText.Instantiate();
        expText.SetExpText(transform.position, (GameManager.Instance.underground - 1) * 4 + idx + 1);


        if (isGoldStone)
        {
            Gold.Dropped(transform.position, idx.ToString());
        }

        hpUI.Release();
        GameManager.Instance.AddDestroyOreStone();
        Player.Instance.AddExp(idx + 1);

        GameEventBus.Publish(new OreStoneDestroyedEvent(this));
    }
}

public class OreStoneDestroyedEvent
{
    public OreStone oreStone;
    public OreStoneDestroyedEvent(OreStone stone)
    {
        oreStone = stone;
    }
}