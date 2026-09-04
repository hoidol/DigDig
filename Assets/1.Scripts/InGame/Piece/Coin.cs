using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//광석 
public class Coin : MonoBehaviour, IPickable
{
    public string Key => "Coin";
    // public OreType oreType;
    public static CoinPoolingSystem poolingSystem = new();

    public bool IsTaken { get; set; }
    public Transform Transform => transform;

    public SpriteRenderer spriteRenderer;
    CancellationTokenSource moveCts;
    const float MOVE_SPEED = 20f;

    public static void Instantiate(Vector2 pos, int count, float size)
    {
        if (Random.value > Character.Instance.coinChance)//OreItem 10% 확률로 드랍
            return;

        for (int i = 0; i < count; i++)
        {
            Vector2 position = pos + Random.insideUnitCircle * size;
            poolingSystem.Get(position);
        }
    }

    public void Droped(Vector2 pos)
    {
        transform.position = pos;
        IsTaken = false;
        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
        moveCts?.Cancel();
    }

    public void PickedUp()
    {
        Character.Instance.AddCoin(1);
        poolingSystem.Return(this);
    }

    public void Take(IPicker picker)
    {
        if (IsTaken) return;
        IsTaken = true;

        moveCts?.Cancel();
        moveCts = new CancellationTokenSource();
        MoveToPickerAsync(picker, moveCts.Token).Forget();
    }

    async UniTaskVoid MoveToPickerAsync(IPicker picker, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            Vector2 target = picker.Transform.position;
            transform.position = Vector2.MoveTowards(transform.position, target, MOVE_SPEED * Time.deltaTime);

            if (Vector2.Distance(transform.position, target) < 0.05f)
            {
                picker.PickUp(this);
                return;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsTaken)
            return;

        if (collision.CompareTag("Player"))
        {
            Take(Character.Instance);
        }
    }
}
