using UnityEngine;
using UnityEngine.UI;

// 로비에 보이는 보스 그림 (인게임 Boss와 별개, 그림만). LobbyReturnController가 그림·자리를 정함
// SpriteRenderer(월드) / Image(UI) 중 연결된 쪽에 그림을 넣음
public class LobbyBoss : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Image image;

    void Reset()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        image = GetComponentInChildren<Image>(true);
    }

    // sprite가 null이면 지금 그림 유지
    public void Show(Sprite sprite)
    {
        gameObject.SetActive(true);
        if (sprite == null)
            return;
        if (spriteRenderer != null)
            spriteRenderer.sprite = sprite;
        if (image != null)
            image.sprite = sprite;
    }

    // 그림만 켜고 끔 (최종 보스 다음 = 그림 없이 "Clear" 글씨만)
    public void SetSpriteVisible(bool visible)
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;
        if (image != null)
            image.enabled = visible;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
