using UnityEngine;

// 상태이상에 따라 색이 바뀌는 스프라이트 (적 몸통 등에 붙임)
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteForStatusEffect : MonoBehaviour
{
    SpriteRenderer[] srs;
    Color originColor;

    void Awake()
    {
        Init();
    }

    // 비활성 상태라 Awake 전에 호출될 수 있어서 지연 초기화
    void Init()
    {
        if (srs != null)
            return;
        srs = GetComponentsInChildren<SpriteRenderer>();
        originColor = srs[0].color;
    }

    public void SetTint(Color tint)
    {
        Init();
        for (int i = 0; i < srs.Length; i++)
        {
            srs[i].color = originColor * tint;
        }
    }

    public void ResetTint()
    {
        Init();
        for (int i = 0; i < srs.Length; i++)
        {
            srs[i].color = originColor;
        }
    }
}
