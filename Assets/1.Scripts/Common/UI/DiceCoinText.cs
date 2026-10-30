using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class DiceCoinText : MonoBehaviour
{
    // 오브젝트 풀링을 Queue로 변경
    private static Queue<DiceCoinText> pool = new Queue<DiceCoinText>();

    public static new DiceCoinText Instantiate()
    {
        if (coinTextPrefab == null)
            coinTextPrefab = UnityEngine.Resources.Load<DiceCoinText>("UI/SlotCoinText");

        // 씬 전환 등으로 파괴된 오브젝트는 건너뜀 (static 풀이라 이전 씬 참조가 남을 수 있음)
        while (pool.Count > 0)
        {
            DiceCoinText gText = pool.Dequeue();
            if (gText == null)
                continue;

            gText.inPool = false;
            gText.gameObject.SetActive(true);
            return gText;
        }

        return UnityEngine.Object.Instantiate(coinTextPrefab);
    }

    bool inPool;
    public void Release()
    {
        if (inPool)
            return;

        inPool = true;
        transform.DOKill();
        gameObject.SetActive(false);
        pool.Enqueue(this);
    }

    public TMPro.TMP_Text coinText;
    public static List<DiceCoinText> list = new List<DiceCoinText>();
    static DiceCoinText coinTextPrefab;


    private void OnDestroy()
    {
        list.Remove(this);
    }

    public void SetDiceCoinText(UnityEngine.Vector3 point, string text)
    {
        transform.position = point;
        if (coinText != null)
            coinText.text = text;
        transform.localScale = UnityEngine.Vector3.zero;


        float randomSize = 1f;
        float moveUpAmount = UnityEngine.Random.Range(1.0f, 1.5f);
        // 이펙트 동작은 필요시 DOTween 등으로 추가 구현
        // DOTween을 이용해 이펙트 구현
        // 크기 커졌다가 (팝) 올라가면서 페이드아웃
        transform.localScale = UnityEngine.Vector3.zero;

        float duration = 1f;
        float popDuration = 0.08f;

        // DOTween 필요: using DG.Tweening;
        transform.DOKill(); // 재사용 시 이전 트윈 정리
        transform.DOScale(randomSize, popDuration).SetEase(Ease.OutBack);
        transform.DOMoveY(transform.position.y + 0.4f, duration).SetEase(Ease.OutSine).OnComplete(Release);

    }
}
