using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RewardEffect : MonoBehaviour
{
    public Image image;
    public RectTransform rectTransform;
    public async UniTaskVoid Move(CurrencyType currencyType, Vector2 startPos,Vector2 destinationPos)
    {

        image.sprite = Resources.Load<Sprite>($"Icons/{currencyType}");
        rectTransform.position = startPos + Random.insideUnitCircle *150;
        rectTransform.localScale = Vector2.zero;
        CancellationToken token = this.GetCancellationTokenOnDestroy();
        await UniTask.Delay(Random.Range(0,500),cancellationToken: token);
        await rectTransform.DOScale(Vector2.one,Random.Range(0.3f,0.4f)).ToUniTask(cancellationToken: token);
        await rectTransform.DOMove(destinationPos, Random.Range(0.6f,1f)).ToUniTask(cancellationToken: token);
        gameObject.SetActive(false);

    }
    
}