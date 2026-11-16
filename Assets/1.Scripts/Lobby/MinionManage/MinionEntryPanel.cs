using UnityEngine;
using UnityEngine.EventSystems;
namespace Lobby
{
    // 카드 어디를 눌러도 정보 창이 열리도록 칸 루트에서 클릭을 받는다.
    // (썸네일 버튼이 받는 클릭은 그 버튼이 처리하고, 카드 틀·글씨 등 나머지 영역의 클릭이 여기로 올라온다)
    public class MinionEntryPanel : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            // 스크롤 드래그 끝에 오는 클릭은 무시
            if (eventData.dragging)
                return;
            OnClickedPanel();
        }

        public MinionPanel minionPanel;
        public MinionData minionData;
        public  UserMinion userMinion;
        public virtual void SetData(MinionData minionData)
        {
            this.minionData = minionData;
            if(minionData == null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
            userMinion = UserDataManager.Instance.userMinionManager.GetUserMinion(minionData.key); 
            if(minionPanel == null)
            {
                Debug.Log("MinionEntryPanel if(minionPanel == null)");
            }
            minionPanel.SetData(minionData);
        }

        public virtual void UpdatePanel()
        {
            if(minionData ==null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
        }
        
        public virtual void OnClickedPanel()
        {
            MinionInfoCanvas.Instance.OpenCanvas(minionData);
        }
        
    }    
}
