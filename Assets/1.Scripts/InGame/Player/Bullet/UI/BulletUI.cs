using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class BulletUI : MonoBehaviour
{
    public Image thum;
    RectTransform rt;

    void Awake() => rt = GetComponent<RectTransform>();

    public void SetBulletData(BulletData bulletData)
    {
        thum.sprite = bulletData.thumbnail;
    }

    public void Fired()
    {
        Vector2 startPos = rt.anchoredPosition;
        float startRot = rt.localEulerAngles.z;

        float dirX = Random.Range(25f, 55f) * (Random.value > 0.3f ? 1f : -1f);
        float peakY = Random.Range(40f, 70f);
        float spinDir = Random.value > 0.5f ? 1f : -1f;

        Sequence seq = DOTween.Sequence();
        seq.Insert(0f, rt.DOAnchorPosX(startPos.x + dirX, 0.5f).SetEase(Ease.OutQuad));
        seq.Insert(0f, rt.DOAnchorPosY(startPos.y + peakY, 0.18f).SetEase(Ease.OutQuad));
        seq.Insert(0.18f, rt.DOAnchorPosY(startPos.y - 30f, 0.32f).SetEase(Ease.InQuad));
        seq.Insert(0f, rt.DOLocalRotate(new Vector3(0, 0, startRot + spinDir * Random.Range(120f, 240f)), 0.5f));
        seq.Insert(0.2f, thum.DOFade(0f, 0.3f));
        seq.OnComplete(() =>
        {
            rt.anchoredPosition = startPos;
            rt.localEulerAngles = new Vector3(0, 0, startRot);
            thum.color = new Color(thum.color.r, thum.color.g, thum.color.b, 1f);
            gameObject.SetActive(false);
        });
    }
}
