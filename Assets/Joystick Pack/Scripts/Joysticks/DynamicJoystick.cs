using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicJoystick : Joystick
{
    public float MoveThreshold { get { return moveThreshold; } set { moveThreshold = Mathf.Abs(value); } }

    [SerializeField] private float moveThreshold = 1;


    CancellationTokenSource waitCts;
    protected async override void Start()
    {
        MoveThreshold = moveThreshold;
        base.Start();
#if UNITY_EDITOR
        background.gameObject.SetActive(false);
        return;
#endif
        background.gameObject.SetActive(true);
        WaitInactive().Forget();
    }
    public async UniTaskVoid WaitInactive()
    {
        waitCts?.Cancel();
        waitCts = new CancellationTokenSource();

        await UniTask.Delay(3000, cancellationToken: waitCts.Token);
        background.gameObject.SetActive(false);
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
#if UNITY_EDITOR
        background.gameObject.SetActive(false);
        return;
#endif
        waitCts?.Cancel();  // 실행 중인 WaitInactive 취소
        background.gameObject.SetActive(true);
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
#if UNITY_EDITOR
        background.gameObject.SetActive(false);
        return;
#endif
        WaitInactive().Forget();
        base.OnPointerUp(eventData);
    }

    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (magnitude > moveThreshold)
        {
            Vector2 difference = normalised * (magnitude - moveThreshold) * radius;
            background.anchoredPosition += difference;
        }
        base.HandleInput(magnitude, normalised, radius, cam);
    }
}