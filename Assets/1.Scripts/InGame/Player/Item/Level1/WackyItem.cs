using UnityEngine;

public class WackyItem : Item, IFired
{
    float[] PROBS = {0.2f,0.35f,0.5f};


    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"{PROBS[lv-1] * 100:0}% 확률로 랜덤 방향으로 탄 발사";
    }

    public void OnFired(Vector2 dir)
    {
         if (Random.value >= PROBS[count-1]) return;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Player.Instance.Shoot(new NormalBullet(), randomDir);
        Player.Instance.AddHp(-itemData.consumeHp);
    }
}