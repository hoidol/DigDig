using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class BossCanvas : CanvasUI<BossCanvas>
{
    protected override void PlayOpenSound() { } // 보스 등장 소리(BossAppear)가 따로 남
    public Image hpImage;
    Boss boss;
    public void SetBoss(Boss b)
    {
        OpenCanvas();
        boss = b;
        UpdateBoss();
    }

    public override void CloseCanvas()
    {
        boss = null;
        base.CloseCanvas();
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