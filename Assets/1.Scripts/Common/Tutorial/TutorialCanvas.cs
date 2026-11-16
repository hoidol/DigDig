using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 대화창에 나오는 화자. 대화 블록은 key로 고른다 (이름·초상화는 여기 한 곳에서 관리)
[Serializable]
public class TutorialSpeaker
{
    public string key;      // 대화 블록의 speakerKey
    public string name;     // 번역 key면 번역문, 아니면 적은 글씨 그대로
    public Sprite portrait; // 없으면 초상화 숨김
}

// 튜토리얼 전용 화면 (Tutorial 오브젝트의 자식 Canvas). 화면 블록 + 화면 중앙 대화창 + 손가락 안내
// 블록 = 전체 화면 Raycast Target 이미지: UI 버튼과 필드 입력(MinionFieldController는 UI 위 터치를 무시)을 모두 막는다
public class TutorialCanvas : MonoBehaviour
{
    public GameObject blockObject;      // 전체 화면 투명 이미지 (Raycast Target)

    [Header("대화창")]
    public GameObject dialogueObject;   // 대화창 전체 (터치 영역 포함)
    public TMP_Text dialogueText;
    public TMP_Text speakerNameText;    // 화자 이름 (없어도 됨)
    public Image portraitImage;         // 화자 초상화 (없어도 됨)
    public Button dialogueTouchButton;  // 전체 화면 터치 → 다음 대사
    public List<TutorialSpeaker> speakers = new List<TutorialSpeaker>();

    [Header("손 안내 (TutorialHand). 손끝이 가리키는 점 = pivot, Raycast Target은 꺼 둘 것")]
    public RectTransform finger;
    public float fingerDragTime = 1f;   // 드래그 연출: 출발 → 도착 시간
    public float fingerHoldTime = 0.3f; // 드래그 연출: 출발·도착에서 멈추는 시간

    [Header("포커스 (대상 UI만 밝게, 나머지 어둡게 + 대상 밖 터치 막음). 비어 있으면 실행 중에 기본 모양으로 만든다")]
    public RectTransform focusRoot;     // 전체 화면, 평소엔 꺼 둠
    public Image focusHole;             // 구멍 (Mask, showMaskGraphic 꺼짐). 스프라이트 = 구멍 모양
    public TutorialFocusDim focusDim;   // 구멍의 자식, 어두운 화면 (구멍 밖에만 그려짐)
    public TutorialFocusBlocker focusBlocker; // 전체 화면 투명, 구멍 밖 터치 막음
    public Color focusDimColor = new Color(0, 0, 0, 0.7f);
    public float focusPadding = 16f;    // 대상 크기보다 넓힐 여백 (Canvas 단위)
    public float focusPulse = 8f;       // 구멍이 커졌다 작아지는 폭 (0 = 고정)
    public float focusFadeTime = 0.25f;
    public bool copyTargetShape = true; // 대상 Image의 스프라이트로 구멍 모양을 맞춤 (둥근 버튼이면 둥근 구멍)

    Action onDialogueTouched;
    bool isInit;
    Coroutine fingerRoutine;
    Transform focusTarget;
    float focusTime;
    static readonly Vector3[] corners = new Vector3[4];

    void Init()
    {
        if (isInit)
            return;
        isInit = true;
        if (dialogueTouchButton != null)
            dialogueTouchButton.onClick.AddListener(OnClickedDialogue);
    }

    public void SetBlock(bool isBlock)
    {
        if (blockObject != null)
            blockObject.SetActive(isBlock);
    }

    // 대사 표시. 화면을 터치하면 onTouched 호출 (다음 대사로 넘길지는 호출한 쪽이 정함)
    public void ShowDialogue(string speakerKey, string text, Action onTouched)
    {
        Init();
        onDialogueTouched = onTouched;

        TutorialSpeaker speaker = GetSpeaker(speakerKey);
        if (speakerNameText != null)
        {
            string speakerName = speaker != null ? DialogueTutorialStep.GetText(speaker.name) : "";
            speakerNameText.text = speakerName;
            speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(speakerName));
        }
        if (portraitImage != null)
        {
            portraitImage.sprite = speaker != null ? speaker.portrait : null;
            portraitImage.gameObject.SetActive(portraitImage.sprite != null);
        }

