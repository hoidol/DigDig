using System;
using UnityEngine;
using DG.Tweening;
public class PlayerInLobby : MonoSingleton<PlayerInLobby>
{
    public Transform root;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void StartGame(Action end)
    {
        root.DOScale(Vector3.one, 0.4f)
            .OnComplete(() =>
            {
                end?.Invoke();
                Destroy(gameObject);
            });
    }
}
