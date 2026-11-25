using System;
using System.Collections.Generic;
using UnityEngine;

// (2026-11-23) 인게임 미션 하나 = 조건 판정만 (상태·보상 지급은 InGameMissionSystem).
// 새 미션 종류 = 이 클래스를 상속해 IsComplete(필요하면 IsFailed·GetProgress)만 구현 → InGameMissionSetting 인스펙터에서 추가
// 판정은 MissionContext(지금 필드·wave 상황)만 보고 하므로 씬 없이 테스트 가능
[Serializable]
public abstract class Mission
{
    [Tooltip("완료 보상: 완료 후 처음 오는 브레이크타임 주사위 코인 (wave 클리어 순간에 완료되면 바로 그 브레이크타임)")]
    public int rewardDiceCoin = 1;
    [Tooltip("비우면 기본 문구(번역 TitleKey). {0}{1}… = 미션 수치")]
    public string customTitle;

    public string GetTitle()
    {
        string format = string.IsNullOrEmpty(customTitle) ? TranslateManager.GetText(TitleKey) : customTitle;
        try
        {
            return string.Format(format, GetTitleArgs());
        }
        catch (FormatException)
        {
            return format;
        }
    }

    // 기본 제목 번역 key (Translate.csv)
    public abstract string TitleKey { get; }
    protected virtual object[] GetTitleArgs() => Array.Empty<object>();

    // 완료 조건
    public abstract bool IsComplete(MissionContext ctx);
    // 더 이상 완료할 수 없음 (예: 정한 wave가 지나감)
    public virtual bool IsFailed(MissionContext ctx) => false;
    // 진행도 (goal 0 = 표시 안 함)
    public virtual (int cur, int goal) GetProgress(MissionContext ctx) => (0, 0);
}

// 판정 시점의 상황. InGameMissionSystem이 채움 (테스트는 직접 채움)
public class MissionContext
{
    public struct FieldMinion
    {
        public string key;
        public int mergeLevel; // 0부터 (표시 레벨 = mergeLevel + 1)
        public FieldMinion(string key, int mergeLevel) { this.key = key; this.mergeLevel = mergeLevel; }
    }

    public int waveIdx = -1;               // 마지막으로 시작한 wave (아직 없으면 -1)
    public int minionCountAtWaveStart;     // 그 wave를 시작할 때 필드 미니언 수
    public bool isWaveCleared;             // 지금 판정 = waveIdx wave를 클리어한 순간
    public readonly List<FieldMinion> fieldMinions = new(); // 지금 필드(타일 위) 미니언
}