        if (dialogueText != null)
            dialogueText.text = text;
        if (dialogueObject != null)
            dialogueObject.SetActive(true);
    }

    public TutorialSpeaker GetSpeaker(string speakerKey)
    {
        for (int i = 0; i < speakers.Count; i++)
        {
            if (speakers[i].key == speakerKey)
                return speakers[i];
        }
        return null;
    }

    public void HideDialogue()
    {
        onDialogueTouched = null;
        if (dialogueObject != null)
            dialogueObject.SetActive(false);
    }

    void OnClickedDialogue()
    {
        onDialogueTouched?.Invoke();
    }

    // 손가락 드래그 연출: from → to를 HideFinger 할 때까지 반복 (대상이 움직여도 따라가도록 매 프레임 위치를 다시 구함)
    public void PlayFingerDrag(Transform from, Transform to)
    {
        HideFinger();
        if (finger == null || from == null || to == null)
            return;
        finger.gameObject.SetActive(true);
        fingerRoutine = StartCoroutine(CoFingerDrag(from, to));
    }

    // 손가락 터치 연출: target 위에서 눌렀다 떼는 모양을 HideFinger 할 때까지 반복
    public void PlayFingerTap(Transform target)
    {
        HideFinger();
        if (finger == null || target == null)
            return;
        finger.gameObject.SetActive(true);
        fingerRoutine = StartCoroutine(CoFingerTap(target));
    }

    public void HideFinger()
    {
        if (fingerRoutine != null)
        {
            StopCoroutine(fingerRoutine);
            fingerRoutine = null;
        }
        if (finger != null)
        {
            finger.localScale = Vector3.one;
            finger.gameObject.SetActive(false);
        }
    }

    IEnumerator CoFingerDrag(Transform from, Transform to)
    {
        while (from != null && to != null)
        {
            for (float t = 0; t < fingerHoldTime; t += Time.unscaledDeltaTime)
            {
                if (from == null) yield break;
                finger.position = GetScreenPos(from);
                yield return null;
            }
            for (float t = 0; t < fingerDragTime; t += Time.unscaledDeltaTime)
            {
                if (from == null || to == null) yield break;
                finger.position = Vector3.Lerp(GetScreenPos(from), GetScreenPos(to), Mathf.SmoothStep(0, 1, t / fingerDragTime));
                yield return null;
            }
            for (float t = 0; t < fingerHoldTime; t += Time.unscaledDeltaTime)
            {
                if (to == null) yield break;
                finger.position = GetScreenPos(to);
                yield return null;
            }
        }
    }

    IEnumerator CoFingerTap(Transform target)
    {
        float time = 0;
        while (target != null)
        {
            finger.position = GetScreenPos(target);
            finger.localScale = Vector3.one * (1f - 0.15f * Mathf.PingPong(time * 2f, 1f));
            time += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    // 포커스: target(UI 또는 필드 오브젝트)만 밝게 남기고 나머지를 어둡게. 구멍 안 터치만 아래로 통과
    public void ShowFocus(Transform target)
    {
        if (target == null)
        {
            HideFocus();
            return;
        }
        EnsureFocus();
        focusTarget = target;
        focusTime = 0;

        Image targetImage = target.GetComponent<Image>();
        Sprite shape = copyTargetShape && targetImage != null ? targetImage.sprite : null;
        focusHole.sprite = shape;
        focusHole.type = shape != null && targetImage.hasBorder ? Image.Type.Sliced : Image.Type.Simple;
        if (shape != null)
            focusHole.pixelsPerUnitMultiplier = targetImage.pixelsPerUnitMultiplier;

        focusRoot.gameObject.SetActive(true);
        UpdateFocus();
    }

    public void HideFocus()
    {
        focusTarget = null;
        if (focusRoot != null)
            focusRoot.gameObject.SetActive(false);
    }

    public bool IsFocusing => focusTarget != null && focusRoot != null && focusRoot.gameObject.activeInHierarchy;

    void LateUpdate()
    {
        if (focusTarget == null)
            return;
        focusTime += Time.unscaledDeltaTime;
        UpdateFocus();
    }

    // 대상이 움직여도 따라가도록 매 프레임 구멍 위치·크기를 다시 구한다
    void UpdateFocus()
    {
        if (focusTarget == null || !focusTarget.gameObject.activeInHierarchy)
        {
            // 대상이 꺼져 있으면 어둡게·막지 않음 (아무것도 못 누르는 상황 방지)
            focusDim.enabled = false;
            focusBlocker.gameObject.SetActive(false);
            return;
        }
        focusDim.enabled = true;
        focusBlocker.gameObject.SetActive(true);

        Rect rect = GetLocalRect(focusTarget, focusRoot);
        float pulse = focusPulse * Mathf.PingPong(focusTime * 2f, 1f);
        RectTransform holeRect = focusHole.rectTransform;
        holeRect.anchoredPosition = rect.center;
        holeRect.sizeDelta = rect.size + Vector2.one * (focusPadding + pulse) * 2f;

        // 어두운 화면은 구멍의 자식이지만 항상 전체 화면을 덮는다
        RectTransform dimRect = focusDim.rectTransform;
        dimRect.position = focusRoot.TransformPoint(focusRoot.rect.center);
        dimRect.sizeDelta = focusRoot.rect.size;

        Color color = focusDimColor;
        color.a *= focusFadeTime > 0 ? Mathf.Clamp01(focusTime / focusFadeTime) : 1f;
        focusDim.color = color;
    }

    // target이 화면에서 차지하는 영역을 space(이 Canvas 안) 좌표로. RectTransform이 아니면 위치 기준 기본 크기
    static Rect GetLocalRect(Transform target, RectTransform space)
    {
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        if (target is RectTransform targetRect)
        {
            Canvas canvas = target.GetComponentInParent<Canvas>();
            Canvas root = canvas != null ? canvas.rootCanvas : null;
            Camera cam = root != null && root.renderMode != RenderMode.ScreenSpaceOverlay
                ? (root.worldCamera != null ? root.worldCamera : Camera.main) : null;
            targetRect.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++)
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, corners[i]);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(space, screen, null, out Vector2 local);
                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(space, GetScreenPos(target), null, out Vector2 center);
        return new Rect(center - Vector2.one * 80f, Vector2.one * 160f);
    }

    // 포커스 오브젝트 중 없는 것을 기본 모양으로 만든다 (에디터 메뉴도 이걸로 프리팹에 굽는다)
    // 순서: 화면 블록 바로 위 → 대화창·손가락보다 아래에 그려짐
    public void EnsureFocus()
    {
        int layer = gameObject.layer;
        if (focusRoot == null)
        {
            focusRoot = CreateRect("Focus", transform, layer);
            Stretch(focusRoot);
            focusRoot.SetSiblingIndex(blockObject != null ? blockObject.transform.GetSiblingIndex() + 1 : 0);
            focusRoot.gameObject.SetActive(false);
        }
        if (focusHole == null)
        {
            RectTransform hole = CreateRect("Hole", focusRoot, layer);
            focusHole = hole.gameObject.AddComponent<Image>();
            focusHole.raycastTarget = false;
            Mask mask = hole.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;
        }
        if (focusDim == null)
        {
            RectTransform dim = CreateRect("Dim", focusHole.transform, layer);
            focusDim = dim.gameObject.AddComponent<TutorialFocusDim>();
            focusDim.color = focusDimColor;
        }
        focusDim.raycastTarget = false;
        if (focusBlocker == null)
        {
            RectTransform blocker = CreateRect("Blocker", focusRoot, layer);
            Stretch(blocker);
            Image image = blocker.gameObject.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0);
            image.raycastTarget = true;
            focusBlocker = blocker.gameObject.AddComponent<TutorialFocusBlocker>();
        }
        focusBlocker.hole = focusHole.rectTransform;
    }

    static RectTransform CreateRect(string name, Transform parent, int layer)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = layer;
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    // 대상(필드 오브젝트·월드 Canvas·화면 Canvas의 UI)의 화면 좌표. 손가락은 Overlay Canvas라 화면 좌표 = 위치
    public static Vector3 GetScreenPos(Transform target)
    {
        Canvas canvas = target.GetComponentInParent<Canvas>();
        if (canvas != null)
            canvas = canvas.rootCanvas;
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return target.position;

        Camera cam = canvas != null && canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        Vector3 pos = cam.WorldToScreenPoint(target.position);
        pos.z = 0;
        return pos;
    }
}
