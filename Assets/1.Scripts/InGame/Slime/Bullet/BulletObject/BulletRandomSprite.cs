using UnityEngine;

// 탄이 켜질 때마다 sprites 중 하나로 그림을 바꿈 (Joker 카드 탄: 스페이드·다이아·클로버·하트 중 랜덤)
public class BulletRandomSprite : MonoBehaviour
{
    public SpriteRenderer target;
    public Sprite[] sprites;

    void OnEnable()
    {
        if (target == null)
            target = GetComponent<SpriteRenderer>();
        if (target != null && sprites != null && sprites.Length > 0)
            target.sprite = sprites[Random.Range(0, sprites.Length)];
    }
}
