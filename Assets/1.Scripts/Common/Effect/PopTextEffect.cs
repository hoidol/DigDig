using DG.Tweening;
using TMPro;
using UnityEngine;

// 이펙트 프리팹 안의 글씨(TextMeshPro, 월드)를 켜질 때마다 귀엽게 튀어나오게 함 (머지 "레벨업!" 등)
// 0 → 크게 튀어나옴(OutBack) → 살짝 출렁 + 위로 둥실 → 사라짐
public class PopTextEffect : MonoBehaviour
{
    public TMP_Text text;
    [Tooltip("글씨와 같이 사라질 그림 (Joker 카드 그림 등). 글씨 자식으로 두면 같이 튀어나옴")]
    public SpriteRenderer[] fadeSprites;
    [Tooltip("튀어나오는 시간")] public float popDuration = 0.25f;
    [Tooltip("최대 크기 배율 (원래 크기 기준)")] public float popScale = 1.25f;
    [Tooltip("위로 떠오르는 거리")] public float floatHeight = 0.5f;
    [Tooltip("떠오르는 시간 (튀어나온 뒤)")] public float floatDuration = 0.6f;
    [Tooltip("사라지는 시간 (떠오르기 끝부분과 겹침)")] public float fadeDuration = 0.25f;
    [Tooltip("좌우로 살짝 기울었다 돌아오는 각도")] public float wobbleAngle = 8f;

    Vector3 baseScale;
    Vector3 baseLocalPos;
    Sequence seq;

    void Awake()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();
        baseScale = transform.localScale;
        baseLocalPos = transform.localPosition;
    }

    void OnEnable()
    {
        Play();
    }

    void OnDisable()
    {
        seq?.Kill();
        seq = null;
    }

    public void Play()
    {
        seq?.Kill();
        transform.localScale = Vector3.zero;
        transform.localPosition = baseLocalPos;
        transform.localRotation = Quaternion.identity;
        if (text != null)
            text.alpha = 1f;
        SetSpriteAlpha(1f);

        seq = DOTween.Sequence().SetLink(gameObject);
        seq.Append(transform.DOScale(baseScale * popScale, popDuration).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(baseScale, 0.12f).SetEase(Ease.OutQuad));
        seq.Join(transform.DOPunchRotation(new Vector3(0, 0, wobbleAngle), 0.3f, 6, 0.5f));
        seq.Insert(popDuration, transform.DOLocalMoveY(baseLocalPos.y + floatHeight, floatDuration).SetEase(Ease.OutSine));
        if (text != null)
            seq.Insert(popDuration + floatDuration - fadeDuration, DOTween.To(() => text.alpha, a => text.alpha = a, 0f, fadeDuration));
        if (fadeSprites != null && fadeSprites.Length > 0)
            seq.Insert(popDuration + floatDuration - fadeDuration, DOTween.To(() => 1f, SetSpriteAlpha, 0f, fadeDuration));
    }

    void SetSpriteAlpha(float a)
    {
        if (fadeSprites == null)
            return;
        foreach (SpriteRenderer sr in fadeSprites)
        {
            if (sr == null)
                continue;
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }
    }
}
