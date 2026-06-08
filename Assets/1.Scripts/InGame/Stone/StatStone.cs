using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class StatStone : EventObject, IHittable, IWayPointerTarget
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



    public override Sprite GetThum() => statData.thum;



    // public float curHp;
    // public float maxHp;
    public Transform maskTr;
    public Image groundGuageImage;
    // public TMP_Text statInfoText;
    // public Transform damageTextPoint;
    StatData statData;
    public void Spawn(Vector2 pos, StatData statData, int lv, Vector2Int index)
    {
        RegisterIndex(index);
        this.statData = statData;
        destroying = false;
        transform.position = pos;
        float distance = Vector2.Distance(Vector2.zero, transform.position);
        float disMulti = distance / 4.5f;
        if (disMulti <= 1)
            disMulti = 1;

        // this.maxHp = GameManager.Instance.stageData.oreHp * disMulti * 3;
        // curHp = maxHp;
        maskTr.localScale = new Vector3(1, 0, 1);
        groundGuageImage.fillAmount = 0;
        curTimer = MaxTime;
        // statInfoText.text = statData.GetDescription(lv);

        Appear(pos);
    }
    [SerializeField] float maxGauge = 10;
    [SerializeField] float curGauge = 0;

    void Update()
    {
        if (entered)
        {
            curGauge += Time.deltaTime;
            if (curGauge >= maxGauge)
            {
                curGauge = maxGauge;
                Destroy();
            }
        }
        else
        {
            if (curGauge > 0)
                curGauge -= Time.deltaTime * 2;
            else
            {
                curGauge = 0;
            }
        }
        float gaugePercent = curGauge / maxGauge;
        if (gaugePercent > 0)
        {
            maskTr.localScale = new Vector3(1, gaugePercent, 1);
            groundGuageImage.fillAmount = gaugePercent;
        }
        else
        {
            maskTr.localScale = new Vector3(1, 0, 1);
            groundGuageImage.fillAmount = 0;
        }


        if (curTimer > 0)
            curTimer -= Time.deltaTime;
        else
            Destroy();
    }
    public bool CanHit()
    {
        return false;
    }

    public void TakeDamage(DamageData damage)
    {
        if (!CanHit())
            return;

    }
    bool destroying = false;
    public override void Destroy()
    {
        destroying = true;
        Debug.Log("StatStone Destroy()");
        StatCanvas.Instance.OpenCanvas();
        WayPointerCanvas.Instance?.Remove(this);
        Return();
    }

    public override void Appear(Vector2 spawnPos)
    {
        WayPointerCanvas.Instance.AddWayPoint(this);
        ClearArea(spawnPos);
    }

    public override void ClearArea(Vector2 pos)
    {
        MapManager.Instance.ClearTilesInRadius(pos, 3f, 2);
    }
    bool entered = false;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("StatStone OnTiggerEnter2D entered ");
            entered = true;
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            entered = false;
            Debug.Log("StatStone OnTiggerExit2D exited ");
        }
    }

    public void ApplyStatusEffect(StatusEffect effect)
    {

    }

}
