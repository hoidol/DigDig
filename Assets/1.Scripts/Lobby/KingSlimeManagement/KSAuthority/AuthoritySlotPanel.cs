using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

// 권능 목록 한 칸 (한 줄 = 한 묶음). 설명 / 선택됨(강조 테두리) / 잠김(Lv.N) 표시, 누르면 권능 정보 창
public class AuthoritySlotPanel : MonoBehaviour
{
    [FormerlySerializedAs("equipmentThumPanel")] public AuthorityThumPanel thumPanel;
    [FormerlySerializedAs("equiptingPanel")][FormerlySerializedAs("equippedPanel")] public GameObject selectedPanel;
    public GameObject lockPanel;
    public TMP_Text lockText;
    public AuthorityData authorityData;
    public TMP_Text descText; // 권능 설명 (정보 창을 열지 않아도 묶음 3개를 비교)
    public GameObject highlightObject; // 지금 고른 권능 강조 (묶음마다 하나)

    public void SetAuthority(AuthorityData authorityData)
    {
        this.authorityData = authorityData;
        gameObject.SetActive(authorityData != null);
        if (authorityData == null)
            return;

        var userAuthority = UserDataManager.Instance.userAuthorityManager;
        bool unlocked = userAuthority.IsTierUnlocked(authorityData.tier);
        thumPanel.SetAuthorityData(authorityData);
        bool selected = userAuthority.IsSelected(authorityData);
        selectedPanel.SetActive(selected);
        if (highlightObject != null)
            highlightObject.SetActive(selected);
        if (descText != null)
            descText.text = authorityData.Desc;
        if (lockPanel != null)
            lockPanel.SetActive(!unlocked);
        if (lockText != null)
            lockText.text = $"Lv.{AuthorityData.GetUnlockLevel(authorityData.tier)}";
    }

    public void OnClickedSlot()
    {
        if (authorityData != null)
            AuthorityInfoCanvas.Instance.OpenCanvas(authorityData);
    }
}
