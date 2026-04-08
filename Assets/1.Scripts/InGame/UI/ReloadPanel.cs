using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReloadPanel : MonoBehaviour
{
    public List<GameObject> bulletUIs;
    public HorizontalLayoutGroup horizontalLayoutGroup;
    public GameObject bulletUIPrefab;
    public TMP_Text bulletText;
    int maxCount;

    void Start()
    {
        GameEventBus.Subscribe<PlayerUpdateEvent>(OnPlayerUpdated);
        GameEventBus.Subscribe<BulletFiredEvent>(OnBulletFired);
        GameEventBus.Subscribe<BulletChargedEvent>(OnBulletCharged);
    }

    void OnPlayerUpdated(PlayerUpdateEvent e)
    {
        int newMax = e.player.statMgr.BulletCount;
        if (newMax != maxCount)
        {
            maxCount = newMax;

            while (bulletUIs.Count < maxCount)
            {
                var ui = Instantiate(bulletUIPrefab, bulletUIPrefab.transform.parent);
                bulletUIs.Add(ui);
            }
            for (int i = 0; i < bulletUIs.Count; i++)
                bulletUIs[i].SetActive(i < maxCount);

            horizontalLayoutGroup.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(horizontalLayoutGroup.GetComponent<RectTransform>());
            horizontalLayoutGroup.enabled = false;
        }

        bulletText.text = $"{e.player.curBulletCount}/{maxCount}";
    }

    void OnBulletFired(BulletFiredEvent e)
    {
        UpdateBulletUI(e.currentBulletCount, e.maxBulletCount);
    }

    void OnBulletCharged(BulletChargedEvent e)
    {
        UpdateBulletUI(e.currentBulletCount, e.maxBulletCount);
    }

    void UpdateBulletUI(int cur, int max)
    {
        for (int i = 0; i < bulletUIs.Count; i++)
            bulletUIs[i].SetActive(i < cur);

        bulletText.text = $"{cur}/{max}";
    }
}
