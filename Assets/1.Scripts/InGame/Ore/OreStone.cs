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
    public void Init(int idx, Color color)
    {
        this.idx = idx;
        this.maxHp = 8 + idx * 10;
        curHp = maxHp;
        hpUI = null;
        GetComponentInChildren<SpriteRenderer>().color = color;
    }

    public void TakeDamage(float damage)
    {
        curHp -= damage;
        DamageText damageText = DamageText.Instantiate();
        damageText.SetDamageText(hpPoint.transform.position, damage.ToString());
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
        Destroy(gameObject);
        Ore ore = OreManager.Instance.GetOre();
        ore.Droped(transform.position, idx.ToString());
        ore.transform.position = transform.position;
        hpUI.Release();
        GameManager.Instance.AddDestroyOreStone();
    }
}
