using UnityEngine;
using UnityEngine.UI;
namespace Lobby
{
    // 장착 슬롯 한 칸 (MinionManageCanvas 위쪽 5칸 / MinionEquipCanvas 안 5칸 공용)
    // 클릭: 장착 창 안이면 이 칸에 장착(MinionEquipCanvas.Selected), 그 외(미니언 관리 화면)면 정보 창(MinionInfoCanvas)
    // 버튼(비워 두면 thumImage의 Button) 클릭은 코드에서 연결 → 인스펙터 OnClick에 따로 걸지 말 것
    public class MinionEquipedSlotPanel : MinionPanel
    {
        public Image thumImage;
        public Button slotButton;
        public int idx;
        [SerializeField] UserMinion userMinion;

        void Awake()
        {
            if (slotButton == null && thumImage != null)
                slotButton = thumImage.GetComponent<Button>();
            if (slotButton != null)
                slotButton.onClick.AddListener(OnClickedSlot);
        }

        public void SetData(UserMinion userMinion)
        {
            this.userMinion = userMinion;
            thumImage.SetThum(userMinion.MinionData.thum);
            SetData(userMinion.MinionData);
        }

        public void OnClickedSlot()
        {
            if (GetComponentInParent<MinionEquipCanvas>(true) != null)
            {
                MinionEquipCanvas.Instance.Selected(idx);
                return;
            }
            if (userMinion != null && userMinion.MinionData != null)
                MinionInfoCanvas.Instance.OpenCanvas(userMinion.MinionData);
        }
    }
}
