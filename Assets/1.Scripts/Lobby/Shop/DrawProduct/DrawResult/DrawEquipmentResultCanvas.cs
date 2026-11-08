using UnityEngine;
namespace Lobby
{
    public class DrawEquipmentResultCanvas : DrawResultCanvas<DrawEquipmentResultCanvas> 
{
    protected override GradeType SetCardData(DrawResultCard card, string key)
    {
        EquipmentData equipmentData = EquipmentManager.Instance.GetEquipmentData(key);
        card.GetComponentInChildren<EquipmentThumPanel>(true).SetEquipmentData(equipmentData);
        return equipmentData.grade;
    }
}
}
