using System;
using DG.Tweening;
using UnityEngine;

namespace Lobby
{
    // 뽑기 결과 카드 한 장: 뒷면 → (높은 등급이면 흔들림) → 뒤집혀서 앞면
    // 그리드(GridLayoutGroup)가 위치·크기를 정하므로 연출은 회전·스케일로만
    public class DrawResultCard : MonoBehaviour
    {
        public const float SHAKE_DURATION = 0.5f;
        public const float FLIP_DURATION = 0.3f;

        public GameObject back;
        public GameObject front;

        Sequence sequence;
        public bool IsRevealed { get; private set; }

        public void ShowBack()
        {
            sequence?.Kill();
            IsRevealed = false;
            back.SetActive(true);
            front.SetActive(false);
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        // delay 뒤 뒤집기. shake면 뒤집기 전에 SHAKE_DURATION 동안 흔들림. 일시정지·배속과 무관하게 실시간
        // onRevealed: 앞면 공개가 끝나면 호출
        public void Reveal(float delay, bool shake, Action onRevealed = null)
        {
            ShowBack();
            sequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            sequence.AppendInterval(delay);
            if (shake)
                sequence.Append(transform.DOShakeRotation(SHAKE_DURATION, new Vector3(0, 0, 15), 25, 90, false));
            sequence.Append(transform.DOScaleX(0f, FLIP_DURATION / 2).SetEase(Ease.InQuad));
            sequence.AppendCallback(ShowFront);
            sequence.Append(transform.DOScaleX(1f, FLIP_DURATION / 2).SetEase(Ease.OutQuad));
            sequence.OnComplete(() =>
            {
                IsRevealed = true;
                onRevealed?.Invoke();
            });
        }

        public void RevealNow()
        {
            sequence?.Kill();
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            ShowFront();
            IsRevealed = true;
        }

        void ShowFront()
        {
            back.SetActive(false);
            front.SetActive(true);
        }
    }
}
