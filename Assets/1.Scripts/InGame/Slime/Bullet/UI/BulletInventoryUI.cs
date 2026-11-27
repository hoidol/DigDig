using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class BulletInventoryUI : MonoSingleton<BulletInventoryUI>
{
    [SerializeField] BulletUI bulletUIPrefab;

    readonly List<BulletUI> pool = new();
    [SerializeField] RectTransform parentTr;

    [SerializeField] List<BulletUI> bulletUIs = new List<BulletUI>();//현재 상태
    [SerializeField] RectTransform curBulletOrderIndicatorRectTr;

    void OnDestroy()
    {
        // GameEventBus.Unsubscribe<RemovedBulletEvent>(OnRemovedBulletEvent);
    }

    public void AddedBullet(string bulletKey)
    {
        UpdateContainer().Forget();
    }

    public void FiredBullet(string bulletKey, int shotOrder)
    {
        // if (Player.Instance.weapon.loadedBullets.Count > 0)
        // {
        //     // curBulletOrderIndicatorRectTr.gameObject.SetActive(true);
        //     BulletUI bUI = bulletUIs[Player.Instance.weapon.loadedBullets.Count - 1];
        //     curBulletOrderIndicatorRectTr.position =
        //     new Vector2(curBulletOrderIndicatorRectTr.position.x, bUI.rt.position.y);
        // }
        // else
        // {
        //     BulletUI bUI = bulletUIs[bulletUIs.Count - 1];
        //     curBulletOrderIndicatorRectTr.position =
        //     new Vector2(curBulletOrderIndicatorRectTr.position.x, bUI.rt.position.y);
        // }

    }
    public void EndReload()
    {
        UpdateContainer().Forget();
    }
    public void RemovedBullet(string bulletKey)
    {
        UpdateContainer().Forget();
    }
    BulletUI GetOrCreate()
    {
        BulletUI inactive = pool.Find(ui => !ui.gameObject.activeSelf);
        if (inactive != null)
        {
            inactive.gameObject.SetActive(true);
            inactive.transform.SetParent(parentTr);
            return inactive;
        }

        BulletUI ui = Instantiate(bulletUIPrefab);
        ui.transform.SetParent(parentTr, false);

        pool.Add(ui);
        return ui;
    }

    async UniTask UpdateContainer()
    {
        // List<string> curBullets = Player.Instance.weapon.bulletInventory.curBullets;

        // foreach (var ui in pool)
        // {
        //     ui.gameObject.SetActive(false);
        // }
        // bulletUIs.Clear();
        // for (int i = 0; i < curBullets.Count; i++)
        // {
        //     BulletUI bulletUI = GetOrCreate();
        //     bulletUI.SetBulletData(BulletManager.bullets[curBullets[i]].bulletData);
        //     bulletUIs.Add(bulletUI);
        // }

        await UniTask.Yield();
        // if (Player.Instance.weapon.loadedBullets.Count > 0)
        // {
        //     // curBulletOrderIndicatorRectTr.gameObject.SetActive(true);
        //     BulletUI bUI = bulletUIs[Player.Instance.weapon.loadedBullets.Count - 1];
        //     curBulletOrderIndicatorRectTr.position =
        //     new Vector2(curBulletOrderIndicatorRectTr.position.x, bUI.rt.position.y);
        // }
        // else
        // {
        //     BulletUI bUI = bulletUIs[bulletUIs.Count - 1];
        //     curBulletOrderIndicatorRectTr.position =
        //     new Vector2(curBulletOrderIndicatorRectTr.position.x, bUI.rt.position.y);
        // }
    }
}
