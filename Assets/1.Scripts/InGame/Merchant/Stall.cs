using UnityEngine;

public class Stall : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public void SetItemData(ItemData itemData)
    {
        spriteRenderer.sprite = itemData.thumbnail;
    }
}
