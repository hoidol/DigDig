using System.Collections.Generic;
using UnityEngine;

// 지옥 슬라임의 지옥불 영역: from ~ to 두 점의 가운데에 놓는 직사각형
// - 크기·모양은 프리팹 그대로 (기본 = 가로로 넓은 모양). 스케일은 바꾸지 않음
// - 두 점이 세로로 놓여 있으면(세로 차이가 더 크면) rotation z = 90으로 세로 설치, 아니면 0
// - 피해 판정은 InfernoSlime이 GetEnemies로 그때그때 확인 → 영역을 벗어난 적은 바로 피해 대상에서 빠짐
// 프리팹: SpriteRenderer(자신 또는 자식)의 크기가 피해 영역 (가로 기준)
public class Hellfire : MonoBehaviour
{
    Vector2 center;
    Vector2 baseSize; // 회전 0(가로) 기준 월드 크기
    float angle;
    bool sizeReady;

    void InitSize()
    {
        if (sizeReady)
            return;
        sizeReady = true;

        Quaternion prevRot = transform.rotation;
        transform.rotation = Quaternion.identity;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>(true);
        baseSize = sr != null ? (Vector2)sr.bounds.size : (Vector2)transform.lossyScale;
        transform.rotation = prevRot;
    }

    public void Set(Vector2 from, Vector2 to)
    {
        InitSize();

        Vector2 diff = to - from;
        bool vertical = Mathf.Abs(diff.y) > Mathf.Abs(diff.x);
        center = (from + to) * 0.5f;
        angle = vertical ? 90f : 0f;

        transform.position = center;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public List<IHittable> GetEnemies(LayerMask layerMask)
    {
        List<IHittable> result = new List<IHittable>();
        foreach (Collider2D col in Physics2D.OverlapBoxAll(center, baseSize, angle, layerMask))
        {
            if (col.TryGetComponent(out IHittable h))
                result.Add(h);
        }
        return result;
    }
}
