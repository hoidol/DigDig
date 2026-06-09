using UnityEngine;
using DG.Tweening;

public class BlessingStone : MonoBehaviour
{
    static readonly BlessingStonePoolingSystem pool = new();

    public void Return()
    {
        DOTween.Kill(transform);
        pool.Return(this);
    }

    public static void Instantiate(Vector2 pos)
    {
        BlessingStone blessingStone = pool.Get(pos);

        DOVirtual.DelayedCall(2f, () =>
        {
            blessingStone.transform.DOMove(Player.Instance.transform.position, 0.3f)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    blessingStone.Return();
                    BlessingCanvas.Instance.OpenCanvas();
                });
        });
    }
}
