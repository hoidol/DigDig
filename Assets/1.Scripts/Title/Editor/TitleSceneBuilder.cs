using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// (2026-11-27) Title 씬을 실제로 만듦: Assets/4.Scenes/Title.unity (빌드 설정 맨 앞) — 다시 실행하면 새로
// 카메라 · EventSystem(열린 씬의 것을 복사, Lobby를 열고 실행) · TitleCanvas(게임 이름 · 서버 연결 중 · 로그인 창(게스트·Google·Apple) · 닉네임 창) · TitleManager
// 이후 모양은 씬에서 직접 수정
public static class TitleSceneBuilder
{
    const string SCENE_PATH = "Assets/4.Scenes/Title.unity";
    static readonly Color PANEL = new Color(0.08f, 0.08f, 0.11f, 0.92f);

    [MenuItem("Tools/Title/Create Title Scene")]
    public static void Build()
    {
        EventSystem srcEvent = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
        if (srcEvent == null)
        {
            Debug.LogWarning("[TitleSceneBuilder] EventSystem을 복사할 씬(Lobby)을 열고 실행");
            return;
        }
        Scene active = SceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

        var camGo = new GameObject("Main Camera", typeof(Camera));
        SceneManager.MoveGameObjectToScene(camGo, scene);
        camGo.tag = "MainCamera";
        var cam = camGo.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.12f, 0.18f);
        cam.orthographic = true;

        GameObject ev = Object.Instantiate(srcEvent.gameObject);
        ev.name = "EventSystem";
        SceneManager.MoveGameObjectToScene(ev, scene);

        var root = new GameObject("TitleCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        SceneManager.MoveGameObjectToScene(root, scene);
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1242f, 2208f);
        scaler.matchWidthOrHeight = 0f;

        TMP_Text title = NewText("GameTitle", root.transform, 110f, Color.white);
        Place(title.rectTransform, new Vector2(0f, 520f), new Vector2(1100f, 200f));
        title.text = PlayerSettings.productName;

        TMP_Text connecting = NewText("ConnectingText", root.transform, 40f, new Color(0.8f, 0.82f, 0.9f));
        Place(connecting.rectTransform, new Vector2(0f, -300f), new Vector2(900f, 80f));
        AddTranslate(connecting, "Title_Connecting");

        // 로그인 창
        RectTransform login = NewImage("LoginPanel", root.transform, PANEL);
        Place(login, new Vector2(0f, -320f), new Vector2(760f, 620f));
        TMP_Text loginTitle = NewText("TitleText", login, 48f, Color.white);
        Place(loginTitle.rectTransform, new Vector2(0f, 230f), new Vector2(700f, 80f));
        AddTranslate(loginTitle, "Title_LoginTitle");
        GameObject guest = LoginButton<FirebaseAnonyLoginButton>(login, "GuestLoginButton", new Vector2(0f, 90f), new Color(0.45f, 0.48f, 0.55f), "Guest", true);
        GameObject google = LoginButton<FirebaseGoogleLoginButton>(login, "GoogleLoginButton", new Vector2(0f, -40f), Color.white, "Google", false);
        GameObject apple = LoginButton<FirebaseAppleLoginButton>(login, "AppleLoginButton", new Vector2(0f, -170f), Color.black, "Apple", false);
        google.GetComponentInChildren<TMP_Text>().color = Color.black;

        // 닉네임 창
        RectTransform nick = NewImage("NicknamePanel", root.transform, new Color(0f, 0f, 0f, 0.8f));
        Box(nick);
        RectTransform box = NewImage("Box", nick, PANEL);
        Place(box, new Vector2(0f, 80f), new Vector2(900f, 640f));
        TMP_Text nickTitle = NewText("TitleText", box, 50f, Color.white);
        Place(nickTitle.rectTransform, new Vector2(0f, 230f), new Vector2(840f, 80f));
        AddTranslate(nickTitle, "Nickname_Title");

