using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 슬롯 머신 릴 하나. Mask 안에서 reinforceSlots(3개)를 위→아래로 굴리고, 멈추면 가운데 슬롯이 curSlot
// timeScale 0(다른 UI 일시정지)에서도 돌도록 unscaled 시간 사용
public abstract class ReinforceSlotContainer : MonoBehaviour
{
    public ReinforceSlotType reinforceSlotType;
    public ReinforceSlot[] reinforceSlots;//3개 있음
    public ReinforceSlot curSlot;

    public float spinSpeed = 3000f; // 정속 회전 속도 (px/s)
    public float spinTime = 1.5f; // 정속으로 도는 시간
    public float stopTime = 0.8f; // 감속해서 멈추는 시간
    public int stopExtraSlots = 3; // 감속 중에 더 지나가는 슬롯 수

    RectTransform[] slotRects;
    float spacing; // 슬롯 간 세로 간격
    float centerY; // 가운데(curSlot) 위치
    public bool IsSpinning { get; private set; }

    void Awake()
    {
        slotRects = reinforceSlots.Select(s => (RectTransform)s.transform).ToArray();
        centerY = ((RectTransform)curSlot.transform).anchoredPosition.y;

        float[] ys = slotRects.Select(r => r.anchoredPosition.y).OrderByDescending(y => y).ToArray();
        spacing = ys[0] - ys[1];
    }
    protected string[] keys; // 이 릴에 나올 수 있는 값 목록
    public string ResultKey => curSlot.key; // 멈춘 뒤 가운데 칸 값

    // 릴에 나올 값 목록 (종류별 컨테이너가 정함)
    protected abstract string[] GetKeys();

    // key에 해당하는 썸네일 (종류별 컨테이너가 정함)
    protected abstract Sprite GetThum(string key);

    // 캔버스 열 때 호출: 값 목록 갱신 + 칸마다 랜덤 값 채우기
    public virtual void SetSlot()
    {
        keys = GetKeys();
        foreach (ReinforceSlot slot in reinforceSlots)
            SetRandomSlot(slot);
    }

    void SetRandomSlot(ReinforceSlot slot)
    {
        string key = GetRandomKey();
        slot.SetSlot(key, GetThum(key));
    }

    protected virtual string GetRandomKey()
    {
        return keys[Random.Range(0, keys.Length)];
    }

    public async UniTask Spin(CancellationToken token)
    {
        if (IsSpinning)
            return;
        IsSpinning = true;

        // 정속 회전
        float elapsed = 0f;
        while (elapsed < spinTime)
        {
            if (await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow())
                return;
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;
            Move(spinSpeed * dt);
        }

        // 감속: 다음 정렬 위치 + stopExtraSlots칸만큼 ease-out으로 이동 → 정확히 슬롯이 가운데에 멈춤
        float total = DistanceToNextAlign() + spacing * stopExtraSlots;
        float moved = 0f;
        elapsed = 0f;
        while (elapsed < stopTime)
        {
            if (await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow())
                return;
            elapsed = Mathf.Min(elapsed + Time.unscaledDeltaTime, stopTime);
            float t = elapsed / stopTime;
            float target = total * (1f - Mathf.Pow(1f - t, 3f)); // EaseOutCubic
            Move(target - moved);
            moved = target;
        }

        Snap();
        IsSpinning = false;
    }

    // 모든 슬롯을 아래로 이동, 맨 아래를 벗어난 슬롯은 맨 위로 올리고 내용 새로 뽑기
    void Move(float delta)
    {
        float bottom = centerY - spacing * 1.5f;
        float loop = spacing * slotRects.Length;
        for (int i = 0; i < slotRects.Length; i++)
        {
            Vector2 pos = slotRects[i].anchoredPosition;
            pos.y -= delta;
            if (pos.y < bottom)
            {
                pos.y += loop;
                SetRandomSlot(reinforceSlots[i]);
            }
            slotRects[i].anchoredPosition = pos;
        }
    }

    // 가운데 아래로 내려가며 가장 먼저 가운데에 도착할 슬롯까지의 거리
    float DistanceToNextAlign()
    {
        float min = float.MaxValue;
        foreach (RectTransform r in slotRects)
        {
            float d = r.anchoredPosition.y - centerY;
            if (d >= 0 && d < min)
                min = d;
        }
        return min == float.MaxValue ? 0f : min;
    }

    // 오차 정리: 가운데에 가장 가까운 슬롯을 curSlot으로, 나머지는 간격에 맞춰 정렬
    void Snap()
    {
        int centerIdx = 0;
        for (int i = 1; i < slotRects.Length; i++)
        {
            if (Mathf.Abs(slotRects[i].anchoredPosition.y - centerY) < Mathf.Abs(slotRects[centerIdx].anchoredPosition.y - centerY))
                centerIdx = i;
        }

        for (int i = 0; i < slotRects.Length; i++)
        {
            Vector2 pos = slotRects[i].anchoredPosition;
            pos.y = centerY + Mathf.Round((pos.y - centerY) / spacing) * spacing;
            slotRects[i].anchoredPosition = pos;
        }
        curSlot = reinforceSlots[centerIdx];
    }
}

public enum ReinforceSlotType
{
    Target, //
    Buff,
    Value
}

public enum ReinforceTargetType //총 7개
{
    KingSlime, //1개
    Slime, //5개
    None, //꽝
}

public enum ReinforceBuffType
{
    AttackPower, //*5% *10%,*15% *20%
    AttackSpeed, //*5% *10%,*15%
    CritChance, //+2% +3%,+4% +5%
    CritPower, //+20%,+25% +30%
    MaxHp,
    None, //꽝
}
