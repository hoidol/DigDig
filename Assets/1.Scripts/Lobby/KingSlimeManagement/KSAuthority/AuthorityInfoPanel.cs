using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

// 권능 이미지 + 이름 + 설명 + 장착 슬라임 중 해당 종 수
public class AuthorityInfoPanel : MonoBehaviour
{
    [FormerlySerializedAs("equipmentThumPanel")] public AuthorityThumPanel thumPanel;
    public TMP_Text nameText;
    public TMP_Text descText;
    public TMP_Text matchText; // matchTags가 없으면 숨김

    public void SetPanel(AuthorityData data)
    {
        thumPanel.SetAuthorityData(data);
        nameText.text = data.Title;
        descText.text = data.Desc;

        bool showMatch = data.matchTags != MinionTag.None;
        matchText.gameObject.SetActive(showMatch);
        if (showMatch)
        {
            var equippedTags = AuthorityManager.GetEquippedMinionTags();
            int count = AuthorityData.CountMatchingMinions(data.matchTags, equippedTags);
            matchText.text = string.Format(TranslateManager.GetText("Authority_MatchMinion"), count, equippedTags.Count);
        }
    }
}
