using UnityEngine;

public class ToastCanvas : MonoSingleton<ToastCanvas>
{
    public ToastPanel toastPanelPrefab;
    public RectTransform initRectTr;

    readonly StackPoolingSystem<ToastPanel> pool = new();

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
        ToastPanel toastPanel = pool.Get(initRectTr.position, transform);
        toastPanel.Toast(message, OnToastComplete);
    }

    void OnToastComplete(ToastPanel toastPanel)
    {
        pool.Return(toastPanel);
    }
}
