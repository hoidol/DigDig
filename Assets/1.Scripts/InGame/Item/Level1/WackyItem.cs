using UnityEngine;

//35프로 확률로 랜덤 방향으로 추가 발사
public class WackyItem : Item, IFired
{
    float PROBS = 0.5f;


    public override string GetDescription()
    {
        return $"{PROBS * 100:0}% 확률로 랜덤 방향으로 탄 발사";
        // return string.Format(TranslateManager.GetText("{key}_Desc"),$"{PROBS * 100:0}");
    }

    public void OnFired(ref BulletSpec bullet, ref AllyBulletObject bulletObject, Vector2 dir)
    {
        for (int i = 0; i < count; i++)
        {
            if (Random.value >= PROBS)
                continue;
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Character.Instance.Shoot(new NormalBulletSpec(), randomDir);
        }
        // Character.Instance.AddHp(-itemData.consumeHp);
    }
}