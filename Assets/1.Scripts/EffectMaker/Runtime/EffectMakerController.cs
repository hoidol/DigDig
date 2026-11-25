using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EffectMaker
{
    // EffectMaker 씬을 플레이하면 스스로 생겨서 9칸(환경 + 카메라)을 만들고 주기마다 스킬(이펙트)을 발동.
    // 씬에 배치하지 않음 → 다른 씬(InGame/Lobby)에서는 아무것도 하지 않는다.
    // 게임 초기화(StartSetting)는 이 씬에서 건너뜀 (StartSetting.IsToolScene).
    public class EffectMakerController : MonoBehaviour
    {
        public const string SCENE_NAME = "EffectMaker";

        public static EffectMakerController Instance { get; private set; }

        public EffectMakerSettings Settings { get; private set; }
        public readonly EffectMakerStage[] stages = new EffectMakerStage[EffectMakerStage.CELL_COUNT];
        // 에디터 창이 채워 줌 → 화면 칸 이름 옆에 ★ 표시
        public readonly bool[] cellFavorite = new bool[EffectMakerStage.CELL_COUNT];

        float castTimer;
        GUIStyle labelStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (SceneManager.GetActiveScene().name != SCENE_NAME)
                return;
            new GameObject(nameof(EffectMakerController)).AddComponent<EffectMakerController>();
        }

        void Awake()
        {
            Instance = this;
            Settings = EffectMakerSettings.Load();

            // 씬의 기본 카메라는 끔 (9칸 카메라가 화면 전체를 나눠 씀)
            foreach (var c in FindObjectsByType<Camera>(FindObjectsSortMode.None))
                c.enabled = false;

            Rebuild();
        }

        void OnDestroy()
        {
            Settings?.Save();
            Time.timeScale = 1f;
            if (Instance == this)
                Instance = null;
        }

        public void Rebuild()
        {
            for (int i = 0; i < stages.Length; i++)
            {
                stages[i]?.Destroy();
                stages[i] = new EffectMakerStage(i);
                stages[i].Build(transform, Settings);
                stages[i].prefab = EffectMakerStage.LoadPrefab(Settings.cellPaths[i]);
            }
            castTimer = 0.3f;
        }

        public void ApplyView()
        {
            foreach (var stage in stages)
                stage?.ApplyView(Settings);
        }

        public void SetCell(int i, string path)
        {
            Settings.cellPaths[i] = path;
            Settings.Save();
            var stage = stages[i];
            if (stage == null)
                return;
            stage.ClearEffects();
            stage.prefab = EffectMakerStage.LoadPrefab(path);
            stage.Cast(Settings);
        }

        public void CastAll()
        {
            foreach (var stage in stages)
                stage?.Cast(Settings);
            castTimer = Mathf.Max(0.2f, Settings.interval);
        }

        public void ClearAll()
        {
            foreach (var stage in stages)
                stage?.ClearEffects();
        }

        void Update()
        {
            // 멈춤·속도는 timeScale로 → DOTween 등 게임 이펙트 연출도 같이 멈추고 느려짐
            Time.timeScale = Settings.paused ? 0f : Settings.timeScale;
            float dt = Time.deltaTime;
            if (dt <= 0f)
                return;

            castTimer -= dt;
            bool cast = castTimer <= 0f;
            if (cast)
                castTimer = Mathf.Max(0.2f, Settings.interval);
            foreach (var stage in stages)
            {
                if (stage == null)
                    continue;
                if (cast)
                    stage.Cast(Settings);
                stage.Tick(dt, Settings);
            }
        }

        // 게임 화면 칸마다 번호 + 이펙트 이름
        void OnGUI()
        {
            if (labelStyle == null)
                labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };

            for (int i = 0; i < stages.Length; i++)
            {
                var stage = stages[i];
                if (stage == null)
                    continue;
                var vp = stage.ViewportRect;
                var rect = new Rect(vp.x * Screen.width + 6f, (1f - vp.yMax) * Screen.height + 4f, vp.width * Screen.width - 12f, 22f);
                string path = Settings.cellPaths[i];
                string name = string.IsNullOrEmpty(path) ? "(비어 있음)" : Path.GetFileNameWithoutExtension(path);
                string text = $"{(cellFavorite[i] ? "★ " : "")}{i + 1}. {name}";

                GUI.color = new Color(0f, 0f, 0f, 0.6f);
                GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text, labelStyle);
                GUI.color = cellFavorite[i] ? new Color(1f, 0.85f, 0.3f) : Color.white;
                GUI.Label(rect, text, labelStyle);
                GUI.color = Color.white;
            }
        }
    }
}
