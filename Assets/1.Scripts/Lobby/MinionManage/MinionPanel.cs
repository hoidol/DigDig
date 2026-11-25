
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Lobby
{
    public class MinionPanel : MonoBehaviour
    {
        public TMP_Text titleText;
        public TMP_Text descText;
        public Image thumImge;
        public virtual void SetData(MinionData uMData)
        {
            UserMinion userMinion = UserDataManager.Instance.userMinionManager.GetUserMinion(uMData.key);
            if (titleText != null)
                titleText.text = uMData.Title;
            if (descText != null)
                descText.text = uMData.GetDescription();

            if (thumImge != null)
            {
                thumImge.SetThum(uMData.thum); // SetNativeSize 후 칸 크기에 맞춤 (MinionEquipedSlotPanel.thumImage와 같은 방식)
                // (2026-11-24) 잠김(유저 레벨 미달)만 검은 실루엣. 구매 가능은 그림이 보임
                thumImge.color = UserDataManager.Instance.userMinionManager.GetBuyState(uMData.key) == MinionBuyState.Locked
                    ? Color.black : Color.white;
            }

        }

    }
}
