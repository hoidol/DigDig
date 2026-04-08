using System;

public interface IBossMovement
{
    void StartMove(Boss boss, Action onComplete);
    void Cancel();
}
