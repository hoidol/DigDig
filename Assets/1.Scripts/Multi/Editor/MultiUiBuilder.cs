using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// (2026-11-27) 멀티 UI를 씬·프리팹에 실제로 만듦 (이후 모양은 직접 수정)
// - Tools/Lobby/Add Multi Game Buttons : 로비 BattleCanvas에 [온라인 게임](Play 왼쪽) 버튼 (난이도 버튼 복사)
// - Tools/Lobby/Add Online Waiting Canvas : 로비 대기 창 (화면 전체, 가운데 N/8 · 300→0초 · 코드 · 공유) + 예전 [이어하기] 버튼 삭제
// - Tools/Multi/Create MultiGameUI Scene : Assets/4.Scenes/MultiGameUI.unity (오른쪽 위 다른 유저 목록 · 보기 · Ready · 결과) + 빌드 설정에 추가
// 다시 실행하면 지우고 새로 — ⚠ MultiGameUI 씬은 이후 직접 수정함(2026-11-27 StatusRow_Template: 미니언 글씨 삭제·WAVE N 등) → 다시 실행하면 그 수정이 사라짐
public static class MultiUiBuilder
{
    const string STATUS_PREFAB = "Assets/Resources/UI/MultiMatchStatus.prefab";
    const string UI_SCENE = "Assets/4.Scenes/MultiGameUI.unity";
    static readonly Color PANEL = new Color(0.08f, 0.08f, 0.11f, 0.92f);
    static readonly Color ROW = new Color(0.22f, 0.22f, 0.26f, 1f);
    static readonly Color READY = new Color(0.35f, 0.85f, 0.45f, 1f);

    // ---------------- 로비 버튼
    [MenuItem("Tools/Lobby/Add Multi Game Buttons")]
    public static void AddLobbyButtons()
    {
        BattleCanvas battle = Object.FindFirstObjectByType<BattleCanvas>(FindObjectsInactive.Include);
        Transform src = battle != null ? battle.transform.Find("Panel/DifficultyButton") : null;
        if (src == null)
        {
            Debug.LogWarning("[MultiUiBuilder] Lobby 씬 BattleCanvas/Panel/DifficultyButton 없음");
            return;
        }
        foreach (string n in new[] { "MultiGameButton", "LocalResume" })
        {
            Transform old = src.parent.Find(n);
            if (old != null)
                Undo.DestroyObjectImmediate(old.gameObject);
        }
        GameObject multi = CloneButton(src, "MultiGameButton", "Multi_StartButton", new Vector2(-380f, 544f));
        var mb = multi.AddComponent<MultiGameButton>();
        mb.labelText = multi.GetComponentInChildren<TMP_Text>(true);
        // 전적 "우승 N · 클리어 N" (멀티 게임 버튼 아래)
        Transform oldRecord = src.parent.Find("MultiRecordText");
        if (oldRecord != null)
            Undo.DestroyObjectImmediate(oldRecord.gameObject);
        TMP_Text record = NewText("MultiRecordText", src.parent, 26f, TextAlignmentOptions.Center, Color.white);
        Undo.RegisterCreatedObjectUndo(record.gameObject, "Multi buttons");
        CopyRect((RectTransform)src, record.rectTransform);
        record.rectTransform.anchoredPosition = new Vector2(-380f, 462f);
        record.rectTransform.sizeDelta = new Vector2(300f, 40f);
        record.text = "우승 0 · 클리어 0";
        record.gameObject.AddComponent<MultiRecordText>();

        // (2026-11-27) 이어하기 버튼 삭제 (대기 중 로컬 게임·이어하기 없음)

        EditorSceneManager.MarkSceneDirty(battle.gameObject.scene);
        Debug.Log("[MultiUiBuilder] 로비 멀티 게임·이어하기 버튼 생성 (씬 저장은 직접)");
    }

