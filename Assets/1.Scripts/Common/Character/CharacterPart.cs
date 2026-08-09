using UnityEngine;

public class CharacterPart : MonoBehaviour
{
    public EquipPartType equipPartType;
    public SpriteRenderer spriteRenderer;
    public void UpdateCharacter()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        UserEquipment[] equippedEquipments = UserManager.Instance.userEquipmentManager.GetEquippedUserEquipments();
        spriteRenderer.sprite = null;
        for (int i = 0; i < equippedEquipments.Length; i++)
        {
            if (equippedEquipments[i].equipmentData.equipPartType == equipPartType)
            {
                spriteRenderer.sprite = equippedEquipments[i].equipmentData.thum;
            }
        }
    }
}