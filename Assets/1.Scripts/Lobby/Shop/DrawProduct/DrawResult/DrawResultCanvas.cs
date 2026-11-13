using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Lobby
{
    // 뽑기 결과: 카드를 그리드(5열)에 깔고 순서대로 뒤집는다. revealShakeGrade 이상은 뒤집기 전에 흔들림
    // 결과가 전부 공개되면 touchToCloseText가 보이고, 화면을 누르면 닫힌다
    public abstract class DrawResultCanvas<T> : CanvasUI<T>  where T : MonoBehaviour
{
    public Transform cardContainer;     // GridLayoutGroup
    public DrawResultCard cardTemplate; // 꺼 둔 카드 (복사해서 씀)
    public float revealInterval = 0.12f;
    public GradeType revealShakeGrade = GradeType.S;
    public GameObject touchToCloseText;      // 전부 공개되기 전에는 숨김

    readonly List<DrawResultCard> cards = new List<DrawResultCard>();

    // 카드 앞면에 key 데이터 표시, 그 등급 반환
    protected abstract GradeType SetCardData(DrawResultCard card, string key);
    // 결과를 낸 상자 종류에 맞는 것만 보임
    public GameObject normalChest;
    public GameObject uniqueChest;

    public virtual void OpenCanvas(string[] pickedKeys, bool isUniqueChest, Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        if (normalChest != null)
            normalChest.SetActive(!isUniqueChest);
        if (uniqueChest != null)
            uniqueChest.SetActive(isUniqueChest);
        cardTemplate.gameObject.SetActive(false);
        SetCanClose(pickedKeys.Length == 0);

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

    // 전부 공개된 뒤 화면 아무 곳이나 누르면 닫힘
    public bool CanClose { get; private set; }
    int canCloseFrame;

    void OnCardRevealed()
    {
        if (!IsRevealing)
            SetCanClose(true);
    }

    void SetCanClose(bool canClose)
    {
        CanClose = canClose;
        canCloseFrame = Time.frameCount;
        if (touchToCloseText != null)
            touchToCloseText.SetActive(canClose);
    }

    void Update()
    {
        // 공개가 끝난 프레임의 입력은 무시 (마지막 카드 공개와 동시에 닫히지 않게)
        if (CanClose && Time.frameCount > canCloseFrame && Input.GetMouseButtonDown(0))
            CloseCanvas();
    }

    // 배경 버튼 등에 연결돼 있어도 공개가 끝나기 전에는 닫지 않는다
    public void OnClickedClose()
    {
        if (!CanClose)
            return;
        CloseCanvas();
    }
}
}
