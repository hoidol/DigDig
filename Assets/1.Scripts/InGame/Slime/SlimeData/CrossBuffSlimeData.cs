using UnityEngine;

// 십자 4칸 버프 슬라임(CrossBuffSlime) 공용 데이터
// buffValues: mergeLv별 버프 값. Multiply면 배율(1.2 = +20%), Add면 더하는 값(치명타 확률은 % 단위)
// 기본값은 자식에서 DefaultBuffOpType / DefaultBuffValues를 재정의. 에셋에 직렬화된 값이 있으면 그 값이 우선
public abstract class CrossBuffSlimeData : EnhanceSlimeData
{
    public StatOpType buffOpType;
    public float[] buffValues;

    protected virtual StatOpType DefaultBuffOpType => StatOpType.Multiply;
    protected virtual float[] DefaultBuffValues => new[] { 1.1f, 1.15f, 1.2f, 1.25f, 1.3f };

    // 필드 초기값 대신 생성자에서 넣어야 자식 재정의 값이 들어감 (Reset은 기존 에셋에선 안 불림)
    protected CrossBuffSlimeData()
    {
        buffOpType = DefaultBuffOpType;
        buffValues = DefaultBuffValues;
    }

    public float GetBuffValue(int mergeLv) => buffValues.GetByMergeLevel(mergeLv);

    // 설명용 % 표기 (Multiply 1.2 → 20, Add 10 → 10)
    protected int GetBuffPercent(int mergeLv)
    {
        float v = GetBuffValue(mergeLv);
        return Mathf.RoundToInt(buffOpType == StatOpType.Multiply ? (v - 1f) * 100f : v);
    }
}
