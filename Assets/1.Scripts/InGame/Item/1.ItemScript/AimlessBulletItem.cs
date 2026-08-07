using UnityEngine;

// 오발탄 - 25% 확률로 랜덤 방향 추가 총알 발사
public class AimlessBulletItem : Item, IBullet
{
    float[] PROBS = {0.25f,0.3f,0.35f};

    bool isFiring;


    
    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return $"{PROBS[lv-1] * 100:0}% 확률로 오발탄 추가 발사";
    }

    public void OnBulletFired(CharacterBulletObject bullet)
    {
        if (isFiring) return;
        if (Random.value >= PROBS[count-1]) return;

        isFiring = true;
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Character.Instance.Shoot(new NormalBullet(), randomDir);
        isFiring = false;
    }
}
