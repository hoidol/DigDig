using UnityEngine;

//20프로 확률로 랜덤 방향으로 추가 발사
public class WackyItem : Item, IFired
{
    float PROBS = 0.2f;


    public override string GetDescription()
    {
        return $"{PROBS * 100:0}% 확률로 랜덤 방향으로 탄 발사";
    }

    public void OnFired(ref Bullet bullet, ref CharacterBulletObject bulletObject, Vector2 dir)
    {
         if (Random.value >= PROBS *count) return;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Character.Instance.Shoot(new NormalBullet(), randomDir);
        Character.Instance.AddHp(-itemData.consumeHp);
    }
}