        GameObject inputGo = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
        inputGo.name = "InputField";
        var inputRt = (RectTransform)inputGo.transform;
        inputRt.SetParent(box, false);
        Place(inputRt, new Vector2(0f, 70f), new Vector2(760f, 130f));
        TMP_InputField input = inputGo.GetComponent<TMP_InputField>();
        foreach (TMP_Text t in inputGo.GetComponentsInChildren<TMP_Text>(true))
        {
            if (Font != null) t.font = Font;
            t.fontSize = 52f;
            t.alignment = TextAlignmentOptions.Center;
        }
        if (input.placeholder is TMP_Text ph)
            AddTranslate(ph, "Nickname_Placeholder");
        input.characterLimit = UserMyInfoManager.NICKNAME_MAX;

        TMP_Text error = NewText("ErrorText", box, 30f, new Color(0.8f, 0.82f, 0.9f));
        Place(error.rectTransform, new Vector2(0f, -40f), new Vector2(800f, 60f));
        error.text = "2~10자";
        RectTransform confirm = NewImage("ConfirmButton", box, new Color(0.35f, 0.85f, 0.45f, 1f));
        Place(confirm, new Vector2(0f, -190f), new Vector2(460f, 140f));
        Button confirmButton = confirm.gameObject.AddComponent<Button>();
        TMP_Text confirmText = NewText("Text", confirm, 52f, Color.white);
        Box(confirmText.rectTransform);
        AddTranslate(confirmText, "Nickname_Confirm");
        var nickPanel = nick.gameObject.AddComponent<NicknamePanel>();
        nickPanel.inputField = input;
        nickPanel.confirmButton = confirmButton;
        nickPanel.errorText = error;

        var manager = new GameObject("TitleManager").AddComponent<TitleManager>();
        SceneManager.MoveGameObjectToScene(manager.gameObject, scene);
        manager.connectingObject = connecting.gameObject;
        manager.loginPanel = login.gameObject;
        manager.googleButton = google;
        manager.appleButton = apple;
        manager.nicknamePanel = nickPanel;
        login.gameObject.SetActive(false);
        nick.gameObject.SetActive(false);

        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        EditorSceneManager.CloseScene(scene, true);
        if (active.IsValid())
            SceneManager.SetActiveScene(active);

        // 빌드 설정 맨 앞 (첫 씬)
        var scenes = EditorBuildSettings.scenes.Where(s => s.path != SCENE_PATH).ToList();
        scenes.Insert(0, new EditorBuildSettingsScene(SCENE_PATH, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log($"[TitleSceneBuilder] {SCENE_PATH} 생성 + 빌드 설정 첫 씬");
    }

    static GameObject LoginButton<T>(Transform parent, string name, Vector2 pos, Color color, string label, bool translateGuest) where T : Component
    {
        RectTransform rt = NewImage(name, parent, color);
        Place(rt, pos, new Vector2(600f, 110f));
        rt.gameObject.AddComponent<Button>();
        rt.gameObject.AddComponent<T>();
        TMP_Text t = NewText("Text", rt, 44f, Color.white);
        Box(t.rectTransform);
        if (translateGuest)
            AddTranslate(t, "Guest");
        else
            t.text = label;
        return rt.gameObject;
    }

    static TMP_FontAsset Font => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/Main_Regular SDF.asset");
    static Material FontMat => AssetDatabase.FindAssets("Main_Regular Outline t:Material").Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault();
    static Sprite RoundSprite => AssetDatabase.FindAssets("Button_01_White_Bg t:Sprite").Select(g => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault();

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
        if (RoundSprite != null)
        {
            img.sprite = RoundSprite;
            img.type = Image.Type.Sliced;
        }
        img.color = color;
        return rt;
    }

    static TMP_Text NewText(string name, Transform parent, float size, Color color)
    {
        RectTransform rt = NewRect(name, parent);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (Font != null) t.font = Font;
        if (FontMat != null) t.fontSharedMaterial = FontMat;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
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

    static void Box(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
