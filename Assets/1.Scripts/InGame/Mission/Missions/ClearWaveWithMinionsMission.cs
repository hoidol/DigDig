using System;
using UnityEngine;

// 미니언 N기 이하로 wave X 클리어 (예: 미니언 하나로 Wave 1 클리어)
// 미니언 수 = 그 wave를 시작할 때 필드 미니언 수 (wave 중엔 카드를 놓을 수 없음). 0기는 인정 안 함
[Serializable]
public class ClearWaveWithMinionsMission : Mission
{
    [Tooltip("wave 번호 (화면 표시, 1부터)")]
    public int waveNumber = 1;
    [Tooltip("wave 시작 때 필드 미니언이 이 수 이하 (1기 이상)")]
    public int maxMinionCount = 1;

    public override string TitleKey => "Mission_ClearWaveWithMinions";
    protected override object[] GetTitleArgs() => new object[] { maxMinionCount, waveNumber };

    int TargetWaveIdx => waveNumber - 1;
    bool IsCountOk(int count) => count >= 1 && count <= maxMinionCount;

    public override bool IsComplete(MissionContext ctx)
        => ctx.isWaveCleared && ctx.waveIdx == TargetWaveIdx && IsCountOk(ctx.minionCountAtWaveStart);

    // 그 wave가 지나갔거나, 그 wave를 미니언 수가 맞지 않게 시작함
    public override bool IsFailed(MissionContext ctx)
        => ctx.waveIdx > TargetWaveIdx
        || (ctx.waveIdx == TargetWaveIdx && !IsCountOk(ctx.minionCountAtWaveStart));
}
