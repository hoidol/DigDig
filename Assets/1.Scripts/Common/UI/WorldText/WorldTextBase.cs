using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public abstract class WorldTextBase<T> : MonoBehaviour where T : WorldTextBase<T>
{
    public TMP_Text damageText;

    static readonly Stack<T> pool = new();
    static T prefab;

    protected static void Show(Vector2 point, string text, string prefabPath)
    {
        T dText = Get(prefabPath);
        dText.transform.position = point + Random.insideUnitCircle * 0.2f;
        dText.SetText(text);
    }

    static T Get(string prefabPath)
    {
        if (pool.Count > 0)
        {
            T dText = pool.Pop();
            dText.gameObject.SetActive(true);
            return dText;
        }

        if (prefab == null)
            prefab = Resources.Load<T>(prefabPath);

        return Instantiate(prefab);
    }

    protected void Return()
    {
        gameObject.SetActive(false);
        pool.Push((T)this);
    }

    private void OnDestroy()
    {
        pool.Clear();
    }

    public virtual void SetText(string text)
    {
        damageText.text = text;
        transform.localScale = Vector3.zero;
        damageText.alpha = 1;

        float moveUpAmount = Random.Range(1.0f, 1.5f);

        transform.DOScale(1f, 0.1f).SetEase(Ease.OutBack);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(transform.position + new Vector3(0, moveUpAmount, 0), 1.0f)
            .SetEase(Ease.InCubic));
        seq.Join(damageText.DOFade(0, 0.2f).SetDelay(0.6f));
        seq.OnComplete(Return);
    }
}
