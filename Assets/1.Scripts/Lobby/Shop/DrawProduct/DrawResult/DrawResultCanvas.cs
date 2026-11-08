using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Lobby
{
    // 뽑기 결과: 카드를 그리드(5열)에 깔고 순서대로 뒤집는다. revealShakeGrade 이상은 뒤집기 전에 흔들림
    // 닫기 버튼은 결과가 전부 공개된 뒤에 보인다
    public abstract class DrawResultCanvas<T> : CanvasUI<T>  where T : MonoBehaviour
{
    public Transform cardContainer;     // GridLayoutGroup
    public DrawResultCard cardTemplate; // 꺼 둔 카드 (복사해서 씀)
    public float revealInterval = 0.12f;
    public GradeType revealShakeGrade = GradeType.S;
    public GameObject closeButton;      // 전부 공개되기 전에는 숨김

    readonly List<DrawResultCard> cards = new List<DrawResultCard>();

    // 카드 앞면에 key 데이터 표시, 그 등급 반환
    protected abstract GradeType SetCardData(DrawResultCard card, string key);

    public virtual void OpenCanvas(string[] pickedKeys, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        cardTemplate.gameObject.SetActive(false);
        SetCloseButtonVisible(pickedKeys.Length == 0);

        float delay = 0f;
        for (int i = 0; i < pickedKeys.Length; i++)
        {
            DrawResultCard card = GetCard(i);
            card.gameObject.SetActive(true);
            bool shake = SetCardData(card, pickedKeys[i]) >= revealShakeGrade;
            card.Reveal(delay, shake, OnCardRevealed);
            delay += revealInterval + (shake ? DrawResultCard.SHAKE_DURATION : 0f);
        }
        for (int i = pickedKeys.Length; i < cards.Count; i++)
            cards[i].gameObject.SetActive(false);
    }

    DrawResultCard GetCard(int index)
    {
        while (cards.Count <= index)
            cards.Add(Instantiate(cardTemplate, cardContainer));
        return cards[index];
    }

    public bool IsRevealing => cards.Any(c => c.gameObject.activeSelf && !c.IsRevealed);

    void OnCardRevealed()
    {
        if (!IsRevealing)
            SetCloseButtonVisible(true);
    }

    void SetCloseButtonVisible(bool visible)
    {
        if (closeButton != null)
            closeButton.SetActive(visible);
    }

    public void OnClickedClose()
    {
        CloseCanvas();
    }
}
}
