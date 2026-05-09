using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Merchant : EventObject
{

    // public const float stayTime = 300; //유지 시간 
    // public float stayTimer;
    bool isAppear;

    public Image timerImage;
    // public GameObject body;
    public Grade maxGrade;
    public int count;



    void Update()
    {
        if (!isAppear)
            return;

        if (curTimer <= 0)
        {
            Disappear();
            return;
        }

        timerImage.fillAmount = curTimer / maxTime;
        curTimer -= Time.deltaTime;
    }

    public override void OnAppear(Vector2 spawnPos)
    {
        base.OnAppear(spawnPos);
        isAppear = true;
        //Vector2 pos = EventManager.Instance.CalcSpawnPosition();
        // transform.position = pos;

        ClearArea(transform.position);

    }
    public void Disappear()
    {
        isAppear = false;
        ItemStoreCanvas.Instance.CloseCanvas(transform);
        GameEventBus.Publish(new MerchantClosedEvent());
        OnDestroy();
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ItemStoreCanvas.Instance.OpenCanvas(transform, maxGrade, count, () =>
            {
                Player.Instance.UpdatePlayer();
            });
        }
    }

}
public class MerchantClosedEvent
{

}

