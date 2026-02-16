using UnityEngine;
using DG.Tweening;
public class UIPopupEffect : MonoBehaviour
{
     float sizeUpTimer = 0.3f;
     float sizeBackTimer =0.3f;
    RectTransform rectTransform;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    private void OnEnable()
    {
        rectTransform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        SoundMgr.Instance.PlaySound(SFXType.ViewOpen);
        rectTransform.DOScale(1.05f, sizeUpTimer).OnComplete(() =>
        {
            rectTransform.DOScale(1f, sizeBackTimer);
        });
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
