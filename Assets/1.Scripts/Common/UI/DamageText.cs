using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class DamageText : MonoBehaviour
{
    public TMP_Text damageText;
    public static List<DamageText> list = new List<DamageText>();
    static DamageText damageTextPrefab;
    public static DamageText Instantiate()
    {
        DamageText dText = GetDamageTextInPooling();
        return dText;
    }
    static DamageText GetDamageTextInPooling()
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].gameObject.activeSelf)
                continue;
            list[i].gameObject.SetActive(true);
            return list[i];
        }
        if (damageTextPrefab == null)
        {
            damageTextPrefab = Resources.Load<DamageText>("UI/DamageText");
        }

        DamageText dText = Instantiate(damageTextPrefab);
        list.Add(dText);
        return dText;
    }

    private void OnDestroy()
    {
        list.Remove(this);
    }
    public void SetDamageText(Vector3 point, string text)
    {

        transform.position = point;
        damageText.text = text;
        transform.localScale = Vector3.zero;

        damageText.alpha = 1; // 투명도 초기화

        float randomSize = 1f;//Vector3.one;// Random.Range(0.8f, 1f);
        float moveUpAmount = Random.Range(1.0f, 1.5f);
        //float sideMove = Random.Range(-0.5f, 0.5f); // 좌우 랜덤 이동
        transform.DOScale(randomSize, 0.1f).SetEase(Ease.OutBack);
        Sequence seq = DOTween.Sequence();
        // 2. 짧게 뒤로 갔다가 앞으로 튕기듯이 움직임
        // seq.Append(transform.DOMove(transform.position - Vector3.forward * 0.2f, 0.05f))
        //    .Append(transform.DOMove(transform.position + Vector3.forward * 0.2f, 0.1f));

        // 3. 부드럽게 위쪽으로 곡선을 그리며 이동
        seq.Append(transform.DOMove(transform.position + new Vector3(0, moveUpAmount, 0), 1.0f)
            .SetEase(Ease.InCubic));

        // 4. 서서히 투명해지면서 사라짐
        seq.Join(damageText.DOFade(0, 0.2f).SetDelay(0.6f));

        // 5. 애니메이션 끝나면 비활성화
        seq.OnComplete(() => gameObject.SetActive(false));

    }

}
