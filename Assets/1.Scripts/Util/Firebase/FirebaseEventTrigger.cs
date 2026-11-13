using System;
using UnityEngine;
using UnityEngine.UI;

// 코드 수정 없이 씬·프리팹에서 이벤트를 찍는 컴포넌트.
// - 버튼 오브젝트에 붙이면 클릭마다 기록 (logOnClick)
// - 창 등에 붙이면 켜질 때마다 기록 (logOnEnable)
// - 그 밖은 UnityEvent(인스펙터)에서 Log() 연결
public class FirebaseEventTrigger : MonoBehaviour
{
    [Serializable]
    public struct Param
    {
        public string key;
        public string value;
    }

    public string eventName;
    public Param[] parameters;
    public bool logOnClick = true;
    public bool logOnEnable;

    void Awake()
    {
        if (logOnClick && TryGetComponent(out Button button))
            button.onClick.AddListener(Log);
    }

    void OnEnable()
    {
        if (logOnEnable) Log();
    }

    public void Log()
    {
        var list = new (string, object)[parameters?.Length ?? 0];
        for (int i = 0; i < list.Length; i++)
            list[i] = (parameters[i].key, parameters[i].value);
        FirebaseEvent.Log(eventName, list);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!string.IsNullOrEmpty(eventName) && !FirebaseEvent.IsValidName(eventName))
            Debug.LogWarning($"[FirebaseEventTrigger] 잘못된 이벤트 이름: {eventName} ({name})", this);
    }
#endif
}
