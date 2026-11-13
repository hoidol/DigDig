using System;
using UnityEngine;
using UnityEngine.UI;

// 켜짐/꺼짐 스위치 (Layer Lab Swich_01: 하위 On / Off 오브젝트 중 하나만 보임). 누르면 반대로 바뀌고 onChanged 호출
[RequireComponent(typeof(Button))]
public class SettingSwitch : MonoBehaviour
{
    public GameObject onObject;
    public GameObject offObject;

    public bool IsOn { get; private set; }
    public Action<bool> onChanged;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Toggle);
    }

    // 표시만 바꿈 (onChanged 호출 안 함)
    public void SetOn(bool on)
    {
        IsOn = on;
        if (onObject != null)
            onObject.SetActive(on);
        if (offObject != null)
            offObject.SetActive(!on);
    }

    public void Toggle()
    {
        SetOn(!IsOn);
        onChanged?.Invoke(IsOn);
    }
}
