using UnityEngine;
using UnityEngine.UI;

// 슬롯 머신 칸 하나. 컨테이너가 key와 썸네일을 넣어주면 표시만 담당
public abstract class ReinforceSlot : MonoBehaviour
{
    public const string NONE_KEY = "None"; // 꽝

    public string key;//현재 슬롯에 담김 값
    public Image thum;

    public virtual void SetSlot(string key, Sprite sprite)
    {
        this.key = key;
        thum.sprite = sprite;
    }
}
