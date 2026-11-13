using System.Collections.Generic;
using UnityEngine;

// 튜토리얼 실행 창구 (프리팹 Resources/Init/TutorialManager, 씬을 오가며 유지)
// 튜토리얼은 씬에 미리 두지 않는다: TryBegin(종류)에서 tutorialPrefabs 중 그 종류를 지금 씬에 생성해 시작하고,
// 끝나면 Tutorial이 자기 오브젝트(전용 Canvas 포함)를 삭제한다. 씬이 바뀌면 씬과 함께 사라짐(완료 기록 없음 → 다음에 처음부터)
// 한 번에 하나만 진행, 끝까지 진행한 종류는 PlayerPrefs에 기록해 다시 시작하지 않는다
public class TutorialManager : MonoSingleton<TutorialManager>
{
    const string COMPLETED_PREFS_KEY = "Tutorial_Completed_";

    [Header("종류별 튜토리얼 프리팹 (Tutorial.tutorialType으로 찾음)")]
    public List<Tutorial> tutorialPrefabs = new List<Tutorial>();

    public Tutorial CurrentTutorial { get; private set; }
    public bool IsPlaying => CurrentTutorial != null;

    // 테스트(플레이 진입 전에 켬) / BuildSetting.skipTutorial: 튜토리얼을 시작하지 않는다. 완료 기록은 건드리지 않음
    public const string DISABLE_FOR_TEST_KEY = "Tutorial.DisableForTest"; // 에디터 SessionState key
    public static bool IsDisabled
    {
        get
        {
#if UNITY_EDITOR
            if (UnityEditor.SessionState.GetBool(DISABLE_FOR_TEST_KEY, false))
                return true;
#endif
            return BuildSetting.Instance != null && BuildSetting.Instance.skipTutorial;
        }
    }

    // 지금 진행해야 하는 튜토리얼인지 (아직 안 끝냈고, 꺼 두지 않음)
    public static bool ShouldPlay(TutorialType tutorialType)
    {
        return !IsDisabled && !IsCompleted(tutorialType);
    }

    // 처음 실행한 유저는 로비 대신 바로 InGame으로 보낸다 (LobbyManager.Start). 앱을 켠 동안 한 번만
    // → 튜토리얼을 끝내지 못하고 로비로 돌아와도 다시 보내지 않음 (다음 실행 때 다시 시도)
    static bool isFirstPlayRedirected;
    public static bool TryRedirectToFirstPlay()
    {
        if (isFirstPlayRedirected || !ShouldPlay(TutorialType.FirstPlay))
            return false;
        isFirstPlayRedirected = true;
        return true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic()
    {
        isFirstPlayRedirected = false;
    }

    public static string GetCompletedPrefsKey(TutorialType tutorialType) => COMPLETED_PREFS_KEY + tutorialType;

    public static bool IsCompleted(TutorialType tutorialType)
    {
        return PlayerPrefs.GetInt(GetCompletedPrefsKey(tutorialType), 0) == 1;
    }

    public static void SetCompleted(TutorialType tutorialType, bool isCompleted)
    {
        PlayerPrefs.SetInt(GetCompletedPrefsKey(tutorialType), isCompleted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public Tutorial GetPrefab(TutorialType tutorialType)
    {
        for (int i = 0; i < tutorialPrefabs.Count; i++)
        {
            if (tutorialPrefabs[i] != null && tutorialPrefabs[i].tutorialType == tutorialType)
                return tutorialPrefabs[i];
        }
        return null;
    }

    // 시작했으면 true. 이미 완료했거나, 꺼 두었거나(IsDisabled), 다른 튜토리얼이 진행 중이거나, 프리팹이 없으면 false
    public bool TryBegin(TutorialType tutorialType)
    {
        if (IsPlaying || !ShouldPlay(tutorialType))
            return false;
        Tutorial prefab = GetPrefab(tutorialType);
        if (prefab == null)
        {
            Debug.LogError($"TutorialManager 튜토리얼 프리팹 없음: {tutorialType}");
            return false;
        }

        // 부모 없이 생성 → 지금 씬에 속해 씬이 바뀌면 같이 사라진다
        Tutorial tutorial = Instantiate(prefab);
        tutorial.name = prefab.name;
        if (!tutorial.CanBegin())
        {
            Destroy(tutorial.gameObject);
            return false;
        }

        CurrentTutorial = tutorial;
        tutorial.onEnded += OnTutorialEnded;
        tutorial.Begin();
        return true;
    }

    void OnTutorialEnded(Tutorial tutorial)
    {
        tutorial.onEnded -= OnTutorialEnded;
        if (CurrentTutorial == tutorial)
            CurrentTutorial = null;

        SetCompleted(tutorial.tutorialType, true);
    }

    // Tutorial.OnDestroy: 끝나기 전에 사라진 경우(씬 이동 등) 진행 중 상태만 정리, 완료 기록 없음
    public void OnTutorialDestroyed(Tutorial tutorial)
    {
        if (CurrentTutorial != tutorial)
            return;
        tutorial.onEnded -= OnTutorialEnded;
        CurrentTutorial = null;
    }
}
