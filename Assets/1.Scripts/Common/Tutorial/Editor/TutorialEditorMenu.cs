using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 튜토리얼 에디터 메뉴
// - Create Tutorial Prefabs: TutorialManager 프리팹(Resources/Init) + FirstPlay 튜토리얼 프리팹(전용 Canvas + 기본 블록 순서)을 만들고
//   매니저의 tutorialPrefabs에 등록한다 (이미 있으면 건너뜀). 튜토리얼은 씬에 두지 않는다 (TutorialManager가 생성)
//   만든 뒤 모양(대화창·손가락)과 블록 값(대사·칸·미니언 key), 숨길 UI 이름(hideObjectNames)은 프리팹에서 고친다
// - Reset Tutorial Progress: 완료 기록(PlayerPrefs) 삭제 → 다시 처음 실행한 유저처럼
public static class TutorialEditorMenu
{
    const string KING_SPEAKER_KEY = "KingSlime";
    const string KING_SPEAKER_NAME = "마왕을 꿈꾸는 슬라임";

    public const string MANAGER_PREFAB_PATH = "Assets/Resources/Init/TutorialManager.prefab";
    public const string TUTORIAL_PREFAB_DIR = "Assets/3.Prefabs/Tutorial";
    public const string FIRST_PLAY_PREFAB_PATH = TUTORIAL_PREFAB_DIR + "/FirstPlayTutorial.prefab";

