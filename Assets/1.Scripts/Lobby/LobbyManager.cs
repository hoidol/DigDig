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
            // 처음 실행한 유저는 로비 대신 바로 InGame (첫 플레이 튜토리얼). 에너지 소모 없음
            if (TutorialManager.TryRedirectToFirstPlay())
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("InGame");
                return;
            }

            await UniTask.WhenAll(
                StageManager.Instance.LoadTask,
                BulletManager.Instance.LoadTask,
                RuneManager.Instance.LoadTask,
                EnemyManager.Instance.LoadTask,
                MinionManager.Instance.LoadTask
            );
            UserDataManager.Instance.GrantAutoUnlocks(); // (2026-11-27) 미니언 데이터를 읽은 뒤 자동 해금 미니언 지급
            MultiMatchManager.CleanupOldRoomsOnLobby(); // (2026-11-27) 시작 5시간 지난 멀티 방 정리 (누구든 로비에 들어오면, 10분에 한 번)
            PlayRecordBackup.UploadPendingAsync().Forget(); // (2026-11-27) 못 올린 플레이 기록 다시 올리기
            var sound = SoundManager.Instance;
            if (sound != null)
                sound.PlayBGM(BGMType.Lobby);
            var fadeDone = new UniTaskCompletionSource();
            StartLobby(fadeDone.Task);
            FadeCanvs.Instance.FadeIn("", () => fadeDone.TrySetResult());
        }

        async void StartLobby(UniTask fadeDone)
        {
            OpenCanvas(LobbyState.Battle);
            LobbyCanvas.Instance.UpdateCanvas();
            var myInfo = UserDataManager.Instance.userMyInfoManager;
            var panels = FindObjectsByType<MyInfoPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            // 인게임에서 받은 경험치: 받기 전 값으로 보여 주고 페이드가 끝나면 바가 차오름 (레벨 경계마다 레벨업 창)
            int gainExp = Mathf.Min(UserMyInfoManager.unshownGainExp, myInfo.userMyInfoData.exp);
            UserMyInfoManager.unshownGainExp = 0;
            if (gainExp > 0)
            {
                int displayExp = myInfo.userMyInfoData.exp - gainExp;
                foreach (var p in panels)
                    p.BeginExpAnimation(myInfo.userMyInfoData.lv, displayExp);
                await fadeDone;
            }
            else
            {
                // Awake의 GameEventBus.Clear가 패널 구독보다 늦게 돌 수 있어 이벤트에만 의존하지 않고 직접 갱신
                UpdateMyInfoPanels();
            }

            // 보스를 처음 처치하고 돌아왔으면 보스 처치 연출 먼저 (페이드가 끝난 뒤, 끝나면 경험치 바·레벨업 창)
            await LobbyReturnController.PlayPendingAsync(fadeDone);

            // 레벨업 창을 닫아야 다음 레벨업 (여러 레벨이 한 번에 올라도 하나씩 보여줌)
            bool leveledUp = false;
            int fromExp = gainExp > 0 ? myInfo.userMyInfoData.exp - gainExp : 0;
            while (true)
            {
                int lv = myInfo.userMyInfoData.lv;
                if (gainExp > 0)
                {
                    int toExp = Mathf.Min(myInfo.userMyInfoData.exp, myInfo.GetMaxExp(lv));
                    await UniTask.WhenAll(System.Array.ConvertAll(panels, p => p.AnimateExp(lv, fromExp, toExp)));
                    fromExp = 0;
                }

                if (!myInfo.LevelUp())
                    break;
                lv = myInfo.userMyInfoData.lv;
                leveledUp = true;
                var closed = new UniTaskCompletionSource();
                UserLevelUpCanvas.Instance.OpenCanvas(() => closed.TrySetResult());
                if (gainExp > 0)
                {
                    // 새 레벨의 빈 바에서 다시 차오름 (최대 레벨이면 MAX)
                    foreach (var p in panels)
                        p.BeginExpAnimation(lv, 0);
                }
                else
                {
                    UpdateMyInfoPanels();
                }
                await closed.Task;

                // 이 레벨에 해금된 미니언 안내 (자동 = 획득, 광고·유료 = 해금 가능)
                foreach (string key in myInfo.lastUnlockedMinionKeys)
                {
                    bool owned = UserDataManager.Instance.userMinionManager.GetUserMinion(key).own;
                    ToastCanvas.Toast(string.Format(TranslateManager.GetText(owned ? "Minion_LevelGranted" : "Minion_LevelUnlocked"), TranslateManager.GetText($"Minion.{key}")));
                }
            }
            if (gainExp > 0)
                foreach (var p in panels)
                    p.EndExpAnimation();

            // (2026-11-24) 레벨업으로 슬라임 스탯 포인트가 생겼으면 안내 (레벨 1당 1포인트, 슬라임 관리 화면에서 찍음)
            int remainPoints = UserDataManager.Instance.userSlimeManager.RemainPoints;
            if (leveledUp && remainPoints > 0)
                ToastCanvas.Toast(string.Format(TranslateManager.GetText("SlimeStat_PointGained"), remainPoints));
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
    Minion,
    Battle,
    SlimeManagement,
    Town
}
