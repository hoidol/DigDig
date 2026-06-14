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

    [SerializeField] List<BulletUI> loadedBulletUIs = new List<BulletUI>();//현재 상태


    void OnDestroy()
    {
        // GameEventBus.Unsubscribe<RemovedBulletEvent>(OnRemovedBulletEvent);
    }

    public void AddedBullet(string bulletKey)
    {
        UpdateContainer();
    }

    public void FiredBullet(string bulletKey, int shotOrder)
    {

        loadedBulletUIs[loadedBulletUIs.Count - 1].transform.SetParent(transform, true);
        loadedBulletUIs[loadedBulletUIs.Count - 1].Fired();
        loadedBulletUIs.RemoveAt(loadedBulletUIs.Count - 1);
        UpdateContainer();
    }
    public void EndReload()
    {
        UpdateContainer();
    }
    public void RemovedBullet(string bulletKey)
    {
        UpdateContainer();
    }


    BulletUI GetOrCreate()
    {
        BulletUI inactive = pool.Find(ui => !ui.gameObject.activeSelf);
        if (inactive != null)
        {
            inactive.gameObject.SetActive(true);
            return inactive;
        }

        BulletUI ui = Instantiate(bulletUIPrefab);
        ui.transform.SetParent(parentTr, false);

        pool.Add(ui);
        return ui;
    }

    void UpdateContainer()
    {
        List<string> curLoadedBullets = Player.Instance.weapon.loadedBullets;

        foreach (var ui in pool)
        {
            ui.gameObject.SetActive(false);
        }
        loadedBulletUIs.Clear();
        for (int i = 0; i < curLoadedBullets.Count; i++)
        {
            BulletUI bulletUI = GetOrCreate();
            // Debug.Log($"curLoadedBullets[i] {curLoadedBullets[i]}");
            bulletUI.SetBulletData(BulletManager.bullets[curLoadedBullets[i]].bulletData);
            loadedBulletUIs.Add(bulletUI);
        }
    }
}
