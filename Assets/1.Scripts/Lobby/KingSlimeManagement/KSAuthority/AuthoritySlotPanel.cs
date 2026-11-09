using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

// 권능 목록 한 칸 (한 줄 = 한 묶음). 선택됨 / 잠김(Lv.N) 표시, 누르면 권능 정보 창
public class AuthoritySlotPanel : MonoBehaviour
{
    [FormerlySerializedAs("equipmentThumPanel")] public AuthorityThumPanel thumPanel;
    [FormerlySerializedAs("equiptingPanel")][FormerlySerializedAs("equippedPanel")] public GameObject selectedPanel;
    public GameObject lockPanel;
    public TMP_Text lockText;
    public AuthorityData authorityData;

    public void SetAuthority(AuthorityData authorityData)
    {
        this.authorityData = authorityData;
        gameObject.SetActive(authorityData != null);
        if (authorityData == null)
            return;

        var userAuthority = UserDataManager.Instance.userAuthorityManager;
        bool unlocked = userAuthority.IsTierUnlocked(authorityData.tier);
        thumPanel.SetAuthorityData(authorityData);
        selectedPanel.SetActive(userAuthority.IsSelected(authorityData));
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
