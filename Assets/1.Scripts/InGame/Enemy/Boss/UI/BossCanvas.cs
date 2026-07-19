using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class BossCanvas : CanvasUI<BossCanvas>
{
    public Image hpImage;
    Boss boss;
    public void SetBoss(Boss b)
    {
        OpenCanvas();
        boss = b;
        UpdateBoss();
    }

    void Update()
    {
        if (boss == null)
            return;
        UpdateBoss();
    }

    void UpdateBoss()
    {
        hpImage.fillAmount = boss.curHp / boss.maxHp;
    }
}