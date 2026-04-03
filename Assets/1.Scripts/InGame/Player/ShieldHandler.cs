using UnityEngine;

public class ShieldHandler : MonoBehaviour
{
    public bool IsShielded { get; private set; }

    public void Activate()
    {
        IsShielded = true;
    }

    // 데미지 차단 시도. 막으면 true 반환
    public bool TryBlock()
    {
        if (!IsShielded) return false;
        IsShielded = false;
        return true;
    }
}
