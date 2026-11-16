using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// 튜토리얼 포커스의 어두운 화면 (반전 마스크). 부모 = 구멍(Image + Mask, showMaskGraphic 꺼짐)
// Mask는 구멍 모양을 스텐실에 쓰고 자식에게 "스텐실 Equal(구멍 안만 그림)" 머티리얼을 준다
// → 이 이미지는 비교를 NotEqual로 바꾼 머티리얼을 써서 구멍 밖에만 그려진다 (구멍 모양 = 구멍 Image의 스프라이트 알파)
// 터치는 막지 않는다 (Raycast Target 끔). 터치 막기 = TutorialFocusBlocker
public class TutorialFocusDim : Image
{
    Material cutoutMaterial;
    Material cutoutSource;

    public override Material materialForRendering
    {
        get
        {
            Material source = base.materialForRendering; // Mask가 바꾼 머티리얼 (StencilMaterial 캐시라 같은 객체가 반복해서 옴)
            if (source != cutoutSource || cutoutMaterial == null)
            {
                DestroyCutout();
                cutoutSource = source;
                cutoutMaterial = new Material(source) { name = source.name + " (TutorialCutout)", hideFlags = HideFlags.HideAndDontSave };
                cutoutMaterial.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
            }
            return cutoutMaterial;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DestroyCutout();
    }

    void DestroyCutout()
    {
        if (cutoutMaterial == null)
            return;
        if (Application.isPlaying)
            Destroy(cutoutMaterial);
        else
            DestroyImmediate(cutoutMaterial);
        cutoutMaterial = null;
    }
}
