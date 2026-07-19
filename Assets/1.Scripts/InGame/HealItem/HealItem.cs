using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class HealItem : MonoBehaviour, IPickable
{
    public string Key => "HealItem";
    public static HealItemPoolingSystem poolingSystem = new();

    public bool IsTaken { get; set; }
    public Transform Transform => transform;

    // Tween autoAttractTween;
    CancellationTokenSource moveCts;
    const float MOVE_SPEED = 20f;

    public static void Instantiate(Vector2 pos)
    {
        Vector2 position = pos + Random.insideUnitCircle;
        poolingSystem.Get(position);
    }

    public void Droped(Vector2 pos)
    {
        transform.position = pos;
        IsTaken = false;
        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
        // autoAttractTween?.Kill();
        moveCts?.Cancel();
        // autoAttractTween = DOVirtual.DelayedCall(5f, () => Take(Player.Instance));
    }

    public void PickedUp()
    {
        Player.Instance.AddHp(5);
        poolingSystem.Return(this);
    }

    public void Take(IPicker picker)
    {
        if (IsTaken) return;
        IsTaken = true;
        // autoAttractTween?.Kill();

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
            Take(Player.Instance);
        }
    }
}
