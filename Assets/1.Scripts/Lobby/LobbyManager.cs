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
                RelicManager.Instance.LoadTask,
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
            bool relicTierUnlocked = false;
            while (UserDataManager.Instance.userMyInfoManager.LevelUp())
            {
                int lv = UserDataManager.Instance.userMyInfoManager.userMyInfoData.lv;
                relicTierUnlocked |= System.Array.IndexOf(RelicData.TIER_UNLOCK_LEVELS, lv) >= 0;
                var closed = new UniTaskCompletionSource();
                UserLevelUpCanvas.Instance.OpenCanvas(() => closed.TrySetResult());
                UpdateMyInfoPanels();
                await closed.Task;
            }

            // 레벨업으로 유물 묶음이 새로 열렸고 아직 안 골랐으면 유물 선택으로 안내
            if (relicTierUnlocked && UserDataManager.Instance.userRelicManager.GetUnselectedUnlockedTiers().Count > 0)
                OpenRelicSelect();
        }

        public void OpenRelicSelect()
        {
            OpenCanvas(LobbyState.KingSlimeManagement);
            if (GetLobbyCanvas(LobbyState.KingSlimeManagement) is KSManagementCanvas ks)
                ks.SelectMenu(KSMenuType.Relic);
            ToastCanvas.Toast(TranslateManager.GetText("Relic_TierUnlocked"));
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
