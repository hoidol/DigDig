using UnityEngine;

// 상태이상에 따라 색이 바뀌는 스프라이트 (적 몸통 등에 붙임)
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteForStatusEffect : MonoBehaviour {
    SpriteRenderer sr;
    Color originColor;

    void Awake()
    {
        Init();
    }

    // 비활성 상태라 Awake 전에 호출될 수 있어서 지연 초기화
    void Init()
    {
        if (sr != null)
            return;
        sr = GetComponent<SpriteRenderer>();
        originColor = sr.color;
    }

    public void SetTint(Color tint)
    {
        Init();
        sr.color = originColor * tint;
    }

    public void ResetTint()
    {
        Init();
        sr.color = originColor;
    }
}
