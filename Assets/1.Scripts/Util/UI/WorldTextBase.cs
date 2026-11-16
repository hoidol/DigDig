using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

// 피해 숫자 종류: 약점(방어 타입 배율 > 1) / 내성(< 1) / 보통
public enum DamageTextType { Normal, Weak, Resist }

// 월드 글씨 연출 옵션 (피해 숫자용. 다른 글씨는 Default)
public struct WorldTextStyle
{
    public float size;          // 크기 배율 (피해량 비례 등)
    public bool crit;           // 치명타: 더 크게 + 흔들림 + "!"
    public DamageTextType type; // 약점·내성 표시

    public static WorldTextStyle Default => new WorldTextStyle { size = 1f };
}

public abstract class WorldTextBase<T> : MonoBehaviour where T : WorldTextBase<T>
{
    public TMP_Text damageText;

    static readonly Stack<T> pool = new();
    static T prefab;

    // 연출 수치 (피해 숫자)
    const float POP_OVERSHOOT = 1.4f;   // 처음 튈 때 최종 크기의 몇 배까지
    const float POP_UP_TIME = 0.08f;
    const float POP_SETTLE_TIME = 0.12f;
    const float FLASH_TIME = 0.15f;     // 흰색 → 원래 색
    const float CRIT_SIZE = 1.6f;
    const float WEAK_SIZE = 1.25f;
    const float RESIST_SIZE = 0.8f;

    Color baseColor;
    bool baseColorCached;
    Sequence seq;

    protected static void Show(Vector2 point, string text, Color color, string prefabPath)
        => Show(point, text, color, prefabPath, WorldTextStyle.Default);

    protected static void Show(Vector2 point, string text, Color color, string prefabPath, WorldTextStyle style)
    {
        T dText = Get(prefabPath);
        dText.transform.position = point + Random.insideUnitCircle * 0.2f;
        dText.Play(text, style);
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
        seq?.Kill();
        pool.Clear();
    }

    public virtual void SetText(string text, Color color) => Play(text, WorldTextStyle.Default);

    // 처음 0.1~0.2초에 크게 튀고(오버슛 + 흰색 번쩍임) 좌우로 살짝 튀었다 떨어지는 포물선 후 사라짐
    // 치명타 = 더 크게 + 흔들림 + "!", 약점 = 크게 + "약점!" + 약점 색, 내성 = 작게 + 회색 + 힘없이 아래로
    protected void Play(string text, WorldTextStyle style)
    {
        if (!baseColorCached)
        {
            baseColor = damageText.color; // 프리팹 글씨 색 (보통/치명타 색)
            baseColorCached = true;
        }
        seq?.Kill();
        transform.DOKill();
        damageText.DOKill();

        bool weak = style.type == DamageTextType.Weak;
        bool resist = style.type == DamageTextType.Resist;

        string body = style.crit ? text + "!" : text;
        damageText.text = weak ? $"<size=55%>{TranslateManager.GetText("DamageText_Weak")}</size>\n{body}" : body;

        Color target = baseColor;
        if (!style.crit && weak)
            target = ColorSetting.weakDamageColor;
        else if (resist)
            target = ColorSetting.resistDamageColor;

        float size = style.size > 0 ? style.size : 1f;
        if (style.crit) size *= CRIT_SIZE;
        if (weak) size *= WEAK_SIZE;
        if (resist) size *= RESIST_SIZE;

        transform.localScale = Vector3.zero;
        transform.rotation = Quaternion.identity;
        damageText.color = resist ? target : Color.white; // 내성은 번쩍이지 않음

        Vector3 start = transform.position;
        float side = Random.value < 0.5f ? -1f : 1f;
        Vector3 end;
        float jumpPower, moveTime, fadeDelay;
        if (resist)
        {
            end = start + new Vector3(side * Random.Range(0.1f, 0.3f), -0.35f, 0);
            jumpPower = 0.25f;
            moveTime = 0.6f;
            fadeDelay = 0.35f;
        }
        else
        {
            end = start + new Vector3(side * Random.Range(0.25f, 0.6f), Random.Range(0.3f, 0.6f), 0);
            jumpPower = style.crit ? 1.0f : 0.7f;
            moveTime = style.crit ? 0.85f : 0.75f;
            fadeDelay = style.crit ? 0.65f : 0.55f;
        }

        seq = DOTween.Sequence().SetLink(gameObject);
        seq.Insert(0f, transform.DOScale(size * POP_OVERSHOOT, POP_UP_TIME).SetEase(Ease.OutQuad));
        seq.Insert(POP_UP_TIME, transform.DOScale(size, POP_SETTLE_TIME).SetEase(Ease.InOutQuad));
        if (!resist)
            seq.Insert(0.05f, damageText.DOColor(target, FLASH_TIME));
        seq.Insert(0f, transform.DOJump(end, jumpPower, 1, moveTime).SetEase(Ease.Linear));
        if (style.crit)
            seq.Insert(0f, transform.DOPunchRotation(new Vector3(0, 0, 18f), 0.35f, 8));
        seq.Insert(fadeDelay, damageText.DOFade(0f, 0.25f));
        seq.OnComplete(Return);
    }
}
