using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace Lobby
{
    public class LobbyManager : MonoSingleton<LobbyManager>
{
    public BaseLobbyCanvas[] lobbyCanvases;
    public LobbyStateButton[] lobbyStateButtons;
    public LobbyCanvas lobbyCanvas;
    void Awake()
    {

        lobbyCanvases = FindObjectsByType<BaseLobbyCanvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
            EquipmentManager.Instance.LoadTask,
            SlimeManager.Instance.LoadTask

        );

        FadeCanvs.Instance.FadeIn("", () =>
        {
            StartLobby();
        });
    }

    async void StartLobby()
    {        
        OpenCanvas(LobbyState.Battle);
        LobbyCanvas.Instance.UpdateCanvas();
        while (UserDataManager.Instance.userMyInfoManager.LevelUp())
        {
            UserLevelUpCanvas.Instance.OpenCanvas();
        }
    }

    public LobbyState lobbyState;
    public void OpenCanvas(LobbyState state)
    {
        lobbyState = state;
        for (int i = 0; i < lobbyCanvases.Length; i++)
        {
            lobbyCanvases[i].CloseCanvas();
        }
        var canvas = lobbyCanvases.FirstOrDefault(c => c.state == state);
        canvas?.OpenCanvas();
        lobbyCanvas.UpdateCanvas();

        for(int i = 0; i < lobbyStateButtons.Length; i++)
        {
            lobbyStateButtons[i].UpdateButton();
        }
    }

    public BaseLobbyCanvas GetLobbyCanvas(LobbyState state)
    {
        var canvas = lobbyCanvases.FirstOrDefault(c => c.state == state);
        return canvas;
    }
}
}

public enum LobbyState
{
    Shop,
    Slime,
    Battle,
    Equipment,
}
