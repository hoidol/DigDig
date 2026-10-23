using System.Collections.Generic;
using UnityEngine;

public class ToastCanvas : MonoSingleton<ToastCanvas>
{
    public ToastPanel toastPanelPrefab;
    public RectTransform initRectTr;

    readonly StackPoolingSystem<ToastPanel> pool = new();
    readonly HashSet<string> showingMessages = new(); // 현재 보여지고 있는 메시지

    void Awake()
    {
        pool.prefab = toastPanelPrefab;
    }

    public static void Toast(string message)
    {
        Instance.ShowToast(message);
    }

    void ShowToast(string message)
    {
        // 같은 메시지가 이미 떠 있으면 생성 안 함
        if (!showingMessages.Add(message))
            return;

        ToastPanel toastPanel = pool.Get(initRectTr.position, transform);
        toastPanel.Toast(message, OnToastComplete);
    }

    void OnToastComplete(ToastPanel toastPanel)
    {
        showingMessages.Remove(toastPanel.messageText.text);
        pool.Return(toastPanel);
    }
}
