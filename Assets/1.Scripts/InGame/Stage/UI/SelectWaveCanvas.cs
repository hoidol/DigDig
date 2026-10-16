using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SelectWaveCanvas : CanvasUI<SelectWaveCanvas>
{
    public SelectWavePanel[] selectWavePanels;

    UniTaskCompletionSource<(WaveType, WaveWayDirection)> tcs;

    // 선택지 개수별 방향 배치
    static readonly WaveWayDirection[][] DIRECTIONS_BY_COUNT =
    {
        new[] { WaveWayDirection.Top },
        new[] { WaveWayDirection.Left, WaveWayDirection.Right },
        new[] { WaveWayDirection.Left, WaveWayDirection.Top, WaveWayDirection.Right },
    };

    public async UniTask<(WaveType waveType, WaveWayDirection direction)> OpenCanvas(PhaseData phaseData, System.Action closeCallback = null)
    {
        if (phaseData.waveTypes.Count == 0)
            return (WaveType.Normal, WaveWayDirection.Top);

        base.OpenCanvas(closeCallback);

        List<WaveType> shuffled = new List<WaveType>(phaseData.waveTypes);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        if (shuffled.Count > DIRECTIONS_BY_COUNT.Length)
        {
            Debug.LogWarning($"[SelectWaveCanvas] 선택지 {shuffled.Count}개 → {DIRECTIONS_BY_COUNT.Length}개만 표시");
            shuffled.RemoveRange(DIRECTIONS_BY_COUNT.Length, shuffled.Count - DIRECTIONS_BY_COUNT.Length);
        }
        WaveWayDirection[] directions = DIRECTIONS_BY_COUNT[shuffled.Count - 1];

        for (int i = 0; i < selectWavePanels.Length; i++)
        {
            if (i < shuffled.Count)
                selectWavePanels[i].Set(shuffled[i], directions[i], OnPanelClicked);
            else
                selectWavePanels[i].Hide();
        }

        tcs = new UniTaskCompletionSource<(WaveType, WaveWayDirection)>();
        var selected = await tcs.Task;
        CloseCanvas();
        return selected;
    }
    void OnPanelClicked(WaveType waveType, WaveWayDirection direction)
    {
        tcs?.TrySetResult((waveType, direction));
    }
}

public enum WaveWayDirection
{
    Left,Top,Right
}
