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

    Action onDialogueTouched;
    bool isInit;
    Coroutine fingerRoutine;

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
