using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class GoldText : MonoBehaviour
{
    // 오브젝트 풀링을 Queue로 변경
    private static Queue<GoldText> pool = new Queue<GoldText>();

    public static new GoldText Instantiate()
    {
        if (goldTextPrefab == null)
            goldTextPrefab = UnityEngine.Resources.Load<GoldText>("UI/GoldText");

        if (pool.Count > 0)
        {
            GoldText gText = pool.Dequeue();
            gText.gameObject.SetActive(true);
            return gText;
        }
        else
        {
            GoldText gText = UnityEngine.Object.Instantiate(goldTextPrefab);
            return gText;
        }
    }

    public void Release()
    {
        gameObject.SetActive(false);
        pool.Enqueue(this);
    }

    public TMPro.TMP_Text goldText;
    public SpriteRenderer goldRdr;
    public static List<GoldText> list = new List<GoldText>();
    static GoldText goldTextPrefab;


    private void OnDestroy()
    {
        list.Remove(this);
    }

    public void SetGoldText(UnityEngine.Vector3 point, string text)
    {
        transform.position = point;
        if (goldText != null)
            goldText.text = text;
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
        transform.DOScale(randomSize, popDuration).SetEase(Ease.OutBack);
        transform.DOMoveY(transform.position.y + 0.4f, duration).SetEase(Ease.OutSine).OnComplete(() =>
        {

            gameObject.SetActive(false);
        });

    }
}
