using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LobbyManager : MonoSingleton<LobbyManager>
{

    void Awake()
    {
        GameEventBus.Clear();
    }

    async void Start()
    {
        await UniTask.WhenAll(
            StageManager.Instance.LoadTask,
            BulletManager.Instance.LoadTask,
            ItemManager.Instance.LoadTask,
            EnemyManager.Instance.LoadTask,
            EquipmentManager.Instance.LoadTask
        );

        FadeCanvs.Instance.FadeIn("", () =>
        {
            LobbyCanvas.Instance.UpdateCanvas();
        });
    }

}

