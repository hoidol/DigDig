using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class ExpText : MonoBehaviour
{
    public TMP_Text expText;
    public static List<ExpText> list = new List<ExpText>();
    static ExpText expTextPrefab;
    public static ExpText Instantiate()
    {
        ExpText dText = GetExpTextInPooling();
        return dText;
    }
    static ExpText GetExpTextInPooling()
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].gameObject.activeSelf)
                continue;
            list[i].gameObject.SetActive(true);
            return list[i];
        }
        if (expTextPrefab == null)
        {
            expTextPrefab = Resources.Load<ExpText>("UI/ExpText");
        }

        ExpText dText = Instantiate(expTextPrefab);
        list.Add(dText);
        return dText;
    }

    private void OnDestroy()
    {
        list.Remove(this);
    }
    public void SetExpText(Vector3 point, int exp)
    {

        transform.position = point;
        expText.text = $"exp+{exp}";
        transform.localScale = Vector3.zero;

        expText.alpha = 1;

        float randomSize = 1f;//Vector3.one;// Random.Range(0.8f, 1f);
        float moveUpAmount = Random.Range(1.0f, 1.5f);
        //float sideMove = Random.Range(-0.5f, 0.5f); // 좌우 랜덤 이동
        transform.DOScale(randomSize, 0.1f).SetEase(Ease.OutBack);
        Sequence seq = DOTween.Sequence();

        // 3. 부드럽게 위쪽으로 곡선을 그리며 이동
        seq.Append(transform.DOMove(transform.position + new Vector3(0, moveUpAmount, 0), 1.0f)
            .SetEase(Ease.InCubic));

        // 4. 서서히 투명해지면서 사라짐
        seq.Join(expText.DOFade(0, 0.2f).SetDelay(0.6f));

        // 5. 애니메이션 끝나면 비활성화
        seq.OnComplete(() => gameObject.SetActive(false));

    }

}
