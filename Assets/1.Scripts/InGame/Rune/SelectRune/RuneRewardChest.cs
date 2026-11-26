using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// (2026-11-23) 엘리트·보스 처치 자리에 떨어지는 보상 상자 (그림 = SelectRuneCanvas.rewardChestSprite)
// 처치 → 위에서 통통 떨어져 그 자리에 놓임 → 브레이크타임에 룬 보상 창이 열릴 때 하나씩 튀어 오르며 사라짐
// → 창에도 같은 상자 그림이 보여서 "이 상자 = 룬 선택"이 이어짐. 충돌·입력 없음(보기만)
public class RuneRewardChest : MonoBehaviour
{
    const float SCALE = 0.6f;
    const float DROP_HEIGHT = 1.2f;
    const float DROP_TIME = 0.45f;

    static readonly List<RuneRewardChest> chests = new();
    public static int Count => chests.Count;

    SpriteRenderer sr;

    public static void Spawn(Vector3 pos)
    {
        Sprite sprite = SelectRuneCanvas.Instance != null ? SelectRuneCanvas.Instance.rewardChestSprite : null;
        if (sprite == null)
            return;

        GameObject go = new GameObject("RuneRewardChest");
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        RuneRewardChest chest = go.AddComponent<RuneRewardChest>();
        chest.sr = go.AddComponent<SpriteRenderer>();
        chest.sr.sprite = sprite;
        chest.sr.sortingLayerName = "UnderDefault"; // 바닥에 놓인 느낌: 유닛·적보다 아래, 타일보다 위
        chest.sr.sortingOrder = 50;
        chests.Add(chest);
        chest.PlayDrop();
    }

    // 위에서 떨어져 착지 → 납작했다 통통 돌아옴 → 살짝 둥실 대기
    void PlayDrop()
    {
        Transform body = transform;
        Vector3 landPos = body.position;
        body.position = landPos + Vector3.up * DROP_HEIGHT;
        body.localScale = Vector3.one * SCALE * 0.3f;

        DOTween.Sequence()
            .Append(body.DOMove(landPos, DROP_TIME).SetEase(Ease.InQuad))
            .Join(body.DOScale(SCALE, DROP_TIME * 0.6f).SetEase(Ease.OutBack))
            .Append(body.DOScale(new Vector3(SCALE * 1.25f, SCALE * 0.75f, 1f), 0.08f))
            .Append(body.DOScale(SCALE, 0.25f).SetEase(Ease.OutBack))
            .Append(body.DOMoveY(landPos.y + 0.08f, 0.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo))
            .SetLink(gameObject);
    }

    // 룬 보상 창이 열릴 때 가장 먼저 떨어진 상자 하나를 치움 (튀어 올랐다 사라짐)
    public static void ConsumeOne()
    {
        while (chests.Count > 0 && chests[0] == null)
            chests.RemoveAt(0);
        if (chests.Count == 0)
            return;
        RuneRewardChest chest = chests[0];
        chests.RemoveAt(0);
        chest.PlayConsume();
    }

    void PlayConsume()
    {
        transform.DOKill();
        DOTween.Sequence()
            .Append(transform.DOMoveY(transform.position.y + 0.4f, 0.2f).SetEase(Ease.OutQuad))
            .Join(transform.DOScale(SCALE * 1.3f, 0.2f).SetEase(Ease.OutQuad))
            .Append(transform.DOScale(0f, 0.15f).SetEase(Ease.InBack))
            .Join(sr.DOFade(0f, 0.15f))
            .OnComplete(() => Destroy(gameObject))
            .SetLink(gameObject);
    }

    public static void ClearAll()
    {
        foreach (var chest in chests)
            if (chest != null)
                Destroy(chest.gameObject);
        chests.Clear();
    }

    void OnDestroy()
    {
        chests.Remove(this);
    }
}
