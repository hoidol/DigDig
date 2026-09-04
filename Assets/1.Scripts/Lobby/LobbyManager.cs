using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LobbyManager : MonoSingleton<LobbyManager>
{
    public BaseLobbyCanvas[] lobbyCanvases;
    public LobbyCanvas lobbyCanvas;
    void Awake()
    {
        
        lobbyCanvases = FindObjectsByType<BaseLobbyCanvas>( FindObjectsInactive.Include ,FindObjectsSortMode.None);
        lobbyCanvas = FindFirstObjectByType<LobbyCanvas>();
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
            OpenCanvas(LobbyState.Battle);
            LobbyCanvas.Instance.UpdateCanvas();
        });
    }
    public LobbyState lobbyState;
    public void OpenCanvas(LobbyState state)
    {
        lobbyState = state;
        var canvas = lobbyCanvases.FirstOrDefault(c => c.state == state);
        canvas?.OpenCanvas();
        lobbyCanvas.UpdateCanvas();
    }

    public BaseLobbyCanvas GetLobbyCanvas(LobbyState state)
    {
        var canvas = lobbyCanvases.FirstOrDefault(c => c.state == state);
        return canvas;
    }
}
public enum LobbyState
{
    Shop,
    Slime,
    Battle,
    Equipment,
}
