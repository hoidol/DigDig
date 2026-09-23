using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class RewardEffectCanvas : CanvasUI<RewardEffectCanvas>
{
    public RewardEffect prefab;
    public PoolingSystem<RewardEffect> poolingSystem;
    // #if UNITY_EDITOR
    // public Transform startTr;
    // public Transform endTr;
    // public int count;

    // void Update()
    // {
    //   if(Input.GetKeyDown(KeyCode.S))
    //   {
    //     ShowEffect(CurrencyType.Gold,startTr.position,endTr.position,count);
    //   }
    // }
    // #endif
    void Awake()
    {
      poolingSystem = new PoolingSystem<RewardEffect>();
      poolingSystem.SetPrefab(prefab);
    }
    public void ShowEffect(CurrencyType currencyType, Vector2 startPos,Vector2 destinationPos, int count)
    {
        for(int i = 0; i < count; i++)
        {
          RewardEffect rewardEffect = poolingSystem.GetObject<RewardEffect>(transform);
          rewardEffect.Move(currencyType, startPos, destinationPos).Forget();
        }
    }
}
