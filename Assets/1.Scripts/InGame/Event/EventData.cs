using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
#endif

//[CreateAssetMenu(fileName = "EventData", menuName = "Data/EventData", order = 0)]
[System.Serializable]
public class EventData
{
    public EventType[] eventTypes;
    public EventTrigger[] triggers;
    public float[] chances; // 0이면 100%
    public Vector2 distanceFromPlayer;

    [Header("LowHp 전용")]
    [Range(0f, 1f)]
    public float hpThreshold = 0.3f;

    [Header("OrdealEnd 전용 - 1 입력 시 첫 시련 클리어 시 등장")]
    public int ordealClearCount;

}

public enum EventTrigger
{
    OrdealEnd,
    LowHp
}
