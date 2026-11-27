using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EffectMaker
{
    // 이펙트 고르기 조작 창. 화면은 Game 뷰(카메라 9개가 3x3으로 나눠 찍음),
    // 이 창은 설정·후보 넘기기·★ 고르기 담당. EffectMaker 씬을 플레이할 때만 동작.
    public class EffectMakerWindow : EditorWindow
    {
        public const string SCENE_PATH = "Assets/4.Scenes/EffectMaker.unity";
        const string CANDIDATES_SESSION_KEY = "EffectMaker_Candidates";
        const float LEFT_WIDTH = 320f;

        EffectFavoriteList favorites;
        List<string> candidates = new List<string>();
        string[] minionKeys = Array.Empty<string>();
        Vector2 leftScroll;
        bool showFavorites = true;

        static EffectMakerController Ctrl => EffectMakerController.Instance;

        [MenuItem("Tools/Effect Maker")]
        public static void Open()
        {
            var window = GetWindow<EffectMakerWindow>("Effect Maker");
            window.minSize = new Vector2(700f, 480f);
            window.Show();
        }

        void OnEnable()
        {
            favorites = EffectFavoriteList.Load();
            LoadMinionKeys();
            // 컴파일마다 프리팹을 다시 읽지 않도록 에디터 세션 동안 목록 유지
            string cached = SessionState.GetString(CANDIDATES_SESSION_KEY, null);
            if (cached != null)
                candidates = cached.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        // ---------------------------------------------------------------- GUI

        void OnGUI()
        {
            if (favorites == null)
                return;

            bool inScene = SceneManager.GetActiveScene().path == SCENE_PATH;
            if (!EditorApplication.isPlaying || !inScene || Ctrl == null)
            {
                DrawStartPanel(inScene);
                return;
            }

            var s = Ctrl.Settings;
            // 처음이면 후보 첫 페이지로 채움
            if (s.cellPaths.All(string.IsNullOrEmpty))
            {
                if (candidates.Count == 0)
                    RefreshCandidates(s);
                FillPage(s);
            }
            UpdateFavoriteMarks(s);

            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel(s);
            DrawCells(s);
            EditorGUILayout.EndHorizontal();
        }

        void DrawStartPanel(bool inScene)
        {
            EditorGUILayout.HelpBox(
                "EffectMaker 씬을 플레이하면 Game 뷰에 9칸이 나오고, 이 창에서 이펙트를 고릅니다.\n" +
                "이 씬에서는 게임 초기화(매니저·유저 데이터·사운드)가 실행되지 않습니다.",
                MessageType.Info);

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(inScene ? "준비 중..." : "지금은 다른 씬을 플레이 중입니다.", MessageType.None);
                return;
            }
            if (GUILayout.Button(inScene ? "▶ 플레이" : "EffectMaker 씬 열고 플레이", GUILayout.Height(32f)))
            {
                if (!inScene)
                {
                    if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        return;
                    EditorSceneManager.OpenScene(SCENE_PATH);
                }
                EditorApplication.isPlaying = true;
            }
        }

        void DrawLeftPanel(EffectMakerSettings s)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(LEFT_WIDTH));
            leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

            // 재생
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(s.paused ? "▶ 재생" : "❚❚ 멈춤", GUILayout.Height(26f)))
                s.paused = !s.paused;
            if (GUILayout.Button("지금 발동", GUILayout.Height(26f)))
                Ctrl.CastAll();
            if (GUILayout.Button("지우기", GUILayout.Height(26f)))
                Ctrl.ClearAll();
            EditorGUILayout.EndHorizontal();
            s.timeScale = EditorGUILayout.Slider("속도", s.timeScale, 0.1f, 3f);

            // 이펙트
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("이펙트", EditorStyles.boldLabel);
            s.interval = EditorGUILayout.Slider("발동 간격(초)", s.interval, 0.3f, 10f);
            s.spawnTarget = (SpawnTarget)EditorGUILayout.Popup("나오는 자리", (int)s.spawnTarget, SPAWN_TARGET_NAMES);
            s.followTarget = EditorGUILayout.Toggle("대상 따라가기", s.followTarget);
            s.effectScale = EditorGUILayout.Slider("크기", s.effectScale, 0.1f, 3f);
            s.offsetY = EditorGUILayout.Slider("높이 보정", s.offsetY, -2f, 2f);
            s.loopTime = EditorGUILayout.Slider("루프 이펙트 유지(초)", s.loopTime, 0.2f, 10f);
            s.maxLife = EditorGUILayout.Slider("최대 유지(초)", s.maxLife, 0.5f, 20f);

            // 환경
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("환경", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            s.casterType = (CasterType)EditorGUILayout.Popup("시전자", (int)s.casterType, CASTER_NAMES);
            if (s.casterType == CasterType.Minion && minionKeys.Length > 0)
            {
                int idx = Mathf.Max(0, Array.IndexOf(minionKeys, s.minionKey));
                idx = EditorGUILayout.Popup("미니언", idx, minionKeys);
                s.minionKey = minionKeys[idx];
            }
            s.casterTile = EditorGUILayout.Popup("시전자 칸", s.casterTile, TILE_NAMES);
            s.enemyCount = EditorGUILayout.IntSlider("적 수", s.enemyCount, 0, 16);
            if (EditorGUI.EndChangeCheck())
                Ctrl.Rebuild();

            EditorGUI.BeginChangeCheck();
            s.enemyRadius = EditorGUILayout.Slider("적 회전 반지름", s.enemyRadius, 1f, 4f);
            s.enemySpeed = EditorGUILayout.Slider("적 회전 속도", s.enemySpeed, -180f, 180f);
            s.cameraSize = EditorGUILayout.Slider("카메라 크기", s.cameraSize, 1.5f, 8f);
            s.bgColor = EditorGUILayout.ColorField("배경색", s.bgColor);
            if (EditorGUI.EndChangeCheck())
                Ctrl.ApplyView();

            // 후보
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("후보 이펙트", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("폴더 (; 로 구분)");
            s.folders = EditorGUILayout.TextArea(s.folders, GUILayout.MinHeight(34f));
            s.nameFilter = EditorGUILayout.TextField("이름 검색", s.nameFilter);
            s.onlyParticle = EditorGUILayout.Toggle("파티클 있는 것만", s.onlyParticle);
            if (GUILayout.Button("목록 갱신"))
            {
                RefreshCandidates(s);
                s.page = 0;
                FillPage(s);
            }

            int pageCount = Mathf.Max(1, Mathf.CeilToInt(candidates.Count / (float)EffectMakerStage.CELL_COUNT));
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = s.page > 0;
            if (GUILayout.Button("◀ 이전 9개"))
            {
                s.page--;
                FillPage(s);
            }
            GUI.enabled = s.page < pageCount - 1;
            if (GUILayout.Button("다음 9개 ▶"))
            {
                s.page++;
                FillPage(s);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginChangeCheck();
            int newPage = EditorGUILayout.IntSlider($"페이지 (후보 {candidates.Count}개)", s.page + 1, 1, pageCount) - 1;
            if (EditorGUI.EndChangeCheck() && newPage != s.page)
            {
                s.page = newPage;
                FillPage(s);
            }

            // 즐겨찾기
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("고른 이펙트 (★)", EditorStyles.boldLabel);
            s.usage = EditorGUILayout.TextField(new GUIContent("용도", "★를 누를 때 이 이름으로 묶어서 저장 (예: 지뢰 진화 B)"), s.usage);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("이 용도 ★ 9칸에 보기"))
                ShowFavorites(s);
            if (GUILayout.Button("파일", GUILayout.Width(50f)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(EffectFavoriteList.FILE_PATH);
                if (asset != null)
                    EditorGUIUtility.PingObject(asset);
            }
            EditorGUILayout.EndHorizontal();
            showFavorites = EditorGUILayout.Foldout(showFavorites, $"목록 ({favorites.items.Count})", true);
            if (showFavorites)
                DrawFavoriteList(s);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        void DrawFavoriteList(EffectMakerSettings s)
        {
            EffectFavorite remove = null;
            foreach (var group in favorites.items.GroupBy(f => string.IsNullOrEmpty(f.usage) ? "(용도 없음)" : f.usage))
            {
                EditorGUILayout.LabelField(group.Key, EditorStyles.miniBoldLabel);
                foreach (var fav in group)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(Path.GetFileNameWithoutExtension(fav.path), EditorStyles.linkLabel))
                        Ping(fav.path);
                    if (GUILayout.Button("적용", GUILayout.Width(40f)))
                    {
                        // 저장할 때의 설정으로 1번 칸에
                        s.effectScale = fav.scale;
                        s.spawnTarget = fav.spawnTarget;
                        Ctrl.SetCell(0, fav.path);
                    }
                    if (GUILayout.Button("X", GUILayout.Width(22f)))
                        remove = fav;
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.BeginChangeCheck();
                    fav.memo = EditorGUILayout.TextField("  메모", fav.memo);
                    if (EditorGUI.EndChangeCheck())
                        favorites.Save();
                }
            }
            if (remove != null)
            {
                favorites.items.Remove(remove);
                favorites.Save();
            }
        }

        // 오른쪽: Game 뷰 칸과 같은 3x3 배치로 칸별 조작
        void DrawCells(EffectMakerSettings s)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("칸 (Game 뷰와 같은 배치)", EditorStyles.boldLabel);
            for (int row = 0; row < 3; row++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int col = 0; col < 3; col++)
                    DrawCell(s, row * 3 + col);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        void DrawCell(EffectMakerSettings s, int i)
        {
            string path = s.cellPaths[i];
            bool hasPrefab = !string.IsNullOrEmpty(path);
            bool isFav = Ctrl.cellFavorite[i];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinWidth(120f));

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = hasPrefab;
            var prevColor = GUI.contentColor;
            if (isFav)
                GUI.contentColor = new Color(1f, 0.8f, 0.2f);
            if (GUILayout.Button(isFav ? "★" : "☆", GUILayout.Width(28f)))
                favorites.Toggle(s.usage, path, s.effectScale, s.spawnTarget);
            GUI.contentColor = prevColor;
            GUI.enabled = true;
            string label = hasPrefab ? $"{i + 1}. {Path.GetFileNameWithoutExtension(path)}" : $"{i + 1}. (비어 있음)";
            if (GUILayout.Button(new GUIContent(label, path), EditorStyles.label) && hasPrefab)
                Ping(path);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("◀", EditorStyles.miniButtonLeft, GUILayout.Width(24f)))
                StepCell(s, i, -1);
            if (GUILayout.Button("▶", EditorStyles.miniButtonRight, GUILayout.Width(24f)))
                StepCell(s, i, 1);
            if (GUILayout.Button("발동", EditorStyles.miniButton, GUILayout.Width(40f)))
                Ctrl.stages[i]?.Cast(s);
            EditorGUI.BeginChangeCheck();
            var picked = (GameObject)EditorGUILayout.ObjectField(EffectMakerStage.LoadPrefab(path), typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck())
                Ctrl.SetCell(i, picked != null ? AssetDatabase.GetAssetPath(picked) : "");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        // ---------------------------------------------------------------- 칸 / 후보

        void UpdateFavoriteMarks(EffectMakerSettings s)
        {
            for (int i = 0; i < EffectMakerStage.CELL_COUNT; i++)
                Ctrl.cellFavorite[i] = !string.IsNullOrEmpty(s.cellPaths[i]) && favorites.Find(s.usage, s.cellPaths[i]) != null;
        }

        void StepCell(EffectMakerSettings s, int i, int dir)
        {
            if (candidates.Count == 0)
                return;
            int idx = candidates.IndexOf(s.cellPaths[i]);
            idx = idx < 0 ? (dir > 0 ? 0 : candidates.Count - 1) : (idx + dir + candidates.Count) % candidates.Count;
            Ctrl.SetCell(i, candidates[idx]);
        }

        void FillPage(EffectMakerSettings s)
        {
            for (int i = 0; i < EffectMakerStage.CELL_COUNT; i++)
            {
                int idx = s.page * EffectMakerStage.CELL_COUNT + i;
                Ctrl.SetCell(i, idx < candidates.Count ? candidates[idx] : "");
            }
        }

        void ShowFavorites(EffectMakerSettings s)
        {
            var list = favorites.items.Where(f => f.usage == s.usage).ToList();
            if (list.Count == 0)
            {
                ShowNotification(new GUIContent($"'{s.usage}' 용도로 고른 이펙트가 없습니다"));
                return;
            }
            for (int i = 0; i < EffectMakerStage.CELL_COUNT; i++)
                Ctrl.SetCell(i, i < list.Count ? list[i].path : "");
        }

        void RefreshCandidates(EffectMakerSettings s)
        {
            candidates = new List<string>();
            try
            {
                CollectCandidates(s);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                SessionState.SetString(CANDIDATES_SESSION_KEY, string.Join("\n", candidates));
                EffectMakerStage.ClearLifeCache();
            }
        }

        void CollectCandidates(EffectMakerSettings s)
        {
            var folders = s.folders.Split(new[] { ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim().TrimEnd('/'))
                .Where(AssetDatabase.IsValidFolder)
                .ToArray();
            if (folders.Length == 0)
                return;

            var paths = AssetDatabase.FindAssets("t:Prefab", folders)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .Where(p => string.IsNullOrEmpty(s.nameFilter)
                            || Path.GetFileNameWithoutExtension(p).IndexOf(s.nameFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(p => p, StringComparer.Ordinal)
                .ToList();

            if (!s.onlyParticle)
            {
                candidates = paths;
                return;
            }
            for (int i = 0; i < paths.Count; i++)
            {
                if (i % 20 == 0
                    && EditorUtility.DisplayCancelableProgressBar("Effect Maker", $"이펙트 찾는 중 {i}/{paths.Count}", i / (float)paths.Count))
                    break;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
                if (prefab != null && prefab.GetComponentInChildren<ParticleSystem>(true) != null)
                    candidates.Add(paths[i]);
            }
        }

        void LoadMinionKeys()
        {
            minionKeys = AssetDatabase.FindAssets("t:Texture2D", new[] { EffectMakerStage.MINION_SPRITE_FOLDER })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => Path.GetDirectoryName(p)?.Replace('\\', '/') == EffectMakerStage.MINION_SPRITE_FOLDER)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(k => k != "enemy1" && k != "Body 1")
                .OrderBy(k => k, StringComparer.Ordinal)
                .ToArray();
        }

        static void Ping(string path)
        {
            var prefab = EffectMakerStage.LoadPrefab(path);
            if (prefab != null)
                EditorGUIUtility.PingObject(prefab);
        }

        static readonly string[] SPAWN_TARGET_NAMES = { "시전자 위치", "가장 가까운 적", "적 전부", "타일 가운데", "시전자 → 적 방향" };
        static readonly string[] CASTER_NAMES = { "미니언", "슬라임" };
        static readonly string[] TILE_NAMES = { "왼쪽 위", "오른쪽 위", "왼쪽 아래", "오른쪽 아래" };
    }
}
