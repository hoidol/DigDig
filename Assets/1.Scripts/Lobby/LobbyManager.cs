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
            StartLobby();
            FadeCanvs.Instance.FadeIn("", () =>
            {
            });
        }

        async void StartLobby()
        {
            OpenCanvas(LobbyState.Battle);
            LobbyCanvas.Instance.UpdateCanvas();
            // 인게임에서 받은 경험치 반영. Awake의 GameEventBus.Clear가 패널 구독보다 늦게 돌 수 있어 이벤트에만 의존하지 않고 직접 갱신
            UpdateMyInfoPanels();
            // 레벨업 창을 닫아야 다음 레벨업 (여러 레벨이 한 번에 올라도 하나씩 보여줌)
            while (UserDataManager.Instance.userMyInfoManager.LevelUp())
            {
                var closed = new UniTaskCompletionSource();
                UserLevelUpCanvas.Instance.OpenCanvas(() => closed.TrySetResult());
                UpdateMyInfoPanels();
                await closed.Task;
            }
        }

        void UpdateMyInfoPanels()
        {
            foreach (var panel in FindObjectsByType<MyInfoPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                panel.UpdatePanel();
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

            for (int i = 0; i < lobbyStateButtons.Length; i++)
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
    KingSlimeManagement,
    Town
}
