using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Merchant : MonoBehaviour
{
    public MerchantView view;

    public Stall[] stalls;
    public float stayTime = 60; //유지 시간 
    public float stayTimer;
    bool isAppear;
    void Awake()
    {
        view = GetComponentInChildren<MerchantView>();
        stalls = GetComponentsInChildren<Stall>();
        GameEventBus.Subscribe<WaveEndEvent>(EndWave);
    }

    void EndWave(WaveEndEvent e)
    {
        if (e.waveData.merchantGrade == MerchantGrade.None)
            return;

        Appear(e.waveData);
    }

    void Update()
    {
        if (!isAppear)
            return;

        if (stayTimer <= 0)
        {
            Disappear();
            return;
        }

        view.SetTimer(stayTimer / stayTime);
        stayTimer -= Time.deltaTime;
    }

    void Appear(WaveData waveData)
    {
        isAppear = true;

        //플레이어와 중심점 방향으로 상점이 생김 - 뚫여있을 확률이 매우 높음 + 최종 경계를 벗어나지 않음
        Vector2 pos = (Vector2)Player.Instance.transform.position + (Vector2.zero - (Vector2)Player.Instance.transform.position).normalized * 3.5f;
        transform.position = pos;
        view.gameObject.SetActive(true);
        stayTimer = stayTime;

        List<ItemData> itemDatas = ItemManager.Instance.GetItems(MerchantGradeToGrade(waveData.merchantGrade), 4);
        for (int i = 0; i < stalls.Length; i++)
        {
            stalls[i].SetItemData(itemDatas[i]);
        }
        //상점이 열리는 곳에 방해되는 것이 있으면 다 파괴
        Collider2D[] cols = Physics2D.OverlapCircleAll(pos, 5, LayerMask.GetMask("Hittable"));
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].GetComponent<IHittable>().TakeDamage(999999);
        }

    }
    void Disappear()
    {
        isAppear = false;
        GameEventBus.Publish(new MerchantClosedEvent());
    }
    Grade MerchantGradeToGrade(MerchantGrade merchantGrade)
    {
        if (MerchantGrade.Normal == merchantGrade)
        {
            return Grade.Normal;
        }
        else if (MerchantGrade.Rare == merchantGrade)
        {
            return Grade.Rare;
        }
        if (MerchantGrade.Unique == merchantGrade)
        {
            return Grade.Unique;
        }
        return Grade.Normal;
    }
}
public class MerchantClosedEvent
{

}

public enum MerchantGrade
{
    None, //없음
    Normal, //노말 등급
    Rare, // 레어 등급
    Unique
}