    [MenuItem("Tools/Tutorial/Create Tutorial Prefabs")]
    public static void CreatePrefabs()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(MANAGER_PREFAB_PATH) == null)
        {
            GameObject manager = new GameObject("TutorialManager");
            manager.AddComponent<TutorialManager>();
            PrefabUtility.SaveAsPrefabAsset(manager, MANAGER_PREFAB_PATH);
            UnityEngine.Object.DestroyImmediate(manager);
            Debug.Log($"[Tutorial] 생성 {MANAGER_PREFAB_PATH}");
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(FIRST_PLAY_PREFAB_PATH) == null)
        {
            if (!AssetDatabase.IsValidFolder(TUTORIAL_PREFAB_DIR))
                AssetDatabase.CreateFolder("Assets/3.Prefabs", "Tutorial");
            GameObject root = CreateFirstPlayTutorialObject();
            PrefabUtility.SaveAsPrefabAsset(root, FIRST_PLAY_PREFAB_PATH);
            UnityEngine.Object.DestroyImmediate(root);
            Debug.Log($"[Tutorial] 생성 {FIRST_PLAY_PREFAB_PATH}");
        }

        // 매니저 프리팹에 튜토리얼 프리팹 등록
        Tutorial firstPlay = AssetDatabase.LoadAssetAtPath<Tutorial>(FIRST_PLAY_PREFAB_PATH);
        GameObject managerRoot = PrefabUtility.LoadPrefabContents(MANAGER_PREFAB_PATH);
        TutorialManager tutorialManager = managerRoot.GetComponent<TutorialManager>();
        if (firstPlay != null && !tutorialManager.tutorialPrefabs.Contains(firstPlay))
        {
            tutorialManager.tutorialPrefabs.Add(firstPlay);
            PrefabUtility.SaveAsPrefabAsset(managerRoot, MANAGER_PREFAB_PATH);
            Debug.Log("[Tutorial] TutorialManager.tutorialPrefabs에 FirstPlayTutorial 등록");
        }
        PrefabUtility.UnloadPrefabContents(managerRoot);
        AssetDatabase.SaveAssets();
    }

    // 진행 순서 = 자식 순서. 블록을 복사·삭제·순서 변경해서 흐름을 바꾼다
    static GameObject CreateFirstPlayTutorialObject()
    {
        GameObject root = new GameObject("FirstPlayTutorial");
        Tutorial tutorial = root.AddComponent<Tutorial>();
        tutorial.tutorialType = TutorialType.FirstPlay;
        tutorial.canvas = CreateCanvas(root.transform);

        Transform steps = new GameObject("Steps").transform;
        steps.SetParent(root.transform, false);

        AddStep<SetKingTileTutorialStep>(steps, "01_킹 위치 지정 (오른쪽 아래 그룹)", s => s.tileIndex = new Vector2Int(4, 4));
        AddStep<WaitFadeEndTutorialStep>(steps, "02_페이드 끝날 때까지 대기");
        AddDialogue(steps, "03_대화");
        AddStep<StartWaveTutorialStep>(steps, "04_wave 시작");
        AddStep<WaitTutorialStep>(steps, "05_대기", s => s.seconds = 3f);
        AddStep<MoveKingTutorialStep>(steps, "06_킹 이동 (왼쪽 위 그룹의 왼쪽 아래)", s => s.targetTileIndex = new Vector2Int(0, 2));
        AddStep<WaitTutorialStep>(steps, "07_대기", s => s.seconds = 2f);
        AddDialogue(steps, "08_대화");
        AddStep<MoveKingTutorialStep>(steps, "09_킹 이동 (칸 입력)", s => s.targetTileIndex = new Vector2Int(3, 2));
        AddDialogue(steps, "10_대화");
        AddStep<ShowSpawnButtonTutorialStep>(steps, "11_소환 버튼 보이기");
        // 1기씩 블록으로 분리 (사이에 대화를 끼울 수 있음). 1번째와 3번째를 같은 key로 → 합성 블록에서 3번째를 1번째 위로
        AddStep<SpawnMinionTutorialStep>(steps, "12_소환 1 (Ice)", s => { s.minionKey = "Ice"; s.tileIndex = new Vector2Int(1, 1); });
        AddStep<SpawnMinionTutorialStep>(steps, "12_소환 2 (Flame)", s => { s.minionKey = "Flame"; s.tileIndex = new Vector2Int(2, 1); });
        AddStep<SpawnMinionTutorialStep>(steps, "12_소환 3 (Ice, 1번째와 같은 종)", s => { s.minionKey = "Ice"; s.tileIndex = new Vector2Int(1, 2); });
        AddDialogue(steps, "13_대화");
        AddStep<MergeMinionTutorialStep>(steps, "14_합성 (3번째 → 1번째)", s =>
        {
            s.fromTileIndex = new Vector2Int(1, 2);
            s.toTileIndex = new Vector2Int(1, 1);
        });
        AddDialogue(steps, "15_대화");
        return root;
    }

    static void AddDialogue(Transform parent, string name)
    {
        AddStep<DialogueTutorialStep>(parent, name, s =>
        {
            s.speakerKey = KING_SPEAKER_KEY;
            s.lines = new[] { "(대사를 입력하세요)" };
        });
    }

    static void AddStep<T>(Transform parent, string name, Action<T> setup = null) where T : TutorialStep
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        T step = go.AddComponent<T>();
        setup?.Invoke(step);
    }

    // 전용 Canvas: 화면 블록(투명) + 화면 중앙 대화창. 토스트(5000)·페이드보다 아래, 게임 UI(100)보다 위
    static TutorialCanvas CreateCanvas(Transform parent)
    {
        GameObject canvasGO = new GameObject("TutorialCanvas", typeof(RectTransform));
        canvasGO.layer = LayerMask.NameToLayer("UI");
        canvasGO.transform.SetParent(parent, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1242, 2208);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        canvasGO.AddComponent<GraphicRaycaster>();
        TutorialCanvas tutorialCanvas = canvasGO.AddComponent<TutorialCanvas>();

        // 화면 블록
        Image block = CreateStretchImage("Block", canvasGO.transform, new Color(0, 0, 0, 0));
        tutorialCanvas.blockObject = block.gameObject;

        // 대화창: 전체 화면 터치 영역 + 가운데 창
        Image touch = CreateStretchImage("Dialogue", canvasGO.transform, new Color(0, 0, 0, 0.4f));
        tutorialCanvas.dialogueObject = touch.gameObject;
        tutorialCanvas.dialogueTouchButton = touch.gameObject.AddComponent<Button>();
        tutorialCanvas.dialogueTouchButton.transition = Selectable.Transition.None;

        GameObject panelGO = new GameObject("Panel", typeof(RectTransform));
        panelGO.layer = canvasGO.layer;
        panelGO.transform.SetParent(touch.transform, false);
        RectTransform panelRect = (RectTransform)panelGO.transform;
        panelRect.sizeDelta = new Vector2(1000, 420);
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        panelImage.raycastTarget = false;

        // 초상화: 창 왼쪽 위에 걸침 (스프라이트는 speakers에서 지정, 없으면 숨김)
        GameObject portraitGO = new GameObject("Portrait", typeof(RectTransform));
        portraitGO.layer = canvasGO.layer;
        portraitGO.transform.SetParent(panelGO.transform, false);
        RectTransform portraitRect = (RectTransform)portraitGO.transform;
        portraitRect.anchorMin = portraitRect.anchorMax = new Vector2(0, 1);
        portraitRect.pivot = new Vector2(0, 0);
        portraitRect.anchoredPosition = new Vector2(30, 10);
        portraitRect.sizeDelta = new Vector2(220, 220);
        Image portrait = portraitGO.AddComponent<Image>();
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;
        tutorialCanvas.portraitImage = portrait;

        // 화자 이름: 창 위쪽
        TextMeshProUGUI nameText = CreateText("SpeakerName", panelGO.transform, 40, TextAlignmentOptions.Left);
        RectTransform nameRect = nameText.rectTransform;
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.offsetMin = new Vector2(50, -90);
        nameRect.offsetMax = new Vector2(-50, -25);
        nameText.color = new Color(1f, 0.85f, 0.3f);
        nameText.text = KING_SPEAKER_NAME;
        tutorialCanvas.speakerNameText = nameText;

        TextMeshProUGUI text = CreateText("Text", panelGO.transform, 48, TextAlignmentOptions.Center);
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(50, 40);
        textRect.offsetMax = new Vector2(-50, -100);
        text.text = "대사";
        tutorialCanvas.dialogueText = text;

        tutorialCanvas.speakers.Add(new TutorialSpeaker { key = KING_SPEAKER_KEY, name = KING_SPEAKER_NAME });

        // 손가락 안내: 임시 모양(원하는 손가락 이미지로 교체). 손끝이 가리키는 점 = pivot
        GameObject fingerGO = new GameObject("Finger", typeof(RectTransform));
        fingerGO.layer = canvasGO.layer;
        fingerGO.transform.SetParent(canvasGO.transform, false);
        RectTransform fingerRect = (RectTransform)fingerGO.transform;
        fingerRect.anchorMin = fingerRect.anchorMax = Vector2.zero;
        fingerRect.pivot = new Vector2(0.5f, 1f);
        fingerRect.sizeDelta = new Vector2(120, 120);
        Image fingerImage = fingerGO.AddComponent<Image>();
        fingerImage.raycastTarget = false;
        tutorialCanvas.finger = fingerRect;
        fingerGO.SetActive(false);

        touch.gameObject.SetActive(false);
        return tutorialCanvas;
    }

    static TextMeshProUGUI CreateText(string name, Transform parent, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    static Image CreateStretchImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = go.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = true;
        return image;
    }

    [MenuItem("Tools/Tutorial/Reset Tutorial Progress")]
    static void ResetProgress()
    {
        foreach (TutorialType type in Enum.GetValues(typeof(TutorialType)))
        {
            TutorialManager.SetCompleted(type, false);
        }
        Debug.Log("[Tutorial] 완료 기록 삭제");
    }
}
