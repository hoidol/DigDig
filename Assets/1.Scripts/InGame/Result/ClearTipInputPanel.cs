using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-26) 결과 화면(STAGE CLEAR)일 때만: 작은 입력칸 "클리어 팁 남기기"(최대 40자) + 등록 버튼 → ClearTipManager.UploadAsync
// 이 스테이지·난이도를 클리어해야 보임(ResultCanvas가 기록을 저장한 뒤 Show). 올리면 입력칸 잠김 + 토스트, 실패하면 다시 시도 가능
// 금칙어는 올릴 때 *** 로 가려짐. 모양은 씬에 만들어 둠 (메뉴 Tools/InGame/Add ClearTip Input To ResultCanvas). 실행 중 생성 없음 → 씬에서 직접 수정
public class ClearTipInputPanel : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button submitButton;
    public TMP_Text submitText;
    [Tooltip("기본 모양 위치 (결과 창 가운데 기준)")] public Vector2 position = new Vector2(0f, -520f);

    string stageKey;
    bool submitted;
    bool uploading;
    RectTransform root;

    public bool Submitted => submitted;

    public void Show(string key)
    {
        stageKey = key;
        if (inputField == null || submitButton == null)
        {
            Debug.LogWarning("[ClearTipInputPanel] 입력칸·등록 버튼 연결 없음 → 메뉴 Tools/InGame/Add ClearTip Input To ResultCanvas");
            return;
        }
        submitted = false;
        uploading = false;
        inputField.text = "";
        inputField.interactable = true;
        submitButton.interactable = true;
        if (submitText != null)
            submitText.text = TranslateManager.GetText("ClearTip_Submit");
        RootObject.SetActive(true);
    }

    public void Hide()
    {
        if (RootObject != null)
            RootObject.SetActive(false);
    }

    // 켜고 끄는 묶음 (빌더가 만든 ClearTipInput, 없으면 입력칸의 부모)
    public GameObject rootObject;
    GameObject RootObject => rootObject != null ? rootObject : root != null ? root.gameObject : inputField != null ? inputField.transform.parent.gameObject : null;

    public async void OnClickedSubmit()
    {
        if (submitted || uploading || inputField == null)
            return;
        string text = ClearTipManager.Normalize(inputField.text);
        if (string.IsNullOrEmpty(text))
        {
            ToastCanvas.Toast(TranslateManager.GetText("ClearTip_EmptyInput"));
            return;
        }
        uploading = true;
        submitButton.interactable = false;
        bool ok = await ClearTipManager.UploadAsync(stageKey, text);
        if (this == null)
            return;
        uploading = false;
        if (ok)
        {
            submitted = true;
            inputField.interactable = false;
            if (submitText != null)
                submitText.text = TranslateManager.GetText("ClearTip_Done");
            ToastCanvas.Toast(TranslateManager.GetText("ClearTip_Uploaded"));
        }
        else
        {
            submitButton.interactable = true;
            ToastCanvas.Toast(TranslateManager.GetText("ClearTip_UploadFail"));
        }
    }

    // 기본 모양: 가로 입력칸 + 오른쪽 등록 버튼 (에디터 빌더가 씬에 만들 때 씀)
    public void BuildUI()
    {
        TMP_Text fontSrc = GetComponentInChildren<TMP_Text>(true);
        root = new GameObject("ClearTipInput", typeof(RectTransform)).GetComponent<RectTransform>();
        root.SetParent(transform, false);
        root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(900f, 90f);
        root.anchoredPosition = position;
        rootObject = root.gameObject;

        GameObject inputGo = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
        var irt = (RectTransform)inputGo.transform;
        irt.SetParent(root, false);
        irt.anchorMin = new Vector2(0f, 0f);
        irt.anchorMax = new Vector2(0.74f, 1f);
        irt.offsetMin = irt.offsetMax = Vector2.zero;
        inputGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.92f);
        inputField = inputGo.GetComponent<TMP_InputField>();
        inputField.characterLimit = ClearTipManager.MAX_LENGTH;
        inputField.lineType = TMP_InputField.LineType.SingleLine;
        foreach (TMP_Text t in inputGo.GetComponentsInChildren<TMP_Text>(true))
        {
            if (fontSrc != null)
            {
                t.font = fontSrc.font;
                t.fontSharedMaterial = fontSrc.fontSharedMaterial;
            }
            t.fontSize = 32f;
            t.alignment = TextAlignmentOptions.MidlineLeft;
        }
        if (inputField.placeholder is TMP_Text ph)
        {
            ph.text = string.Format(TranslateManager.GetText("ClearTip_Placeholder"), ClearTipManager.MAX_LENGTH);
            ph.color = new Color(0.4f, 0.4f, 0.45f, 0.8f);
        }
        if (inputField.textComponent != null)
            inputField.textComponent.color = new Color(0.12f, 0.12f, 0.15f);

        var btnGo = new GameObject("SubmitButton", typeof(RectTransform), typeof(Image), typeof(Button));
        var brt = (RectTransform)btnGo.transform;
        brt.SetParent(root, false);
        brt.anchorMin = new Vector2(0.76f, 0f);
        brt.anchorMax = new Vector2(1f, 1f);
        brt.offsetMin = brt.offsetMax = Vector2.zero;
        btnGo.GetComponent<Image>().color = new Color(0.25f, 0.6f, 1f, 1f);
        submitButton = btnGo.GetComponent<Button>();
        submitButton.onClick.AddListener(OnClickedSubmit);
        var tgo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var trt = (RectTransform)tgo.transform;
        trt.SetParent(brt, false);
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        submitText = tgo.GetComponent<TextMeshProUGUI>();
        if (fontSrc != null)
        {
            submitText.font = fontSrc.font;
            submitText.fontSharedMaterial = fontSrc.fontSharedMaterial;
        }
        submitText.fontSize = 34f;
        submitText.enableAutoSizing = true;
        submitText.fontSizeMin = 18f;
        submitText.fontSizeMax = 34f;
        submitText.alignment = TextAlignmentOptions.Center;
        submitText.color = Color.white;
        submitText.raycastTarget = false;
    }
}