    static GameObject CloneButton(Transform src, string name, string key, Vector2 pos)
    {
        GameObject go = Object.Instantiate(src.gameObject, src.parent);
        Undo.RegisterCreatedObjectUndo(go, "Multi buttons");
        go.name = name;
        foreach (MonoBehaviour mb in go.GetComponents<MonoBehaviour>().Where(m => !(m is Button) && !(m is Graphic)).ToArray())
            Object.DestroyImmediate(mb);
        Button button = go.GetComponent<Button>();
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(button.onClick, i);
        var rt = (RectTransform)go.transform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(220f, 120f);
        TMP_Text text = go.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            foreach (TranslateText tt in text.GetComponents<TranslateText>())
                Object.DestroyImmediate(tt);
            text.text = TranslateManager.GetText(key);
            text.enableAutoSizing = true;
            text.fontSizeMin = 20;
            text.fontSizeMax = 44;
            var t = text.gameObject.AddComponent<TranslateText>();
            t.key = key;
        }
        // 복사본에 남은 다른 글씨(난이도 이름 등)는 끔
        foreach (TMP_Text extra in go.GetComponentsInChildren<TMP_Text>(true).Where(x => x != text))
            extra.gameObject.SetActive(false);
        return go;
    }

    static void CopyRect(RectTransform src, RectTransform dst)
    {
        dst.anchorMin = src.anchorMin;
        dst.anchorMax = src.anchorMax;
        dst.pivot = src.pivot;
    }

    // ---------------- 로비 대기 창 (2026-11-27, 이전 오른쪽 위 MultiMatchStatus 프리팹 대체)
    [MenuItem("Tools/Lobby/Add Online Waiting Canvas")]
    public static void AddWaitingCanvas()
    {
        BattleCanvas battle = Object.FindFirstObjectByType<BattleCanvas>(FindObjectsInactive.Include);
        if (battle == null)
        {
            Debug.LogWarning("[MultiUiBuilder] Lobby 씬을 열고 실행");
            return;
        }
        Scene scene = battle.gameObject.scene;
        foreach (GameObject r in scene.GetRootGameObjects())
        {
            if (r.name == "OnlineWaitingCanvas")
                Undo.DestroyObjectImmediate(r);
            else
                foreach (Transform t in r.GetComponentsInChildren<Transform>(true).Where(t => t != null && t.name == "LocalResume").ToArray())
                    Undo.DestroyObjectImmediate(t.gameObject); // 예전 [이어하기]
        }

        var root = new GameObject("OnlineWaitingCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(root, "Online waiting");
        SceneManager.MoveGameObjectToScene(root, scene);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1500; // 로비 화면들(최대 1000) 위, 팝업·토스트(5000) 아래
        SetupScaler(root.GetComponent<CanvasScaler>());

        RectTransform dim = NewImage("Panel", root.transform, new Color(0f, 0f, 0f, 0.88f)); // 화면 전체를 막음
        Box(dim, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RectTransform box = NewImage("Box", dim, PANEL);
        Place(box, new Vector2(0f, 40f), new Vector2(960f, 1420f));

        TMP_Text title = NewText("TitleText", box, 60f, TextAlignmentOptions.Center, Color.white);
        Place(title.rectTransform, new Vector2(0f, 600f), new Vector2(880f, 90f));
        AddTranslate(title, "Multi_StartButton");
        TMP_Text record = NewText("RecordText", box, 32f, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.3f));
        Place(record.rectTransform, new Vector2(0f, 520f), new Vector2(880f, 50f));
        record.text = "우승 0 · 클리어 0";

        TMP_Text count = NewText("CountText", box, 230f, TextAlignmentOptions.Center, Color.white);
        Place(count.rectTransform, new Vector2(0f, 260f), new Vector2(880f, 280f));
        count.text = "1/8";
        TMP_Text timer = NewText("TimerText", box, 70f, TextAlignmentOptions.Center, new Color(0.6f, 0.9f, 1f));
        Place(timer.rectTransform, new Vector2(0f, 60f), new Vector2(880f, 100f));
        timer.text = "300초";
        TMP_Text notice = NewText("NoticeText", box, 38f, TextAlignmentOptions.Center, new Color(0.85f, 0.87f, 0.92f));
        Place(notice.rectTransform, new Vector2(0f, -40f), new Vector2(880f, 60f));
        notice.text = "다른 유저를 기다리는 중…";
        TMP_Text aiNotice = NewText("AiNoticeText", box, 26f, TextAlignmentOptions.Center, new Color(0.65f, 0.68f, 0.75f));
        Place(aiNotice.rectTransform, new Vector2(0f, -100f), new Vector2(880f, 44f));
        aiNotice.text = "부족한 인원은 AI봇으로 대체됩니다.";

        TMP_Text code = NewText("CodeText", box, 44f, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.3f));
        Place(code.rectTransform, new Vector2(0f, -220f), new Vector2(880f, 70f));
        code.text = "코드 1234567";
        RectTransform host = Badge(box, "HostMark", new Color(1f, 0.8f, 0.2f, 1f), "★");
        host.anchorMin = host.anchorMax = host.pivot = new Vector2(0.5f, 0.5f);
        host.anchoredPosition = new Vector2(250f, -220f);
        host.sizeDelta = new Vector2(44f, 44f);

        Button invite = WideButton(box, "InviteButton", "Multi_Invite", new Vector2(0f, -340f), new Color(0.25f, 0.55f, 0.95f, 1f)); // 방 코드 + 게임 링크
        Button cancel = WideButton(box, "CancelButton", "Multi_CancelButton", new Vector2(0f, -540f), new Color(0.75f, 0.25f, 0.25f, 1f));

        var panel = root.AddComponent<MultiWaitingPanel>();
        panel.root = dim.gameObject;
        panel.countText = count;
        panel.timerText = timer;
        panel.noticeText = notice;
        panel.aiNoticeText = aiNotice;
        panel.codeText = code;
        panel.recordText = record;
        panel.hostMark = host.gameObject;
        panel.cancelButton = cancel;
        panel.inviteButton = invite;
        dim.gameObject.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[MultiUiBuilder] 로비 온라인 게임 대기 창 생성 (씬 저장은 직접)");
    }

    static Button WideButton(RectTransform parent, string name, string key, Vector2 pos, Color color)
    {
        RectTransform b = NewImage(name, parent, color);
        Place(b, pos, new Vector2(360f, 120f));
        Button button = b.gameObject.AddComponent<Button>();
        TMP_Text t = NewText("Text", b, 42f, TextAlignmentOptions.Center, Color.white);
        Box(t.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddTranslate(t, key);
        return button;
    }

    // 패널 아래쪽 작은 버튼 (가로 범위 xMin~xMax)
    static Button SmallButton(RectTransform parent, string name, string key, Vector2 min, Vector2 max, Color color)
    {
        RectTransform b = NewImage(name, parent, color);
        b.anchorMin = min;
        b.anchorMax = new Vector2(max.x, 0f);
        b.pivot = new Vector2(0.5f, 0f);
        b.offsetMin = new Vector2(10f, 10f);
        b.offsetMax = new Vector2(-10f, 54f);
        Button button = b.gameObject.AddComponent<Button>();
        TMP_Text t = NewText("Text", b, 24f, TextAlignmentOptions.Center, Color.white);
        Box(t.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddTranslate(t, key);
        return button;
    }

    // ---------------- 직접 수정한 씬에 새 부분만 추가 (2026-11-27)
    // MultiGameUI: Ready 줄 템플릿에 방장 표시 (없을 때만)
    [MenuItem("Tools/Multi/Add Host Mark To Ready Rows")]
    public static void AddHostMark()
    {
        MultiReadyPanel ready = Object.FindFirstObjectByType<MultiReadyPanel>(FindObjectsInactive.Include);
        if (ready == null || ready.rowTemplate == null)
        {
            Debug.LogWarning("[MultiUiBuilder] MultiGameUI 씬(Ready 창 줄 템플릿)을 열고 실행");
            return;
        }
        MultiPlayerRow row = ready.rowTemplate;
        if (row.hostMark == null)
        {
            RectTransform mark = NewImage("HostMark", row.transform, new Color(1f, 0.8f, 0.2f, 1f));
            Undo.RegisterCreatedObjectUndo(mark.gameObject, "Host mark");
            mark.anchorMin = mark.anchorMax = mark.pivot = new Vector2(1f, 0.5f);
            mark.anchoredPosition = new Vector2(-214f, 0f);
            mark.sizeDelta = new Vector2(100f, 50f);
            TMP_Text t = NewText("Text", mark, 28f, TextAlignmentOptions.Center, Color.black);
            Box(t.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddTranslate(t, "Multi_Host");
            mark.gameObject.SetActive(false);
            Undo.RecordObject(row, "Host mark");
            row.hostMark = mark.gameObject;
            EditorUtility.SetDirty(row);
        }
        EditorSceneManager.MarkSceneDirty(ready.gameObject.scene);
        Debug.Log("[MultiUiBuilder] Ready 줄 방장 표시 추가 (씬 저장은 직접)");
    }

    // (2026-11-27) MultiGameUI: 다른 유저 목록에서 순위 글씨 삭제 (줄의 RankText · 위쪽 MyRankText), 닉네임·킹 글씨를 왼쪽으로 당김
    [MenuItem("Tools/Multi/Remove Rank From Players Panel")]
    public static void RemoveRankFromPlayersPanel()
    {
        MultiPlayersPanel panel = Object.FindFirstObjectByType<MultiPlayersPanel>(FindObjectsInactive.Include);
        if (panel == null)
        {
            Debug.LogWarning("[MultiUiBuilder] MultiGameUI 씬을 열고 실행");
            return;
        }
        if (panel.myRankText != null)
            Undo.DestroyObjectImmediate(panel.myRankText.gameObject);
        MultiStatusRow row = panel.rowTemplate;
        if (row != null)
        {
            Transform rank = row.transform.Find("RankText");
            if (rank != null)
            {
                float shift = ((RectTransform)rank).offsetMax.x;
                Undo.DestroyObjectImmediate(rank.gameObject);
                foreach (TMP_Text t in new[] { row.nicknameText, row.kingText })
                    if (t != null && t.rectTransform.offsetMin.x >= shift)
                    {
                        Undo.RecordObject(t.rectTransform, "Remove rank");
                        t.rectTransform.offsetMin = new Vector2(t.rectTransform.offsetMin.x - shift + 6f, t.rectTransform.offsetMin.y);
                    }
            }
        }
        EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
        Debug.Log("[MultiUiBuilder] 다른 유저 목록 순위 글씨 삭제");
    }

    // Lobby: [멀티 게임] 위에 작은 글씨 [방만들기]·[코드입력] + 코드 입력 창 (있으면 새로)
    [MenuItem("Tools/Lobby/Add Multi Room Buttons")]
    public static void AddRoomButtons()
    {
        MultiGameButton quick = Object.FindObjectsByType<MultiGameButton>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(b => b.mode == MultiMatchManager.JoinMode.Quick);
        if (quick == null)
        {
            Debug.LogWarning("[MultiUiBuilder] Lobby 씬 [멀티 게임] 버튼(MultiGameButton)이 없음 — Tools/Lobby/Add Multi Game Buttons 먼저");
            return;
        }
        Transform parent = quick.transform.parent;
        foreach (string n in new[] { "MultiCreateRoomButton", "MultiEnterCodeButton", "MultiCodeInputPanel" })
        {
            Transform old = FindDeep(quick.transform.root, n);
            if (old != null)
                Undo.DestroyObjectImmediate(old.gameObject);
        }
        var qrt = (RectTransform)quick.transform;
        float topY = qrt.anchoredPosition.y + qrt.sizeDelta.y * (1f - qrt.pivot.y);
        GameObject create = TextButton(parent, qrt, "MultiCreateRoomButton", "Multi_CreateRoom", new Vector2(qrt.anchoredPosition.x - 58f, topY + 28f));
        GameObject codeBtn = TextButton(parent, qrt, "MultiEnterCodeButton", "Multi_EnterCode", new Vector2(qrt.anchoredPosition.x + 58f, topY + 28f));
        create.AddComponent<MultiGameButton>().mode = MultiMatchManager.JoinMode.Create;
        var codeComp = codeBtn.AddComponent<MultiGameButton>();
        codeComp.mode = MultiMatchManager.JoinMode.Code;

        // 코드 입력 창 (캔버스 맨 위)
        Canvas canvas = quick.GetComponentInParent<Canvas>();
        RectTransform panel = NewImage("MultiCodeInputPanel", canvas.transform, new Color(0f, 0f, 0f, 0.8f));
        Undo.RegisterCreatedObjectUndo(panel.gameObject, "Room buttons");
        Box(panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RectTransform box = NewImage("Box", panel, PANEL);
        Place(box, new Vector2(0f, 80f), new Vector2(860f, 600f));
        TMP_Text title = NewText("TitleText", box, 48f, TextAlignmentOptions.Center, Color.white);
        Place(title.rectTransform, new Vector2(0f, 220f), new Vector2(800f, 80f));
        AddTranslate(title, "Multi_CodeInputTitle");
        GameObject inputGo = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
        inputGo.name = "InputField";
        var irt = (RectTransform)inputGo.transform;
        irt.SetParent(box, false);
        Place(irt, new Vector2(0f, 70f), new Vector2(620f, 130f));
        TMP_InputField input = inputGo.GetComponent<TMP_InputField>();
        foreach (TMP_Text t in inputGo.GetComponentsInChildren<TMP_Text>(true))
        {
            if (Font != null) t.font = Font;
            t.fontSize = 60f;
            t.alignment = TextAlignmentOptions.Center;
        }
        if (input.placeholder is TMP_Text ph)
            ph.text = "0000000";
        TMP_Text error = NewText("ErrorText", box, 28f, TextAlignmentOptions.Center, new Color(1f, 0.45f, 0.45f));
        Place(error.rectTransform, new Vector2(0f, -30f), new Vector2(800f, 50f));
        error.text = "";
        RectTransform enter = NewImage("EnterButton", box, READY);
        Place(enter, new Vector2(110f, -190f), new Vector2(300f, 120f));
        Button enterButton = enter.gameObject.AddComponent<Button>();
        TMP_Text enterText = NewText("Text", enter, 46f, TextAlignmentOptions.Center, Color.white);
        Box(enterText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddTranslate(enterText, "Multi_CodeEnter");
        RectTransform close = NewImage("CloseButton", box, new Color(0.45f, 0.48f, 0.55f, 1f));
        Place(close, new Vector2(-210f, -190f), new Vector2(260f, 120f));
        Button closeButton = close.gameObject.AddComponent<Button>();
        TMP_Text closeText = NewText("Text", close, 46f, TextAlignmentOptions.Center, Color.white);
        Box(closeText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddTranslate(closeText, "Multi_Close");
        var codePanel = panel.gameObject.AddComponent<MultiCodeInputPanel>();
        codePanel.inputField = input;
        codePanel.enterButton = enterButton;
        codePanel.closeButton = closeButton;
        codePanel.errorText = error;
        codeComp.codeInputPanel = codePanel;
        panel.gameObject.SetActive(false);

        EditorSceneManager.MarkSceneDirty(quick.gameObject.scene);
        Debug.Log("[MultiUiBuilder] 로비 방만들기·코드입력 버튼 + 코드 입력 창 생성 (씬 저장은 직접)");
    }

    // 글씨만 있는 작은 버튼 (밑줄 느낌, 배경 투명)
    static GameObject TextButton(Transform parent, RectTransform like, string name, string key, Vector2 pos)
    {
        RectTransform rt = NewRect(name, parent);
        Undo.RegisterCreatedObjectUndo(rt.gameObject, "Room buttons");
        rt.anchorMin = like.anchorMin;
        rt.anchorMax = like.anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(112f, 44f);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.001f); // 누르는 영역
        rt.gameObject.AddComponent<Button>();
        TMP_Text t = NewText("Text", rt, 26f, TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.55f));
        Box(t.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        t.fontStyle = FontStyles.Underline;
        AddTranslate(t, key);
        return rt.gameObject;
    }

    static Transform FindDeep(Transform root, string name) =>
        root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);

    // ---------------- MultiGameUI 씬 (Ready 창)
    [MenuItem("Tools/Multi/Create MultiGameUI Scene")]
    public static void CreateUiScene()
    {
        Scene active = SceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

        var root = new GameObject("MultiGameUICanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        SceneManager.MoveGameObjectToScene(root, scene);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 250;
        SetupScaler(root.GetComponent<CanvasScaler>());

        // 다른 유저 목록 (오른쪽 위) · 보기 (전체 화면) — Ready·결과 창보다 아래 (형제 순서)
        MultiSpectatePanel spectate = BuildSpectate(root.transform);
        BuildPlayersPanel(root.transform, spectate);
        spectate.transform.SetAsLastSibling();

        // Ready 창: 화면 전체 어둡게 + 가운데 상자
        RectTransform ready = NewImage("ReadyPanel", root.transform, new Color(0f, 0f, 0f, 0.85f));
        Box(ready, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RectTransform box = NewImage("Box", ready, PANEL);
        box.anchorMin = box.anchorMax = new Vector2(0.5f, 0.5f);
        box.sizeDelta = new Vector2(940f, 1500f);

        TMP_Text title = NewText("TitleText", box, 56f, TextAlignmentOptions.Center, Color.white);
        Place(title.rectTransform, new Vector2(0f, 650f), new Vector2(860f, 90f));
        AddTranslate(title, "Multi_ReadyTitle");
        TMP_Text countdown = NewText("CountdownText", box, 130f, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.3f));
        Place(countdown.rectTransform, new Vector2(0f, 520f), new Vector2(400f, 160f));
        countdown.text = "10";

        RectTransform rows = NewRect("Rows", box);
        Place(rows, new Vector2(0f, -20f), new Vector2(860f, 8 * 96f + 7 * 10f));
        var vlg = rows.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10f;
        vlg.childControlWidth = vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        RectTransform row = NewImage("PlayerRow_Template", rows, ROW);
        var le = row.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = le.minHeight = 96f;
        TMP_Text nick = NewText("NicknameText", row, 38f, TextAlignmentOptions.MidlineLeft, Color.white);
        Box(nick.rectTransform, Vector2.zero, Vector2.one, new Vector2(30f, 0f), new Vector2(-220f, 0f));
        nick.text = "닉네임";
        RectTransform readyMark = NewImage("ReadyMark", row, READY);
        readyMark.anchorMin = readyMark.anchorMax = readyMark.pivot = new Vector2(1f, 0.5f);
        readyMark.anchoredPosition = new Vector2(-20f, 0f);
        readyMark.sizeDelta = new Vector2(180f, 64f);
        TMP_Text readyText = NewText("Text", readyMark, 32f, TextAlignmentOptions.Center, Color.white);
        Box(readyText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        readyText.text = "READY";
        RectTransform meMark = NewImage("MeMark", row, new Color(1f, 0.85f, 0.3f, 1f));
        meMark.anchorMin = new Vector2(0f, 0f);
        meMark.anchorMax = new Vector2(0f, 1f);
        meMark.pivot = new Vector2(0f, 0.5f);
        meMark.sizeDelta = new Vector2(10f, 0f);
        var rowComp = row.gameObject.AddComponent<MultiPlayerRow>();
        rowComp.nicknameText = nick;
        rowComp.readyMark = readyMark.gameObject;
        rowComp.meMark = meMark.gameObject;
        row.gameObject.SetActive(false);

        RectTransform btn = NewImage("ReadyButton", box, READY);
        Place(btn, new Vector2(0f, -620f), new Vector2(520f, 150f));
        Button readyButton = btn.gameObject.AddComponent<Button>();
        TMP_Text btnText = NewText("Text", btn, 56f, TextAlignmentOptions.Center, Color.white);
        Box(btnText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        btnText.text = "READY";

        var panel = ready.gameObject.AddComponent<MultiReadyPanel>();
        panel.root = ready.gameObject;
        panel.countdownText = countdown;
        panel.titleText = title;
        panel.readyButton = readyButton;
        panel.readyButtonText = btnText;
        panel.rowTemplate = rowComp;
        panel.rowContainer = rows;

        BuildResultPanel(root.transform);

        EditorSceneManager.SaveScene(scene, UI_SCENE);
        EditorSceneManager.CloseScene(scene, true);
        if (active.IsValid())
            SceneManager.SetActiveScene(active);

        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == UI_SCENE))
        {
            scenes.Add(new EditorBuildSettingsScene(UI_SCENE, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        Debug.Log($"[MultiUiBuilder] {UI_SCENE} 생성 + 빌드 설정 추가");
    }

    static MultiPlayersPanel BuildPlayersPanel(Transform parent, MultiSpectatePanel spectate)
    {
        RectTransform panel = NewRect("PlayersPanel", parent);
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(1f, 1f);
        panel.anchoredPosition = new Vector2(-12f, -150f);
        panel.sizeDelta = new Vector2(580f, 820f);

        RectTransform toggle = NewImage("ToggleButton", panel, PANEL);
        toggle.anchorMin = toggle.anchorMax = toggle.pivot = new Vector2(1f, 1f);
        toggle.anchoredPosition = Vector2.zero;
        toggle.sizeDelta = new Vector2(580f, 64f);
        Button toggleButton = toggle.gameObject.AddComponent<Button>();
        TMP_Text toggleText = NewText("Text", toggle, 30f, TextAlignmentOptions.MidlineRight, Color.white);
        Box(toggleText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0f, 0f), new Vector2(-20f, 0f));
        toggleText.text = "참가자 ▲";

        RectTransform list = NewImage("List", panel, new Color(0f, 0f, 0f, 0.55f));
        list.anchorMin = list.anchorMax = list.pivot = new Vector2(1f, 1f);
        list.anchoredPosition = new Vector2(0f, -70f);
        list.sizeDelta = new Vector2(580f, 7 * 100f + 6 * 6f + 16f);
        var vlg = list.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 6f;
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.childControlWidth = vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        RectTransform row = NewImage("StatusRow_Template", list, ROW);
        var le = row.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = le.minHeight = 100f;
        TMP_Text nick = Line(row, "NicknameText", 26f, 30f, Color.white, 12f, 140f);
        nick.text = "닉네임";
        TMP_Text wave = NewText("WaveText", row, 26f, TextAlignmentOptions.MidlineRight, new Color(0.6f, 0.9f, 1f));
        Box(wave.rectTransform, new Vector2(1f, 0.66f), new Vector2(1f, 1f), new Vector2(-230f, 0f), new Vector2(-116f, -2f));
        wave.text = "WAVE 12";
        TMP_Text king = Line(row, "KingText", 22f, 0f, new Color(0.85f, 0.85f, 0.9f), 12f, 116f);
        king.text = "Lv.5 기사";
        RectTransform view = NewImage("ViewButton", row, new Color(0.25f, 0.55f, 0.95f, 1f));
        view.anchorMin = view.anchorMax = view.pivot = new Vector2(1f, 0.5f);
        view.anchoredPosition = new Vector2(-10f, 0f);
        view.sizeDelta = new Vector2(100f, 76f);
        Button viewButton = view.gameObject.AddComponent<Button>();
        TMP_Text viewText = NewText("Text", view, 28f, TextAlignmentOptions.Center, Color.white);
        Box(viewText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddTranslate(viewText, "Multi_View");
        RectTransform reached = Badge(row, "ReachedMark", new Color(1f, 0.8f, 0.2f, 1f), "★");
        var rowComp = row.gameObject.AddComponent<MultiStatusRow>();
        rowComp.nicknameText = nick;
        rowComp.waveText = wave;
        rowComp.kingText = king;
        rowComp.viewButton = viewButton;
        rowComp.reachedMark = reached.gameObject;
        row.gameObject.SetActive(false);

        var comp = panel.gameObject.AddComponent<MultiPlayersPanel>();
        comp.listRoot = list.gameObject;
        comp.toggleButton = toggleButton;
        comp.toggleText = toggleText;
        comp.rowTemplate = rowComp;
        comp.rowContainer = list;
        comp.spectatePanel = spectate;
        return comp;
    }

    // 줄 하나 (y = 가운데 기준, 왼쪽 여백 left, 오른쪽 여백 right)
    static TMP_Text Line(RectTransform parent, string name, float size, float y, Color color, float left, float right)
    {
        TMP_Text t = NewText(name, parent, size, TextAlignmentOptions.MidlineLeft, color);
        t.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        t.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        t.rectTransform.offsetMin = new Vector2(left, y - 16f);
        t.rectTransform.offsetMax = new Vector2(-right, y + 16f);
        t.enableWordWrapping = false;
        t.overflowMode = TextOverflowModes.Ellipsis;
        return t;
    }

    static RectTransform Badge(RectTransform parent, string name, Color color, string label)
    {
        RectTransform b = NewImage(name, parent, color);
        b.anchorMin = b.anchorMax = b.pivot = new Vector2(0f, 1f);
        b.anchoredPosition = new Vector2(4f, -4f);
        b.sizeDelta = new Vector2(30f, 30f);
        TMP_Text t = NewText("Text", b, 22f, TextAlignmentOptions.Center, Color.white);
        Box(t.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        t.text = label;
        b.gameObject.SetActive(false);
        return b;
    }

    static MultiSpectatePanel BuildSpectate(Transform parent)
    {
        RectTransform panel = NewImage("SpectatePanel", parent, new Color(0.05f, 0.05f, 0.07f, 0.98f));
        Box(panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TMP_Text nick = NewText("NicknameText", panel, 56f, TextAlignmentOptions.Center, Color.white);
        Place(nick.rectTransform, new Vector2(0f, 860f), new Vector2(1100f, 80f));
        nick.text = "닉네임";
        TMP_Text info = NewText("InfoText", panel, 34f, TextAlignmentOptions.Center, new Color(0.6f, 0.9f, 1f));
        Place(info.rectTransform, new Vector2(0f, 780f), new Vector2(1100f, 60f));
        info.text = "W12 · Lv.5 기사 · 진행 중 W12";
        RectTransform board = NewRect("Board", panel);
        Place(board, new Vector2(0f, 80f), new Vector2(1120f, 1240f));
        TMP_Text minions = NewText("MinionsText", panel, 28f, TextAlignmentOptions.Top, new Color(0.85f, 0.87f, 0.92f));
        Place(minions.rectTransform, new Vector2(0f, -640f), new Vector2(1120f, 150f));
        minions.text = "[불똥이 lv3][뭉치 lv1]";

        // 칸 템플릿
        RectTransform cell = NewImage("Cell_Template", board, new Color(0.25f, 0.27f, 0.32f, 1f));
        cell.sizeDelta = new Vector2(150f, 150f);
        RectTransform minionImg = NewRect("MinionImage", cell);
        Box(minionImg, Vector2.zero, Vector2.one, new Vector2(10f, 18f), new Vector2(-10f, -10f));
        Image mi = minionImg.gameObject.AddComponent<Image>();
        mi.preserveAspect = true;
        mi.raycastTarget = false;
        TMP_Text lv = NewText("LvText", cell, 26f, TextAlignmentOptions.BottomRight, Color.white);
        Box(lv.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 2f), new Vector2(-6f, 0f));
        lv.text = "lv3";
        RectTransform evo = Badge(cell, "EvolvedMark", new Color(1f, 0.8f, 0.2f, 1f), "★");
        RectTransform runeImg = NewRect("RuneImage", cell);
        runeImg.anchorMin = runeImg.anchorMax = runeImg.pivot = new Vector2(1f, 1f);
        runeImg.anchoredPosition = new Vector2(-4f, -4f);
        runeImg.sizeDelta = new Vector2(44f, 44f);
        Image ri = runeImg.gameObject.AddComponent<Image>();
        ri.preserveAspect = true;
        ri.raycastTarget = false;
        RectTransform king = NewImage("KingMark", cell, new Color(1f, 0.85f, 0.3f, 0.9f));
        Box(king, new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.85f), Vector2.zero, Vector2.zero);
        TMP_Text kingText = NewText("Text", king, 40f, TextAlignmentOptions.Center, Color.black);
        Box(kingText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        kingText.text = "KING";
        RectTransform blocked = NewImage("BlockedMark", cell, new Color(0f, 0f, 0f, 0.75f));
        Box(blocked, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var cellComp = cell.gameObject.AddComponent<MultiSpectateCell>();
        cellComp.bgImage = cell.GetComponent<Image>();
        cellComp.minionImage = mi;
        cellComp.lvText = lv;
        cellComp.evolvedMark = evo.gameObject;
        cellComp.runeImage = ri;
        cellComp.kingMark = king.gameObject;
        cellComp.blockedMark = blocked.gameObject;
        cell.gameObject.SetActive(false);

        RectTransform close = NewImage("CloseButton", panel, new Color(0.75f, 0.25f, 0.25f, 1f));
        Place(close, new Vector2(0f, -860f), new Vector2(420f, 130f));
        Button closeButton = close.gameObject.AddComponent<Button>();
        TMP_Text closeText = NewText("Text", close, 48f, TextAlignmentOptions.Center, Color.white);
        Box(closeText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddTranslate(closeText, "Multi_Close");

        var comp = panel.gameObject.AddComponent<MultiSpectatePanel>();
        comp.root = panel.gameObject;
        comp.nicknameText = nick;
        comp.infoText = info;
        comp.minionsText = minions;
        comp.boardRoot = board;
        comp.cellTemplate = cellComp;
        comp.closeButton = closeButton;
        panel.gameObject.SetActive(false);
        return comp;
    }

    static void BuildResultPanel(Transform parent)
    {
        RectTransform panel = NewImage("ResultPanel", parent, new Color(0f, 0f, 0f, 0.85f));
        Box(panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RectTransform box = NewImage("Box", panel, PANEL);
        box.anchorMin = box.anchorMax = new Vector2(0.5f, 0.5f);
        box.sizeDelta = new Vector2(940f, 1500f);
        TMP_Text title = NewText("TitleText", box, 80f, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.3f));
        Place(title.rectTransform, new Vector2(0f, 630f), new Vector2(860f, 110f));
        title.text = "우승!";
        TMP_Text record = NewText("RecordText", box, 34f, TextAlignmentOptions.Center, Color.white);
        Place(record.rectTransform, new Vector2(0f, 540f), new Vector2(860f, 60f));
        record.text = "우승 0 · 클리어 0";

        RectTransform rows = NewRect("Rows", box);
        Place(rows, new Vector2(0f, -20f), new Vector2(860f, 8 * 96f + 7 * 10f));
        var vlg = rows.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10f;
        vlg.childControlWidth = vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        RectTransform row = NewImage("ResultRow_Template", rows, ROW);
        var le = row.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = le.minHeight = 96f;
        TMP_Text rank = NewText("RankText", row, 40f, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.3f));
        Box(rank.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(10f, 0f), new Vector2(80f, 0f));
        rank.text = "1";
        TMP_Text nick = NewText("NicknameText", row, 34f, TextAlignmentOptions.MidlineLeft, Color.white);
        Box(nick.rectTransform, new Vector2(0f, 0f), new Vector2(0.55f, 1f), new Vector2(96f, 0f), Vector2.zero);
        nick.text = "닉네임";
        TMP_Text status = NewText("StatusText", row, 30f, TextAlignmentOptions.MidlineRight, new Color(0.6f, 0.9f, 1f));
        Box(status.rectTransform, new Vector2(0.55f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-24f, 0f));
        status.text = "도착 12:34";
        RectTransform meMark = NewImage("MeMark", row, new Color(1f, 0.85f, 0.3f, 1f));
        meMark.anchorMin = new Vector2(0f, 0f);
        meMark.anchorMax = new Vector2(0f, 1f);
        meMark.pivot = new Vector2(0f, 0.5f);
        meMark.sizeDelta = new Vector2(10f, 0f);
        RectTransform champ = Badge(row, "ChampionMark", new Color(1f, 0.8f, 0.2f, 1f), "★");
        var rowComp = row.gameObject.AddComponent<MultiResultRow>();
        rowComp.rankText = rank;
        rowComp.nicknameText = nick;
        rowComp.statusText = status;
        rowComp.meMark = meMark.gameObject;
        rowComp.championMark = champ.gameObject;
        row.gameObject.SetActive(false);

        RectTransform lobby = NewImage("LobbyButton", box, READY);
        Place(lobby, new Vector2(0f, -630f), new Vector2(520f, 150f));
        Button lobbyButton = lobby.gameObject.AddComponent<Button>();
        TMP_Text lobbyText = NewText("Text", lobby, 52f, TextAlignmentOptions.Center, Color.white);
        Box(lobbyText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddTranslate(lobbyText, "Multi_ToLobby");

        var comp = panel.gameObject.AddComponent<MultiResultPanel>();
        comp.root = panel.gameObject;
        comp.titleText = title;
        comp.recordText = record;
        comp.rowTemplate = rowComp;
        comp.rowContainer = rows;
        comp.lobbyButton = lobbyButton;
        panel.gameObject.SetActive(false);
    }

    // ---------------- 공용
    static TMP_FontAsset Font => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/Main_Regular SDF.asset");
    static Material FontMat => AssetDatabase.FindAssets("Main_Regular Outline t:Material").Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault();
    static Sprite RoundSprite => AssetDatabase.FindAssets("Button_01_White_Bg t:Sprite").Select(g => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault();

    static void SetupScaler(CanvasScaler scaler)
    {
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1242f, 2208f);
        scaler.matchWidthOrHeight = 0f;
    }

    static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    static RectTransform NewImage(string name, Transform parent, Color color)
    {
        RectTransform rt = NewRect(name, parent);
        var img = rt.gameObject.AddComponent<Image>();
        Sprite s = RoundSprite;
        if (s != null)
        {
            img.sprite = s;
            img.type = Image.Type.Sliced;
        }
        img.color = color;
        return rt;
    }

    static TMP_Text NewText(string name, Transform parent, float size, TextAlignmentOptions align, Color color)
    {
        RectTransform rt = NewRect(name, parent);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (Font != null)
            t.font = Font;
        if (FontMat != null)
            t.fontSharedMaterial = FontMat;
        t.fontSize = size;
        t.alignment = align;
        t.color = color;
        t.raycastTarget = false;
        return t;
    }

    static void AddTranslate(TMP_Text text, string key)
    {
        text.text = TranslateManager.GetText(key);
        text.gameObject.AddComponent<TranslateText>().key = key;
    }

    static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void Box(RectTransform rt, Vector2 min, Vector2 max, Vector2 offMin, Vector2 offMax)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = offMin;
        rt.offsetMax = offMax;
    }
}
