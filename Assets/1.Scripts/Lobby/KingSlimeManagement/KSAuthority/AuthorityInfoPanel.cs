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

        bool showMatch = data.matchTags != SlimeTag.None;
        matchText.gameObject.SetActive(showMatch);
        if (showMatch)
        {
            var equippedTags = AuthorityManager.GetEquippedSlimeTags();
            int count = AuthorityData.CountMatchingSlimes(data.matchTags, equippedTags);
            matchText.text = string.Format(TranslateManager.GetText("Authority_MatchSlime"), count, equippedTags.Count);
        }
    }
}
