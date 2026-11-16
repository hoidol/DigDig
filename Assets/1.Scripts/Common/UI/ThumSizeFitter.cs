using UnityEngine;
using UnityEngine.UI;

// 썸네일 이미지 크기 맞추기: SetNativeSize로 스프라이트 원래 비율을 살린 뒤, 칸(box) 안에 꽉 차게 비율 유지로 키우거나 줄임
// 미니언 이미지는 Multiple로 잘린 조각마다 크기가 달라서 SetNativeSize만 하면 칸마다 크기가 들쭉날쭉 → 칸 크기에 맞춤
// box: 비워 두면(0) 처음 맞출 때의 RectTransform 크기를 기준으로 기억 (인스펙터에서 직접 정해도 됨)
[RequireComponent(typeof(Image))]
public class ThumSizeFitter : MonoBehaviour
{
    public Vector2 box;

    public void Fit(Image image)
    {
        RectTransform rect = image.rectTransform;
        if (box.x <= 0 || box.y <= 0)
            box = rect.rect.size;

        image.SetNativeSize(); // anchor를 한 점으로 모으고 sizeDelta = 스프라이트 원래 크기
        Vector2 native = rect.sizeDelta;
        if (native.x <= 0 || native.y <= 0 || box.x <= 0 || box.y <= 0)
            return;
        float scale = Mathf.Min(box.x / native.x, box.y / native.y);
        rect.sizeDelta = native * scale;
    }
}

public static class ThumImageExtensions
{
    // 썸네일 스프라이트 설정 + SetNativeSize 후 칸 크기에 맞춤 (ThumSizeFitter가 없으면 붙여서 처음 크기를 칸으로 기억)
    public static void SetThum(this Image image, Sprite sprite)
    {
        if (image == null)
            return;
        image.sprite = sprite;
        if (sprite == null)
            return;
        if (!image.TryGetComponent(out ThumSizeFitter fitter))
            fitter = image.gameObject.AddComponent<ThumSizeFitter>();
        fitter.Fit(image);
    }
}
