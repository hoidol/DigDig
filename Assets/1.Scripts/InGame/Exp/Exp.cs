using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Exp : MonoBehaviour, IPickable
{
    public string Key => "Exp";
    public static ExpPoolingSystem poolingSystem = new();

    public bool IsTaken { get; set; }
    public Transform Transform => transform;

    // Tween autoAttractTween;
    CancellationTokenSource moveCts;
    const float MOVE_SPEED = 20f;

    public static void Instantiate(Vector2 pos, int count, float size)
    {
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
        // autoAttractTween?.Kill();
        moveCts?.Cancel();
        // autoAttractTween = DOVirtual.DelayedCall(5f, () => Take(Player.Instance));
    }

    public void PickedUp()
    {
        Character.Instance.AddExp(1);
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
            Take(Character.Instance);
        }
    }
}
