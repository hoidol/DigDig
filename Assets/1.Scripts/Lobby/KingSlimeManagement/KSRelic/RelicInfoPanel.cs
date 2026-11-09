using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

// 유물 이미지 + 이름 + 설명 + 장착 슬라임 중 해당 종 수
public class RelicInfoPanel : MonoBehaviour
{
    [FormerlySerializedAs("equipmentThumPanel")] public RelicThumPanel thumPanel;
    public TMP_Text nameText;
    public TMP_Text descText;
    public TMP_Text matchText; // matchTags가 없으면 숨김

    public void SetPanel(RelicData data)
    {
        thumPanel.SetRelicData(data);
        nameText.text = data.Title;
        descText.text = data.Desc;

        bool showMatch = data.matchTags != SlimeTag.None;
        matchText.gameObject.SetActive(showMatch);
        if (showMatch)
        {
            var equippedTags = RelicManager.GetEquippedSlimeTags();
            int count = RelicData.CountMatchingSlimes(data.matchTags, equippedTags);
            matchText.text = string.Format(TranslateManager.GetText("Relic_MatchSlime"), count, equippedTags.Count);
        }
    }
}
