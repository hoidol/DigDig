using DG.Tweening;
using UnityEngine;

public class PanelPopupEffect : MonoBehaviour
{
    public RectTransform[] rectTrs;

    void OnEnable()
    {
        for (int i = 0; i < rectTrs.Length; i++)
        {
            RectTransform rTr = rectTrs[i];
            rTr.DOKill();
            rTr.localScale = new Vector3(0, 0, 0);
            rTr.DOScale(1.1f, 0.2f + i * 0.15f).SetUpdate(true).OnComplete(() =>
            {
                rTr.DOScale(1, 0.25f).SetUpdate(true);
            });
        }
    }
}
