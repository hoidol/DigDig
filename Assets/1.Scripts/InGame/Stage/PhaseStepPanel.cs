using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PhaseStepPanel : MonoBehaviour
{
    static Color inactiveBgColor = new Color(0.4706f, 0.4706f, 0.4706f, 1f); //#767676 회색 
    static Color clearColor = new Color(0.0902f, 0.5686f, 0f, 1f); //#179100


    public Image bgImage;
    public Image innerImage;
    public bool isBoss;
    public int idx;
    bool isClear = false;
    public void Init(int idx)
    {
        this.idx = idx;
        isClear = false;
        if (innerImage != null)
        {
            innerImage.gameObject.SetActive(false);
            innerImage.color = Color.red;
        }

    }
    public void UpdatePanel(int curIdx)
    {

        if (innerImage != null && idx == curIdx)
        {
            innerImage.DOKill();

            innerImage.gameObject.SetActive(true);
            innerImage.color = isClear ? clearColor : Color.red;

            if (!isClear)
            {
                var seq = DOTween.Sequence().SetLoops(-1);
                seq.Append(innerImage.DOFade(0f, 0.35f).SetEase(Ease.InQuad));  // 꺼질때 느리게
                seq.Append(innerImage.DOFade(1f, 0.15f).SetEase(Ease.OutQuad)); // 켜질때 빠르게
            }

        }
        bgImage.color = idx == curIdx ? isBoss ? Color.red : Color.white : inactiveBgColor;
    }
    public void Clear()
    {
        innerImage.color = clearColor;
    }
}
